using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Lachesis.GamePlay
{
    public class CarAIChaseState : CarAIState
    {
        public CarAI owner;
        private static readonly float maxlostTargetTime = 2f;
        private float m_curlostTargetTime;
        protected internal override void OnInit(FSM<CarAI> carAI)
        {
            base.OnInit(carAI);
            owner = carAI.Owner;
        }
        
        protected internal override void OnEnter(FSM<CarAI> carAI)
        {
            base.OnEnter(carAI);
            Debug.Log($"{owner.name}进入追逐状态");
            owner.Reset();
            
            m_curlostTargetTime = 0;
        }

        protected internal override void OnUpdate(FSM<CarAI> carAI, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(carAI, elapseSeconds, realElapseSeconds);

            if(!Targeting(realElapseSeconds))
            {
                ChangeState<CarAIPatrolState>(carAI);
                return;
            }
            owner.carController.ChangeCarTurnState(owner.steeringDelta);
            owner.carController.ChangeCarForwardState(owner.motorDelta);
            owner.carController.ChangeCarBoostState(owner.boost);
            owner.carController.ChangeCarHandBrakeState(owner.handBrake);
            owner.SetSteeringMove();
            
            PathProgress();
        }
        
        private bool Targeting(float realElapseSeconds)
        {
            NavMeshPath path = owner.navMeshPath;
            float searchRange = GameEntry.ConfigManager.GetConfig<GlobalConfig>().carAISearchRange;
            
            if(owner.destination!=null)
            {
                //有目标，离开范围，如果maxlostTargetTime内没有回到范围内，丢失目标
                if(Vector3.Distance(owner.destination.position, owner.transform.position)>searchRange)
                {
                    m_curlostTargetTime+=realElapseSeconds;
                    if(m_curlostTargetTime>maxlostTargetTime)
                    { 
                        owner.destination = null;
                        m_curlostTargetTime = 0;
                    }
                }
                else
                { 
                    //重新找到目标，丢失目标时间清零
                    m_curlostTargetTime = 0;
                }
            }
            
            //丢失目标，看能不能找新的目标，找个最近的    
            if(owner.destination==null)
            {
                    var playerTrans = GameEntry.EntityManager.GetEntityTransforms(EntityEnum.CarPlayer);
                    float minDis = Single.PositiveInfinity;
                    Transform nearPlayerTrans = null;
                    foreach (var trans in playerTrans)
                    {
                        if(NavMesh.SamplePosition(trans.position, out NavMeshHit hit, 1, owner.NavMeshLayerBite) &&
                           NavMesh.CalculatePath(owner.transform.position, hit.position, owner.NavMeshLayerBite, path))
                        {
                            var dis = Vector3.Distance(owner.transform.position, trans.position);
                            if(minDis>dis)
                            {
                                minDis = dis;
                                if(minDis<searchRange)
                                    nearPlayerTrans = trans;
                            }
                        }
                    }
                    owner.destination = nearPlayerTrans;
            }
            
            //若查找新目标失败，真正丢失目标
            return owner.destination!=null;
        }
        
        private void PathProgress() //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
        {
            wayPointManager();
            owner.SetMotorMove();
            owner.ListOptimizer();
            
            void wayPointManager()
            {
                if (owner.currentWayPoint >= owner.waypoints.Count)
                    owner.allowMovement = false;
                else
                {
                    if (Vector3.Distance(owner.waypoints[owner.currentWayPoint], owner.destination.position) > 3.5)
                        //离目标太远的路径点就抛弃掉，重新创建路径
                        owner.waypoints.RemoveAt(owner.currentWayPoint);
                    //CustomPath(owner.destination);
                    if (owner.currentWayPoint >= owner.waypoints.Count)
                    {
                        owner.allowMovement = false;
                    }
                    else
                    {
                        owner.postionToFollow = owner.waypoints[owner.currentWayPoint];
                        owner.allowMovement = true;
                        if (Vector3.Distance(owner.transform.position, owner.postionToFollow) < 2)
                            owner.currentWayPoint++;
                    }
                }
                
                if (owner.currentWayPoint >= owner.waypoints.Count - 1)
                    CustomPath(owner.destination);
            }
        }
        private void CustomPath(Transform destination) //Creates a path to the Custom destination
        {
            if(destination==null) return;
            
            Vector3 sourcePostion;

            if (owner.waypoints.Count < 2)
            {
                sourcePostion = owner.transform.position;
                Calculate(destination.position, sourcePostion, owner.transform.forward, owner.NavMeshLayerBite);
            }
            else
            {
                sourcePostion = owner.waypoints[owner.waypoints.Count - 1];
                Vector3 direction = (owner.waypoints[owner.waypoints.Count - 1] - owner.waypoints[owner.waypoints.Count - 2]).normalized;
                Calculate(destination.position, sourcePostion, direction, owner.NavMeshLayerBite);
            }

            
        }
        
        public void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaBite)
        {
            NavMeshPath path = owner.navMeshPath;
            //判断目标是否已经在navmesh导航之外
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 1, NavMeshAreaBite) &&
                NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshAreaBite, path))
            {
                if (path.corners.ToList().Count() > 1&& owner.CheckForAngle(path.corners[1], sourcePostion, direction))
                {   //这里不知道为啥第一次会生成一个奇怪位置的waypoints但是目前表现正常先不管
                    owner.waypoints.AddRange(path.corners.ToList());
                    owner.DebugLog("Custom Path generated successfully", false);
                }
                else
                {
                    if (path.corners.Length > 2 && owner.CheckForAngle(path.corners[2], sourcePostion, direction))
                    {
                        owner.waypoints.AddRange(path.corners.ToList());
                        owner.DebugLog("Custom Path generated successfully", false);
                    }
                    else
                    {
                        owner.DebugLog("Failed to generate a Custom path. Waypoints are outside the AIFOV. Generating a new one", false);
                    }
                }
            }
            else
            {
                //owner.DebugLog("Failed to generate a Custom path. Invalid Path. Generating a new one", false);
                owner.destination = null;
            }
        }
        
        protected internal override void OnLeave(FSM<CarAI> carAI, bool isShutdown)
        {
            base.OnLeave(carAI, isShutdown);
            
        }
        
        protected internal override void OnDestroy(FSM<CarAI> carAI)
        {
            base.OnDestroy(carAI);
        }
    }
}

