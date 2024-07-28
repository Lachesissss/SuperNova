using System.Collections;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CarAttacker : MonoBehaviour
    {
        public float impactForce = 50000f; // 冲击力大小
        public float frictionRestoreDelay = 2f; // 恢复摩擦力的延迟时间
        private bool hasCollided;

        private void OnTriggerEnter(Collider other)
        {
            if (hasCollided) return;
            // 检查碰撞对象是否是另一辆碰碰车
            var otherRigidbody = other.attachedRigidbody;
            if (otherRigidbody != null && otherRigidbody != GetComponent<Rigidbody>())
            {
                var otherWheelColliders = otherRigidbody.GetComponentsInChildren<WheelCollider>();

                // 标记已经发生碰撞
                hasCollided = true;
                // 施加瞬间力
                var forceDirection = (otherRigidbody.transform.position - transform.position).normalized;
                // 确保力是水平的
                forceDirection.y = 0;
                forceDirection = forceDirection.normalized;
                otherRigidbody.AddForce(forceDirection * impactForce, ForceMode.Impulse);

                // 暂时降低摩擦力
                StartCoroutine(TemporarilyReduceFriction(otherWheelColliders));

                // 重置碰撞标记
                StartCoroutine(ResetCollisionFlag());
            }
        }

        private IEnumerator ResetCollisionFlag()
        {
            // 等待一段时间再重置碰撞标记
            yield return new WaitForSeconds(0.5f);
            hasCollided = false;
        }


        private IEnumerator TemporarilyReduceFriction(WheelCollider[] wheelColliders)
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = GameEntry.instance.globalConfig.underAttackCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = GameEntry.instance.globalConfig.underAttackSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;

                // var softSuspension = new JointSpring
                // {
                //     spring = 0,
                //     damper = 0,
                //     targetPosition = wheelColliders[i].suspensionSpring.targetPosition
                // };
                // wheelColliders[i].suspensionSpring = softSuspension;
            }

            // 等待一段时间
            yield return new WaitForSeconds(frictionRestoreDelay);

            // 恢复原来的摩擦力设置
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                var forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = GameEntry.instance.globalConfig.defaultCarForwardFrictionStiffness;
                wheelColliders[i].forwardFriction = forwardFriction;

                var sidewaysFriction = wheelColliders[i].forwardFriction;
                sidewaysFriction.stiffness = GameEntry.instance.globalConfig.defaultCarSidewaysFrictionStiffness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
            }
        }
    }
}