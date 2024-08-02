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

        public override void Activate(CarComponent source, CarComponent target, object userData = null)
        {
            var strongerEffect = GameEntry.EntityManager.CreateEntity<StrongerEffect>(EntityEnum.StrongerEffect, source.transform);
            source.AddEffectEntity(strongerEffect);
            source.bodyRb.mass = m_globalConfig.strongerCarMass;
            GameEntry.instance.StartCoroutine(DelayToRecoverBodyMass(source, strongerEffect));
        }

        private IEnumerator DelayToRecoverBodyMass(CarComponent source, StrongerEffect strongerEffect)
        {
            yield return new WaitForSeconds(15f);
            source.bodyRb.mass = m_globalConfig.defaultCarMass;
            source.RemoveEffectEntity(strongerEffect);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.StrongerEffect, strongerEffect);
        }
    }
}

