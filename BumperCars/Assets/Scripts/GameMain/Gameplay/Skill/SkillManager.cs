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
        Dictionary<SkillEnum, SkillConfigItem> m_skillConfigItemDict = new();
        //List<Skill> curSkills = new();
        public void Initialize(SkillConfig skillConfig)
        {
            foreach (var config in skillConfig.skillResources)
            {
               m_skillConfigItemDict.Add(config.skillEnum, config); 
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
            if(m_skillConfigItemDict.TryGetValue(skillEnum, out var cfg))
            {
                var skill = (Skill)Activator.CreateInstance(cfg.skillType.GetSkillType());
                skill.Init();
                skill.skillEnum = skillEnum;
                skill.skillName = m_skillConfigItemDict[skillEnum].skillName;
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
           if(m_skillConfigItemDict.TryGetValue(skillEnum, out var configItem))
           {
               return configItem.iconName;
           } 
           Debug.LogError($"查找技能[{skillEnum}]的图标名称失败");
           return string.Empty;
        }
        
        public SkillConfigItem GetSkillConfigItem(SkillEnum skillEnum)
        {
            if(m_skillConfigItemDict.TryGetValue(skillEnum, out var configItem))
            {
                return configItem;
            } 
            Debug.LogError($"查找技能[{skillEnum}]的技能配置");
            return null;
        }
    }
}


