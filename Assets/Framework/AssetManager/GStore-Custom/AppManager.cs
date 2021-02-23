using System;
using UnityEngine;
using GStore;
using System.Collections.Generic;

/// <summary>
/// 游戏启动类
/// </summary>
public class AppManager : SingletonMono<AppManager>
{
    public void Awake()
    {
        Camera camera = FindObjectOfType(typeof(Camera)) as Camera;
        UnityEngine.Object.DontDestroyOnLoad(camera);
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
#if SHOW_FPS
       if (!gameObject.GetComponent<ShowFPS>())
        {
            gameObject.AddComponent<ShowFPS>();
        }
#endif

#if ENABLE_G_PROFILER
        UnityEngine.Profiling.Profiler.enabled = true;
#else
        UnityEngine.Profiling.Profiler.enabled = false;
#endif

        BaseSetup.Setup();
    }

    /// <summary>
    /// 只有Android会监听到该方法
    /// </summary>
    /// <param name="focus"></param>
    public void OnApplicationFocus(bool focus)
    {
    }

    /// <summary>
    /// IOS和Android Home键和锁屏键 都会监听到该方法
    /// </summary>
    /// <param name="pauseStatus">true:表示锁屏(按Home键) false:表示解锁</param>
    void OnApplicationPause(bool pauseStatus)
    {
        try
        {
            //Launcher.OnPause(pauseStatus);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("app OnApplicationPause exception: " + ex.Message + ex.StackTrace);
        }
    }

    /// <summary>
    /// Note that iOS applications are usually suspended and do not quit. 
    /// You should tick "Exit on Suspend" in Player settings for iOS builds to cause the game to quit and not suspend, 
    /// otherwise you may not see this call. If "Exit on Suspend" is not ticked then you will see calls to OnApplicationPause instead.
    /// </summary>
    void OnApplicationQuit()
    {
        try
        {
            Application.Quit();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("app OnApplicationQuit exception: " + ex.Message + " " + ex.StackTrace);
        }
    }

}
