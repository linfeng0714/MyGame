using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GStore
{
    public class CoroutineRunner : SingletonMono<CoroutineRunner>
    {
        /// <summary>
        /// 全局启动协程
        /// </summary>
        /// <param name="function"></param>
        public static Coroutine Run(IEnumerator function)
        {
            if (Application.isPlaying)
            {
                return Instance.StartCoroutine(function);
            }
            else
            {
#if UNITY_EDITOR
                //处理编辑器没有运行的情况
                EditorCoroutineRunner.StartEditorCoroutine(function);
#endif
                return null;
            }
        }

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <param name="function"></param>
        public static void Wait(IEnumerator function)
        {
            while (function.MoveNext())
            {
                if (function.Current != null)
                {
                    var itor = function.Current as IEnumerator;
                    if (itor != null)
                    {
                        Wait(itor);
                    }
                    else
                    {
                        // Skip WaitForSeconds, WaitForEndOfFrame and WaitForFixedUpdate
                        return;
                    }
                }
            }
        }
    }
}
