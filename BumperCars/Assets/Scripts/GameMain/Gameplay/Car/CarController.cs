using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    
    public class CarController : Entity
    {
        public class CarControllerData
        {
            public CarComponent carComponent;
            public string controllerName;
        }
        
        public CarComponent carComponent;
        public string controllerName;
        public List<Skill> skillSlots;
        protected GlobalConfig m_globalConfig;
        public bool IsHasCar=>carComponent!=null;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            skillSlots = new();
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            GameEntry.EventManager.Subscribe(GetSkillEventArgs.EventId, OnGetSkill);
            if(userData is CarControllerData data)
            {
                carComponent = data.carComponent;
                ClearSkills();
                controllerName = data.controllerName;
                carComponent.carControllerName = controllerName;
                carComponent.controller = this;
            }
            else
            {
                Debug.LogError("初始化CarController需要传入CarControllerData！");
            }
            
        }

        public void ClearSkills()
        {
            skillSlots.Clear();
            for (var i = 0; i < m_globalConfig.maxSkillCount; i++)
                skillSlots.Add(null);
            GameEntry.EventManager.Fire(this, PlayerBattleUIUpdateEventArgs.Create());
        }
        
        public override void OnReturnToPool(bool isShutDown = false)
        {
            base.OnReturnToPool(isShutDown);
            GameEntry.EventManager.Unsubscribe(GetSkillEventArgs.EventId, OnGetSkill);
        }

        private void OnGetSkill(object sender, GameEventArgs e)
        {
            if (e is GetSkillEventArgs args)
            {
                if(args.userName==controllerName)
                {
                    for(var i=0;i<skillSlots.Count;i++)
                    {
                        if(skillSlots[i]==null)
                        {
                            skillSlots[i] = GameEntry.SkillManager.CreateSkill(args.skillEnum);
                            GameEntry.EventManager.Fire(this, PlayerBattleUIUpdateEventArgs.Create());
                            Debug.Log($"{controllerName} 获得了技能卡 {skillSlots[i].skillName},并装配在了{i+1}号技能槽");
                            break;
                        }
                    }
                }
            }
        }
        
        //释放技能，可以没有目标,因为只能是实体车对实体车释放，所有参数类型是CarComponent
        public void ActivateSkill(int index, CarComponent target = null)
        {
            if(skillSlots[index]==null)
            {
                return;
            }
            else
            {
                if(skillSlots[index].isNeedTarget&&(target==null))
                {
                    Debug.Log($"{controllerName} 释放 {skillSlots[index].skillName}失败，需要目标");
                    return;
                }
                skillSlots[index].Activate(carComponent, target);
                var config = GameEntry.SkillManager.GetSkillConfigItem(skillSlots[index].skillEnum);
                Debug.Log($"{controllerName} 释放了技能[{skillSlots[index].skillName}],{config.activateText}!");
                if(!m_globalConfig.isUnlimitedFire) //无限火力
                {
                    skillSlots[index] = null;
                }
                GameEntry.EventManager.Fire(this, PlayerBattleUIUpdateEventArgs.Create());
            }
        }
        
        public bool IsSkillSlotFull()
        {
            foreach (var skill in skillSlots)
            {
                if(skill==null)
                    return false;
            }
            return true;
        }
        
        public void ClearCar() 
        {
            if(carComponent!=null)
            {
                GameEntry.EntityManager.ReturnEntity(EntityEnum.Car, carComponent);
                carComponent = null;
            }
        }
        
        public static void SwitchCar(CarController carCtrl1, CarController carCtrl2)
        {
            var carComponent1 = carCtrl1.carComponent;
            var carComponent2 = carCtrl2.carComponent;
            //交换技能
            int skillCount = GameEntry.ConfigManager.GetConfig<GlobalConfig>().maxSkillCount;

            //交换名称(ID)
            (carComponent1.carControllerName, carComponent2.carControllerName) = (carComponent2.carControllerName, carComponent1.carControllerName);
            carCtrl1.carComponent = carComponent2;
            carCtrl2.carComponent = carComponent1;
        }
    }
}

