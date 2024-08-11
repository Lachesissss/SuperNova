using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lachesis.Core;
using UnityEngine;

namespace Lachesis.GamePlay
{
    public class GameEntry : MonoBehaviour
    {
        public static GameEntry instance;

        private static readonly List<GameModule> s_GameModules = new();
        
        public ProcedureType entranceProcedure; //初始流程
        
        public GameObject canvasRoot;
        
        private ProcedureBase m_EntranceProcedure;

        public ProcedureBase CurrentProcedure => ProcedureManager.CurrentProcedure;

        public float CurrentProcedureTime => ProcedureManager.CurrentProcedureTime;

        public static EventManager EventManager { get; private set; }

        public static ConfigManager ConfigManager { get; private set; }
        public static FSMManager FSMManager { get; private set; }

        public static EntityManager EntityManager { get; private set; }
        public static NetworkEntityManager NetworkEntityManager { get; private set; }
        public static AtlasManager AtlasManager { get; private set; }
        public static SkillManager SkillManager { get; private set; }
        public static NetworkSkillManager NetworkSkillManager { get; private set; }
        public static ProcedureManager ProcedureManager { get; private set; }

        public static PlayerInputManager PlayerInputManager { get; private set; }

        private void Awake()
        {
            instance = this;
            ProcedureManager = GetModule<ProcedureManager>();
            FSMManager = GetModule<FSMManager>();
            PlayerInputManager = GetModule<PlayerInputManager>();
            PlayerInputManager.Initialize();
            ConfigManager = GetModule<ConfigManager>();
            ConfigManager.Initialize();
            EntityManager = GetModule<EntityManager>();
            NetworkEntityManager = GetModule<NetworkEntityManager>();
            EntityManager.Initialize(ConfigManager.GetConfig<EntityConfig>()); //EntityManager应该在EventManager之前ShutDown，防止Unsubscribe不存在的事件Handler
            NetworkEntityManager.Initialize(ConfigManager.GetConfig<NetworkEntityConfig>()); 
            EventManager = GetModule<EventManager>(); //这里先简单处理一下，后面给GameMoudle加个priority
            AtlasManager = GetModule<AtlasManager>();
            SkillManager = GetModule<SkillManager>();
            NetworkSkillManager = GetModule<NetworkSkillManager>();
            

            if (ProcedureManager == null)
            {
                Debug.LogError("Procedure manager is invalid.");
                return;
            }

            if (EventManager == null)
            {
                Debug.LogError("Procedure manager is invalid.");
                return;
            }

            if (FSMManager == null) Debug.LogError("FSM manager is invalid.");
        }

        private IEnumerator Start()
        {
            //通过反射初始化流程，进入默认流程（主菜单）
            var procedureBases = new List<ProcedureBase>();
            var assembly = Assembly.GetExecutingAssembly();
            var subclassTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ProcedureBase)) && !t.IsAbstract);

            foreach (var type in subclassTypes)
            {
                var instance = (ProcedureBase)Activator.CreateInstance(type);
                procedureBases.Add(instance);
            }
            
            m_EntranceProcedure = (ProcedureBase)Activator.CreateInstance(entranceProcedure.GetPrecedureType());
            if (m_EntranceProcedure==null)
            {
                Debug.LogError("Entrance procedure is invalid.");
                yield break;
            }

            ProcedureManager.Initialize(GetModule<FSMManager>(), procedureBases);
            AtlasManager.SetConfig(ConfigManager.GetConfig<AtlasConfig>());
            SkillManager.Initialize(ConfigManager.GetConfig<SkillConfig>());
            NetworkSkillManager.Initialize(ConfigManager.GetConfig<NetworkSkillConfig>());
            yield return new WaitForEndOfFrame();

            ProcedureManager.StartProcedure(m_EntranceProcedure.GetType());
        }

        // 游戏主循环入口
        private void Update()
        {
            foreach (var module in s_GameModules) module.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
        
        private void FixedUpdate()
        {
            foreach (var module in s_GameModules) module.FixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            foreach (var module in s_GameModules) module.Shutdown();
        }

        private void OnApplicationQuit()
        {
            StopAllCoroutines();
        }

        /// <summary>
        ///     获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            var interfaceType = typeof(T);

            if (!interfaceType.FullName.StartsWith("Lachesis.", StringComparison.Ordinal))
            {
                Debug.LogError($"You must get a Lachesis game module, but '{interfaceType.FullName}' is not.");
                return null;
            }

            var moduleType = Type.GetType(interfaceType.FullName);
            if (moduleType == null)
            {
                Debug.LogError($"Can not find Game Framework module type '{interfaceType.FullName}'.");
                return null;
            }

            return GetModule(moduleType) as T;
        }

        public void GameStartCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        /// <summary>
        ///     获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private static GameModule GetModule(Type moduleType)
        {
            foreach (var module in s_GameModules)
                if (module.GetType() == moduleType)
                    return module;

            return CreateModule(moduleType);
        }

        /// <summary>
        ///     创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static GameModule CreateModule(Type moduleType)
        {
            var module = (GameModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                Debug.LogError($"Can not create module '{moduleType.FullName}'.");

                return null;
            }

            s_GameModules.Add(module);
            return module;
        }
    }
}