#if UNITY_EDITOR || ENABLE_G_PROFILER
#if !UNITY_EDITOR && UNITY_ANDROID
        //#define UWA_PROFILER
#else
#define UNITY_PROFILER
#endif
#endif

using System.Diagnostics;

/// <summary>
/// Profile工具封装
/// </summary>
public static class ProfilerTool
{
    /// <summary>
    /// 记录主线程id
    /// </summary>
    private static int s_main_thread_id;

    /// <summary>
    /// 静态构造
    /// </summary>
    static ProfilerTool()
    {
#if ENABLE_G_PROFILER
        UnityEngine.Profiling.Profiler.enabled = true;
#else
        UnityEngine.Profiling.Profiler.enabled = false;
#endif
        s_main_thread_id = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// 检测当前调用堆栈是否在主线程
    /// </summary>
    public static bool IsCallingFromMainThread
    {
        get { return System.Threading.Thread.CurrentThread.ManagedThreadId == s_main_thread_id; }
    }

    [Conditional("ENABLE_G_PROFILER")]
    public static void BeginSample(string _sample_name, ProfilerFlags _flag = ProfilerFlags.ALL)
    {
#if UNITY_PROFILER
        if ((_flag & ProfilerFlags.UNITY_PROFILER) == 0)
        {
            return;
        }
        if (IsCallingFromMainThread == false)
        {
            return;
        }
        UnityEngine.Profiling.Profiler.BeginSample(_sample_name);

#elif UWA_PROFILER
        if ((_flag & ProfilerFlags.UWA_PROFILER) == 0)
        {
            return;
        }
        if (IsCallingFromMainThread == false)
        {
            return;
        }
        UWAEngine.PushSample(_sample_name);
#endif
    }

    [Conditional("ENABLE_G_PROFILER")]
    public static void EndSample(ProfilerFlags _flag = ProfilerFlags.ALL)
    {
#if UNITY_PROFILER
        if ((_flag & ProfilerFlags.UNITY_PROFILER) == 0)
        {
            return;
        }
        if (IsCallingFromMainThread == false)
        {
            return;
        }
        UnityEngine.Profiling.Profiler.EndSample();
#elif UWA_PROFILER
        if ((_flag & ProfilerFlags.UWA_PROFILER) == 0)
        {
            return;
        }
        if (IsCallingFromMainThread == false)
        {
            return;
        }
        UWAEngine.PopSample();
#endif
    }

    [Conditional("ENABLE_G_PROFILER")]
    public static void BeginSampleNoCheck(string _sample_name)
    {
#if UNITY_PROFILER

        UnityEngine.Profiling.Profiler.BeginSample(_sample_name);

#elif UWA_PROFILER
        
        UWAEngine.PushSample(_sample_name);        
#endif
    }

    [Conditional("ENABLE_G_PROFILER")]
    public static void EndSampleNoCheck()
    {
#if UNITY_PROFILER
        UnityEngine.Profiling.Profiler.EndSample();
#elif UWA_PROFILER
        
        UWAEngine.PopSample();
#endif
    }

    public enum ProfilerFlags
    {
        UNITY_PROFILER = 1,//启用Unity Profiler
        UWA_PROFILER = 2,//启用UWA Profiler

        ALL = 3,//启用全部
    }
}
