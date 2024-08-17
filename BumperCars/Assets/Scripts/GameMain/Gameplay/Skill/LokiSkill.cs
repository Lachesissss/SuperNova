using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class LokiSkill : Skill
    {
        private static readonly Vector3 effectDeltaPos = new(0, 1.2f, 0);
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
            var minDis = float.PositiveInfinity;
            target = null;
            foreach (var controller in BattleModel.Instance.carControllers)
                if (controller is CarPlayer && controller.IsHasCar && controller != source.controller)
                {
                    var dis = Vector3.Distance(controller.carComponent.transform.position, source.transform.position);
                    if (minDis > dis)
                    {
                        minDis = dis;
                        target = controller.carComponent;
                    }
                }

            return target != null;
        }

        public override bool TryActivate(CarComponent source, object userData = null)
        {
            if (TryGetSkillTarget(source, out var target))
            {
                source.StartCoroutine(DelayToSteal(source, target));
                return true;
            }

            return false;
        }

        private IEnumerator DelayToSteal(CarComponent source, CarComponent target)
        {
            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create($"[{source.carControllerName}]将在5秒后施展诡计"));
            var lokiEffect = GameEntry.EntityManager.CreateEntity<LokiEffect>(EntityEnum.LokiEffect, source.transform, effectDeltaPos);
            yield return new WaitForSeconds(5f);
            if (source.controller == null || target.controller == null) yield break;
            var stealList = new List<Skill>();
            for (var i = 0; i < target.controller.skillSlots.Count; i++)
            {
                if (target.controller.skillSlots[i] != null) stealList.Add(target.controller.skillSlots[i]);
                target.controller.skillSlots[i] = null;
            }

            for (var i = 0; i < source.controller.skillSlots.Count; i++)
                if (source.controller.skillSlots[i] == null && stealList.Count > 0)
                {
                    source.controller.skillSlots[i] = stealList[0];
                    stealList.RemoveAt(0);
                }

            GameEntry.EventManager.Invoke(this, CoinStealEventArgs.Create(source.carControllerName, target.carControllerName));
            GameEntry.EventManager.Invoke(this, PlayerSkillSlotsUIUpdateEventArgs.Create());
            source.RemoveEffectEntity(lokiEffect);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.LokiEffect, lokiEffect);
        }
    }
}