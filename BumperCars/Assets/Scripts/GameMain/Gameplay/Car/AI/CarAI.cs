using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lachesis.GamePlay
{
    public class CarAI : Entity
    {
        public CarController carController;
        public Transform destination; 
        public float steeringDelta;
        public float motorDelta;
        public bool boost;
        public bool handBrake;
        public NavMeshPath navMeshPath;
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
            navMeshPath = new NavMeshPath();
        }
        
        public override void OnInit(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnInit(pos, rot, userData);
            m_FsmManager = GameEntry.FSMManager;
            CarAIState[] carAIStates = {new CarAIChaseState(), new CarAIPatrolState()};
            m_CarAIFsm = m_FsmManager.CreateFsm(this, carAIStates);
            m_CarAIFsm.Start<CarAIPatrolState>();
            CalculateNavMashLayerBite();
            if(userData is string str)
            {
                carController.carName = str;
            }
        }

        public override void OnReturnToPool()
        {
            base.OnReturnToPool();
            m_CarAIFsm.Clear();
            Reset();
            var rb = carController.bodyRb;
            if(rb!=null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
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
                if (Vector3.Distance(transform.position, postionToFollow) < 1)
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
        
        public void SetSteeringMove() // Applies steering to the Current waypoint
        {
            
            Vector3 relativeVector =  transform.InverseTransformPoint(postionToFollow);
            steeringDelta = (relativeVector.x / relativeVector.magnitude);
            if(Vector3.Dot(transform.forward, relativeVector.normalized)<0)
            {
                if(steeringDelta>=0)
                {
                    steeringDelta = 1;
                }
                else
                {
                    steeringDelta = -1;
                }
            }
            
            
        }
        
        public void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 3)
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
                //DrawFOV();
                DrawTargetArea();
            }

            void DrawFOV()
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
            
            void DrawTargetArea()
            {
                if(destination==null)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }

                float angleStep = 360.0f / 100;
                var radius = GameEntry.ConfigManager.GetConfig<GlobalConfig>().carAISearchRange;
                for (int i = 0; i < 100; i++)
                {
                    float angleCurrent = i * angleStep * Mathf.Deg2Rad; 
                    float angleNext = (i + 1) * angleStep * Mathf.Deg2Rad; 
    
                    Vector3 pointCurrent = transform.position + new Vector3(Mathf.Cos(angleCurrent), 1f/radius, Mathf.Sin(angleCurrent)) * radius;
                    Vector3 pointNext = transform.position + new Vector3(Mathf.Cos(angleNext), 1f/radius, Mathf.Sin(angleNext)) * radius;

                    Gizmos.DrawLine(pointCurrent, pointNext);
                }
            }
        }
    }
}

