using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SkillPickUpItem : Entity
    {
        public SkillEnum skillEnum;

        public override void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnInit(pos, rot, userData);
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
                //发出获取技能卡的事件
                GameEntry.EventManager.Fire(this, GetSkillEventArgs.Create(skillEnum, controller.carName));
                GameEntry.EntityManager.ReturnEntity<SkillPickUpItem>(EntityEnum.SkillPickUpItem, this.gameObject);
            }
        }
    }
}

