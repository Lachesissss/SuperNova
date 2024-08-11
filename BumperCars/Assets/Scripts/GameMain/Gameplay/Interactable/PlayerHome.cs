using UnityEngine;

namespace Lachesis.GamePlay
{
    public class PlayerHome : EntityComponent
    {
        public enum PlayerHomeType
        {
            Player1,
            Player2
        }

        private string playerName;

        public PlayerHomeType type;

        public override void OnEntityInit(object userData = null)
        {
            base.OnEntityInit(userData);
            if (type == PlayerHomeType.Player1) playerName = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p1Name;
            if (type == PlayerHomeType.Player2) playerName = GameEntry.ConfigManager.GetConfig<GlobalConfig>().p2Name;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BumperCar"))
            {
                var controller = other.GetComponent<CarBody>().carComponent;
                if (playerName == controller.carControllerName)
                    GameEntry.EventManager.Invoke(this, PlayerArriveHomeEventArgs.Create(controller.carControllerName));
            }
        }
    }
}