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

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            GameEntry.EventManager.Subscribe(GetSkillEventArgs.EventId, OnGetSkill);
            if (userData is CarControllerData data)
            {
                carComponent = data.carComponent;
                controllerName = data.controllerName;
                ClearSkills();
                SetCarInfo();
            }
            else
            {
                Debug.LogError("初始化CarController需要传入CarControllerData！");
            }
        }

        public void SetCar(CarComponent car)
        {
            if (carComponent != null)
            {
                ClearCar();
                Debug.LogWarning("在Controller有控制对象的时候SetCar会将当前控制对象回收，请确认是否符合预期");
            }

            carComponent = car;
            SetCarInfo();
        }

        private void SetCarInfo()
        {
            carComponent.carControllerName = controllerName;
            carComponent.controller = this;
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


        public void ActivateSkill(int index)
        {
            if (skillSlots[index] == null || carComponent == null) //释放技能必须要有一个控制对象
            {
                return;
            }
            else
            {
                if (skillSlots[index].TryActivate(carComponent))
                {
                    if (!m_globalConfig.isUnlimitedFire) //无限火力
                        skillSlots[index] = null;
                    GameEntry.EventManager.Fire(this, PlayerBattleUIUpdateEventArgs.Create());
                    var config = GameEntry.SkillManager.GetSkillConfigItem(skillSlots[index].skillEnum);
                    Debug.Log($"{carComponent.carControllerName} 释放了技能[{skillSlots[index].skillName}],{config.activateText}!");
                }
                else
                {
                    Debug.Log($"{carComponent.carControllerName} 释放 {skillSlots[index]}失败，需要目标");
                }
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
                carComponent.controller = null;
                GameEntry.EntityManager.ReturnEntity(EntityEnum.Car, carComponent);
                carComponent = null;
            }
        }
        
        public static void SwitchCar(CarController carCtrl1, CarController carCtrl2)
        {
            var carComponent1 = carCtrl1.carComponent;
            var carComponent2 = carCtrl2.carComponent;
            carCtrl1.carComponent = carComponent2;
            carCtrl2.carComponent = carComponent1;
            carCtrl1.OnSwitchCar();
            carCtrl2.OnSwitchCar();
        }

        public virtual void OnSwitchCar()
        {
            SetCarInfo();
        }
    }
}

