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
        SkillCard2
    }

    public class EntityManager : GameModule
    {
        private readonly Dictionary<EntityEnum, List<GameObject>> m_hideEntityPool = new();
        private readonly Dictionary<EntityEnum, List<GameObject>> m_activeEntityDict = new();
        private readonly Dictionary<EntityEnum, GameObject> m_entityDict = new();
        private EntityConfig m_entityConfig;

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

        public GameObject CreateEntity(EntityEnum entityEnum, Action<GameObject> initFun)
        {
            GameObject obj = null;
            if (m_hideEntityPool[entityEnum].Count > 0)
            {
                obj = m_hideEntityPool[entityEnum][0];
                m_hideEntityPool[entityEnum].RemoveAt(0);
                m_activeEntityDict[entityEnum].Add(obj);
            }
            else
            {
                obj = Object.Instantiate(m_entityDict[entityEnum], Vector3.zero, Quaternion.identity);
                m_activeEntityDict[entityEnum].Add(obj);
            }

            if (initFun != null) initFun.Invoke(obj);
            return obj;
        }

        public void ReturnEntity(EntityEnum entityEnum, GameObject gameObject, Action<GameObject> returnFun)
        {
            m_activeEntityDict[entityEnum].Remove(gameObject);
            m_hideEntityPool[entityEnum].Add(gameObject);
            if (returnFun != null) returnFun.Invoke(gameObject);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }
    }
}