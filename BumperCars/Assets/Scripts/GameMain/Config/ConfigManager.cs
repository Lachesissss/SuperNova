using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.Core
{
    public class ConfigManager : GameModule
    {
        private readonly Dictionary<Type, ScriptableObject> configDictionary = new();

        public void Initialize()
        {
            LoadConfigs();
        }

        // 从 Resources/Config 加载所有配置文件
        private void LoadConfigs()
        {
            var configs = Resources.LoadAll<ScriptableObject>("Config");

            foreach (var config in configs) configDictionary[config.GetType()] = config;
        }

        public T GetConfig<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            if (configDictionary.ContainsKey(type)) return configDictionary[type] as T;

            Debug.LogError($"Config of type {type} not found.");
            return null;
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