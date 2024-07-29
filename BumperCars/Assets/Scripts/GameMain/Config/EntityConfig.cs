using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Lachesis.GamePlay
{
    [CreateAssetMenu(fileName = "EntityConfig", menuName = "ScriptableObject/EntityConfig", order = 1)]
    public class EntityConfig : ScriptableObject
    {
        [Serializable]
        public struct EntityResource
        {
            [FormerlySerializedAs("item")] public EntityEnum itemEnum;
            public GameObject prefab;
        }

        public List<EntityResource> entityResources;
    }
}