using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Lachesis.GamePlay
{
    public class CarAIPatrolState : CarAIState
    {
        public CarAI owner;
        protected internal override void OnInit(FSM<CarAI> carAI)
        {
            base.OnInit(carAI);
            owner = carAI.Owner;
        }
        
        protected internal override void OnEnter(FSM<CarAI> carAI)
        {
            base.OnEnter(carAI);
            Debug.Log($"{owner.name}进入巡逻状态");
            owner.Reset();
        }

        protected internal override void OnUpdate(FSM<CarAI> carAI, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(carAI, elapseSeconds, realElapseSeconds);
            if(!carAI.Owner.IsHasCar) return; //先这样处理，后面应该搞个单独的Idle状态
            if(carAI.Owner.destination!=null)
            {
                ChangeState<CarAIChaseState>(carAI); //注意：changestate下一帧才生效，因此当前帧如果不想执行后续逻辑就必须return
                return;
            }
            owner.carComponent.ChangeCarTurnState(owner.steeringDelta);
            owner.carComponent.ChangeCarForwardState(owner.motorDelta);
            owner.carComponent.ChangeCarBoostState(owner.boost);
            owner.carComponent.ChangeCarHandBrakeState(owner.handBrake);
            owner.SetSteeringMove();
            PathProgress();
            
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
                    owner.postionToFollow = owner.waypoints[owner.currentWayPoint];
                    owner.allowMovement = true;
                    if (Vector3.Distance(owner.carComponent.transform.position, owner.postionToFollow) < 2)
                        owner.currentWayPoint++;
                }

                if (owner.currentWayPoint >= owner.waypoints.Count - 1)
                    RandomPath();
            }
            
        }
        
        public void RandomPath() // Creates a path to a random destination
        {
            //检测范围内是否有玩家，有的话将玩家设为目标（只有在创建新Path时才检测一次，为的是不会每帧都大规模检测，但这样AI可能有点笨
            NavMeshPath path = owner.navMeshPath;
            float searchRange = GameEntry.ConfigManager.GetConfig<GlobalConfig>().carAISearchRange;
            var cars = ProcedureBattle.carControllers;
            float minDis = Single.PositiveInfinity;
            Transform nearPlayerTrans = null;
            foreach (var carController in cars)
            {
                if(carController is CarPlayer player && player.IsHasCar)
                {
                    if(NavMesh.SamplePosition(player.carComponent.transform.position, out NavMeshHit hit, 1, owner.NavMeshLayerBite) &&
                       NavMesh.CalculatePath(owner.carComponent.transform.position, hit.position, owner.NavMeshLayerBite, path))
                    {
                        var dis = Vector3.Distance(owner.carComponent.transform.position, player.carComponent.transform.position);
                        if(minDis>dis)
                        {
                            minDis = dis;
                            if(minDis<searchRange)
                                nearPlayerTrans = player.carComponent.transform;
                        }
                    }
                }
                
                
            }
            owner.destination = nearPlayerTrans;
            
            
            
            Vector3 sourcePostion;

            if (owner.waypoints.Count == 0)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10;
                randomDirection.y = 0;
                randomDirection += owner.carComponent.transform.position;
                sourcePostion = owner.carComponent.transform.position;
                Calculate(randomDirection, sourcePostion, owner.carComponent.transform.forward, owner.NavMeshLayerBite);
            }
            else
            {
                sourcePostion = owner.waypoints[owner.waypoints.Count - 1];
                Vector3 randomPostion = Random.insideUnitSphere * 10;
                randomPostion += sourcePostion;
                Vector3 direction = (owner.waypoints[owner.waypoints.Count - 1] - owner.waypoints[owner.waypoints.Count - 2]).normalized;
                Calculate(randomPostion, sourcePostion, direction, owner.NavMeshLayerBite);
            }

            void Calculate(Vector3 destination, Vector3 sourcePostion, Vector3 direction, int NavMeshAreaByte)
            {
                if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 1, NavMeshAreaByte) &&
                    NavMesh.CalculatePath(sourcePostion, hit.position, NavMeshAreaByte, path))
                {
                    if(NavMesh.FindClosestEdge(destination, out NavMeshHit closestHit,  NavMeshAreaByte))
                    {
                        //如果离边缘过近，则返回
                        if(closestHit.distance<3)
                        {
                            owner.DebugLog("Failed to generate a random path. Invalid Path. Generating a new one", false);
                            return;
                        }
                            
                    }
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
                    owner.DebugLog("Failed to generate a random path. Invalid Path. Generating a new one", false);
                }
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

