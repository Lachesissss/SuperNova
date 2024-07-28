using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Player : MonoBehaviour
    {
        public CarController carController;

        private void Update()
        {
            carController.ChangeCarTurnState(GameEntry.PlayerInput.steeringDeltaP1);
            carController.ChangeCarForwardState(GameEntry.PlayerInput.motorDeltaP1);
            carController.ChangeCarBoostState(GameEntry.PlayerInput.boostP1);
            carController.ChangeCarHandBrakeState(GameEntry.PlayerInput.handbrakeP1);
        }
    }
}