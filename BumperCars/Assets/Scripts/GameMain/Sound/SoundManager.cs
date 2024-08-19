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
        SettlementBg,
    }

    public class PlayingAudio
    {
        public AudioSource audioSource;
        public SoundConfigItem cfg;
        public PlayingAudio(AudioSource audioSource, SoundConfigItem cfg)
        {
            this.audioSource = audioSource;
            this.cfg = cfg;
        }
    }
    
    public class SoundManager : GameModule
    {
        private readonly Dictionary<SoundEnum, SoundConfigItem> m_soundConfigItemDict = new();
        private GlobalSoundRoot m_soundRoot;
        private GlobalConfig m_globalConfig;
        private readonly Dictionary<Entity, List<PlayingAudio>> entityPlayingAudioSources = new();

        public void Initialize(SoundConfig soundConfig)
        {
            m_soundRoot = GameEntry.EntityManager.CreateEntity<GlobalSoundRoot>(EntityEnum.GlobalSoundRoot, Vector3.zero, Quaternion.identity);
            GameEntry.EventManager.AddListener(VolumeChangeEventArgs.EventId, OnVolumeChange);
            m_globalConfig =GameEntry.ConfigManager.GetConfig<GlobalConfig>();
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
        public AudioSource PlayerSound(Entity source, SoundEnum soundEnum, bool isGlobal = true, float pitch = 1, bool continueOnEntityRecycle = true)
        {
            var playingAudio = m_soundRoot.PlayerSound(soundEnum, isGlobal, pitch);
            if (!continueOnEntityRecycle)
            {
                if (!entityPlayingAudioSources.ContainsKey(source)) entityPlayingAudioSources.Add(source, new List<PlayingAudio>());
                entityPlayingAudioSources[source].Add(playingAudio);
            }

            return playingAudio.audioSource;
        }

        private void OnVolumeChange(object sender, GameEventArgs e)
        {
            if(e is VolumeChangeEventArgs args)
            {
                foreach (var kv in entityPlayingAudioSources)
                {
                    foreach (var playingAudio in kv.Value)
                    {
                        playingAudio.audioSource.volume = (playingAudio.cfg.isMusic?m_globalConfig.musicVolume*0.3f:m_globalConfig.audioEffectVolume*0.6f)*m_globalConfig.mainVolume;
                    }
                }
            }
        }
        
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var kv in entityPlayingAudioSources)
            {
                if (!kv.Key.IsValid)
                {
                    foreach (var playingAudio in kv.Value) playingAudio.audioSource.Stop();
                    kv.Value.Clear();
                }
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