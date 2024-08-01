using System.Collections;
using System.Collections.Generic;
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
                var controller = other.GetComponent<CarBody>().carController;
                if(!controller.IsSkillSlotFull())
                {
                    //发出获取技能卡的事件
                    GameEntry.EventManager.Fire(this, GetSkillEventArgs.Create(skillEnum, controller.carName));
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.SkillPickUpItem, this);
                }
                else
                {
                    Debug.Log($"拾取失败，{controller.carName}身上的技能槽已满！");
                }
            }
        }
    }
}

