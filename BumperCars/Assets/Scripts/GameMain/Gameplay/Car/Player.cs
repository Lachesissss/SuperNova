using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Player : Entity
    {
        public CarController carController;

        private void Update()
        {
            carController.ChangeCarTurnState(GameEntry.PlayerInputManager.steeringDeltaP1);
            carController.ChangeCarForwardState(GameEntry.PlayerInputManager.motorDeltaP1);
            if(GameEntry.PlayerInputManager.boostP1)
            {
                carController.DoBoost();
            }
            if(GameEntry.PlayerInputManager.skill1P1)
            {
                carController.ActivateSkill(0, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill2P1)
            {
                carController.ActivateSkill(1, GetSkillTarget());
            }
            if(GameEntry.PlayerInputManager.skill3P1)
            {
                carController.ActivateSkill(2, GetSkillTarget());
            }
            carController.ChangeCarHandBrakeState(GameEntry.PlayerInputManager.handbrakeP1);
        }

        public override void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnInit(pos, rot, userData);
            if(userData is string str)
            {
                carController.carName = str;
            }
            carController.Reset();
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
        }
        
        private CarController GetSkillTarget()
        {
            return null;
        }
    }
}