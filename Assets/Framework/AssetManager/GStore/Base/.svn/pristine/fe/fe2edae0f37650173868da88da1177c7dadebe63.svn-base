using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GStore
{
    /// <summary>
    /// Application工具类
    /// </summary>
    public static class ApplicationUtils
    {
        /// <summary>
        /// 封装UnityEngine.Application.isPlaying，可在多线程中访问。
        /// </summary>
        public static bool IsPlaying { get; private set; }

        static ApplicationUtils()
        {
            IsPlaying = UnityEngine.Application.isPlaying;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        private static void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange obj)
        {
            IsPlaying = UnityEngine.Application.isPlaying;
        }
#endif
    }
}
