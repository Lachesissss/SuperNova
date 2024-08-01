using System.Collections;
using System.Collections.Generic;
using Lachesis.Core;
using UnityEditor;
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
        //private Dictionary<AtlasEnum, string> m_atlasPath = new();
        
        public void SetConfig(AtlasConfig config)
        {
            m_atlasConfig = config;
            foreach (var atlasResource in m_atlasConfig.atlasResources)
            {
                m_atlasDict.Add(atlasResource.atlasEnum, atlasResource.atlas);
                //m_atlasPath.Add(atlasResource.atlasEnum, atlasResource.atlasPath);
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


