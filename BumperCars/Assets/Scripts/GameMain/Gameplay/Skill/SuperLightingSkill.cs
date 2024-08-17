using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Lachesis.GamePlay
{
    public class SuperLightingSkill : Skill
    {
        public float attackSpeed = 10f;
        private readonly Vector3 deltaPos = new(0, 0.05f, 0);

        private static readonly List<Vector3> effectDeltaPositions = new()
        {
            new Vector3(0, 0.8f, -1f),
            new Vector3(-0.6f, 0.8f, -0.8f),
            new Vector3(0.6f, 0.8f, -0.8f),
            new Vector3(1f, 0.8f, 0f),
            new Vector3(-1f, 0.8f, 0f),
            new Vector3(0.6f, 0.8f, 0.8f),
            new Vector3(-0.6f, 0.8f, 0.8f),
            new Vector3(0f, 0.8f, 1f),
            new Vector3(0, 1.5f, -1f),
            new Vector3(-0.6f, 1.5f, -0.8f),
            new Vector3(0.6f, 1.5f, -0.8f),
            new Vector3(1f, 1.5f, 0f),
            new Vector3(-1f, 1.5f, 0f),
            new Vector3(0.6f, 1.5f, 0.8f),
            new Vector3(-0.6f, 1.5f, 0.8f),
            new Vector3(0f, 1.5f, 1f)
        };

        private static readonly Vector3 targetEffectDelta = new(0, 0.5f, -1f);
        private GlobalConfig m_globalConfig;

        public override void Init(object userData = null)
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override bool TryActivate(CarComponent source, object userData = null)
        {
            if (TryGetSkillTarget(source, out var target))
            {
                source.StartCoroutine(DelayToAttack(source));
                return true;
            }

            return false;
        }

        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            target = source;
            return true;
        }

        private IEnumerator DelayToAttack(CarComponent source)
        {
            var lightingPrepareList = new List<LightingPrepareEffect>();
            for (var i = 0; i < effectDeltaPositions.Count; i++)
            {
                yield return new WaitForSeconds(0.125f);
                var lightingPrepare =
                    GameEntry.EntityManager.CreateEntity<LightingPrepareEffect>(EntityEnum.LightingPrepareEffect, source.transform, effectDeltaPositions[i]);
                lightingPrepareList.Add(lightingPrepare);
                source.AddEffectEntity(lightingPrepare);
            }

            for (var i = 0; i < effectDeltaPositions.Count; i++)
            {
                yield return new WaitForSeconds(0.25f);
                var target = SelectRandomTarget(source, BattleModel.Instance.carControllers);
                source.RemoveEffectEntity(lightingPrepareList[i]);
                GameEntry.EntityManager.ReturnEntity(EntityEnum.LightingPrepareEffect, lightingPrepareList[i]);

                if (target == null || !target.IsValid) continue; //如果此时target已经没了，就不发闪电了

                var lighting = GameEntry.EntityManager.CreateEntity<LightningEffect>(EntityEnum.LightningEffect, source.transform);
                source.AddEffectEntity(lighting);
                lighting.StartObject = source.gameObject;
                lighting.EndObject = null;
                lighting.StartPosition = effectDeltaPositions[i];
                lighting.EndPosition = target.transform.position + targetEffectDelta;
                target.transform.position += deltaPos;
                GameEntry.instance.StartCoroutine(DelayToDestoryLightingEffect(source, lighting));

                void OnHit()
                {
                    // 施加瞬间水平力
                    var forceDirection = target.transform.position - source.transform.position;
                    forceDirection.y = 0;
                    forceDirection = forceDirection.normalized;
                    target.bodyRb.velocity += forceDirection * (attackSpeed * (source.bodyRb.mass / target.bodyRb.mass));
                    // 暂时降低摩擦力
                    var otherWheelColliders = target.GetComponentsInChildren<WheelCollider>();
                    GameEntry.instance.StartCoroutine(TemporarilyReduceFriction(otherWheelColliders, target.entityEnum == EntityEnum.BossCar));
                    GameEntry.EntityManager.CreateEntity<LightingHitEffect>(EntityEnum.LightingHitEffect, target.transform, targetEffectDelta);
                }

                var attackInfo = new AttackInfo();
                attackInfo.attacker = source.carControllerName;
                attackInfo.underAttacker = target.carControllerName;
                attackInfo.attackTime = DateTime.Now;
                attackInfo.attackType = AttackType.Skill;
                attackInfo.userData = skillEnum;
                attackInfo.attackDamge = (int)Mathf.Ceil(source.bodyRb.mass * 3 / target.bodyRb.mass);
                GameEntry.EventManager.Invoke(this, AttackEventArgs.Create(attackInfo, OnHit));
            }

            lightingPrepareList.Clear();
        }

        private IEnumerator DelayToDestoryLightingEffect(CarComponent source, LightningEffect lightning)
        {
            yield return new WaitForSeconds(1f);
            //删掉特效实体
            source.RemoveEffectEntity(lightning);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.LightningEffect, lightning);
        }

        private IEnumerator TemporarilyReduceFriction(WheelCollider[] wheelColliders, bool isBoss)
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness =
                    isBoss ? m_globalConfig.bossUnderAttackCarForwardFrictionStiffness : m_globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness =
                    isBoss ? m_globalConfig.bossUnderAttackSidewaysFrictionStiffness : m_globalConfig.underAttackSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }

            // 等待一段时间
            yield return new WaitForSeconds(m_globalConfig.frictionRestoreDelay);

            // 恢复原来的摩擦力设置
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.defaultCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.defaultCarSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
        }

        private static readonly Random _random = new();

        public static CarComponent SelectRandomTarget(CarComponent source, List<CarController> targets)
        {
            var aiList = targets.OfType<CarPlayer>().Where(b => b.IsHasCar && b.carComponent != source).ToList();
            var playerList = targets.OfType<CarAI>().Where(c => c.IsHasCar && c.carComponent != source).ToList();

            // 没有有效的目标时返回null
            if (aiList.Count == 0 && playerList.Count == 0) return null;

            // 设定选择的概率
            var probability = _random.NextDouble();
            if (probability < 0.4 && aiList.Count > 0) // 80%的概率选择C类
                return aiList[_random.Next(aiList.Count)].carComponent;
            if (playerList.Count > 0) // 20%的概率选择B类
                return playerList[_random.Next(playerList.Count)].carComponent;
            // 当对应类别为空时选择非空的另一类别
            return playerList.Count > 0 ? playerList[_random.Next(playerList.Count)].carComponent : aiList[_random.Next(aiList.Count)].carComponent;
        }
    }
}