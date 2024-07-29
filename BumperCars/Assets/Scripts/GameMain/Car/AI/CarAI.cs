using System;
using System.Collections.Generic;
using System.Linq;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class CarAI : MonoBehaviour
    {
        public CarController carController;
        public Transform destination; 
        public float steeringDelta;
        public float motorDelta;
        public bool boost;
        public bool handBrake;
       
        public Vector3 postionToFollow = Vector3.zero;
        public bool allowMovement;
        
        private FSM<CarAI> m_CarAIFsm;
        private FSMManager m_FsmManager;
        #region NavMesh寻路
        
        
        public List<string> NavMeshLayers;
        public int currentWayPoint;
        public float AIFOV = 180;
        
        public int NavMeshLayerBite;
        public List<Vector3> waypoints = new List<Vector3>();

        #endregion
        
        [Header("Debug")]
        public bool ShowGizmos;
        public bool Debugger;

        public CarAIState CurrentProcedure
        {
            get
            {
                if (m_CarAIFsm == null) Debug.LogError("You must initialize procedure first.");

                return (CarAIState)m_CarAIFsm.CurrentState;
            }
        }

        public void Reset()
        {
            waypoints.Clear();
            currentWayPoint = 0;
            allowMovement = false;
            postionToFollow = Vector3.zero;
        }

        private void Start()
        {
            m_FsmManager = GameEntry.FSMManager;
            CarAIState[] carAIStates = {new CarAIChaseState(), new CarAIPatrolState()};
            m_CarAIFsm = m_FsmManager.CreateFsm(this, carAIStates);
            m_CarAIFsm.Start<CarAIChaseState>();
            CalculateNavMashLayerBite();
        }

        private void CalculateNavMashLayerBite()
        {
            if (NavMeshLayers == null || NavMeshLayers[0] == "AllAreas")
                NavMeshLayerBite = NavMesh.AllAreas;
            else if (NavMeshLayers.Count == 1)
                NavMeshLayerBite += 1 << NavMesh.GetAreaFromName(NavMeshLayers[0]);
            else
            {
                foreach (string Layer in NavMeshLayers)
                {
                    int I = 1 << NavMesh.GetAreaFromName(Layer);
                    NavMeshLayerBite += I;
                }
            }
        }
        
        public void DebugLog(string text, bool IsCritical)
        {
            if (Debugger)
            {
                if (IsCritical)
                    Debug.LogError(text);
                else
                    Debug.Log(text);
            }
        }
        
        public bool CheckForAngle(Vector3 pos, Vector3 source, Vector3 direction) //calculates the angle between the car and the waypoint 
        {
            Vector3 distance = (pos - source).normalized;
            float CosAngle = Vector3.Dot(distance, direction);
            float Angle = Mathf.Acos(CosAngle) * Mathf.Rad2Deg;

            if (Angle < AIFOV)
                return true;
            else
                return false;
        }
        
        public void SetMotorMove()
        {
            if(allowMovement)
            {
                var speedOfWheels = carController.GetRPM();
                if(speedOfWheels<carController.maxRPM)
                {
                    if(Vector3.Dot(postionToFollow-transform.position, transform.forward)<0)
                    {
                        //方向不对减速掉头
                        motorDelta = 0.2f;
                    }
                    else
                    {
                        motorDelta = 1;
                    }
                }
                else
                {
                    motorDelta = 0;
                }
                if (Vector3.Distance(transform.position, postionToFollow) < 2)
                {
                    handBrake = true;
                }
                else
                {
                    handBrake = false;
                }
            }
            else
            {
                motorDelta = 0;
                handBrake = true;
            }
        }
        
        public void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 5)
            {
                waypoints.RemoveAt(0);
                currentWayPoint--;
            }
        }
        
        private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
        {
            if (ShowGizmos == true)
            {
                for (int i = 0; i < waypoints.Count; i++)
                {
                    if (i == currentWayPoint)
                        Gizmos.color = Color.blue;
                    else
                    {
                        if (i > currentWayPoint)
                            Gizmos.color = Color.red;
                        else
                            Gizmos.color = Color.green;
                    }
                    Gizmos.DrawWireSphere(waypoints[i], 2f);
                }
                CalculateFOV();
            }

            void CalculateFOV()
            {
                Gizmos.color = Color.white;
                float totalFOV = AIFOV * 2;
                float rayRange = 10.0f;
                float halfFOV = totalFOV / 2.0f;
                Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
                Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
                Vector3 leftRayDirection = leftRayRotation * transform.forward;
                Vector3 rightRayDirection = rightRayRotation * transform.forward;
                Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
                Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
            }
        }
    }
}
