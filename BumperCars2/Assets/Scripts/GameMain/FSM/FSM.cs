using System;
using System.Collections.Generic;
using UnityEngine;

public class FSM<T> : FSMBase where T : class
{
    private readonly Dictionary<Type, FSMState<T>> m_States;
    private float m_CurrentStateTime;
    private bool m_IsDestroyed;

    public FSM()
    {
        Owner = null;
        m_States = new Dictionary<Type, FSMState<T>>();
        CurrentState = null;
        m_CurrentStateTime = 0f;
        m_IsDestroyed = true;
    }

    /// <summary>
    ///     获取有限状态机持有者。
    /// </summary>
    public T Owner { get; private set; }

    /// <summary>
    ///     获取有限状态机持有者类型。
    /// </summary>
    public override Type OwnerType => typeof(T);

    /// <summary>
    ///     获取有限状态机中状态的数量。
    /// </summary>
    public override int FsmStateCount => m_States.Count;

    /// <summary>
    ///     获取有限状态机是否正在运行。
    /// </summary>
    public override bool IsRunning => CurrentState != null;

    /// <summary>
    ///     获取有限状态机是否被销毁。
    /// </summary>
    public override bool IsDestroyed => m_IsDestroyed;

    /// <summary>
    ///     获取当前有限状态机状态。
    /// </summary>
    public FSMState<T> CurrentState { get; private set; }

    /// <summary>
    ///     获取当前有限状态机状态名称。
    /// </summary>
    public override string CurrentStateName => CurrentState != null ? CurrentState.GetType().FullName : null;

    /// <summary>
    ///     获取当前有限状态机状态持续时间。
    /// </summary>
    public override float CurrentStateTime => m_CurrentStateTime;


    /// <summary>
    ///     创建有限状态机。
    /// </summary>
    /// <param name="name">有限状态机名称。</param>
    /// <param name="owner">有限状态机持有者。</param>
    /// <param name="states">有限状态机状态集合。</param>
    /// <returns>创建的有限状态机。</returns>
    public static FSM<T> Create(string name, T owner, List<FSMState<T>> states)
    {
        if (owner == null)
        {
            Debug.LogError("FSM owner is invalid.");
            return null;
        }

        if (states == null || states.Count < 1)
        {
            Debug.LogError("FSM states is invalid.");
            return null;
        }

        var fsm = new FSM<T>();
        fsm.Name = name;
        fsm.Owner = owner;
        fsm.m_IsDestroyed = false;
        foreach (var state in states)
        {
            if (state == null)
            {
                Debug.LogError("FSM states is invalid.");
                return null;
            }

            var stateType = state.GetType();
            if (fsm.m_States.ContainsKey(stateType)) Debug.LogError($"FSM '{new TypeNamePair(typeof(T), name)}' state '{stateType}' is already exist.");

            fsm.m_States.Add(stateType, state);
            state.OnInit(fsm);
        }

        return fsm;
    }

    public static FSM<T> Create(string name, T owner, params FSMState<T>[] states)
    {
        if (owner == null)
        {
            Debug.LogError("FSM owner is invalid.");
            return null;
        }

        if (states == null || states.Length < 1)
        {
            Debug.LogError("FSM states is invalid.");
            return null;
        }

        var fsm = new FSM<T>();
        fsm.Name = name;
        fsm.Owner = owner;
        fsm.m_IsDestroyed = false;
        foreach (var state in states)
        {
            if (state == null)
            {
                Debug.LogError("FSM states is invalid.");
                return null;
            }

            var stateType = state.GetType();
            if (fsm.m_States.ContainsKey(stateType)) Debug.LogError($"FSM '{new TypeNamePair(typeof(T), name)}' state '{stateType}' is already exist.");

            fsm.m_States.Add(stateType, state);
            state.OnInit(fsm);
        }

        return fsm;
    }

    /// <summary>
    ///     清理有限状态机。
    /// </summary>
    public void Clear()
    {
        if (CurrentState != null) CurrentState.OnLeave(this, true);

        foreach (var state in m_States) state.Value.OnDestroy(this);

        Name = null;
        Owner = null;
        m_States.Clear();

        CurrentState = null;
        m_CurrentStateTime = 0f;
        m_IsDestroyed = true;
    }

    /// <summary>
    ///     开始有限状态机。
    /// </summary>
    /// <typeparam name="TState">要开始的有限状态机状态类型。</typeparam>
    public void Start<TState>() where TState : FSMState<T>
    {
        if (IsRunning)
        {
            Debug.LogError("FSM 正在运行，无法重复Start");
            return;
        }

        FSMState<T> state = GetState<TState>();
        if (state == null) Debug.LogError($"FSM '{new TypeNamePair(typeof(T), Name)}' can not start state '{typeof(TState).FullName}' which is not exist.");

        m_CurrentStateTime = 0f;
        CurrentState = state;
        CurrentState.OnEnter(this);
    }

    public void Start(Type stateType)
    {
        if (IsRunning)
        {
            Debug.LogError("FSM 正在运行，无法重复Start");
            return;
        }

        if (stateType == null)
        {
            Debug.LogError("State type is invalid.");
            return;
        }

        if (!typeof(FSMState<T>).IsAssignableFrom(stateType))
        {
            Debug.LogError($"State type '{stateType.FullName}' is invalid.");
            return;
        }

        var state = GetState(stateType);
        if (state == null)
        {
            Debug.LogError($"FSM '{new TypeNamePair(typeof(T), Name)}' can not start state '{stateType.FullName}' which is not exist.");
            return;
        }

        m_CurrentStateTime = 0f;
        CurrentState = state;
        CurrentState.OnEnter(this);
    }

    public TState GetState<TState>() where TState : FSMState<T>
    {
        FSMState<T> state = null;
        if (m_States.TryGetValue(typeof(TState), out state)) return (TState)state;

        return null;
    }

    public FSMState<T>[] GetAllStates()
    {
        var index = 0;
        var results = new FSMState<T>[m_States.Count];
        foreach (var state in m_States) results[index++] = state.Value;

        return results;
    }

    /// <summary>
    ///     有限状态机轮询。
    /// </summary>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    internal override void Update(float elapseSeconds, float realElapseSeconds)
    {
        if (CurrentState == null) return;

        m_CurrentStateTime += elapseSeconds;
        CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
    }

    /// <summary>
    ///     关闭并清理有限状态机。
    /// </summary>
    internal override void Shutdown()
    {
    }

    internal void ChangeState<TState>(object userData = null) where TState : FSMState<T>
    {
        ChangeState(typeof(TState), userData);
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <param name="stateType">要切换到的有限状态机状态类型。</param>
    internal void ChangeState(Type stateType, object userData = null)
    {
        if (CurrentState == null) Debug.LogError("Current state is invalid.");

        var state = GetState(stateType);
        if (state == null) Debug.LogError($"FSM '{new TypeNamePair(typeof(T), Name)}' can not change state to '{stateType.FullName}' which is not exist.");

        CurrentState.OnLeave(this, false);
        m_CurrentStateTime = 0f;
        CurrentState = state;
        state.userData = userData;
        CurrentState.OnEnter(this);
    }

    /// <summary>
    ///     获取有限状态机状态。
    /// </summary>
    /// <param name="stateType">要获取的有限状态机状态类型。</param>
    /// <returns>要获取的有限状态机状态。</returns>
    public FSMState<T> GetState(Type stateType)
    {
        if (stateType == null) Debug.LogError("State type is invalid.");

        if (!typeof(FSMState<T>).IsAssignableFrom(stateType)) Debug.LogError($"State type '{stateType.FullName}' is invalid.");

        FSMState<T> state = null;
        if (m_States.TryGetValue(stateType, out state)) return state;

        return null;
    }
}