using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

// 开启GSTORE_DEBUG_OBJECT_POOL宏可追踪调用堆栈，会产生大量GC，所以只在查问题时使用

namespace GStore.ObjectPool
{
    public interface IObjectPool
    {
        /// <summary>
        /// 所有已分配的个数
        /// </summary>
        int countAll { get; }

        /// <summary>
        /// 所有正在使用的个数
        /// </summary>
        int countActive { get; }

        /// <summary>
        /// 缓存中可用的列表
        /// </summary>
        int countInactive { get; }

        #region 检查工具
#if UNITY_EDITOR
#if GSTORE_DEBUG_OBJECT_POOL

        /// <summary>
        /// 所有调用栈信息
        /// </summary>
        List<System.Diagnostics.StackTrace> GetDebugTraceList();
#endif
#endif
    #endregion
    }

    public class ObjectPool<T> : IObjectPool where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        #region 检查工具
#if UNITY_EDITOR
#if GSTORE_DEBUG_OBJECT_POOL

        /// <summary>
        /// 所有调用栈信息
        /// </summary>
        private readonly Dictionary<T, System.Diagnostics.StackTrace> debug_trace_dict = new Dictionary<T, System.Diagnostics.StackTrace>();

        /// <summary>
        /// 所有调用栈信息
        /// </summary>
        public List<System.Diagnostics.StackTrace> GetDebugTraceList()
        {
            return debug_trace_dict.Values.ToList();
        }
#endif

#endif
        #endregion

        /// <summary>
        /// 所有已分配的个数
        /// </summary>
        public int countAll { get; private set; }

        /// <summary>
        /// 所有正在使用的个数
        /// </summary>
        public int countActive { get { return countAll - countInactive; } }

        /// <summary>
        /// 缓存中可用的列表
        /// </summary>
        public int countInactive { get { return m_Stack.Count; } }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="actionOnGet"></param>
        /// <param name="actionOnRelease"></param>
        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        /// <summary>
        /// 申请临时对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = new T();
                countAll++;
#if UNITY_EDITOR
                if(countAll == 1)
                {
                    PoolChecker.CheckObjectPool(this);
                }
#endif
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                m_ActionOnGet(element);

#if UNITY_EDITOR
            // 获取信息

#if GSTORE_DEBUG_OBJECT_POOL
            // 下面这句会造成大量GC，所以在平时不开启，需要查问题时才开启！！！！
            debug_trace_dict[element] = new System.Diagnostics.StackTrace(2, true);
#endif

#endif

            return element;
        }

        /// <summary>
        /// 释放临时对象
        /// </summary>
        /// <param name="element"></param>
        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (m_ActionOnRelease != null)
                m_ActionOnRelease(element);
            m_Stack.Push(element);

#if UNITY_EDITOR
            // 删除信息
#if GSTORE_DEBUG_OBJECT_POOL
            debug_trace_dict.Remove(element);
#endif

#endif
        }
    }
}
