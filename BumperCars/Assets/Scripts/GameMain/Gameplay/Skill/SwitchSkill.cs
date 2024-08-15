using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SwitchSkill : Skill
    {
        private GlobalConfig m_globalConfig;
        public override void Init(object userData = null)
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            target = source;
            return true;
        }

        public override bool TryActivate(CarComponent source, object userData = null)
        {
            if (source == null) return false;
            GameEntry.EventManager.Invoke(this, SwitchCarEventArgs.Create());
            return true;
        }
        
    }
}