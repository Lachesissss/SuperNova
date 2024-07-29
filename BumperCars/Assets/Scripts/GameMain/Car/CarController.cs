using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class CarController : MonoBehaviour
    {
        public enum DriverMode
        {
            Front,
            Back,
            All
        }

        public DriverMode driverMode = DriverMode.Back;
        public float idealRPM = 20;
        public float maxRPM = 100;
        public float moveTorque = 30000;
        public float handBreakTorque = 60000;
        public float autoBreakTorque = 30000;
        public float antiRoll = 20000f;
        public Transform centerOfGravity;

        public float maxAngle;
        public float targetAngle;
        public float returnAngleSpeed = 400;

        public Rigidbody bodyRb;
        [Header("车轮碰撞体")] public WheelCollider leftFrontCollider;
        public WheelCollider leftBackCollider;
        public WheelCollider rightFrontCollider;
        public WheelCollider rightBackCollider;

        [Header("车轮模型")] public Transform leftFrontTrans;
        public Transform leftBackTrans;
        public Transform rightFrontTrans;
        public Transform rightBackTrans;
        
        
        
        private readonly List<WheelCollider> wheelColliders = new();

        private float m_carTurnValue;

        private float m_carForwardValue;

        private bool m_isBoost;

        private bool m_isHandBrake;

        private GlobalConfig m_globalConfig;
        public float GetSpeed()
        {
            return rightBackCollider.radius * Mathf.PI * rightBackCollider.rpm * 60f / 1000f;
        }

        public float GetRPM()
        {
            return rightBackCollider.rpm;
        }

        private void Awake()
        {
            if (centerOfGravity != null) bodyRb.centerOfMass = centerOfGravity.position;
            m_globalConfig = GameEntry.ConfigManager.GetConfig<GlobalConfig>();
            wheelColliders.Add(leftBackCollider);
            wheelColliders.Add(leftFrontCollider);
            wheelColliders.Add(rightBackCollider);
            wheelColliders.Add(rightFrontCollider);
            foreach (var collider in wheelColliders)
            {
                var forwardFriction = collider.forwardFriction;
                forwardFriction.stiffness = m_globalConfig.defaultCarForwardFrictionStiffness;
                collider.forwardFriction = forwardFriction;

                var sidewaysFriction = collider.forwardFriction;
                sidewaysFriction.stiffness = m_globalConfig.defaultCarSidewaysFrictionStiffness;
                collider.sidewaysFriction = sidewaysFriction;
            }

            Reset();
        }

        public void Reset()
        {
            m_carForwardValue = 0;
            m_carTurnValue = 0;
            m_isBoost = false;
            m_isHandBrake = false;
            bodyRb.velocity = Vector3.zero;
            bodyRb.angularVelocity = Vector3.zero;
            // 确保所有的 WheelColliders 初始状态下不施加动力或转矩
            foreach (var wheel in wheelColliders)
            {
                wheel.motorTorque = 0;
                wheel.brakeTorque = Mathf.Infinity; // 应用无穷大的刹车力，防止车移动
                wheel.steerAngle = 0;
            }

            // 等待一帧以确保所有物理计算完成
            StartCoroutine(ResetBrakeTorque());
        }

        // 在一帧之后重置刹车力
        private IEnumerator ResetBrakeTorque()
        {
            yield return null; // 等待一帧
            foreach (var wheel in wheelColliders) wheel.brakeTorque = 0;
        }

        private void FixedUpdate()
        {
            WheelsUpdate();
        }

        public void ChangeCarTurnState(float turnValue)
        {
            m_carTurnValue = turnValue;
        }

        public void ChangeCarForwardState(float forwardValue)
        {
            m_carForwardValue = forwardValue;
        }

        public void ChangeCarBoostState(bool isBoost)
        {
            m_isBoost = isBoost;
        }

        public void ChangeCarHandBrakeState(bool isHandBrake)
        {
            m_isHandBrake = isHandBrake;
        }

        //控制移动 转向
        private void WheelsUpdate()
        {
            var forwardValue = m_carForwardValue > 0.05 || m_carForwardValue < -0.05 ? m_carForwardValue : 0;
            var scaledTorque = forwardValue * moveTorque;
            var curRPM = leftBackCollider.rpm;
            if (curRPM is float.NaN)
                curRPM = 0;
            if (curRPM < idealRPM)
                scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, curRPM / idealRPM);
            else
                scaledTorque = Mathf.Lerp(scaledTorque, 0, (curRPM - idealRPM) / (maxRPM - idealRPM));

            DoRollBar(leftFrontCollider, rightFrontCollider);
            DoRollBar(leftBackCollider, rightBackCollider);

            if (m_carTurnValue < -0.05f) //左转向
                targetAngle = maxAngle*m_carTurnValue;

            else if (m_carTurnValue > 0.05f) //右转向
                targetAngle = maxAngle*m_carTurnValue;

            else //松开转向后，方向打回
                targetAngle = 0;
            leftFrontCollider.steerAngle = Mathf.MoveTowards(leftFrontCollider.steerAngle, targetAngle, returnAngleSpeed * Time.fixedDeltaTime);
            rightFrontCollider.steerAngle = Mathf.MoveTowards(leftFrontCollider.steerAngle, targetAngle, returnAngleSpeed * Time.fixedDeltaTime);

            leftFrontCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Front ? scaledTorque : 0;
            rightFrontCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Front ? scaledTorque : 0;
            leftBackCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Back ? scaledTorque : 0;
            rightBackCollider.motorTorque = driverMode == DriverMode.All || driverMode == DriverMode.Back ? scaledTorque : 0;

            foreach (var wheelCollider in wheelColliders)
            {
                var targetBreakTorque = forwardValue == 0 ? autoBreakTorque : 0;
                if (m_isHandBrake) targetBreakTorque += handBreakTorque;
                wheelCollider.brakeTorque = targetBreakTorque;
            }

            //当车轮碰撞器位置角度改变，随之也变更车轮模型的位置角度
            WheelsModel_Update(leftFrontTrans, leftFrontCollider);
            WheelsModel_Update(leftBackTrans, leftBackCollider);
            WheelsModel_Update(rightFrontTrans, rightFrontCollider);
            WheelsModel_Update(rightBackTrans, rightBackCollider);
        }

        //控制车轮模型移动 转向
        private void WheelsModel_Update(Transform t, WheelCollider wheel)
        {
            var pos = t.position;
            var rot = t.rotation;

            wheel.GetWorldPose(out pos, out rot);

            t.position = pos;
            t.rotation = rot;
        }

        //防侧翻
        private void DoRollBar(WheelCollider wheelL, WheelCollider wheelR)
        {
            WheelHit hit;
            var travelL = 1f;
            var travelR = 1f;
            var groundedL = wheelL.GetGroundHit(out hit);
            if (groundedL) travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
            var groundedR = wheelR.GetGroundHit(out hit);
            if (groundedR) travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;

            var antiRollForce = (travelL - travelR) * antiRoll;

            if (groundedL)
                bodyRb.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
            if (groundedR)
                bodyRb.AddForceAtPosition(wheelR.transform.up * -antiRollForce, wheelR.transform.position);
        }
        
    }
}