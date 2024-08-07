using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    [Serializable]
    public class SkillConfigItem
    {
        public SkillEnum skillEnum;
        public string skillName;
        public string iconName;
        public string activateText;
        public string killText;
        public bool showTextOnRelease;
        public SkillType skillType;
        public float spawnProb;
        public bool openInPve;
    }
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ScriptableObject/SkillConfig", order = 3)]
    public class SkillConfig : ScriptableObject
    {
        public List<SkillConfigItem> skillResources;
    }
}

