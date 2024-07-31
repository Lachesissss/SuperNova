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

        public string entranceProcedureTypeName; //初始流程
        
        public GameObject canvasRoot;
        
        private ProcedureBase m_EntranceProcedure;

        public ProcedureBase CurrentProcedure => ProcedureManager.CurrentProcedure;

        public float CurrentProcedureTime => ProcedureManager.CurrentProcedureTime;

        public static EventManager EventManager { get; private set; }

        public static ConfigManager ConfigManager { get; private set; }
        public static FSMManager FSMManager { get; private set; }

        public static EntityManager EntityManager { get; private set; }
        public static AtlasManager AtlasManager { get; private set; }
        public static SkillManager SkillManager { get; private set; }
        public static ProcedureManager ProcedureManager { get; private set; }

        public static PlayerInputManager PlayerInputManager { get; private set; }

        private void Awake()
        {
            instance = this;
            ProcedureManager = GetModule<ProcedureManager>();
            FSMManager = GetModule<FSMManager>();
            EventManager = GetModule<EventManager>();
            PlayerInputManager = GetModule<PlayerInputManager>();
            PlayerInputManager.Initialize();
            ConfigManager = GetModule<ConfigManager>();
            ConfigManager.Initialize();
            EntityManager = GetModule<EntityManager>();
            EntityManager.SetConfig(ConfigManager.GetConfig<EntityConfig>());
            AtlasManager = GetModule<AtlasManager>();
            AtlasManager.SetConfig(ConfigManager.GetConfig<AtlasConfig>());
            SkillManager = GetModule<SkillManager>();
            SkillManager.Initialize();

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

            var isEntranceProcedureValid = false;
            foreach (var procedureBase in procedureBases)
            {
                var type = procedureBase.GetType();
                var fullName = $"{type.Namespace}.{type.Name}";
                if (entranceProcedureTypeName == fullName)
                {
                    m_EntranceProcedure = procedureBase;
                    isEntranceProcedureValid = true;
                }
            }

            if (!isEntranceProcedureValid)
            {
                Debug.LogError("Entrance procedure is invalid.");
                yield break;
            }

            ProcedureManager.Initialize(GetModule<FSMManager>(), procedureBases);

            yield return new WaitForEndOfFrame();

            ProcedureManager.StartProcedure(m_EntranceProcedure.GetType());
        }

        // 游戏主循环入口
        private void Update()
        {
            foreach (var module in s_GameModules) module.Update(Time.deltaTime, Time.unscaledDeltaTime);
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