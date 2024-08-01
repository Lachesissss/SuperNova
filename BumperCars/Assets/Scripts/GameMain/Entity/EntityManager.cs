using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lachesis.GamePlay
{
    public enum EntityEnum
    {
        //Car
        CarPlayer,
        CarEnemy,

        //PickupItem
        Coin,
        SkillPickUpItem,
        SkillCard2,
        
        //BattleField
        BattleField,
        
        //UI
        BattleUI,
        MenuUI,
        WinSettlementUI,
    }
    
    public class EntityManager : GameModule
    {
        private readonly Dictionary<EntityEnum, List<Entity>> m_hideEntityPool = new();
        private readonly Dictionary<EntityEnum, List<Entity>> m_activeEntityDict = new();
        private readonly Dictionary<EntityEnum, GameObject> m_entityDict = new();

        private EntityConfig m_entityConfig;
        private List<Transform> m_entitTransforms = new();
        public void SetConfig(EntityConfig config)
        {
            m_entityConfig = config;
            foreach (var entityResource in m_entityConfig.entityResources)
            {
                m_hideEntityPool.Add(entityResource.itemEnum, new List<Entity>());
                m_activeEntityDict.Add(entityResource.itemEnum, new List<Entity>());
                m_entityDict.Add(entityResource.itemEnum, entityResource.prefab);
            }
        }

        public T CreateEntity<T>(EntityEnum entityEnum,Vector3 pos, Quaternion rot, object userData = null) where T : Entity
        {
            GameObject obj = null;
            Entity entity = null;
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
                if(entity==null)
                {
                    Debug.LogError($"获取实体失败prefab上不存在预期的Component{typeof(T).Name}，请检查EntityConfig");
                    return null;
                }
                m_activeEntityDict[entityEnum].Add(entity);
                obj.SetActive(true);
                obj.GetComponent<T>().OnInit(); //仅在第一次创建时调用的函数
            }

            entity.OnReCreateFromPool(pos,rot, userData);
            return entity as T;
        }

        public T CreateEntity<T>(EntityEnum entityEnum, Transform parent, object userData = null) where T : Entity
        {
            GameObject obj = null;
            Entity entity = null;
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
                obj = Object.Instantiate(m_entityDict[entityEnum], parent);
                entity = obj.GetComponent<T>();
                if(entity==null)
                {
                    Debug.LogError($"获取实体失败prefab上不存在预期的Component{typeof(T).Name}，请检查EntityConfig");
                    return null;
                }
                m_activeEntityDict[entityEnum].Add(entity);
                obj.SetActive(true);
                obj.GetComponent<T>().OnInit(); //仅在第一次创建时调用的函数
            }
            entity.OnReCreateFromPool(userData);
            return entity as T;
        }
        public void ReturnEntity(EntityEnum entityEnum, Entity entity)
        {
            entity.OnReturnToPool();
            m_activeEntityDict[entityEnum].Remove(entity);
            m_hideEntityPool[entityEnum].Add(entity);
            entity.gameObject.SetActive(false);
        }
        
        public List<Transform> GetEntityTransforms(EntityEnum entityEnum)
        {
            m_entitTransforms.Clear();
            foreach (var obj in m_activeEntityDict[entityEnum])
            {
                m_entitTransforms.Add(obj.transform);
            }
            return m_entitTransforms;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var kv in m_activeEntityDict)
            {
                foreach (var entity in kv.Value)
                {
                    entity.OnUpdate(elapseSeconds,realElapseSeconds);
                }
            }
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            foreach (var kv in m_activeEntityDict)
            {
                foreach (var entity in kv.Value)
                {
                    entity.OnFixedUpdate(fixedElapseSeconds);
                }
            }
        }

        //所有还处于激活状态的实体调用OnReturnToPool()并回池
        internal override void Shutdown()
        {
            foreach (var kv in m_activeEntityDict)
                if (kv.Value is { Count: > 0 })
                    foreach (var entity in kv.Value)
                    {
                        entity.OnReturnToPool(true);
                    }

            m_activeEntityDict.Clear();
            m_hideEntityPool.Clear();
            m_entityDict.Clear();
        }
    }
}