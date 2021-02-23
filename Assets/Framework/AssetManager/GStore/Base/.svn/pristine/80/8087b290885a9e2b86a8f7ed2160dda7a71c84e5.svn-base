using UnityEngine;
using System;

/// <summary>
/// 游戏中的各种时间
/// </summary>
public static class WTime
{
    /// <summary>
    /// 上次的服务器时间戳
    /// </summary>
    public static long prev_server_time_ticks;

    /// <summary>
    /// 服务器时间戳
    /// </summary>
    /// 
    public static long server_time_ticks;
    public static long ServerTimeTicks
    {
        set {
            //获得服务器时间戳 的时间点
            setServerTimeTickTime = DateTime.Now;
            
            serverTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区

            serverTime = serverTime.AddMilliseconds(value);

            if (prev_server_time_ticks == 0)//第一次
            {
                prev_server_time_ticks = value;
            }
            else
            {
                prev_server_time_ticks = server_time_ticks;
            }

            server_time_ticks = value;

            clienttime = server_time_ticks;
        }
        get { return server_time_ticks; }
    }


    private static DateTime serverTime;

    /// <summary>
    /// 收到新服务器本地时间
    /// </summary>
    public static DateTime setServerTimeTickTime;

    public static long clienttime;
    /// <summary>
    /// 获取日期
    /// </summary>
    /// <param name="_delta_times"></param>
    /// <returns></returns>
    public static DateTime GetTime(long _delta_times) {
        DateTime _temp = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
        _temp = _temp.AddMilliseconds(_delta_times);
        return _temp;
    }

    /// <summary>
    /// 获取服务器时间
    /// </summary>
    public static DateTime ServerTime
    {
        get {

                TimeSpan _delta = DateTime.Now.Subtract(setServerTimeTickTime).Duration();
            
                DateTime _time = serverTime.AddMilliseconds(_delta.TotalMilliseconds);
            
                return _time;

        }
    }

    /// <summary>
    /// 获得倒数
    /// </summary>
    /// <param name="_delta_times"></param>
    /// <returns></returns>
    public static TimeSpan GetDeltaTimeSpan(long _delta_times)
    {

        DateTime cdTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区

        cdTime = cdTime.AddMilliseconds(_delta_times);
        
        TimeSpan _times = cdTime - ServerTime;// cdTime.Subtract(ServerTime).Duration();

        //Debug.Log(cdTime.ToLongTimeString() +"  " + ServerTime.ToLongTimeString() + "  "+ _times.TotalSeconds);

        return _times;
    }

    /// <summary>
    /// 系统时间，但是会受到客户端修改时间影响
    /// </summary>
    public static long SystemTimeTicks
    {
        get
        {
            return System.DateTime.Now.Ticks;
        }
    }

    /// <summary>
    /// 从游戏运行开始，经过的时间，不受timeScale影响；也不受修改客户端时间影响；也不受暂停游戏影响（暂停游戏realtimeSinceStartup继续计算）
    /// </summary>
    public static float RealtimeSinceStartup
    {
        get
        {
            return Time.realtimeSinceStartup;
        }
    }

    public static long RealtimeSinceStartupLong
    {
        get
        {
            return (long)(Time.realtimeSinceStartup * 1000);
        }
    }

    /// <summary>
    /// 帧同步设置的系统时间
    /// </summary>
    public static float FrameSystemTime
    {
        get
        {
            return curRealtimeSinceStartup;
        }
    }

    /// <summary>
    /// 跟realtimeSinceStartup类似，唯一区别是unscaledTime在每帧里不变，而realtimeSinceStartup实时变化
    /// </summary>
    public static float unscaledTime
    {
        get
        {
            return Time.unscaledTime;
        }
    }
    
    public static float update_server_time_scale = -1f;

    private static float curDeltaTime = 0;
    private static float curRealtimeSinceStartup = 0;
    /// <summary>
    /// 上一帧运行花费的时间
    /// </summary>
    public static float deltaTime
    {
        get
        {
            return curDeltaTime;
        }
    }

    public static void SetWTime(float deltaTime)
    {
        curDeltaTime = deltaTime;
        curRealtimeSinceStartup = Time.realtimeSinceStartup;
        clienttime += (long)(deltaTime*1000);
        //if (update_server_time_scale != -1f)
        //{
        //    update_server_time_scale += deltaTime;

        //    //每分钟刷新服务器时间戳
        //    if (update_server_time_scale > 60f)
        //    {
        //        update_server_time_scale = 0f;

        //        Proto_JavaNet.C2SHeartBeatMsg _data = new Proto_JavaNet.C2SHeartBeatMsg();

        //        WorldHandlerMessage.SendHeartbeat(_data);
        //    }
        //}

    }

    #region 逻辑时间
    public static void ResetLogicTime()
    {
        _logicTime = 0;
    }
    public static void UpdateLogicTime(int _mills_delta_time)
    {
        _logicTime += _mills_delta_time;
    }
    private static long _logicTime = 0;
    public static long logicTime
    {
        get { return _logicTime; }
    }

//     public static long RealTimeOrLogicTime
//     {
//         get { return BattleFrameSyncManager.Instance.IsActive ? logicTime : (long)(Time.realtimeSinceStartup * 1000); }
//     }

    #endregion

    /// <summary>
    /// 上一帧运行花费的时间 （不受timeScale影响）
    /// </summary>
    public static float unscaledDeltaTime
    {
        get
        {
            return Time.unscaledDeltaTime;
        }
    }

    /// <summary>
    /// 时间缩放参数；
    /// 会整体影响Time.deltaTime的传入
    /// 值越大，游戏越快，Time.deltaTime越大；
    /// 值越小，游戏越慢，Time.deltaTime越小；
    /// </summary>
    public static float timeScale
    {
        get
        {
            return Time.timeScale;
        }
        set
        {
            Time.timeScale = value;
        }
    }

    private static int mapTimeScale = 0;
    /// <summary>
    /// 地图时间缩放参数，主要用在战斗以及大地图里
    /// </summary>
    /// <value>对应值＝(mapTimeScale*0.001f)倍数</value>
    public static int MapTimeScale
    {
        get
        {
            return mapTimeScale;
        }
        set
        {
            mapTimeScale = value;
        }
    }

    /// <summary>
    /// 当场景跳转时，清空一些时间变量
    /// </summary>
    public static void CleanWhenStateChange()
    {
        mapTimeScale = 0;
    }
}
