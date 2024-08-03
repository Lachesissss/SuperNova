using System;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public enum SkillEnum
    {
        Lighting,
        Stronger,
        FlipVertical,
        //Slip,
        //Swap,
        //Summon,
    }
    
    //TODO:改成Skill的对象池实现
    public class SkillManager : GameModule
    {
        Dictionary<SkillEnum, Type> skillTypeDict = new();
        Dictionary<SkillEnum, SkillConfigItem> skillIconNameDict = new();
        //List<Skill> curSkills = new();
        public void Initialize(SkillConfig skillConfig)
        {
            //这个type只能在代码里先这么配了
            skillTypeDict.Add(SkillEnum.Lighting, typeof(LightingSkill));
            skillTypeDict.Add(SkillEnum.Stronger, typeof(StrongerSkill));
            skillTypeDict.Add(SkillEnum.FlipVertical, typeof(FlipVerticalSkill));
            foreach (var kv in skillTypeDict)
            {
                if(!kv.Value.IsSubclassOf(typeof(Skill)))
                {
                    Debug.LogError($"配置有误，{kv.Value.Name}没有继承自Skill");
                }
            }

            foreach (var config in skillConfig.skillResources)
            {
               skillIconNameDict.Add(config.skillEnum, config); 
            }
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // foreach (var skill in curSkills)
            // {
            //     skill.Update(elapseSeconds, realElapseSeconds);
            // }
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
            
        }
        
        public Skill CreateSkill(SkillEnum skillEnum)
        {
            if(skillTypeDict.TryGetValue(skillEnum, out var type))
            {
                var skill = (Skill)Activator.CreateInstance(type);
                skill.Init();
                skill.skillEnum = skillEnum;
                skill.skillName = skillIconNameDict[skillEnum].skillName;
                //curSkills.Add(skill);
                return skill;
            }
            
            Debug.LogError("尝试创建未配置的技能");
            return null;
        }
        
        // public void DestorySkill(Skill skill)
        // {
        //     curSkills.Remove(skill);
        // }
        
        public string GetSkillIconName(SkillEnum skillEnum)
        {
           if(skillIconNameDict.TryGetValue(skillEnum, out var configItem))
           {
               return configItem.iconName;
           } 
           Debug.LogError($"查找技能[{skillEnum}]的图标名称失败");
           return string.Empty;
        }
        
        public SkillConfigItem GetSkillConfigItem(SkillEnum skillEnum)
        {
            if(skillIconNameDict.TryGetValue(skillEnum, out var configItem))
            {
                return configItem;
            } 
            Debug.LogError($"查找技能[{skillEnum}]的技能配置");
            return null;
        }
    }
}


