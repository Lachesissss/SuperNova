using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Player : MonoBehaviour
    {
        public CarController carController;

        private void Update()
        {
            carController.ChangeCarTurnState(GameEntry.PlayerInputManager.steeringDeltaP1);
            carController.ChangeCarForwardState(GameEntry.PlayerInputManager.motorDeltaP1);
            carController.ChangeCarBoostState(GameEntry.PlayerInputManager.boostP1);
            carController.ChangeCarHandBrakeState(GameEntry.PlayerInputManager.handbrakeP1);
        }
    }
}