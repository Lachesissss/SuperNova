using System.Collections.Generic;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public enum SoundEnum
    {
        Boost,
        ButtonPress,
        CarAttack,
        CoinPlayerRevive,
        GetPoints,
        LightingHit,
        LightingLaunch,
        LightingPrepare,
        MagicShield,
        Marvel,
        Portal,
        SpecialAudio,
        Loki,
        Tab,
        Stronger,
        WitchTime,
        DarkStart,
        DarkHit,
        PickUp,
        PlayerDead,
        Win,
        BattleBg1,
        BattleBg2,
        BattleBg3,
        SelectBg,
        MenuBg,
    }

    public class SoundManager : GameModule
    {
        private readonly Dictionary<SoundEnum, SoundConfigItem> m_soundConfigItemDict = new();
        private GlobalSoundRoot m_soundRoot;
        private readonly Dictionary<Entity, List<AudioSource>> entityPlayingAudioSources = new();

        public void Initialize(SoundConfig soundConfig)
        {
            m_soundRoot = GameEntry.EntityManager.CreateEntity<GlobalSoundRoot>(EntityEnum.GlobalSoundRoot, Vector3.zero, Quaternion.identity);
            foreach (var config in soundConfig.soundResources)
            {
                m_soundConfigItemDict.Add(config.soundType, config);
                m_soundRoot.AddGlobalSound(config.soundType);
            }
        }

        public SoundConfigItem GetSoundConfig(SoundEnum soundEnum)
        {
            if (m_soundConfigItemDict.TryGetValue(soundEnum, out var configItem)) return configItem;

            Debug.LogError("未能找到声音配置");
            return null;
        }

        /// <summary>
        ///     播放声音
        /// </summary>
        /// <param name="source">播放源实体</param>
        /// <param name="soundEnum">声音类型</param>
        /// <param name="isGlobal">是否全局唯一</param>
        /// <param name="pitch">播放速度</param>
        /// <param name="continueOnEntityRecycle">是否在源实体回收时继续播放</param>
        public AudioSource PlayerSound(Entity source, SoundEnum soundEnum, bool isGlobal = true, float pitch = 1, bool continueOnEntityRecycle = true,
            float volume = 1f)
        {
            var playingAudioSource = m_soundRoot.PlayerSound(soundEnum, isGlobal, pitch, volume);
            if (!continueOnEntityRecycle)
            {
                if (!entityPlayingAudioSources.ContainsKey(source)) entityPlayingAudioSources.Add(source, new List<AudioSource>());
                entityPlayingAudioSources[source].Add(playingAudioSource);
            }

            return playingAudioSource;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var kv in entityPlayingAudioSources)
                if (!kv.Key.IsValid)
                {
                    foreach (var audioSource in kv.Value) audioSource.Stop();
                    kv.Value.Clear();
                }
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }
    }
}