using System;
using System.Collections.Generic;
using System.Linq;
using Lachesis.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public enum NetworkSkillEnum
    {
        Lighting,
        Stronger,
        FlipVertical,

        Compress
        //Slip,
        //Swap,
        //Summon,
    }

    //TODO:改成Skill的对象池实现
    public class NetworkSkillManager : GameModule
    {
        private readonly Dictionary<NetworkSkillEnum, NetworkSkillConfigItem> m_skillConfigItemDict = new();

        private float totalWeight;

        //List<Skill> curSkills = new();
        public void Initialize(NetworkSkillConfig skillConfig)
        {
            foreach (var config in skillConfig.skillResources)
            {
                m_skillConfigItemDict.Add(config.skillEnum, config);
                if (config.spawnProb < 0) config.spawnProb = 0;
            }

            totalWeight = m_skillConfigItemDict.Values.Sum(config => config.spawnProb);
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

        public NetworkSkill CreateSkill(NetworkSkillEnum skillEnum)
        {
            if (m_skillConfigItemDict.TryGetValue(skillEnum, out var cfg))
            {
                var skill = (NetworkSkill)Activator.CreateInstance(cfg.skillType.GetSkillType());
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

        public string GetSkillIconName(NetworkSkillEnum skillEnum)
        {
            if (m_skillConfigItemDict.TryGetValue(skillEnum, out var configItem)) return configItem.iconName;
            Debug.LogError($"查找技能[{skillEnum}]的图标名称失败");
            return string.Empty;
        }

        public NetworkSkillConfigItem GetSkillConfigItem(NetworkSkillEnum skillEnum)
        {
            if (m_skillConfigItemDict.TryGetValue(skillEnum, out var configItem)) return configItem;
            Debug.LogError($"查找技能[{skillEnum}]的技能配置");
            return null;
        }

        public NetworkSkillEnum GetRandomSkillEnum()
        {
            var randomValue = Random.Range(0f, totalWeight);
            var randomSkill = NetworkSkillEnum.Lighting;
            var cumulativeWeight = 0f;

            foreach (var kvp in m_skillConfigItemDict)
                if (kvp.Value.spawnProb > 0)
                {
                    cumulativeWeight += kvp.Value.spawnProb;
                    if (randomValue <= cumulativeWeight)
                    {
                        randomSkill = kvp.Key;
                        break;
                    }
                }

            return randomSkill;
        }

        public NetworkSkillEnum GetRandomSkillEnumPve()
        {
            var randomValue = Random.Range(0f, totalWeight);
            var randomSkill = NetworkSkillEnum.Lighting;
            var cumulativeWeight = 0f;

            foreach (var kvp in m_skillConfigItemDict)
                if (kvp.Value.spawnProb > 0 && kvp.Value.openInPve)
                {
                    cumulativeWeight += kvp.Value.spawnProb;
                    if (randomValue <= cumulativeWeight)
                    {
                        randomSkill = kvp.Key;
                        break;
                    }
                }

            return randomSkill;
        }
    }
}