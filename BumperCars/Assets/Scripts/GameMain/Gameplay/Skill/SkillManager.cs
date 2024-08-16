using System;
using System.Collections.Generic;
using System.Linq;
using Lachesis.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public enum SkillEnum
    {
        Lighting,
        Stronger,
        FlipVertical,
        Compress,
        Shield,
        Implosion,
        TownPortal,
        Switch,
        Marvel,
        //Slip,
        //Swap,
        //Summon,
    }
    
    //TODO:改成Skill的对象池实现
    public class SkillManager : GameModule
    {
        Dictionary<SkillEnum, SkillConfigItem> m_skillConfigItemDict = new();
        private float totalWeight;
        private float totalWeightExludeUltimate;
        //List<Skill> curSkills = new();
        public void Initialize(SkillConfig skillConfig)
        {
            foreach (var config in skillConfig.skillResources)
            {
               m_skillConfigItemDict.Add(config.skillEnum, config); 
               if(config.spawnProb<0) config.spawnProb = 0;
            }
            totalWeight = m_skillConfigItemDict.Values.Sum(config => config.spawnProb);
            totalWeightExludeUltimate = m_skillConfigItemDict.Values.Sum(config => config.isUltimate?0: config.spawnProb);
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
        
        public SkillEnum GetRandomSkillEnum(bool excludeUltimate = true)
        {
            if(excludeUltimate)
            {
                float randomValue = Random.Range(0f, totalWeightExludeUltimate);
                SkillEnum randomSkill = SkillEnum.Lighting;
                float cumulativeWeight = 0f;

                foreach (var kvp in m_skillConfigItemDict)
                {
                    if(kvp.Value.isUltimate) continue;
                    if (kvp.Value.spawnProb > 0)
                    {
                        cumulativeWeight += kvp.Value.spawnProb;
                        if (randomValue <= cumulativeWeight)
                        {
                            randomSkill = kvp.Key;
                            break;
                        }
                    }
                }
                return randomSkill;
            }
            else
            {
                float randomValue = Random.Range(0f, totalWeight);
                SkillEnum randomSkill = SkillEnum.Lighting;
                float cumulativeWeight = 0f;

                foreach (var kvp in m_skillConfigItemDict)
                {
                    if (kvp.Value.spawnProb > 0)
                    {
                        cumulativeWeight += kvp.Value.spawnProb;
                        if (randomValue <= cumulativeWeight)
                        {
                            randomSkill = kvp.Key;
                            break;
                        }
                    }
                }
                return randomSkill;
            }
        }
        
        public SkillEnum GetRandomSkillEnumPve()
        {
            float randomValue = Random.Range(0f, totalWeight);
            SkillEnum randomSkill = SkillEnum.Lighting;
            float cumulativeWeight = 0f;

            foreach (var kvp in m_skillConfigItemDict)
            {
                if (kvp.Value.spawnProb > 0&&kvp.Value.openInPve)
                {
                    cumulativeWeight += kvp.Value.spawnProb;
                    if (randomValue <= cumulativeWeight)
                    {
                        randomSkill = kvp.Key;
                        break;
                    }
                }
            }
            return randomSkill;
        }
        
        public List<SkillEnum> GetAllUltimateSkill()
        {
           var list = new List<SkillEnum>();
           foreach (var kv in m_skillConfigItemDict)
           {
               if(kv.Value.isUltimate)
               {
                   list.Add(kv.Key);
               }
           }
           return list;
        }
    }
}


