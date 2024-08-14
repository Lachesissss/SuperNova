using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CarComponent : Entity
    {
        public enum DriverMode
        {
            Front,
            Back,
            All
        }

        public DriverMode driverMode = DriverMode.Back;
        public float idealRPM = 200;
        public float maxRPM = 400;
        public float backIdealRPM = 50;
        public float backMaxRPM = 100;
        public float moveTorque = 30000;
        public float handBreakTorque = 60000;
        public float autoBreakTorque = 10000;
        public float antiRoll = 2000f;
        public Transform centerOfGravity;
        public CarAttacker carAttacker;
        public CarController controller;
        public ParticleSystem energyEffect;
        public ParticleSystem boostEffect;
        public ParticleSystem reviveEffect;
        
        public float maxAngle;
        public float targetAngle;
        public float returnAngleSpeed = 400;
        public static int attackNum = 0;
        public static bool attackNumWrited = false;
        public Rigidbody bodyRb;
        [Header("车轮碰撞体")] public WheelCollider leftFrontCollider;
        public WheelCollider leftBackCollider;
        public WheelCollider rightFrontCollider;
        public WheelCollider rightBackCollider;

        [Header("车轮模型")] public Transform leftFrontTrans;
        public Transform leftBackTrans;
        public Transform rightFrontTrans;
        public Transform rightBackTrans;
        
        private List<Entity> m_effectEntities;
        private float m_startMass;
        //既是名字，也是id
        public string carControllerName;
        
        public readonly List<WheelCollider> wheelColliders = new();
        
        private float m_carTurnValue;

        private float m_carForwardValue;

        private bool m_isHandBrake;

        private GlobalConfig m_globalConfig;
        
        private Vector3 deltaPos = new Vector3(0,0.05f,0);
        
        private bool m_isInWitchTime = false;

        private bool m_isHasMagicShield;

        private Entity m_shieldEffectEntity;
        public bool IsInWitchTime =>m_isInWitchTime;
        
        public enum ClothColor
        {
            Yellow,
            Green,
            Red,
            Black
        }
        
        private static readonly string yellowColor = "F76C09";
        private static readonly string blackColor = "0FC3EE";
        private static readonly string greenColor = "5BD511";
        private static readonly string redcolor = "F70B3E";
        
        [SerializeField]
        private List<Renderer> bodyRenderers;
        public ClothColor clothColor;
        private static readonly int SpecColor = Shader.PropertyToID("_SpecColor");

        public void AddEffectEntity(Entity entity)
        {
            m_effectEntities.Add(entity);
        }

        public void RemoveEffectEntity(Entity entity)
        {
            m_effectEntities.Remove(entity);
        }
        
        public float GetSpeed()
        {
            return rightBackCollider.radius * Mathf.PI * rightBackCollider.rpm * 60f / 1000f;
        }
        
        public float GetRPM()
        {
            return (rightBackCollider.rpm+rightFrontCollider.rpm+leftBackCollider.rpm+leftFrontCollider.rpm)/4f;
        }

        public override void OnInit(object userData = null)
        {
            base.OnInit();
            
            if (centerOfGravity != null) bodyRb.centerOfMass = centerOfGravity.position;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            wheelColliders.Add(leftBackCollider);
            wheelColliders.Add(leftFrontCollider);
            wheelColliders.Add(rightBackCollider);
            wheelColliders.Add(rightFrontCollider);
            carAttacker.Init(this, bodyRb);
            m_effectEntities = new List<Entity>();
            m_startMass = bodyRb.mass;
            //Reset();
        }

        
        
        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            ResetCar(userData);
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            ResetCar(userData);
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            if(isShutDown)
            {
                if(!attackNumWrited)
                {
                    var path = m_globalConfig.isUsingRVO ? "Scripts/RVO/openRVO.txt" : "Scripts/RVO/closeRVO.txt";
                    WriteToFile(path, $"attackNum:{attackNum} deadNum:{BattleModel.Instance.deadCnt}");
                    attackNumWrited = true;
                }
                
                return;
            }
            StopAllCoroutines();
            GameEntry.EventManager.RemoveListener(AttackEventArgs.EventId, OnAttackArrived);
            foreach (var effectEntity in m_effectEntities)
                //这里可能有风险，一个特效去销毁另一个特效的话，不过一般不会这么写
                GameEntry.EntityManager.ReturnEntity(effectEntity.entityEnum, effectEntity);
            m_effectEntities.Clear();
            carAttacker.StopAllCoroutines();
            
        }
        
        public void DoBoost()
        {
            transform.position += deltaPos;
            
            var forceDirection = transform.forward;
            forceDirection.y = 0;
            forceDirection = forceDirection.normalized;
            bodyRb.velocity = forceDirection.normalized * m_globalConfig.impactSpeed + bodyRb.velocity;
            boostEffect.Play();
            // 暂时降低摩擦力
            StartCoroutine(TemporarilyReduceFriction(wheelColliders));
            
        }
        
        public void DoSwitch()
        {
            GameEntry.EventManager.Invoke(this, SwitchCarEventArgs.Create());
        }
        
        
        
        private IEnumerator TemporarilyReduceFriction(List<WheelCollider> wheelColliders)
        {
            for (var i = 0; i < wheelColliders.Count; i++)
            {
                
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.underAttackSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
            m_isInWitchTime = true;
            yield return new WaitForSeconds(m_globalConfig.extremeDodgeTime); // 魔女时间
            m_isInWitchTime = false;
            // 等待一段时间
            yield return new WaitForSeconds(m_globalConfig.frictionRestoreDelay-m_globalConfig.extremeDodgeTime);

            // 恢复原来的摩擦力设置
            for (var i = 0; i < wheelColliders.Count; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.defaultCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.defaultCarSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
        }
        
        private void ResetCar(object userData = null)
        {
            m_carForwardValue = 0;
            m_carTurnValue = 0;
            m_isHandBrake = false;
            m_isInWitchTime = false;
            m_isHasMagicShield = false;
            bodyRb.velocity = Vector3.zero;
            bodyRb.angularVelocity = Vector3.zero;
            bodyRb.mass = m_startMass;
            controller =null;
            carControllerName = "未定义";
            energyEffect.Stop();
            boostEffect.Stop();
            reviveEffect.Play();
            StartCoroutine(StopReviveEffect());
            carAttacker.Reset();
            GameEntry.EventManager.AddListener(AttackEventArgs.EventId, OnAttackArrived);
            
            // 确保所有的 WheelColliders 初始状态下不施加动力或转矩
            foreach (var wheel in wheelColliders)
            {
                wheel.motorTorque = 0;
                wheel.brakeTorque = Mathf.Infinity; // 应用无穷大的刹车力，防止车移动
                wheel.steerAngle = 0;
            }
            foreach (var collider in wheelColliders)
            {
                var forwardFriction = collider.forwardFriction;
                forwardFriction.stiffness = m_globalConfig.defaultCarForwardFrictionStiffness;
                collider.forwardFriction = forwardFriction;

                var sidewaysFriction = collider.forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.defaultCarSidewaysFrictionStiffness;
                collider.sidewaysFriction = sidewaysFriction;
            }
            
            // 等待一帧以确保所有物理计算完成
            StartCoroutine(ResetBrakeTorque());
            
            if(userData is ClothColor color)
            {
                clothColor = color;
                Color setColor;
                switch (color)
                {
                    case ClothColor.Black:
                        setColor =  GetColorByHexString(blackColor);
                        break;
                    case ClothColor.Green:
                        setColor =  GetColorByHexString(greenColor);
                        break;
                    case ClothColor.Red:
                        setColor =  GetColorByHexString(redcolor);
                        break;
                    case ClothColor.Yellow:
                        setColor =  GetColorByHexString(yellowColor);
                        break;
                    default:
                        setColor =  GetColorByHexString(yellowColor);
                        
                        break;
                }
                foreach (var bodyRenderer in bodyRenderers)bodyRenderer.material.SetColor(SpecColor, setColor);;
            }
        }

        private void OnAttackArrived(object sender, GameEventArgs e)
        {
            if(e is AttackEventArgs args)
                if(args.attackInfo.underAttacker == carControllerName)
                {
                    //TODO:碰撞计数
                    attackNum++;
                    if (m_isInWitchTime && args.attackInfo.canDodge)
                    {
                        GameEntry.instance.StartCoroutine(ShowWitchTimeEffect(controller.carComponent.transform));
                    }
                    else
                    {
                        if (m_isHasMagicShield)
                        {
                            m_isHasMagicShield = false;
                            GameEntry.EntityManager.ReturnEntity(EntityEnum.MagicShieldEffect, m_shieldEffectEntity);
                            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create($"[{carControllerName}]的[秘法护盾]效果发动!"));
                        }
                        else
                        {
                            GameEntry.EventManager.Invoke(this, AttackHitArgs.Create(args.attackInfo));
                            args.OnHit.Invoke();
                        }
                    }
                }
        }
        public void GetMagicShield()
        {
            m_isHasMagicShield = true;
            m_shieldEffectEntity = GameEntry.EntityManager.CreateEntity<MagicShieldEffect>(EntityEnum.MagicShieldEffect, transform);
            StartCoroutine(DelayToReturnShield());
        }

        private IEnumerator DelayToReturnShield()
        {
            yield return new WaitForSeconds(10f);
            m_isHasMagicShield = false;
            GameEntry.EntityManager.ReturnEntity(EntityEnum.MagicShieldEffect, m_shieldEffectEntity);
        }
        
        private IEnumerator ShowWitchTimeEffect(Transform source)
        {
            int randomValue = Random.Range(0, 2) == 0 ? -1 : 1;
            bodyRb.mass*=2;
            RotateRigidbody(bodyRb, randomValue*270, 0.5f);
            RotateRigidbody(bodyRb, randomValue*270, 0.5f);
            Time.timeScale = 0.5f;
            var effect = GameEntry.EntityManager.CreateEntity<ExtremeDodgeEffect>(EntityEnum.ExtremeDodgeEffect, source, new Vector3(0,0.3f,0f));
            yield return new WaitForSecondsRealtime(m_globalConfig.extremeDodgeTime);
            Time.timeScale = 1f;
            GameEntry.EntityManager.ReturnEntity(EntityEnum.ExtremeDodgeEffect, effect);
            bodyRb.mass/=2;
            controller.canBoost = true;
            controller.canSwitch = true;
            var coolingInfo = new CoolingInfo();
            coolingInfo.playerName = carControllerName;
            coolingInfo.isBoostCoolingInfoChanged = true;
            coolingInfo.isSwitchCoolingInfoChanged = true;
            coolingInfo.BoostCoolingTime = 0;
            coolingInfo.SwitchCoolingTime = 0;
            GameEntry.EventManager.Invoke(this, PlayerCoolingUIUpdateEventArgs.Create(coolingInfo));
        }
        
        void RotateRigidbody(Rigidbody rb, float angle, float duration)
        {
            // 将角度转换为弧度
            float angleInRadians = angle * Mathf.Deg2Rad;
            // 计算所需的角速度
            float angularVelocity = angleInRadians / duration;
            // 设置 Rigidbody 的角速度
            rb.angularVelocity = new Vector3(0, angularVelocity, 0); // 你可以根据需要调整旋转轴
        }
        
        // 在一帧之后重置刹车力
        private IEnumerator ResetBrakeTorque()
        {
            yield return null; // 等待一帧
            foreach (var wheel in wheelColliders) wheel.brakeTorque = 0;
        }
        
        private IEnumerator StopReviveEffect()
        {
            yield return new WaitForSeconds(1.5f); 
            reviveEffect.Stop();
        }

        public void ChangeCarTurnState(float turnValue)
        {
            m_carTurnValue = turnValue;
        }

        public void ChangeCarForwardState(float forwardValue)
        {
            m_carForwardValue = forwardValue;
        }

        public void ChangeCarBoostState(bool isBoost)
        {

        }

        public void ChangeCarHandBrakeState(bool isHandBrake)
        {
            m_isHandBrake = isHandBrake;
        }

        public override void OnFixedUpdate(float fixedElapseSeconds)
        {
            base.OnFixedUpdate(fixedElapseSeconds);
            WheelsUpdate();
        }

        //控制移动 转向
        private void WheelsUpdate()
        {
            var forwardValue = m_carForwardValue > 0.05 || m_carForwardValue < -0.05 ? m_carForwardValue : 0;
            if(forwardValue>0)
            {
                if(energyEffect.isStopped)
                {
                    energyEffect.Play();
                }
            }
            else
            {
                if(energyEffect.isPlaying)
                {
                    energyEffect.Stop();
                }
            }
            if(forwardValue<0) forwardValue*=0.2f;
            var scaledTorque = forwardValue * moveTorque;
            var curRPM = leftBackCollider.rpm;
            if (curRPM is float.NaN)
                curRPM = 0;

            if (curRPM < idealRPM)
                scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, curRPM / idealRPM);
            else
                scaledTorque = Mathf.Lerp(scaledTorque, 0, (curRPM - idealRPM) / (maxRPM - idealRPM));

            DoRollBar(leftFrontCollider, rightFrontCollider);
            DoRollBar(leftBackCollider, rightBackCollider);

            if (m_carTurnValue < -0.05f) //左转向
                targetAngle = maxAngle*m_carTurnValue;

            else if (m_carTurnValue > 0.05f) //右转向
                targetAngle = maxAngle*m_carTurnValue;

            else //松开转向后，方向打回
                targetAngle = 0;
            leftFrontCollider.steerAngle = Mathf.MoveTowards(leftFrontCollider.steerAngle, targetAngle, returnAngleSpeed * Time.fixedDeltaTime);
            rightFrontCollider.steerAngle = Mathf.MoveTowards(leftFrontCollider.steerAngle, targetAngle, returnAngleSpeed * Time.fixedDeltaTime);

            leftFrontCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Front ? scaledTorque : 0;
            rightFrontCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Front ? scaledTorque : 0;
            leftBackCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Back ? scaledTorque : 0;
            rightBackCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Back ? scaledTorque : 0;

            foreach (var wheelCollider in wheelColliders)
            {
                var targetBreakTorque = forwardValue == 0 ? autoBreakTorque : 0;
                if (m_isHandBrake) targetBreakTorque += handBreakTorque;
                wheelCollider.brakeTorque = targetBreakTorque;
            }

            //当车轮碰撞器位置角度改变，随之也变更车轮模型的位置角度
            WheelsModel_Update(leftFrontTrans, leftFrontCollider);
            WheelsModel_Update(leftBackTrans, leftBackCollider);
            WheelsModel_Update(rightFrontTrans, rightFrontCollider);
            WheelsModel_Update(rightBackTrans, rightBackCollider);
        }

        //控制车轮模型移动 转向
        private void WheelsModel_Update(Transform t, WheelCollider wheel)
        {
            var pos = t.position;
            var rot = t.rotation;

            wheel.GetWorldPose(out pos, out rot);

            t.position = pos;
            t.rotation = rot;
        }

        //防侧翻
        private void DoRollBar(WheelCollider wheelL, WheelCollider wheelR)
        {
            WheelHit hit;
            var travelL = 1f;
            var travelR = 1f;
            var groundedL = wheelL.GetGroundHit(out hit);
            if (groundedL) travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
            var groundedR = wheelR.GetGroundHit(out hit);
            if (groundedR) travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

            var antiRollForce = (travelL - travelR) * antiRoll;

            if (groundedL)
                bodyRb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
            if (groundedR)
                bodyRb.AddForceAtPosition(wheelR.transform.up * -antiRollForce, wheelR.transform.position);
        }
        
        private Color GetColorByHexString(string str)
        {
            if (ColorUtility.TryParseHtmlString("#" + str, out Color newColor))
            {
                // 修改材质的颜色
                return newColor;
            }
            else
            {
                return Color.white;
            }
        }
        
        public static void WriteToFile(string fileName, string content, bool append = false)
        {
            var path = Application.dataPath + "/" + fileName;

            // 根据append参数决定是覆盖写入还是追加写入
            if (append)
                File.AppendAllText(path, content);
            else
                File.WriteAllText(path, content);
        }
    }
}