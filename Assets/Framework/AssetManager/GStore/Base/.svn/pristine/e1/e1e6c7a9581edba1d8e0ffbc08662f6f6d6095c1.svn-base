using UnityEngine;
using System.Collections;
using LitJson;
/// <summary>
/// Unity与Android交互
/// </summary>
namespace GStore
{
    public class AndroidInterface
    {

        #region 游戏功能

        /// <summary>
        /// 获取资源更新存放处的剩余空间
        /// </summary>
        /// <param name="number">电话号码</param>
        public static long GetFreeStorage()
        {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass javaClazz = new AndroidJavaClass("com.good.world3ol.W3AndroidTool");
            if (javaClazz != null)
            {
                long freeStorage = javaClazz.CallStatic<long>("GetUnityFreeStorage");

                Debug.Log("free storage =  " + freeStorage);

                return freeStorage; 
            }
        }
#endif

            return long.MaxValue; //非android平台，不做判断，故返回long的最大值
        }
        /// <summary>
        /// streamAsset目录下文件是否存在
        /// </summary>
        /// <param name="folderName">文件所在的streamasset子目录，如果是streamasset根目录，folderName=""</param>
        /// <param name="filename">文件名字</param>
        /// <returns></returns>
        public static bool FileExist(string folderName, string filename)
        {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass javaClazz = new AndroidJavaClass("com.good.world3ol.W3AndroidTool");
            return javaClazz.CallStatic<bool>("FileExists", folderName, filename);
        }
#endif

            return false;
        }

        /// <summary>
        /// 加载streamAsset目录下的文件
        /// </summary>
        /// <param name="filename">非全路径,是相对streamAsset的路径，如setup.xml, data/data.zip</param>
        /// <returns></returns>
        public static byte[] LoadFileFromStreamAssets(string filename)
        {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass javaClazz = new AndroidJavaClass("com.good.world3ol.W3AndroidTool");
            byte[] _byte = javaClazz.CallStatic<byte[]>("LoadFromAssets", filename);
            return _byte;
        }
#endif

            return null;
        }

        /// <summary>
        /// 获取streamAsset 某个目录下的文件名称列表
        /// </summary>
        /// <param name="folderName">相对于streamasset的目录名字：如 art</param>
        /// <returns></returns>
        public static string[] GetFileList(string folderName)
        {
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass javaClazz = new AndroidJavaClass("com.good.world3ol.W3AndroidTool");
           string [] arr = javaClazz.CallStatic<string []>("GetFileList", folderName);
            return arr;
        }
#endif

            return null;
        }


        public static void CopyAssetFilesToSDCard(string desFilePath, string srcFilePath)
        {
            string dir = System.IO.Path.GetDirectoryName(desFilePath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
#if UNITY_ANDROID
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass javaClazz = new AndroidJavaClass("com.good.world3ol.W3AndroidTool");
            javaClazz.CallStatic("CopyAssetFilesToSDCard", desFilePath, srcFilePath);
        }
#endif

        }
        #endregion
    } 
}
