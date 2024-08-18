using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class MarvelSkill : Skill
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
            var marvelEffect = GameEntry.EntityManager.CreateEntity<MarvelEffect>(EntityEnum.MarvelEffect, source.transform, effectDeltaPos);
            source.AddEffectEntity(marvelEffect);
            source.StartCoroutine(MarvelTime(source, marvelEffect));
            return true;
        }

        private IEnumerator MarvelTime(CarComponent source, MarvelEffect marvelEffect)
        {
            GameEntry.EventManager.Invoke(this, ShowUITipsEventArgs.Create($"[{source.carControllerName}]开始了奇迹时刻!"));
            var marvelAudio = GameEntry.SoundManager.PlayerSound(source, SoundEnum.Marvel, false, 1.4f, false);
            for(int i=0;i<20;i++)
            {
                if(source.controller==null) break;
                foreach (var slot in source.controller.skillSlots)
                {
                    if(slot == null)
                    {
                        GameEntry.EventManager.Invoke(this, GetSkillEventArgs.Create(GameEntry.SkillManager.GetRandomSkillEnum(true), source.carControllerName));
                        break;
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
            
            source.RemoveEffectEntity(marvelEffect);
            marvelAudio.Stop();
            GameEntry.EntityManager.ReturnEntity(EntityEnum.MarvelEffect, marvelEffect);
        }
    }
}