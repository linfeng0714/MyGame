using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 工具类，存放一些通用的静态函数
    /// </summary>
    public class TimeUtil
    {
        /// <summary>
        /// 系统当前时间（毫秒）
        /// 1毫秒=1000微秒 1微秒=1000毫微秒（纳秒）Ticks是以100纳秒为间隔的间隔数
        /// </summary>
        public static long CurrentTimeMillis
        {
            get { return (long)(Time.time * 1000); }
            //get { return System.DateTime.Now.Ticks / 10000; }
        }

        /// <summary>
        /// 真实时间，忽略timeScale
        /// </summary>
        public static long CurrentRealTimeMillis
        {
            get { return (long)(Time.realtimeSinceStartup * 1000); }
        }
    }
}
