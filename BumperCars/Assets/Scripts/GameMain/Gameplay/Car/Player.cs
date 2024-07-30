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
            carController.ChangeCarBoostState(GameEntry.PlayerInputManager.boostP1);
            carController.ChangeCarHandBrakeState(GameEntry.PlayerInputManager.handbrakeP1);
        }

        public override void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnInit(pos, rot, userData);
            if(userData is string str)
            {
                carController.carName = str;
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            var rb = carController.bodyRb;
            if(rb!=null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}