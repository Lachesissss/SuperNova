using System;
using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CarAttacker : MonoBehaviour
    {
        private bool hasCollided;//碰撞冷却标记
        private Vector3 deltaPos = new Vector3(0,0.05f,0);
        private Rigidbody m_carBody;
        private GlobalConfig m_globalConfig;
        private CarComponent m_SelfComponent;
        private void Awake()
        {
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public void Init(CarComponent ctrl, Rigidbody body)
        {
            m_SelfComponent = ctrl;
            m_carBody = body;
        }

        public void Reset()
        {
            hasCollided = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;
            // 检查碰撞对象是否是另一辆碰碰车
            var otherRigidbody = other.attachedRigidbody;
            if (otherRigidbody != null && otherRigidbody != GetComponent<Rigidbody>())
            {
                var otherWheelColliders = otherRigidbody.GetComponentsInChildren<WheelCollider>();
                otherRigidbody.transform.position += deltaPos;
                hasCollided = true;
                // 施加瞬间水平力
                var forceDirection = (otherRigidbody.transform.position - transform.position).normalized;
                var rate =  Vector3.Dot(forceDirection,m_carBody.velocity.normalized) ;
                forceDirection.y = 0;
                forceDirection = forceDirection.normalized;
                otherRigidbody.velocity += (forceDirection.normalized * m_globalConfig.impactSpeed + rate * m_carBody.velocity)*(m_carBody.mass/otherRigidbody.mass);

                // 暂时降低摩擦力
                StartCoroutine(TemporarilyReduceFriction(otherWheelColliders));

                // 重置碰撞标记
                StartCoroutine(ResetCollisionFlag());
                
                var otherCar = otherRigidbody.GetComponent<CarComponent>();
                if(otherCar!=null)
                {
                    var attackInfo = new AttackInfo();
                    attackInfo.attacker = m_SelfComponent.carControllerName;
                    attackInfo.underAttacker = otherCar.carControllerName;
                    attackInfo.attackTime = DateTime.Now;
                    attackInfo.attackType = AttackType.Collide;
                    GameEntry.EventManager.Fire(this, AttackEventArgs.Create(attackInfo));
                }
            }
        }

        private IEnumerator ResetCollisionFlag()
        {
            yield return new WaitForSeconds(0.5f);
            hasCollided = false;
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
    }
}