using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public enum NetworkEntityEnum
    {
        NetworkCar,
        NetworkCarPlayer,
        NetworkCarAI
    }

    public class NetworkEntityManager : GameModule
    {
        private readonly Dictionary<NetworkEntityEnum, List<NetworkEntity>> m_hideEntityPool = new();
        private readonly Dictionary<NetworkEntityEnum, List<NetworkEntity>> m_activeEntityDict = new();
        private readonly Dictionary<NetworkEntityEnum, GameObject> m_entityDict = new();

        private NetworkEntityConfig m_entityConfig;
        private readonly List<Transform> m_entitTransforms = new();

        public void Initialize(NetworkEntityConfig config)
        {
            m_entityConfig = config;
            foreach (var entityResource in m_entityConfig.entityResources)
            {
                m_hideEntityPool.Add(entityResource.entityEnum, new List<NetworkEntity>());
                m_activeEntityDict.Add(entityResource.entityEnum, new List<NetworkEntity>());
                m_entityDict.Add(entityResource.entityEnum, entityResource.prefab);
            }
        }

        public T CreateNetworkEntity<T>(NetworkEntityEnum entityEnum, Vector3 pos, Quaternion rot, object userData = null) where T : NetworkEntity
        {
            GameObject obj = null;
            NetworkEntity entity = null;
            if (m_hideEntityPool[entityEnum].Count > 0)
            {
                entity = m_hideEntityPool[entityEnum][0];
                obj = entity.gameObject;
                m_hideEntityPool[entityEnum].RemoveAt(0);
                m_activeEntityDict[entityEnum].Add(entity);
                obj.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(m_entityDict[entityEnum], Vector3.zero, Quaternion.identity);
                entity = obj.GetComponent<T>();
                if (entity == null)
                {
                    Debug.LogError($"获取实体失败prefab上不存在预期的Component{typeof(T).Name}，请检查EntityConfig");
                    return null;
                }

                m_activeEntityDict[entityEnum].Add(entity);
                obj.SetActive(true);
                obj.GetComponent<T>().OnInit(); //仅在第一次创建时调用的函数
            }

            entity.entityEnum = entityEnum;
            entity.OnReCreateFromPool(pos, rot, userData);
            return entity as T;
        }

        public T CreateNetworkEntity<T>(NetworkEntityEnum entityEnum, Transform parent, object userData = null) where T : NetworkEntity
        {
            GameObject obj = null;
            NetworkEntity entity = null;
            if (m_hideEntityPool[entityEnum].Count > 0)
            {
                entity = m_hideEntityPool[entityEnum][0];
                obj = entity.gameObject;
                m_hideEntityPool[entityEnum].RemoveAt(0);
                m_activeEntityDict[entityEnum].Add(entity);
                //var pos = obj.transform.localPosition;
                //var rot = obj.transform.localRotation;

                //obj.transform.localPosition = pos;
                //obj.transform.localRotation = rot;
                obj.transform.SetParent(parent, false);
                obj.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(m_entityDict[entityEnum], parent);
                entity = obj.GetComponent<T>();
                if (entity == null)
                {
                    Debug.LogError($"获取实体失败prefab上不存在预期的Component{typeof(T).Name}，请检查EntityConfig");
                    return null;
                }

                m_activeEntityDict[entityEnum].Add(entity);
                obj.SetActive(true);
                obj.GetComponent<T>().OnInit(); //仅在第一次创建时调用的函数
            }

            entity.entityEnum = entityEnum;
            entity.OnReCreateFromPool(userData);
            return entity as T;
        }

        public void ReturnNetworkEntity(NetworkEntityEnum entityEnum, NetworkEntity entity)
        {
            if (!m_activeEntityDict[entityEnum].Contains(entity)) return; //重复回收已经回收的实体，直接返回
            entity.OnReturnToPool();
            m_activeEntityDict[entityEnum].Remove(entity);
            m_hideEntityPool[entityEnum].Add(entity);
            entity.gameObject.SetActive(false);
        }

        public List<Transform> GetNetworkEntityTransforms(NetworkEntityEnum entityEnum)
        {
            m_entitTransforms.Clear();
            foreach (var obj in m_activeEntityDict[entityEnum]) m_entitTransforms.Add(obj.transform);
            return m_entitTransforms;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var kv in m_activeEntityDict)
                for (var i = 0; i < kv.Value.Count; i++)
                {
                    if (i > kv.Value.Count) break; //有可能在update中删除其他实体或自己
                    kv.Value[i].OnUpdate(elapseSeconds, realElapseSeconds);
                }
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            foreach (var kv in m_activeEntityDict)
                for (var i = 0; i < kv.Value.Count; i++)
                {
                    if (i > kv.Value.Count) break;
                    kv.Value[i].OnFixedUpdate(fixedElapseSeconds);
                }
        }

        //所有还处于激活状态的实体调用OnReturnToPool()并回池
        internal override void Shutdown()
        {
            foreach (var kv in m_activeEntityDict)
                if (kv.Value is { Count: > 0 })
                    foreach (var entity in kv.Value)
                        entity.OnReturnToPool(true);

            m_activeEntityDict.Clear();
            m_hideEntityPool.Clear();
            m_entityDict.Clear();
        }

        public void ClearProcedureEntity(ProcedureBase procedureBase)
        {
            foreach (var kv in m_activeEntityDict)
                for (var i = 0; i < kv.Value.Count; i++)
                {
                    if (i > kv.Value.Count) break; //有可能在update中删除其他实体或自己
                    if (kv.Value[i].BelongProcedure == procedureBase)
                    {
                        ReturnNetworkEntity(kv.Key, kv.Value[i]);
                        i--;
                    }
                }
        }
    }
}