using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SkillPickUpItem : Entity
    {
        public SkillEnum skillEnum;

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            if(userData is SkillEnum skill)
            {
                skillEnum = skill;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.layer == LayerMask.NameToLayer("BumperCar"))
            {
                var car = other.GetComponent<CarBody>().carComponent;
                if (!car.controller.IsSkillSlotFull())
                {
                    //发出获取技能卡的事件
                    GameEntry.EventManager.Invoke(this, GetSkillEventArgs.Create(skillEnum, car.carControllerName));
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.SkillPickUpItem, this);
                    if (car.controller is CarPlayer)
                        GameEntry.SoundManager.PlayerSound(car, SoundEnum.PickUp, false);
                }
                else
                {
                    Debug.Log($"拾取失败，{car.carControllerName}身上的技能槽已满！");
                }
            }
        }
        
        public static float rotationSpeed = 90f;
        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            float rotationAmount = rotationSpeed * elapseSeconds;

            // 绕世界坐标系的Y轴旋转
            transform.Rotate(Vector3.up, rotationAmount, Space.World);
        }
    }
}

