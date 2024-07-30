using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lachesis.GamePlay;
using UnityEngine;
using UnityEngine.AI;

namespace Lachesis.GamePlay
{
    public class CarAIChaseState : CarAIState
    {
        public CarAI owner;
        private static readonly float maxlostTargetTime = 2f;
        private float curlostTargetTime;
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
            curlostTargetTime = 0;
        }

        protected internal override void OnUpdate(FSM<CarAI> carAI, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(carAI, elapseSeconds, realElapseSeconds);
            // if(owner.destination!=null)
            // {
            //     if(Vector3.Distance(owner.transform.position, owner.destination.position)>GameEntry.ConfigManager.GetConfig<GlobalConfig>().CarAISearchRange)
            //     {
            //         curlostTargetTime+=realElapseSeconds;
            //         if(curlostTargetTime>maxlostTargetTime)
            //         {
            //             owner.destination = null;
            //             curlostTargetTime = 0f;
            //         }
            //     }
            //     else
            //     {
            //         curlostTargetTime = 0f;
            //     }
            // }
            // else
            // {
            //     owner.destination = TryGetPlayerTransform();
            //     
            // }
            if(owner.destination==null)
            {
                ChangeState<CarAIPatrolState>(carAI);
            }
            owner.carController.ChangeCarTurnState(owner.steeringDelta);
            owner.carController.ChangeCarForwardState(owner.motorDelta);
            owner.carController.ChangeCarBoostState(owner.boost);
            owner.carController.ChangeCarHandBrakeState(owner.handBrake);
            SetSteeringMove();
            PathProgress(realElapseSeconds);
        }
        
        //自动探测周围一定范围内的玩家
        // private Transform TryGetPlayerTransform()
        // {
        //     var playerTrans = GameEntry.EntityManager.GetEntityTransforms(EntityEnum.CarPlayer);
        //     float searchRange = GameEntry.ConfigManager.GetConfig<GlobalConfig>().CarAISearchRange;
        //     float minDis = Single.PositiveInfinity;
        //     Transform nearPlayerTrans = null;
        //     foreach (var trans in playerTrans)
        //     {
        //         var dis = Vector3.Distance(owner.transform.position, trans.position);
        //         if(minDis>dis)
        //         {
        //             minDis = dis;
        //             nearPlayerTrans = trans;
        //         }
        //     }
        //     return nearPlayerTrans;
        // }
        
        private void SetSteeringMove() // Applies steering to the Current waypoint
        {
            Vector3 relativeVector =  owner.transform.InverseTransformPoint(owner.postionToFollow);
            owner.steeringDelta = (relativeVector.x / relativeVector.magnitude);
        }
        
        
        private void PathProgress(float realElapseSeconds) //Checks if the agent has reached the currentWayPoint or not. If yes, it will assign the next waypoint as the currentWayPoint depending on the input
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
                    owner.postionToFollow = owner.waypoints[owner.currentWayPoint];
                    owner.allowMovement = true;
                    if (Vector3.Distance(owner.transform.position, owner.postionToFollow) < 2)
                        owner.currentWayPoint++;
                }

                if (owner.currentWayPoint >= owner.waypoints.Count - 1)
                    CustomPath(owner.destination, realElapseSeconds);
            }
        }
        private void CustomPath(Transform destination,float realElapseSeconds) //Creates a path to the Custom destination
        {
            NavMeshPath path = owner.navMeshPath;
            float searchRange = GameEntry.ConfigManager.GetConfig<GlobalConfig>().CarAISearchRange;
            if(destination==null)
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
                destination = nearPlayerTrans;
            }
            else
            {
                if(Vector3.Distance(destination.position, owner.transform.position)>searchRange)
                {
                    curlostTargetTime+=realElapseSeconds;
                    if(curlostTargetTime>maxlostTargetTime)
                    {
                        owner.destination = null;
                        curlostTargetTime = 0;
                        return;
                    }
                }
                else
                {
                    curlostTargetTime = 0;
                }
            }
            
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
                {
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

