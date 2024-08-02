using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lachesis.GamePlay
{
    
    
    public sealed class CarPlayer : CarController
    {
        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if(!IsHasCar) return;
            
            carComponent.ChangeCarTurnState(GameEntry.PlayerInputManager.steeringDeltaP1);
            carComponent.ChangeCarForwardState(GameEntry.PlayerInputManager.motorDeltaP1);
            carComponent.ChangeCarHandBrakeState(GameEntry.PlayerInputManager.handbrakeP1);
            if(GameEntry.PlayerInputManager.boostP1)
            {
                carComponent.DoBoost();
            }
            if(GameEntry.PlayerInputManager.skill1P1)
            {
                ActivateSkill(0, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill2P1)
            {
                ActivateSkill(1, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill3P1)
            {
                ActivateSkill(2, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.switchP1)
            {
                GameEntry.EventManager.Fire(this, SwitchCarEventArgs.Create());
            }
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            if(carComponent==null)//防止交换功能中回池导致状态不对
            {
                Debug.LogError("Player上未找到CarComponent");
            }
            if(userData is string str)
            {
                carComponent.carControllerName = str;
            }
        }

        public override void OnReturnToPool(bool isShowDown = false)
        {
            base.OnReturnToPool(isShowDown);
        }
        
        private CarComponent GetSkillTarget()
        {//先统一获取最近的实体
            float minDis = Single.PositiveInfinity;
            CarComponent closestCar = null;
            foreach (var controller in ProcedureBattle.carControllers)
            {
                if(controller.IsHasCar&&controller!=this)
                {
                    var dis =Vector3.Distance(controller.carComponent.transform.position, carComponent.transform.position);
                    if(minDis>dis)
                    {
                        minDis = dis;
                        closestCar = controller.carComponent;
                    }
                }
            }
            return closestCar;
        }
        
    }
}