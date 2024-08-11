using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Coin : Entity
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BumperCar"))
            {
                var controller = other.GetComponent<CarBody>().carComponent;
                GameEntry.EventManager.Invoke(this, CarriedScoreUpdateEventArgs.Create(controller.carControllerName, 1));
                GameEntry.EntityManager.ReturnEntity(EntityEnum.Coin, this);
            }
        }
    }
}