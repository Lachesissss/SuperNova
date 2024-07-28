using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.Core
{
    public sealed class FSMManager : GameModule
    {
        private readonly Dictionary<TypeNamePair, FSMBase> m_Fsms;
        private readonly List<FSMBase> m_TempFsms;

        /// <summary>
        ///     初始化有限状态机管理器的新实例。
        /// </summary>
        public FSMManager()
        {
            m_Fsms = new Dictionary<TypeNamePair, FSMBase>();
            m_TempFsms = new List<FSMBase>();
        }

        /// <summary>
        ///     有限状态机管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_TempFsms.Clear();
            if (m_Fsms.Count <= 0) return;

            foreach (var fsm in m_Fsms) m_TempFsms.Add(fsm.Value);

            foreach (var fsm in m_TempFsms)
            {
                if (fsm.IsDestroyed) continue;

                fsm.Update(elapseSeconds, realElapseSeconds);
            }
        }

        internal override void Shutdown()
        {
            foreach (var fsm in m_Fsms) fsm.Value.Shutdown();

            m_Fsms.Clear();
            m_TempFsms.Clear();
        }

        public bool HasFsm(TypeNamePair tnPair)
        {
            return m_Fsms.ContainsKey(tnPair);
        }

        public FSMBase GetFsm(TypeNamePair tnPair)
        {
            FSMBase fsm = null;
            if (m_Fsms.TryGetValue(tnPair, out fsm)) return fsm;

            return null;
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>(FSM<T> fsm) where T : class
        {
            if (fsm == null) Debug.LogError("FSM is invalid.");

            return DestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
        }

        private bool DestroyFsm(TypeNamePair tnPair)
        {
            FSMBase fsm = null;
            if (m_Fsms.TryGetValue(tnPair, out fsm))
            {
                fsm.Shutdown();
                return m_Fsms.Remove(tnPair);
            }

            return false;
        }

        public bool DestroyFsm(Type ownerType)
        {
            if (ownerType == null) Debug.LogError("Owner type is invalid.");

            return DestroyFsm(new TypeNamePair(ownerType));
        }

        public bool DestroyFsm<T>() where T : class
        {
            return DestroyFsm(new TypeNamePair(typeof(T)));
        }

        public FSM<T> CreateFsm<T>(T owner, params FSMState<T>[] states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }


        public FSM<T> CreateFsm<T>(string name, T owner, params FSMState<T>[] states) where T : class
        {
            var typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name)) Debug.LogError($"Already exist FSM '{typeNamePair}'.");

            var fsm = FSM<T>.Create(name, owner, states);
            m_Fsms.Add(typeNamePair, fsm);
            return fsm;
        }

        public FSM<T> CreateFsm<T>(string name, T owner, List<FSMState<T>> states) where T : class
        {
            var typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                Debug.LogError($"Already exist FSM '{typeNamePair}'.");
                return null;
            }

            var fsm = FSM<T>.Create(name, owner, states);
            m_Fsms.Add(typeNamePair, fsm);
            return fsm;
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return InternalHasFsm(new TypeNamePair(typeof(T), name));
        }

        private bool InternalHasFsm(TypeNamePair typeNamePair)
        {
            return m_Fsms.ContainsKey(typeNamePair);
        }
    }
}