using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class SmashSkill : Skill
    {
        public float delay = 2f;
        public float radius = 2f;
        public float attackSpeed = 10f;
        private Vector3 deltaPos = new Vector3(0,0.05f,0);
        private Vector3 startSourcePos;
        private Vector3 startTargetPos;
        private GlobalConfig m_globalConfig;

        public override void Init(object userData = null)
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            skillName = "巨力冲击";
        }
        
        public override void Activate(CarController source, CarController target, object userData = null)
        {
            Debug.Log($"{source.carName} 释放了技能 {skillName}");
            return;
            ShowTargetingEffect();
            startSourcePos = source.transform.position;
            startTargetPos = target.transform.position;
            DelayForAttack(source, target, userData);
        }
        
        private IEnumerator DelayForAttack(CarController source, CarController target, object userData)
        {
            yield return new WaitForSeconds(delay);
            ShowAttackEffect();
            if(isInAttackArea(target, out var projecton))
            {
                ShowUnderAttackEffect();
                target.transform.position+= deltaPos;
                // 施加瞬间水平力
                var forceDirection1 = startTargetPos - startSourcePos;
                var forceDirection2 = target.transform.position - projecton;
                var forceDirection = forceDirection1+forceDirection2;
                forceDirection.y = 0;
                forceDirection = forceDirection.normalized;
                target.bodyRb.velocity = forceDirection * attackSpeed;
                
                // 暂时降低摩擦力
                var otherWheelColliders = target.GetComponentsInChildren<WheelCollider>();
                GameEntry.instance.StartCoroutine(TemporarilyReduceFriction(otherWheelColliders));
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
        
        private void ShowTargetingEffect()
        {
            
        }
        
        private void ShowAttackEffect()
        {
            
        }
        
        private void ShowUnderAttackEffect()
        {
            
            ;
        }
        
        private bool isInAttackArea(CarController target, out Vector3 projecton)
        {
            return radius>DistanceFromPointToLineSegment(target.transform.position, startSourcePos, startTargetPos, out projecton);
        }
        
        float DistanceFromPointToLineSegment(Vector3 p, Vector3 a, Vector3 b, out Vector3 projection)
        {
            Vector3 AB = b - a;
            Vector3 AP = p - a;
            float t = Vector3.Dot(AP, AB) / Vector3.Dot(AB, AB);
            projection = a + t * AB;
            return Vector3.Distance(p, projection);
        }
    }
}

