using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class GlobalSoundRoot : Entity
    {
        private readonly Dictionary<SoundEnum, List<AudioSource>> m_audioSourceDict = new();
        private GlobalConfig m_globalConfig;

        public override void OnInit(object userData = null)
        {
            base.OnInit(userData);
            m_globalConfig =GameEntry.ConfigManager.GetConfig<GlobalConfig>();
        }

        public void AddGlobalSound(SoundEnum soundEnum)
        {
            if (m_audioSourceDict.ContainsKey(soundEnum))
            {
                Debug.LogError("无法重复添加全局声音");
                return;
            }

            m_audioSourceDict.Add(soundEnum, new List<AudioSource>());
        }

        /// <summary>
        ///     播放声音
        /// </summary>
        /// <param name="soundEnum">声音类型</param>
        /// <param name="isGlobal">是否覆盖之前相同类型的声音</param>
        /// <param name="pitch">播放速率</param>
        public PlayingAudio PlayerSound(SoundEnum soundEnum, bool isGlobal, float pitch)
        {
            if (m_audioSourceDict.TryGetValue(soundEnum, out var sourceList))
            {
                AudioSource idleSource = null;
                if (isGlobal)
                {
                    if (sourceList.Count > 0) idleSource = sourceList[0];
                }
                else
                {
                    foreach (var source in sourceList)
                        if (!source.isPlaying)
                        {
                            idleSource = source;
                            break;
                        }
                }


                var cfg = GameEntry.SoundManager.GetSoundConfig(soundEnum);
                if (idleSource)
                {
                    idleSource.Play();
                }
                else
                {
                    idleSource = gameObject.AddComponent<AudioSource>();
                    idleSource.loop = cfg.isLoop;
                    idleSource.clip = cfg.clip;
                    idleSource.playOnAwake = false;
                    idleSource.spatialBlend = 0.0f;
                    sourceList.Add(idleSource);
                    idleSource.Play();
                }

                idleSource.pitch = pitch;
                idleSource.volume =(cfg.isMusic?m_globalConfig.musicVolume*0.3f:m_globalConfig.audioEffectVolume*0.6f)* m_globalConfig.mainVolume;
                return new PlayingAudio(idleSource, cfg);
            }

            Debug.LogError($"没有找到{soundEnum}对应的AudioSource列表");
            return null;
        }
    }
}