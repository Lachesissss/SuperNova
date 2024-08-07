using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Lachesis.GamePlay
{
    

    [CreateAssetMenu(fileName = "AtlasConfig", menuName = "ScriptableObject/AtlasConfig", order = 2)]
    public class AtlasConfig : ScriptableObject
    {
        [Serializable]
        public struct AtlasResource
        {
            public AtlasEnum atlasEnum;
            public SpriteAtlas atlas;
        }

        public List<AtlasResource> atlasResources;
    }
}

