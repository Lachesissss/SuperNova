using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
            owner.Reset();
        }

        protected internal override void OnUpdate(FSM<CarAI> carAI, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(carAI, elapseSeconds, realElapseSeconds);
            
            if(owner.destination ==null)
            {
                owner.destination = TryGetPlayerTransform();
            }
            if(carAI.Owner.destination!=null)
            {
                ChangeState<CarAIChaseState>(carAI);
            }
            owner.carController.ChangeCarTurnState(owner.steeringDelta);
            owner.carController.ChangeCarForwardState(owner.motorDelta);
            owner.carController.ChangeCarBoostState(owner.boost);
            owner.carController.ChangeCarHandBrakeState(owner.handBrake);
            SetSteeringMove();
            PathProgress();
            
        }
        
        //TODO：自动探测周围一定范围内的玩家
        private Transform TryGetPlayerTransform()
        {
            return null;
        }
        
        private void SetSteeringMove() // Applies steering to the Current waypoint
        {
            Vector3 relativeVector =  owner.transform.InverseTransformPoint(owner.postionToFollow);
            owner.steeringDelta = (relativeVector.x / relativeVector.magnitude);
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
                    if (Vector3.Distance(owner.transform.position, owner.postionToFollow) < 2)
                        owner.currentWayPoint++;
                }

                if (owner.currentWayPoint >= owner.waypoints.Count - 1)
                    RandomPath();
            }
            
        }
        
        public void RandomPath() // Creates a path to a random destination
        {
            NavMeshPath path = new NavMeshPath();
            Vector3 sourcePostion;

            if (owner.waypoints.Count == 0)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10;
                randomDirection.y = 0;
                randomDirection += owner.transform.position;
                sourcePostion = owner.transform.position;
                Calculate(randomDirection, sourcePostion, owner.transform.forward, owner.NavMeshLayerBite);
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

