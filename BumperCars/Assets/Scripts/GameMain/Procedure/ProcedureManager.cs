using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lachesis.Core
{
    public sealed class ProcedureManager : GameModule
    {
        private FSMManager m_FsmManager;
        private FSM<ProcedureManager> m_ProcedureFsm;

        /// <summary>
        ///     初始化流程管理器的新实例。
        /// </summary>
        public ProcedureManager()
        {
            m_FsmManager = null;
            m_ProcedureFsm = null;
        }

        /// <summary>
        ///     获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

                return (ProcedureBase)m_ProcedureFsm.CurrentState;
            }
        }

        /// <summary>
        ///     获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

                return m_ProcedureFsm.CurrentStateTime;
            }
        }

        /// <summary>
        ///     流程管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void FixedUpdate(float fixedElapseSeconds)
        {
            
        }

        /// <summary>
        ///     关闭并清理流程管理器。
        /// </summary>
        internal override void Shutdown()
        {
            if (m_FsmManager != null)
            {
                if (m_ProcedureFsm != null)
                {
                    m_FsmManager.DestroyFsm(m_ProcedureFsm);
                    m_ProcedureFsm = null;
                }

                m_FsmManager = null;
            }
        }

        /// <summary>
        ///     初始化流程管理器。
        /// </summary>
        /// <param name="fsmManager">有限状态机管理器。</param>
        /// <param name="procedures">流程管理器包含的流程。</param>
        public void Initialize(FSMManager fsmManager, params ProcedureBase[] procedures)
        {
            if (fsmManager == null)
            {
                Debug.LogError("FSM manager is invalid.");
                return;
            }

            m_FsmManager = fsmManager;
            m_ProcedureFsm = m_FsmManager.CreateFsm(this, procedures);
        }

        public void Initialize(FSMManager fsmManager, List<ProcedureBase> procedures)
        {
            Initialize(fsmManager, procedures.ToArray());
        }

        /// <summary>
        ///     开始流程。
        /// </summary>
        /// <typeparam name="T">要开始的流程类型。</typeparam>
        public void StartProcedure<T>() where T : ProcedureBase
        {
            if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

            m_ProcedureFsm.Start<T>();
        }

        /// <summary>
        ///     开始流程。
        /// </summary>
        /// <param name="procedureType">要开始的流程类型。</param>
        public void StartProcedure(Type procedureType)
        {
            if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

            m_ProcedureFsm.Start(procedureType);
        }


        /// <summary>
        ///     获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

            return m_ProcedureFsm.GetState<T>();
        }

        /// <summary>
        ///     获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            if (m_ProcedureFsm == null) Debug.LogError("You must initialize procedure first.");

            return (ProcedureBase)m_ProcedureFsm.GetState(procedureType);
        }
    }
}