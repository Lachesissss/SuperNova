using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.AI;

namespace Lachesis.GamePlay
{
    public sealed class CarAI : CarController
    {
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
        public RVOManager m_RVOManager;
        public static float EmergencyBrakingTime = 0.8f;
        private float lastEmergencyBrakingTime = 0f;//剩余紧急制动时间
        public Vector3 RVODir => m_RVOManager.GetRVOVelocity(carComponent.transform).normalized;
        public bool isUsingRVO;
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

        public void ResetCarAIState()
        {
            waypoints.Clear();
            m_RVOManager.UpdateAgentTarget(carComponent.transform, GetCurTarget());
            currentWayPoint = 0;
            allowMovement = false;
            postionToFollow = Vector3.zero;
            navMeshPath = new NavMeshPath();
            lastEmergencyBrakingTime = 0;
        }

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_RVOManager = GameEntry.RVOManager;
            isUsingRVO = GameEntry.ConfigManager.GetConfig<GlobalConfig>().isUsingRVO;
        }

        public override void OnReCreateFromPool(Vector3 pos, Quaternion rot, object userData = null)
        {
            base.OnReCreateFromPool(pos, rot, userData);
            ResetCarAI(userData);
        }

        public override void OnReCreateFromPool(object userData = null)
        {
            base.OnReCreateFromPool(userData);
            ResetCarAI(userData);
        }

        private void ResetCarAI(object userData)
        {
            m_FsmManager = GameEntry.FSMManager;
            if(userData is CarControllerData)
            {
                m_RVOManager.AddAgent(carComponent.transform, GetCurTarget());
            }
            CarAIState[] carAIStates = { new CarAIChaseState(), new CarAIPatrolState() };
            m_CarAIFsm = m_FsmManager.CreateFsm(this, carAIStates);
            m_CarAIFsm.Start<CarAIPatrolState>();
            NavMeshLayerBite = 0;
            lastEmergencyBrakingTime=0;
            CalculateNavMashLayerBite();
            
        }
        
        public Vector3 GetCurTarget()
        {
            if(waypoints.Count>0)
            {
                for (var i = 0; i < waypoints.Count; i++)
                    if (currentWayPoint == i)
                        return waypoints[i];
            }
            return Vector3.zero;
        }
        
        public  override void SetCar(CarComponent car)
        {
            if (carComponent != null)
            {
                ClearCar();
                Debug.LogWarning("在Controller有控制对象的时候SetCar会将当前控制对象回收，请确认是否符合预期");
            }
            carComponent = car;
            m_RVOManager.AddAgent(carComponent.transform, GetCurTarget());
            SetCarInfo();
        }
        
        public override void ClearCar() 
        {
            if(carComponent!=null)
            {
                carComponent.controller = null;
                GameEntry.EntityManager.ReturnEntity(carComponent.entityEnum, carComponent);
                m_RVOManager.RemoveAgent(carComponent.transform);
                carComponent = null;
            }
        }
        
        public override void OnReturnToPool(bool isShowDown = false)
        {
            base.OnReturnToPool(isShowDown);
            m_CarAIFsm.Clear();
            if(!isShowDown)
            {
                StopAllCoroutines();
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
                if (Vector3.Distance(carComponent.transform.position, postionToFollow) < 1)
                {
                    handBrake = true;
                }
                else
                {
                    handBrake = false;
                }
                if(lastEmergencyBrakingTime>0)
                {
                    handBrake = true;
                }
                var speedOfWheels = carComponent.GetRPM();
                if(speedOfWheels<carComponent.maxRPM)
                {
                    if(Vector3.Dot( postionToFollow-carComponent.transform.position, carComponent.transform.forward)<0)
                    {
                        //方向不对减速掉头
                        motorDelta = 0.3f;
                        
                    }
                    else
                    {
                        motorDelta = 1;
                    }
                    if(isUsingRVO)
                    {
                        var dot = Vector3.Dot( RVODir, (postionToFollow-carComponent.transform.position).normalized);
                        //Debug.Log(dot);
                        if(dot<0.8)
                        {
                            //RVO预测与NavMesh目标方向差距较大，说明其他车辆靠近
                            if(Vector3.Dot( RVODir, carComponent.transform.forward)<0)
                            {
                                motorDelta = 0.3f;
                            }
                            else
                            {
                                motorDelta = 0.4f;
                            }

                            if (dot < -0.8 && lastEmergencyBrakingTime <= 0 && carComponent.bodyRb.velocity.magnitude > 1)
                                lastEmergencyBrakingTime = EmergencyBrakingTime;
                        }
                    }
                    
                }
                else
                {
                    motorDelta = 0;
                }
               
            }
            else
            {
                motorDelta = 0;
                handBrake = true;
            }
        }
        
        private IEnumerator EmergencyBraking()
        {
            handBrake = true;
            yield return new WaitForSeconds(0.8f);
            handBrake = false;
        }
        
        public void SetSteeringMove() // Applies steering to the Current waypoint
        {
            Vector3 relativeVector =  carComponent.transform.InverseTransformPoint(postionToFollow);
            relativeVector.y = 0;
            steeringDelta = (relativeVector.x / relativeVector.magnitude);
            //if(Vector3.Dot(carComponent.transform.forward, relativeVector.normalized)<0)
           
            
            if (Vector3.Dot(carComponent.transform.forward, isUsingRVO?RVODir: relativeVector.normalized) < 0)
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
            // if(isUsingRVO)
            // {
            //     var cross = Vector3.Cross(RVODir,relativeVector.normalized);
            //     if(Vector3.Dot( relativeVector.normalized,  RVODir) < 0.8)
            //     {
            //         if(cross.y>0) steeringDelta = 1;
            //         else steeringDelta = -1;
            //     }
            // }
            
        }
        
        public void ListOptimizer()
        {
            if (currentWayPoint > 1 && waypoints.Count > 3)
            {
                waypoints.RemoveAt(0);
                m_RVOManager.UpdateAgentTarget(carComponent.transform, GetCurTarget());
                currentWayPoint--;
            }
        }
        
        private void OnDrawGizmos() // shows a Gizmos representing the waypoints and AI FOV
        {
            if(!IsHasCar) return;
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
                Gizmos.color = Color.blue; // 设置线条颜色，可以根据需要修改
                var direction = m_RVOManager.GetRVOVelocity(carComponent.transform).normalized;
                Gizmos.DrawLine(carComponent.transform.position, carComponent.transform.position + direction * 2);
                
                if (Application.isPlaying)
                {
                    DrawTargetArea();
                }
            }

            void DrawFOV()
            {
                Gizmos.color = Color.white;
                float totalFOV = AIFOV * 2;
                float rayRange = 10.0f;
                float halfFOV = totalFOV / 2.0f;
                Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
                Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
                Vector3 leftRayDirection = leftRayRotation * carComponent.transform.forward;
                Vector3 rightRayDirection = rightRayRotation * carComponent.transform.forward;
                Gizmos.DrawRay(carComponent.transform.position, leftRayDirection * rayRange);
                Gizmos.DrawRay(carComponent.transform.position, rightRayDirection * rayRange);
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
                var radius = m_globalConfig.carAISearchRange;
                for (int i = 0; i < 100; i++)
                {
                    float angleCurrent = i * angleStep * Mathf.Deg2Rad; 
                    float angleNext = (i + 1) * angleStep * Mathf.Deg2Rad; 
    
                    Vector3 pointCurrent = carComponent.transform.position + new Vector3(Mathf.Cos(angleCurrent), 1f/radius, Mathf.Sin(angleCurrent)) * radius;
                    Vector3 pointNext = carComponent.transform.position + new Vector3(Mathf.Cos(angleNext), 1f/radius, Mathf.Sin(angleNext)) * radius;

                    Gizmos.DrawLine(pointCurrent, pointNext);
                }
            }
        }

        public override void OnSwitchCar()
        {
            base.OnSwitchCar();
            ResetCarAIState(); //切换时暂时失去目标
            destination = null;
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (!IsHasCar) return;
            if(lastEmergencyBrakingTime>0)
            {
                lastEmergencyBrakingTime-=elapseSeconds;
            }
            //Debug.Log($"RVOOutPut: {m_RVOManager.GetRVOVelocity(carComponent.transform)}");
            //Debug.Log($"RB: {carComponent.transform.GetComponent<Rigidbody>().velocity}");
        }
        
        
    }
}

