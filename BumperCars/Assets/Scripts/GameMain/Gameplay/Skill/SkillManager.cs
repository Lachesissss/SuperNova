using System;
using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Lachesis.GamePlay
{
    public enum SkillEnum
    {
        Slip,
        Smash,
        Swap,
        Summon
    }
    
    //TODO:改成Skill的对象池实现
    public class SkillManager : GameModule
    {
        Dictionary<SkillEnum, Type> skillDict = new();
        
        public void Initialize()
        {
            skillDict.Add(SkillEnum.Smash, typeof(SmashSkill));
            foreach (var kv in skillDict)
            {
                if(!kv.Value.IsSubclassOf(typeof(Skill)))
                {
                    Debug.LogError($"配置有误，{kv.Value.Name}没有继承自Skill");
                }
            }
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
            
        }
        
        public Skill CreateSkill(SkillEnum skillEnum)
        {
            if(skillDict.TryGetValue(skillEnum, out var type))
            {
                var skill = (Skill)Activator.CreateInstance(type);
                skill.Init();
                return skill;
            }
            
            Debug.LogError("尝试创建未配置的技能");
            return null;
        }
    }
}


