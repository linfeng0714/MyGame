using UnityEngine;
using System.Collections;

namespace GStore
{
    /// <summary>
    /// FSM有限状态机状态
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public abstract class FSMState<T, U>  where T : class
    {

        /// <summary>
        /// 状态ID
        /// </summary>
        public abstract U StateID { get; }

        /// <summary>
        /// 进入状态
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Enter(T entity) { }

        /// <summary>
        /// 轮询状态
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Execute(T entity) { }

        /// <summary>
        /// 退出状态
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Exit(T entity) { }


    }
}