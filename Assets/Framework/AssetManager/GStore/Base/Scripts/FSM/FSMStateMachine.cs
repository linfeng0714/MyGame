using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GStore
{
    /// <summary>
    /// 有限状态机
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class FSMStateMachine<T, U> where T : class
    {
        private T m_Owner;
        FSMState<T, U> m_CurrentState;
        FSMState<T, U> m_PreviousState;
        FSMState<T, U> m_GlobalState;

        private Dictionary<U, FSMState<T, U>> m_stateRef;

        public FSMStateMachine(T entity)
        {
            m_Owner = entity;

            m_stateRef = new Dictionary<U, FSMState<T, U>>();
        }


        /// <summary>
        /// 更新状态机
        /// </summary>
        public void Update()
        {

            if (m_GlobalState != null)
                m_GlobalState.Execute(m_Owner);
            if (m_CurrentState != null)
                m_CurrentState.Execute(m_Owner);
        }

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="pNewState"></param>
        private void ChangeState(FSMState<T, U> pNewState)
        {
            m_PreviousState = m_CurrentState;

            if (m_PreviousState != null)
                m_PreviousState.Exit(m_Owner);

            m_CurrentState = pNewState;

            if (m_CurrentState != null)
                m_CurrentState.Enter(m_Owner);
        }

        /// <summary>
        /// 改变状态机状态
        /// </summary>
        /// <param name="stateID"></param>
        public void ChangeState(U stateID)
        {
            if (m_stateRef.ContainsKey(stateID))
            {
                FSMState<T, U> state = m_stateRef[stateID];
                ChangeState(state);
                return;
            }

            Debug.LogError("There is no state assiciated with that definition");
        }

        /// <summary>
        /// 恢复之前状态
        /// </summary>
        public void RevertToPreviousState()
        {
            if (m_PreviousState != null)
                ChangeState(m_PreviousState);
        }

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <returns></returns>
        public FSMState<T, U> GetCurrentState()
        {
            return m_CurrentState;
        }

        /// <summary>
        /// 是否处于某状态中
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public bool IsInState(U stateID)
        {
            if (m_CurrentState != null && m_CurrentState.StateID.Equals(stateID))
                return true;
            return false;
        }

        /// <summary>
        /// 设置全局状态
        /// </summary>
        /// <param name="globalState"></param>
        public void SetGlobalState(FSMState<T, U> globalState)
        {
            m_GlobalState = globalState;

            if (m_GlobalState != null)
                m_GlobalState.Enter(m_Owner);
        }

        /// <summary>
        /// 注册状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public FSMState<T, U> RegisterState(FSMState<T, U> state)
        {
            m_stateRef.Add(state.StateID, state);
            return state;
        }

        /// <summary>
        /// 卸载状态
        /// </summary>
        /// <param name="stateID"></param>
        public void UnRegisterState(U stateID)
        {
            if (m_stateRef.ContainsKey(stateID))
                m_stateRef.Remove(stateID);
        }
    }
}