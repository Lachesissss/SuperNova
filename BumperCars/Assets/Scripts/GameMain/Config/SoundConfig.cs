using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    [Serializable]
    public class SoundConfigItem
    {
        public SoundEnum soundType;
        public AudioClip clip;
        public bool isLoop;
    }

    [CreateAssetMenu(fileName = "SoundConfig", menuName = "ScriptableObject/SoundConfig", order = 4)]
    public class SoundConfig : ScriptableObject
    {
        public List<SoundConfigItem> soundResources;
    }
}