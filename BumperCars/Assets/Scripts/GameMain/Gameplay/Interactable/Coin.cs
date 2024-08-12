using UnityEngine;

namespace Lachesis.GamePlay
{
    public class Coin : Entity
    {
        public static float rotationSpeed = 90f;
        public ParticleSystem spawnEffect;
        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            float rotationAmount = rotationSpeed * elapseSeconds;

            // 绕世界坐标系的Y轴旋转
            transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            spawnEffect.Play();
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            spawnEffect.Play();
        }

        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            if(!isShutDown) spawnEffect.Stop();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("BumperCar"))
            {
                var controller = other.GetComponent<CarBody>().carComponent;
                GameEntry.EventManager.Invoke(this, CarriedScoreUpdateEventArgs.Create(controller.carControllerName, 1));
                GameEntry.EventManager.Invoke(this, CoinPickedUpEventArgs.Create(1));
                GameEntry.EntityManager.ReturnEntity(EntityEnum.Coin, this);
            }
        }
    }
}