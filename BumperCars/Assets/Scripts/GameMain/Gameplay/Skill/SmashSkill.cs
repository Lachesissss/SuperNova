using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SmashSkill : Skill
    {
        public float attackSpeed = 10f;
        private Vector3 deltaPos = new Vector3(0,0.05f,0);
        private Vector3 effectDeltaPos = new Vector3(0,0.5f,0);
        private GlobalConfig m_globalConfig;

        public override void Init(object userData = null)
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public override void Activate(CarController source, CarController target, object userData = null)
        {
            var lighting = GameEntry.EntityManager.CreateEntity<LightningBoltScript>(EntityEnum.LightningBolt, GameEntry.instance.transform);
            lighting.StartObject = source.gameObject;
            lighting.EndObject = null;
            lighting.StartPosition = effectDeltaPos;
            lighting.EndPosition =  target.transform.position+effectDeltaPos;
            target.transform.position+= deltaPos;
            // 施加瞬间水平力
            var forceDirection = target.transform.position - source.transform.position;
            forceDirection.y = 0;
            forceDirection = forceDirection.normalized;
            target.bodyRb.velocity += forceDirection * (attackSpeed * (source.bodyRb.mass/target.bodyRb.mass));
                
            // 暂时降低摩擦力
            var otherWheelColliders = target.GetComponentsInChildren<WheelCollider>();
            GameEntry.instance.StartCoroutine(TemporarilyReduceFriction(otherWheelColliders));
            
            if(target!=null)
            {
                var attackInfo = new AttackInfo();
                attackInfo.attacker = source.carName;
                attackInfo.underAttacker = target.carName;
                attackInfo.attackTime = DateTime.Now;
                attackInfo.attackType = AttackType.Skill;
                attackInfo.userData = skillEnum;
                GameEntry.EventManager.Fire(this, AttackEventArgs.Create(attackInfo));
            }
        }
        
        private IEnumerator TemporarilyReduceFriction(WheelCollider[] wheelColliders)
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = m_globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.underAttackSidewaysFrictionStiffness;
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
        
        
        private void ShowUnderAttackEffect()
        {
            
            ;
        }
    }
}

