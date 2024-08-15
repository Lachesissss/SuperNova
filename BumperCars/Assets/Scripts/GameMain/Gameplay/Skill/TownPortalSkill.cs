using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class TownPortal : Skill
    {
        private GlobalConfig m_globalConfig;
        private static Vector3 effectDeltaPos = new Vector3(0,0.5f,0);
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
            var portalEffect = GameEntry.EntityManager.CreateEntity<PortalEffect>(EntityEnum.PortalEffect, source.transform, effectDeltaPos);
            source.AddEffectEntity(portalEffect);
            GameEntry.instance.StartCoroutine(DelayToTownPortal(source, portalEffect));
            return true;
        }

        private IEnumerator DelayToTownPortal(CarComponent source, PortalEffect portalEffect)
        {
            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create($"[{source.carControllerName}]将在5秒后传送回城"));
            yield return new WaitForSeconds(5f);
            if(source.controller==null) yield break;
            source.transform.position = BattleModel.Instance.spawnPosDict[source.controller].position;
            BattleModel.Instance.killOtherPlayerNumDict[source.carControllerName] = 0;
            GameEntry.EventManager.Invoke(this, UltimateSkillUIUpdateArgs.Create());
            source.RemoveEffectEntity(portalEffect);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.PortalEffect, portalEffect);
        }
    }
}