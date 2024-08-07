using System;
using UnityEngine;

public abstract class FSMState<T> where T : class
{
    /// <summary>
    ///     初始化有限状态机状态基类的新实例。
    /// </summary>
    public FSMState()
    {
    }

    public object userData;
    /// <summary>
    ///     有限状态机状态初始化时调用。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    protected internal virtual void OnInit(FSM<T> fsm)
    {
    }

    /// <summary>
    ///     有限状态机状态进入时调用。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    protected internal virtual void OnEnter(FSM<T> fsm)
    {
    }

    /// <summary>
    ///     有限状态机状态轮询时调用。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
    /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
    protected internal virtual void OnUpdate(FSM<T> fsm, float elapseSeconds, float realElapseSeconds)
    {
    }

    /// <summary>
    ///     有限状态机状态离开时调用。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
    protected internal virtual void OnLeave(FSM<T> fsm, bool isShutdown)
    {
    }

    /// <summary>
    ///     有限状态机状态销毁时调用。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    protected internal virtual void OnDestroy(FSM<T> fsm)
    {
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
    /// <param name="fsm">有限状态机引用。</param>
    protected void ChangeState<TState>(FSM<T> fsm, object userData = null) where TState : FSMState<T>
    {
        if (fsm == null)
        {
            Debug.LogError("FSM is invalid.");
            return;
        }

        fsm.ChangeState<TState>(userData);
    }

    /// <summary>
    ///     切换当前有限状态机状态。
    /// </summary>
    /// <param name="fsm">有限状态机引用。</param>
    /// <param name="stateType">要切换到的有限状态机状态类型。</param>
    protected void ChangeState(FSM<T> fsm, Type stateType, object userData = null)
    {
        if (fsm == null)
        {
            Debug.LogError("FSM is invalid.");
            return;
        }

        if (stateType == null)
        {
            Debug.LogError("stateType is invalid.");
            return;
        }

        if (!typeof(FSMState<T>).IsAssignableFrom(stateType))
        {
            Debug.LogError($"State type '{stateType.FullName}' is invalid.");
            return;
        }

        fsm.ChangeState(stateType, userData);
    }
}