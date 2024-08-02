using UnityEngine;

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
                ActivateSkill(0);
            }
            if(GameEntry.PlayerInputManager.skill2P1)
            {
                ActivateSkill(1);
            }
            if(GameEntry.PlayerInputManager.skill3P1)
            {
                ActivateSkill(2);
            }
            if(GameEntry.PlayerInputManager.switchP1)
            {
                GameEntry.EventManager.Fire(this, SwitchCarEventArgs.Create());
            }
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
        }
        
        public override void OnReturnToPool(bool isShowDown = false)
        {
            base.OnReturnToPool(isShowDown);
        }

        public override void OnSwitchCar()
        {
            base.OnSwitchCar();
        }
    }
}