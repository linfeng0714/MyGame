using GStore;

namespace GStore
{
    /// <summary>
    /// 工具类，存放一些通用的静态函数
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 判断集合是否为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsCollectionNullOrEmpty(System.Collections.ICollection collection)
        {
            return collection == null || collection.Count <= 0;
        }

        /// <summary>
        /// Resources路径转换成AssetPath
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string ToAssetPath(string resourcesPath, string suffix)
        {
            return string.Format("{0}{1}{2}", AssetPathDefine.resFolder, resourcesPath, suffix);
        }

        public static void OnCallBack(ObjectCallback callBack, UnityEngine.Object asset, bool isOld)
        {
            if (callBack != null)
            {
                callBack(asset, isOld);
            }
        }

        public static void OnCallBack(ObjectCallback callBack, UnityEngine.Object asset, IsObjectOldFunc func)
        {
            if (callBack != null)
            {
                bool isOld = func != null && func();
                callBack(asset, isOld);
            }
        }

        public static void OnCallBack(GameObjectCallback callBack, UnityEngine.GameObject go, bool isOld)
        {
            if (callBack != null)
            {
                callBack(go, isOld);
            }
        }

        public static void OnCallBack(GameObjectCallback callBack, UnityEngine.GameObject go, IsObjectOldFunc func)
        {
            if (callBack != null)
            {
                bool isOld = func != null && func();
                callBack(go, isOld);
            }
        }
    }

    /// <summary>
    /// 异步加载的对象是否已经过期
    /// </summary>
    /// <returns></returns>
    public delegate bool IsObjectOldFunc();

    /// <summary>
    /// 资源加载回调
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="asset"></param>
    /// <param name="isOld"></param>
    public delegate void ObjectCallback(UnityEngine.Object asset, bool isOld);

    /// <summary>
    /// GameObject回调
    /// </summary>
    /// <param name="go"></param>
    /// <param name="isOld"></param>
    public delegate void GameObjectCallback(UnityEngine.GameObject go, bool isOld);
}