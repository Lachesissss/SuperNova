using System;
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
        SkillCard1,
        SkillCard2,
        
        //BattleField
        BattleField,
        
        //UI
        BattleUI,
    }
    
    public class EntityManager : GameModule
    {
        private readonly Dictionary<EntityEnum, List<GameObject>> m_hideEntityPool = new();
        private readonly Dictionary<EntityEnum, List<GameObject>> m_activeEntityDict = new();
        private readonly Dictionary<EntityEnum, GameObject> m_entityDict = new();

        private EntityConfig m_entityConfig;
        private List<Transform> m_entitTransforms = new();
        public void SetConfig(EntityConfig config)
        {
            m_entityConfig = config;
            foreach (var entityResource in m_entityConfig.entityResources)
            {
                m_hideEntityPool.Add(entityResource.itemEnum, new List<GameObject>());
                m_activeEntityDict.Add(entityResource.itemEnum, new List<GameObject>());
                m_entityDict.Add(entityResource.itemEnum, entityResource.prefab);
            }
        }

        public T CreateEntity<T>(EntityEnum entityEnum,Vector3 pos, Quaternion rot, object userData = null) where T : Entity
        {
            GameObject obj = null;
            if (m_hideEntityPool[entityEnum].Count > 0)
            {
                obj = m_hideEntityPool[entityEnum][0];
                m_hideEntityPool[entityEnum].RemoveAt(0);
                m_activeEntityDict[entityEnum].Add(obj);
                obj.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(m_entityDict[entityEnum], Vector3.zero, Quaternion.identity);
                m_activeEntityDict[entityEnum].Add(obj);
                obj.SetActive(true);
            }
            
            var entityComponent = obj.GetComponent<T>();
            if(entityComponent==null)
            {
                Debug.LogError($"获取实体失败prefab上不存在预期的Component{typeof(T).Name}，请检查EntityConfig");
                return null;
            }
            else
            {
                entityComponent.OnInit(pos,rot, userData);
            }
            
            return entityComponent;
        }

        public T CreateEntity<T>(EntityEnum entityEnum, Transform parent, object userData = null) where T : Entity
        {
            GameObject obj = null;
            if(!m_hideEntityPool.ContainsKey(entityEnum))
            {
                Debug.LogError($"未找到实体枚举{entityEnum.ToString()},请检查EntityConfig");
                return null;
            }
            
            if (m_hideEntityPool[entityEnum].Count > 0)
            {
                obj = m_hideEntityPool[entityEnum][0];
                m_hideEntityPool[entityEnum].RemoveAt(0);
                m_activeEntityDict[entityEnum].Add(obj);
                obj.SetActive(true);
            }
            else
            {
                obj = Object.Instantiate(m_entityDict[entityEnum],parent);
                m_activeEntityDict[entityEnum].Add(obj);
                obj.SetActive(true);
            }
            
            var entityComponent = obj.GetComponent<T>();
            if(entityComponent==null)
            {
                Debug.LogError($"获取实体失败prefab上不存在预期的Component:{typeof(T).Name}，请检查EntityConfig");
                return null;
            }
            else
            {
                entityComponent.OnInit(userData);
            }
            
            return entityComponent;
        }
        public void ReturnEntity<T>(EntityEnum entityEnum, GameObject obj) where T:Entity
        {
            var entityComponent = obj.GetComponent<T>();
            if(entityComponent==null)
            {
                Debug.LogError($"返回实体失败prefab上不存在预期的Component:{typeof(T).Name}，请检查EntityConfig");
                return;
            }
            
            entityComponent.OnReturnToPool();
            m_activeEntityDict[entityEnum].Remove(obj);
            m_hideEntityPool[entityEnum].Add(obj);
            obj.SetActive(false);
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

        }

        internal override void Shutdown()
        {
        }
    }
}