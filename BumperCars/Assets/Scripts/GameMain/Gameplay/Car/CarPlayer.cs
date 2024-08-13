using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    
    
    public sealed class CarPlayer : CarController
    {
        private static bool isP1Exist = false;
        private static bool isP2Exist = false;
        public enum PlayerType
        {
            P1,
            P2,
        }
        private PlayerInputManager input;
        public PlayerType playerType;
        private bool Boost => playerType == PlayerType.P1?input.boostP1:(m_globalConfig.p2UsingJoySticks?input.boostP2J:input.boostP2) ;
        private bool Skill1 => playerType == PlayerType.P1?input.skill1P1:(m_globalConfig.p2UsingJoySticks?input.skill1P2J:input.skill1P2);
        private bool Skill2 => playerType == PlayerType.P1?input.skill2P1:(m_globalConfig.p2UsingJoySticks?input.skill2P2J:input.skill2P2);
        private bool Skill3 => playerType == PlayerType.P1?input.skill3P1:(m_globalConfig.p2UsingJoySticks?input.skill3P2J:input.skill3P2);
        private bool Switch => playerType == PlayerType.P1?input.switchP1:(m_globalConfig.p2UsingJoySticks?input.switchP2J:input.switchP2);
        private float SteeringDelta => playerType == PlayerType.P1?input.steeringDeltaP1:(m_globalConfig.p2UsingJoySticks?input.steeringDeltaP2J:input.steeringDeltaP2);
        private float MotorDelta => playerType == PlayerType.P1?input.motorDeltaP1:(m_globalConfig.p2UsingJoySticks?input.motorDeltaP2J:input.motorDeltaP2);
        private bool Handbrake => playerType == PlayerType.P1?input.handbrakeP1:(m_globalConfig.p2UsingJoySticks?input.handbrakeP2J:input.handbrakeP2);

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            input = GameEntry.PlayerInputManager;
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if(!IsHasCar) return;
            
            carComponent.ChangeCarTurnState(SteeringDelta*0.4f);
            carComponent.ChangeCarForwardState(MotorDelta);
            carComponent.ChangeCarHandBrakeState(Handbrake);
            if(Boost)
            {
                TryBoost();
            }
            if(Switch)
            {
                TrySwitch();
            }
            if(Skill1)
            {
                ActivateSkill(0);
            }
            if(Skill2)
            {
                ActivateSkill(1);
            }
            if(Skill3)
            {
                ActivateSkill(2);
            }
        }
        
        private void TryBoost()
        {
            if(!canBoost) return;
            carComponent.DoBoost();
            var coolingInfo = new CoolingInfo();
            coolingInfo.playerName = controllerName;
            coolingInfo.isBoostCoolingInfoChanged = true;
            coolingInfo.BoostCoolingTime = m_globalConfig.carBoostCoolingTime;
            GameEntry.EventManager.Invoke(this, PlayerCoolingUIUpdateEventArgs.Create(coolingInfo));
            // 冷却时间
            StartCoroutine(StartBoostCoolingTime());
        }

        private void TrySwitch()
        {
            if(!canSwitch) return;
            carComponent.DoSwitch();
            //float randomCooling = Random.Range(m_globalConfig.carSwitchCoolingTimeMin, m_globalConfig.carSwitchCoolingTimeMax);
            float randomCooling = m_globalConfig.carSwitchCoolingTimeMin;
            var coolingInfo = new CoolingInfo();
            coolingInfo.playerName = controllerName;
            coolingInfo.isSwitchCoolingInfoChanged = true;
            coolingInfo.SwitchCoolingTime = randomCooling;
            GameEntry.EventManager.Invoke(this, PlayerCoolingUIUpdateEventArgs.Create(coolingInfo));
            StartCoroutine(StartSwitchCoolingTime(randomCooling));
        }
        
        private IEnumerator StartBoostCoolingTime()
        {
            canBoost = false;
            yield return new WaitForSeconds(m_globalConfig.carBoostCoolingTime);
            canBoost = true;
        }
        
        private IEnumerator StartSwitchCoolingTime(float randomCooling)
        {
            canSwitch = false;
            yield return new WaitForSeconds(randomCooling);
            canSwitch = true;
        }
        
        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            ResetCarPlayer(userData);
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            ResetCarPlayer(userData);
        }

        private void ResetCarPlayer(object userData)
        {
            if(userData is CarControllerData { userData: PlayerType type })
            {
                if(type==PlayerType.P1)
                {
                   if(isP1Exist)Debug.LogError("出错了，P1已存在");
                   else
                   {
                       isP1Exist = true;
                       playerType = PlayerType.P1;
                   } 
                }
                else 
                {
                    if(isP2Exist)Debug.LogError("出错了，P2已存在");
                    else
                    {
                        isP2Exist = true;
                        playerType = PlayerType.P2;
                    } 
                }
            }
            else
            {
                Debug.LogError("没有指定该Player是几P");
            }
        }
        
        public override void OnReturnToPool(bool isShowDown = false)
        {
            base.OnReturnToPool(isShowDown);
            if(playerType==PlayerType.P1) isP1Exist = false;
            else isP2Exist = false;
        }

        public override void OnSwitchCar()
        {
            base.OnSwitchCar();
        }
    }
}