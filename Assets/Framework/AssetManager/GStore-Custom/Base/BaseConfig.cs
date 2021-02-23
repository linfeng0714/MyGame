using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 引擎配置类
    /// </summary>
    public static class BaseConfig
    {
        /// <summary>
        /// 是否启用本地数据
        /// 编辑器模式下：可通过StreamingAsset下的setup.xml配置
        /// 真机下：一定为false，setup.xml的配置无效
        /// 
        /// 开启时：表示数据只读源路径
        /// 
        /// 关闭时：根据UpdateMode的值而定，请查看UpdateMode的注释
        /// </summary>
#if UNITY_EDITOR
        public static bool develMode = false;
#else
        public const bool develMode = false;
#endif

        /// <summary>
        /// 是否启用热更，但有个必要条件，DevelopmentMode必须为false
        /// 打包拷贝数据是会读取setup.xml,如果不开启热更，则拷贝数据到StreamingAsset,否则不拷贝
        /// 
        /// 开启热更：数据打包成Zip，第一运行解压到持久化目录。数据只读取持久化目录的，AssetBundle先读取持久化目录再读取StreamingAsset的
        /// 关闭热更：数据在StreamingAsset
        /// </summary>
        public static bool hotUpdateMode = true;

        /// <summary>
        /// 是否开启加密,但有个必要条件，DevelopmentMode必须为false
        /// 加密流程：
        /// （1）非热更包，拷贝到StreamingAssets下时，会读取setup.xml里的encry,如果加密会对数据进行加密
        /// （2）热更包，注意打Data.zip时数据需要进行加密，以及热更svn上提交的数据也要进行加密
        /// </summary>
        public static bool encry = false;

        /// <summary>
        /// 开启加密的同时是否开启AB加密
        /// </summary>
        public static bool isABEncry = true;

        /// <summary>
        /// 是否是小包，只有开启热更时才会起效，也就是：DevelopmentMode为false，UpdateMode为true；
        /// 如果是小包，打包时不会将AB拷到StreamingAssets,会将StreamingAssets/Data/Data.zip删除
        /// </summary>
        public static bool smallPackage = false;

        /// <summary>
        /// 资源存放的基本目录（持久化目录）
        /// </summary>
        public static string webBasePath
        {
            get
            {
#if UNITY_EDITOR
                return Application.dataPath + "/../HotUpdate/";
#else
                return Application.persistentDataPath;
#endif
            }
        }
    }
}
