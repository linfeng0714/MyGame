using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GStore
{
    /// <summary>
    /// MonoBehaviour事件回调钩子
    /// </summary>
    public class MonoBehaviourCallbackHooks : MonoBehaviour
    {
        /// <summary>
        /// MonoBehaviour回调
        /// </summary>
        private static MonoBehaviourCallbackHooks s_Instance;
        private static MonoBehaviourCallbackHooks Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new GameObject("MonoBehaviourCallbackHooks").AddComponent<MonoBehaviourCallbackHooks>();
                    DontDestroyOnLoad(s_Instance);
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// Update事件
        /// </summary>
        public static UnityEvent UpdateEvent
        {
            get
            {
                return Instance.updateEvent;
            }
        }

        /// <summary>
        /// LateUpdate事件
        /// </summary>
        public static UnityEvent LateUpdateEvent
        {
            get
            {
                return Instance.lateUpdateEvent;
            }
        }

        /// <summary>
        /// 暂停事件
        /// </summary>
        public static UnityEvent<bool> ApplicationPauseEvent
        {
            get
            {
                return Instance.onApplicationPauseEvent;
            }
        }

        /// <summary>
        /// 退出事件
        /// </summary>
        public static UnityEvent ApplicationQuitEvent
        {
            get
            {
                return Instance.onApplicationQuitEvent;
            }
        }

        /// <summary>
        /// 事件列表
        /// </summary>
        private UnityEvent updateEvent = new UnityEvent();
        private UnityEvent lateUpdateEvent = new UnityEvent();
        private UnityEvent<bool> onApplicationPauseEvent = new UnityBoolEvent();
        private UnityEvent onApplicationQuitEvent = new UnityEvent();

        private class UnityBoolEvent : UnityEvent<bool> { }

        private void Update()
        {
            updateEvent.Invoke();
        }

        private void LateUpdate()
        {
            lateUpdateEvent.Invoke();
        }

        private void OnApplicationPause(bool pause)
        {
            onApplicationPauseEvent.Invoke(pause);
        }

        private void OnApplicationQuit()
        {
            onApplicationQuitEvent.Invoke();
        }
    }
}
