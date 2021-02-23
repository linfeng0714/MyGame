using UnityEngine;
using System.Runtime.InteropServices;
/// <summary>
/// Unity与IOS交互类
/// </summary>
namespace GStore
{
    public class IOSInterface
    {
#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern long _GetFreeStorage();
#endif

        /// <summary>
        /// 获取设备可用空间
        /// </summary>
        public static long GetFreeStorage()
        {
#if UNITY_IPHONE
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return _GetFreeStorage();
        }
#endif
            return int.MaxValue;
        }

#if UNITY_IPHONE
    //zh-cn
    //[DllImport("__Internal")]
    //public static extern string W2DeviceLanguageName();
#endif

#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern bool _GetPhoneXModel();
#endif

        /// <summary>
        /// 判断设备是不是iPhone X
        /// </summary>
        /// <returns></returns>
        public static bool GetPhoneXModel()
        {
#if UNITY_IPHONE
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return _GetPhoneXModel();
        }
#endif
            return false;
        }
    } 
}
