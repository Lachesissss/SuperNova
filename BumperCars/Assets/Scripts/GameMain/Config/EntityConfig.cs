using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lachesis.GamePlay
{
    /// <summary>
    /// 配置规则: 每个EntityEnum对应一个预制体，预制体根节点上需要有Entity组件
    /// </summary>
    [CreateAssetMenu(fileName = "EntityConfig", menuName = "ScriptableObject/EntityConfig", order = 1)]
    public class EntityConfig : ScriptableObject
    {
        [Serializable]
        public struct EntityResource
        {
            public EntityEnum itemEnum;
            public GameObject prefab;
        }

        public List<EntityResource> entityResources;
    }
}