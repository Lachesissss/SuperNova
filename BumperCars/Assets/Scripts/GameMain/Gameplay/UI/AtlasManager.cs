using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;
using UnityEngine.U2D;

namespace Lachesis.GamePlay
{
    public enum AtlasEnum
    {
        Skill,
    }
    
    public class AtlasManager : GameModule
    {
        private AtlasConfig m_atlasConfig;
        private Dictionary<AtlasEnum, SpriteAtlas> m_atlasDict = new();
        
        public void SetConfig(AtlasConfig config)
        {
            m_atlasConfig = config;
            foreach (var atlasResource in m_atlasConfig.atlasResources)
            {
                m_atlasDict.Add(atlasResource.atlasEnum, atlasResource.atlas);
            }
        }
        
        public Sprite GetSprite(AtlasEnum atlasEnum, string spriteName)
        {
            var atlas = m_atlasDict[atlasEnum];
            return atlas.GetSprite(spriteName);
        }
        
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        internal override void Shutdown()
        {
            
        }
    }
}


