using System;
using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CarAttacker : MonoBehaviour
    {
        private bool hasCollided;//碰撞冷却标记
        private Vector3 deltaPos = new Vector3(0,0.05f,0);
        public Rigidbody carBody;
        private GlobalConfig m_globalConfig;
        private CarController m_selfController;
        private void Awake()
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            m_selfController = carBody.GetComponent<CarController>();
            if(m_selfController==null)
            {
                Debug.LogError("出错了,没有找到属于的车辆控制器");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;
            // 检查碰撞对象是否是另一辆碰碰车
            var otherRigidbody = other.attachedRigidbody;
            if (otherRigidbody != null && otherRigidbody != GetComponent<Rigidbody>())
            {
                var otherWheelColliders = otherRigidbody.GetComponentsInChildren<WheelCollider>();
                otherRigidbody.transform.position += deltaPos;
                hasCollided = true;
                // 施加瞬间水平力
                var forceDirection = (otherRigidbody.transform.position - transform.position).normalized;
                var rate =  Vector3.Dot(forceDirection,transform.forward) ;
                forceDirection.y = 0;
                forceDirection = forceDirection.normalized;
                otherRigidbody.velocity = forceDirection.normalized * m_globalConfig.impactSpeed + rate * carBody.velocity;

                // 暂时降低摩擦力
                StartCoroutine(TemporarilyReduceFriction(otherWheelColliders));

                // 重置碰撞标记
                StartCoroutine(ResetCollisionFlag());
                
                var otherCar = otherRigidbody.GetComponent<CarController>();
                if(otherCar!=null)
                {
                    var attackInfo = new AttackInfo();
                    attackInfo.attacker = m_selfController.carName;
                    attackInfo.underAttacker = otherCar.carName;
                    attackInfo.attackTime = DateTime.Now;
                    attackInfo.attackType = AttackType.Collide;
                    GameEntry.EventManager.Fire(this, AttackEventArgs.Create(attackInfo));
                }
            }
        }

        private IEnumerator ResetCollisionFlag()
        {
            yield return new WaitForSeconds(0.5f);
            hasCollided = false;
        }


        private IEnumerator TemporarilyReduceFriction(WheelCollider[] wheelColliders)
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.underAttackSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }

            // 等待一段时间
            yield return new WaitForSeconds(m_globalConfig.frictionRestoreDelay);

            // 恢复原来的摩擦力设置
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.defaultCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.defaultCarSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
        }
    }
}