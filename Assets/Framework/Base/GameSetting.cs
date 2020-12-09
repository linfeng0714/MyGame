

namespace Framework.Base
{
    public class GameSetting
    {
        /// <summary>
        /// 是否启用开发模式
        /// 
        /// 编辑器模式下：可配置
        /// 真机下：一定为false
        /// 
        /// true：关闭解压数据和热更流程
        ///      数据:    只读数据源路径；
        ///      AB:      enableAssetBundle = true ： 只读取AB源路径
        ///               enableAssetBundle = false : 不读取AB，读取资源源文件
        /// 
        /// false：开启解压数据和热更流程
        ///       数据:   只从持久化目录
        ///       AB:     enableAssetBundle = true ： AB先读取持久化目录，再读取StreamingAssets目录
        ///               enableAssetBundle = false : 不读取AB，读取资源源文件
        /// </summary>
#if UNITY_EDITOR
        private static bool _developMode = true;
        public static bool developMode
        {
            get
            {
                return _developMode;
            }
            set
            {
                _developMode = value;
            }
        }
#else
        public const bool developMode = false;
#endif

        /// <summary>
        /// 是否启用热更，但有个必要条件，develMode必须为false
        /// true:热更包，打包时，会删除Data.zip,保留GUData.zip
        /// false:非热更包，打包时，会保留Data.zip,删除GUData.zip
        /// </summary>
        public static bool hotUpdateMode = false;

        /// <summary>
        /// 是否开启加密,但有个必要条件，develMode必须为false
        /// </summary>
        public static bool encrypt = false;

        /// <summary>
        /// 开启加密的同时是否开启AB加密
        /// </summary>
        public static bool abEncry = false;

        /// <summary>
        /// 是否是小包，必要条件develMode = false,hotUpdateMode = true
        /// true:StreamingAssets不存在数据和AB，全部会在小包里,通过网络下载这个小包
        /// </summary>
        public static bool smallPackage = false;

        /// <summary>
        /// 对应的HotUpdteConfig.xml里的ID
        /// </summary>
        public static int hotUpdateConfigId;

        ///是否拷贝AB，如果是true，会提交所有热更ab，打包时打包入Data.zip
        public static bool isCopyABToStreamingAssets = true;

        /// <summary>
        /// 是否开启边玩边下
        /// </summary>
        public static bool downloadingWhilePlaying = false;
        /// <summary>
        /// 是否打边玩边下的包
        /// </summary>
        public static bool dwpBuildPackage = false;
        /// <summary>
        /// 边玩边下拷贝资源到热更SVN
        /// </summary>
        public static bool dwpBuildPackageCopyResToSvn = false;
        /// <summary>
        /// 边玩边下的版本
        /// </summary>
        public static int dwpVersion;

        /// <summary>
        /// 压缩bundle
        /// </summary>
        public static bool compressBundle = false;

        /// <summary>
        /// 是否开启自动补充下载
        /// </summary>
        public static bool dwpSupplementUpdate = false;

        /// <summary>
        /// 包体的构建号
        /// </summary>
        public static int buildNumber;
    }
}
