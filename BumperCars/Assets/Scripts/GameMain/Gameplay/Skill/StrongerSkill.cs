using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class StrongerSkill : Skill
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
            var strongerEffect = GameEntry.EntityManager.CreateEntity<StrongerEffect>(EntityEnum.StrongerEffect, source.transform);
            source.AddEffectEntity(strongerEffect);
            source.bodyRb.mass*=1.7f;
            source.StartCoroutine(DelayToRecoverBodyMass(source, strongerEffect));
            
            return true;
        }

        private IEnumerator DelayToRecoverBodyMass(CarComponent source, StrongerEffect strongerEffect)
        {
            var audio = GameEntry.SoundManager.PlayerSound(source, SoundEnum.Stronger, false, 1, false);
            yield return new WaitForSeconds(15f);
            source.bodyRb.mass = m_globalConfig.defaultCarMass;
            source.RemoveEffectEntity(strongerEffect);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.StrongerEffect, strongerEffect);
            audio.Stop();
        }
    }
}

