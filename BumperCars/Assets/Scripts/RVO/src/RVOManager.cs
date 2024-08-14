using System;
using System.Collections.Generic;
using Lachesis.Core;
using RVO;
using UnityEngine;
using Random = System.Random;
using Vector2 = RVO.Vector2;

namespace Lachesis.GamePlay
{
    public class RVOManager : GameModule
    {
        private Dictionary<Transform, int> m_RVOAgentDict;
        private Dictionary<Transform, Vector3> m_RVOAgentTargetDict;
        private Dictionary<Transform, Rigidbody> m_RVOAgentRbDict;
        private Random m_random = new Random();
        public void Init()
        {
            m_RVOAgentDict = new();
            m_RVOAgentTargetDict = new();
            m_RVOAgentRbDict = new();
            Simulator.Instance.setTimeStep(0.1f);
            Simulator.Instance.setAgentDefaults(10.0f, 10, 10f, 10f, 3f, 8.0f, new Vector2(0.0f, 0.0f));

            // add in awake
            Simulator.Instance.processObstacles();
        }
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            if(m_RVOAgentDict==null||m_RVOAgentDict.Count==0) return;
            ROVUpdate();
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
            m_RVOAgentDict.Clear();
            m_RVOAgentTargetDict.Clear();
            m_RVOAgentRbDict.Clear();
        }
        
        public void AddAgent(Transform agentTrans, Vector3 target)
        { 
            Vector2 pos = new Vector2(agentTrans.position.x, agentTrans.position.z);
            var id = Simulator.Instance.addAgent(pos);
            if(m_RVOAgentDict.ContainsKey(agentTrans))
            {
                Debug.LogError("重复添加相同Transform的代理，请先Remove");
                return;
            }
            m_RVOAgentDict.Add(agentTrans, id);
            m_RVOAgentTargetDict.Add(agentTrans, target);
            var Rb = agentTrans.GetComponent<Rigidbody>();
            if(Rb==null)
            {
                Debug.LogError("TransForm上没有找到RigidBody");
                return;
            }
            m_RVOAgentRbDict.Add(agentTrans, Rb);
        }
        
        public void RemoveAgent(Transform agentTrans)
        {
            if(m_RVOAgentDict.ContainsKey(agentTrans))
            {
                m_RVOAgentDict.Remove(agentTrans);
                m_RVOAgentTargetDict.Remove(agentTrans);
                m_RVOAgentRbDict.Remove(agentTrans);
            }
        }
        
        public void UpdateAgentTarget(Transform agentTrans, Vector3 target)
        {
            if(!m_RVOAgentDict.ContainsKey(agentTrans))
            {
                Debug.LogError("不存在目标代理，请先Add");
                return;
            }
            m_RVOAgentTargetDict[agentTrans] = target;
        }
        
        private void ROVUpdate()
        {
            foreach (var kv in m_RVOAgentDict)
            {
                var target = new Vector2(m_RVOAgentTargetDict[kv.Key].x, m_RVOAgentTargetDict[kv.Key].z) ;
                var curPos = new Vector2(kv.Key.position.x, kv.Key.position.z);
                Simulator.Instance.setAgentPosition(kv.Value, curPos);
                var rigidBodyVelocity = m_RVOAgentRbDict[kv.Key].velocity;
                Simulator.Instance.setAgentVelocity(kv.Value, new Vector2(rigidBodyVelocity.x, rigidBodyVelocity.z));
                //Debug.Log($"RVOInput: {rigidBodyVelocity}");
                Vector2 goalVector = target - curPos;
                if (RVOMath.absSq(goalVector) > 1.0f)
                {
                    goalVector = RVOMath.normalize(goalVector);
                }
                Simulator.Instance.setAgentPrefVelocity(kv.Value, goalVector);

                /* Perturb a little to avoid deadlocks due to perfect symmetry. */
                float angle = (float) m_random.NextDouble()*2.0f*(float) Math.PI;
                float dist = (float) m_random.NextDouble()*0.0001f;

                Simulator.Instance.setAgentPrefVelocity(kv.Value, Simulator.Instance.getAgentPrefVelocity(kv.Value) +
                                                                  dist*
                                                                  new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle)));
            }
            Simulator.Instance.doStep();
        }
        
        public Vector3 GetRVOVelocity(Transform trans)
        {
            if(m_RVOAgentDict.TryGetValue(trans, out int id))
            {
                var dir = Simulator.Instance.getAgentVelocity(id);
                return new Vector3(dir.x(),0,dir.y());
            }
            Debug.LogError("获取RVO速度失败，不存在目标代理，请先Add"); 
            return Vector3.zero;
        }
    }
}

