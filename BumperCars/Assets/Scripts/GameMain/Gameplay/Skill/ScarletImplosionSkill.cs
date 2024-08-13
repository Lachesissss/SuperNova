using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class ScarletImplosionSkill : Skill
    {
        private GlobalConfig m_globalConfig;
        public static float attackSpeed = 10f;
        public static Vector3 attackAngularSpeed  = new Vector3(0, Mathf.PI, 0);
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
                GameEntry.instance.StartCoroutine(DelayToAttack(source, target)); 
                return true;
            }

            return false;
        }
        
        private IEnumerator DelayToAttack(CarComponent source, CarComponent target)
        {
            var startPos = target.transform.position+target.bodyRb.velocity*0.8f+new Vector3(0.05f,0.3f,0.05f);//提前预测0.8秒后的位置
            var effect = GameEntry.EntityManager.CreateEntity<ScarletImplosionEffect>(EntityEnum.ScarletImplosionEffect, startPos, target.transform.rotation);
            yield return new WaitForSeconds(1.1f);
            if(Vector3.Distance(target.transform.position, startPos)<=3.5*1.5f)
            {
                void OnHit()
                {
                    // 施加瞬间水平力
                    var forceDirection = target.transform.position - startPos;
                    forceDirection.y = 0;
                    forceDirection = forceDirection.normalized;
                    forceDirection.y = 0.2f;
                    target.bodyRb.velocity += forceDirection * (attackSpeed * (source.bodyRb.mass / target.bodyRb.mass));
                    target.bodyRb.angularVelocity = attackAngularSpeed;
                    // 暂时降低摩擦力
                    var otherWheelColliders = target.GetComponentsInChildren<WheelCollider>();
                    GameEntry.instance.StartCoroutine(TemporarilyReduceFriction(otherWheelColliders, target.entityEnum == EntityEnum.BossCar));
                
                }
            
                var attackInfo = new AttackInfo();
                attackInfo.attacker = source.carControllerName;
                attackInfo.underAttacker = target.carControllerName;
                attackInfo.attackTime = DateTime.Now;
                attackInfo.attackType = AttackType.Skill;
                attackInfo.userData = skillEnum;
                attackInfo.attackDamge = (int) Mathf.Ceil(source.bodyRb.mass*3/ target.bodyRb.mass);
                GameEntry.EventManager.Invoke(this, AttackEventArgs.Create(attackInfo,OnHit));
            } 
            yield return new WaitForSeconds(0.9f);
            GameEntry.EntityManager.ReturnEntity(EntityEnum.ScarletImplosionEffect, effect);
        }

        public override bool TryGetSkillTarget(CarComponent source, out CarComponent target)
        {
            var minDis = float.PositiveInfinity;
            target = null;
            foreach (var controller in BattleModel.Instance.carControllers)
            {
                if(GameEntry.ProcedureManager.CurrentProcedure is ProcedureBattlePve&& controller is CarPlayer) continue;//这里先这样处理，Pve下不索队友
                if (controller.IsHasCar && controller != source.controller)
                {
                    var dis = Vector3.Distance(controller.carComponent.transform.position, source.transform.position);
                    if (minDis > dis)
                    {
                        minDis = dis;
                        target = controller.carComponent;
                    }
                }
            }
                

            return target != null;
        }
        
        private IEnumerator TemporarilyReduceFriction(WheelCollider[] wheelColliders, bool isBoss)
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness =isBoss?m_globalConfig.bossUnderAttackCarForwardFrictionStiffness:m_globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness =isBoss?m_globalConfig.bossUnderAttackSidewaysFrictionStiffness: m_globalConfig.underAttackSidewaysFrictionStiffness;
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
    }
}

