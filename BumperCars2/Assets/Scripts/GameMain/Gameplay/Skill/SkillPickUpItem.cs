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
                var controller = other.GetComponent<CarBody>().carComponent;
                if(!controller.controller.IsSkillSlotFull())
                {
                    //发出获取技能卡的事件
                    GameEntry.EventManager.Invoke(this, GetSkillEventArgs.Create(skillEnum, controller.carControllerName));
                    GameEntry.EntityManager.ReturnEntity(EntityEnum.SkillPickUpItem, this);
                }
                else
                {
                    Debug.Log($"拾取失败，{controller.carControllerName}身上的技能槽已满！");
                }
            }
        }
    }
}

