/*
 * AB加载：
 * 1、路径：(1)AB放在StreamingAssets目录
 *          (2)不同平台打包apk或者ipa都用统一的AssetBundles文件夹存放，如：【StreamingAssets/AssetBundles和http_res/AssetBundles】
 * 2、热更: (1)优先查找读取下载目录里面的AB，下载目录不存在该ab再读取StreamingAssets的。
 *          (2)在游戏中更新AssetBundles美术资源之后，需要重新加载那个总的manifest的bundle文件。
 * 3、加载步骤：先从缓存里面找已加载的AB, 缓存找不到AB才重新加载AB.加载总的manifest--->加载依赖的AB---->加载目标AB。5.3之后使用LoadFromFile/LoadFromFileAsync. 
 * 4、加载安全性：(1)加载的时候如果该ab正在加载中，不在重复加载。已经加载过没有unload掉的也不在重新加载。
 *                (2)异步加载可设置同时加载的最大数量，当大于等于设置的最大数量时，先放进待加载列表。每次加载完成后再检查待加载列表里面是否还有未加载.
 * 5、内存效率：(1)单个AB的大小不宜过大，数量过多的话io操作太频繁，但是热更方便。
 *              (2)目前AB是先缓存起来，在切换场景时候统一卸载。
 *            
 * =============================
 * ab卸载：
 * 	  (1)有AssetBundles资源更新时manifest 可能会变。所以调用CleanCacheBeforeResUpdate，清理缓存的manifest和AB,重新加载一次manifest
 *    (2)UnLoadAllAssetBundle()卸载全部的bundle，不包括shader,目前shader和字体是常驻的。该接口在切换场景时调用.
 *    (3)UnLoadAssetBundle(string bundleName) 卸载指定的bundle, 确定不会再使用的bundle，可以调用此接口卸载掉
 * 
 */

//编辑器运行非pc平台的AB需要替换shader才能正常显示, 开启此设置的代价是有额外操作的耗时以及造成内存中资源冗余
#if UNITY_EDITOR && !UNITY_STANDALONE
#define REPLACE_SHADERS
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GStore;

namespace GStore
{
    /// <summary>
    /// AssetBundle管理器
    /// </summary>
    public class AssetBundleManager : IAssetLoader
    {
        /// <summary>
        /// 所有的ab
        /// </summary>
        private Dictionary<string, AssetBundleInfo> m_AllAssetBundleDict = new Dictionary<string, AssetBundleInfo>();

        /// <summary>
        /// 正在加载中的ab
        /// </summary>
        public Dictionary<string, AssetBundleLoader> m_LoadingDic = new Dictionary<string, AssetBundleLoader>();

        //ab总的manifest
        public AssetBundleManifest m_RootManifest = null;

        /// <summary>
        /// 后缀名
        /// </summary>
        public const string BUNDLE_EXTENSION = ".unity3d";

        /// <summary>
        /// ab存放在streamasset的路径：xxx/assetbundles
        /// </summary>
        private string m_StreamassetAssetBundleFolder = "";

        /// <summary>
        /// ab存放在下载目录的路径：xxx/art
        /// </summary>
        private string m_DownLoadAssetBundleFolder = "";

        /// <summary>
        /// bundle表
        /// </summary>
        private AssetBundleTable m_BundleTable = null;

        /// <summary>
        /// 变体映射
        /// </summary>
        private VariantMapper m_VariantMapper = null;

        public Dictionary<string, AssetBundleInfo> GetAllAssetBundles()
        {
            return m_AllAssetBundleDict;
        }

        /// <summary>
        /// 退出场景时调用
        /// </summary>
        public void DoExitScene()
        {
            UnLoadAllAssetBundle();
        }

        /// <summary>
        /// 初始化ab存放路径：减少每次调用时的拼接字符串
        /// </summary>
        private void InitAssetBundleFolder()
        {
            m_DownLoadAssetBundleFolder = AssetPathDefine.externalBundlePath;

#if UNITY_EDITOR
            m_StreamassetAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                m_StreamassetAssetBundleFolder = System.IO.Path.Combine(Application.dataPath + "!assets", AssetPathDefine.assetBundleFolder);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                m_StreamassetAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
            }
            else
            {
                //其他平台待定
                m_StreamassetAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
            }
#endif
        }

        /// <summary>
        ///StreamingAssets路径,包含ab后缀名
        /// </summary>
        private string GetStreamingAssetsPath(string bundleName)
        {
            if (string.IsNullOrEmpty(m_StreamassetAssetBundleFolder))
            {
                InitAssetBundleFolder();
            }
            return System.IO.Path.Combine(m_StreamassetAssetBundleFolder, bundleName);
        }

        /// <summary>
        /// ab下载目录 包含后缀名
        /// </summary>
        private string GetAssetsBundleDownLoadPath(string bundleName)
        {
            if (string.IsNullOrEmpty(m_DownLoadAssetBundleFolder))
            {
                InitAssetBundleFolder();
            }

            string path = System.IO.Path.Combine(m_DownLoadAssetBundleFolder, bundleName);
            return path;
        }

#if UNITY_EDITOR
        string editorAssetBundleFolder = string.Empty;
        public string GetEditorPath(string bundleName)
        {
            if (string.IsNullOrEmpty(editorAssetBundleFolder))
            {
                string path = Application.dataPath + "/../ExtraResources/Bundle/";

                string platform = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();

                path = System.IO.Path.Combine(path, platform);
                editorAssetBundleFolder = System.IO.Path.Combine(path, AssetPathDefine.assetBundleFolder);
            }

            string filepath = System.IO.Path.Combine(editorAssetBundleFolder, bundleName);
            return filepath;
        }
#endif

        /// <summary>
        /// 获取ab的路径，包含后缀名：优先读取下载路径--->然后查找StreamingAssets--->最后查找打包目录
        /// </summary>
        public string GetAssetsBundleFullPath(string bundleName)
        {
            string path = "";
            if (!GameSetting.developMode)//非开发模式，先读取持久化目录，再读取StreamingAssets
            {
                path = GetAssetsBundleDownLoadPath(bundleName);
                if (FilePath.Exists(path))
                {
                    return path;
                }

                path = GetStreamingAssetsPath(bundleName);
                if (FilePath.Exists(path))
                {
                    return path;
                }
            }
            else//开发模式，读取源AB目录
            {
#if UNITY_EDITOR
                path = GetEditorPath(bundleName);
#endif
            }
            return path;
        }

        /// <summary>
        /// 通过bundlename获取指定的bundle
        /// </summary>
        public AssetBundleInfo GetAssetBundleByBundleName(string bundleName)
        {
            if (m_AllAssetBundleDict == null || m_AllAssetBundleDict.Count <= 0)
            {
                return null;
            }

            AssetBundleInfo bundleInfo = null;
            if (m_AllAssetBundleDict.TryGetValue(bundleName, out bundleInfo))
            {
                return bundleInfo;
            }
            return null;
        }

        /// <summary>
        /// 从列表移除一个bundle
        /// </summary>
        /// <param name="bundleName">Bundle name.</param>
        public void RemoveBundleInfo(string bundleName)
        {
            if (m_AllAssetBundleDict.ContainsKey(bundleName))
            {
                m_AllAssetBundleDict.Remove(bundleName);
            }
        }

        /// <summary>
        /// 添加一个bundle进列表缓存
        /// </summary>
        /// <param name="bundleName">Bundle name.</param>
        /// <param name="info">Info.</param>
        public void AddBundleInfo(string bundleName, AssetBundleInfo info)
        {
            m_AllAssetBundleDict[bundleName] = info;
        }

        /// <summary>
        /// bundle是否正在加载中
        /// </summary>
        /// <returns><c>true</c> if this instance is loading the specified assetid; otherwise, <c>false</c>.</returns>
        /// <param name="assetid">Assetid.</param>
        public bool IsInLoadingDic(string bundleName)
        {
            return m_LoadingDic.ContainsKey(bundleName);
        }

        /// <summary>
        /// 加到loading列表
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        /// <param name="bundleName">Bundle name.</param>
        public void AddToLoadingDic(string bundleName, AssetBundleLoader loader)
        {
            if (m_LoadingDic.ContainsKey(bundleName))
                return;

            m_LoadingDic.Add(bundleName, loader);
        }

        /// <summary>
        /// 从loading列表移除
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        public void RemoveFromLoadingDic(string bundleName)
        {
            if (m_LoadingDic.ContainsKey(bundleName))
                m_LoadingDic.Remove(bundleName);
        }

        /// <summary>
        /// 从loading列表获取loader信息
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        public AssetBundleLoader GetLoaderFromLoadingDic(string bundleName)
        {
            AssetBundleLoader loader = null;
            if (m_LoadingDic.TryGetValue(bundleName, out loader))
            {
                return loader;
            }

            return null;
        }

        #region 初始化
        /// <summary>
        ///游戏启动的时候 和 bundle资源更新之后要重新初始化rootBundle
        /// </summary>
        public void Init()
        {
            if (m_RootManifest != null)
                return;

            m_BundleTable = new AssetBundleTable();
            m_BundleTable.Init(this);

            m_LoadingDic.Clear();
            m_AllAssetBundleDict.Clear();

            InitAssetBundleFolder();
            LoadRootAssetBundle();

            m_VariantMapper = new VariantMapper(m_RootManifest);

            //PreLoadResidentBundle();
        }

        /// <summary>
        ///加载root bundle
        /// </summary>
        private void LoadRootAssetBundle()
        {
            if (m_RootManifest != null)
                return;

            string path = GetAssetsBundleFullPath(AssetPathDefine.assetBundleFolder);
            AssetBundle rootBundle = AssetBundle.LoadFromFile(path);
            if (rootBundle == null)
            {
                Debug.LogError("==bundle log: rootBundle == null");
                return;
            }

            m_RootManifest = rootBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (m_RootManifest == null)
            {
                Debug.LogError("==bundle log:mRootManifest == null");
            }
            rootBundle.Unload(false);
        }

        /// <summary>
        /// 预加载常驻的 bundle
        /// </summary>
        private void PreLoadResidentBundle()
        {
            //读取常驻配置
            byte[] data = LoadBundleFileData(AssetPathDefine.residentBundleTableName);
            if (data == null)
            {
                return;
            }
            string json = StringUtil.GetStringWithoutBOM(data);

            LitJson.JsonData datas = LitJson.JsonMapper.ToObject(json);
            for (int i = 0; i < datas.Count; i++)
            {
                string bundleName = string.Format("{0}{1}", datas[i], BUNDLE_EXTENSION);
                bundleName = RemapVariantName(bundleName);

                AssetBundleLoader loader = new AssetBundleLoader(this);
                loader.LoadBundle(bundleName);

                //设置不卸载
                SetAssetBundleDontDestroyOnLoadByBundleName(bundleName);
            }
        }

        #endregion


        #region 同步加载AB相关

        /// <summary>
        /// 同步加载AB
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(int assetId)
        {
            return LoadAsset<UnityEngine.Object>(assetId);
        }

        /// <summary>
        /// 同步加载AB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T LoadAsset<T>(int assetId) where T : UnityEngine.Object
        {
            return LoadAsset<T>(AssetInfo.GetAssetPath(assetId));
        }

        /// <summary>
        /// 同步加载AB
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(string assetPath)
        {
            return LoadAsset<UnityEngine.Object>(assetPath);
        }

        /// <summary>
        /// 同步加载AB--总入口api
        /// </summary>
        /// <param name="iAssetID">AB的资源id</param>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (m_RootManifest == null)
            {
                return null;
            }

            AssetBundleInfo bundleInfo = GetAssetBundleInfo(assetPath);

            if (bundleInfo == null)
            {
                return null;
            }

            AssetBundleTable.BundleTableInfo info = m_BundleTable.GetBundleTableInfo(assetPath);

            string assetName = info.path;
            if (string.IsNullOrEmpty(assetName))
            {
                Debug.LogError("==bundle log:==assetname为空!!! assetID = " + assetPath.ToString());
                return null;
            }

            T ob = bundleInfo.LoadAsset<T>(assetName);
            if (ob == null)
            {
                Debug.LogError("=============ob===null  assetID=" + assetPath.ToString());
            }

#if REPLACE_SHADERS
            EditorReplaceShader(ref ob);
#endif
            return ob;
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAllAsset(string assetPath)
        {
            return LoadAllAsset<UnityEngine.Object>(assetPath);
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (m_RootManifest == null)
            {
                return null;
            }

            AssetBundleInfo bundleInfo = GetAssetBundleInfo(assetPath);

            if (bundleInfo == null)
            {
                return null;
            }
            return bundleInfo.LoadAllAsset<T>();
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAllAsset(int assetId)
        {
            return LoadAllAsset<UnityEngine.Object>(assetId);
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(int assetId) where T : UnityEngine.Object
        {
            return LoadAllAsset<T>(AssetInfo.GetAssetPath(assetId));
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public void LoadSceneBundle(string sceneName)
        {
            GetAssetBundleInfo(sceneName);
        }

        /// <summary>
        /// 获取AssetBundleInfo
        /// </summary>
        /// <param name="assetKey"></param>
        /// <returns></returns>
        private AssetBundleInfo GetAssetBundleInfo(string assetKey)
        {
            AssetBundleTable.BundleTableInfo info = m_BundleTable.GetBundleTableInfo(assetKey);
            if (info == null)
            {
                //Debug.LogError("==bundle log:==BundleTableInfo为空!!! assetID = " + assetID.ToString());
                return null;
            }

            string bundleName = info.abn;
            bundleName = RemapVariantName(bundleName);

            AssetBundleInfo bundleInfo = GetAssetBundleByBundleName(bundleName);
            if (bundleInfo != null && bundleInfo.Bundle != null) //bundle已经存在
            {
                return bundleInfo;
            }
            AssetBundleLoader loader = GetLoaderFromLoadingDic(bundleName);
            if (loader != null)//在同步load的时候，如果发现该bundle正处于异步加载中，则取消异步加载操作。
            {
                loader.IsOverLoad = true;
            }

            ///缓存中没有该bundle,需要重新加载
            AssetBundleLoader load = new AssetBundleLoader(this);
            bundleInfo = load.LoadBundle(bundleName, true);
            return bundleInfo;
        }

        /// <summary>
        /// 加载文件数据
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public byte[] LoadFileData(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            TextAsset asset = LoadAsset<TextAsset>(assetPath);
            if (asset == null)
            {
                return null;
            }

            return asset.bytes;
        }

        /// <summary>
        /// 读取bundle路径下的文件二进制数据
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public byte[] LoadBundleFileData(string bundleName)
        {
            string filePath = "";
            if (!GameSetting.developMode)//非开发模式，先读取持久化目录，再读取StreamingAssets
            {
                filePath = System.IO.Path.Combine(AssetPathDefine.externalBundlePath, bundleName);

                if (!FilePath.Exists(filePath))
                {
                    filePath = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder + "/" + bundleName);
                }
            }
            else//开发模式，读取AB源目录
            {
#if UNITY_EDITOR
                if (!FilePath.Exists(filePath))
                {
                    filePath = GetEditorPath(bundleName);
                }
#endif
            }

            if (filePath.Contains("://"))
            {
                WWW www = new WWW(filePath);
                float startTime = Time.realtimeSinceStartup;
                while (!www.isDone)
                {
                    float endTime = Time.realtimeSinceStartup;
                    if ((endTime - startTime) >= 5f)
                    {
                        Debug.LogErrorFormat("读取文件超时: file={0}", filePath);
                        return null;
                    }
                    System.Threading.Thread.Sleep(1);
                }
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogErrorFormat("读取文件超时：file={0}, error={1}", filePath, www.error);
                    return null;
                }

                byte[] bytes = www.bytes;
                return bytes;
            }
            else
            {
                if (!FilePath.Exists(filePath))
                {
                    return null;
                }
                return System.IO.File.ReadAllBytes(filePath);
            }
        }

        #endregion


        #region 异步加载AB相关
        /// <summary>
        /// 异步加载AB
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="callBack"></param>
        /// <param name="func"></param>
        public void LoadAssetAsync(int assetId, ObjectCallback callBack, IsObjectOldFunc func)
        {
            LoadAssetAsync(assetId, false, (asset) => { Utils.OnCallBack(callBack, asset, func); });
        }

        /// <summary>
        /// 异步加载AB
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="isSceneBundle"></param>
        /// <param name="callBack"></param>
        private void LoadAssetAsync(int assetId, bool isSceneBundle, System.Action<UnityEngine.Object> callBack = null)
        {
            LoadAssetAsync(AssetInfo.GetAssetPath(assetId), isSceneBundle, callBack);
        }

        /// <summary>
        ///异步加载AB--总入口api
        /// </summary>
        /// <param name="assetID">AB的资源</param>
        ///  <param name="isSceneBundle">是否是场景ab</param>
        /// <param name="CallBack">载入完成后的回调</param>
        private void LoadAssetAsync(string assetID, bool isSceneBundle, System.Action<UnityEngine.Object> callBack = null)
        {
            if (m_RootManifest == null)
            {
                if (callBack != null)
                    callBack(null);
                return;
            }

            AssetBundleTable.BundleTableInfo tableInfo = m_BundleTable.GetBundleTableInfo(assetID);
            if (tableInfo == null)
            {
                Debug.LogError("==bundle log:==BundleTableInfo为空!!! assetID = " + assetID);
                if (callBack != null)
                    callBack(null);

                return;
            }

            string assetName = tableInfo.path;
            if (string.IsNullOrEmpty(assetName))
            {

                Debug.LogError("==bundle log:==assetname为空!!! assetID=" + assetID.ToString());

                if (callBack != null)
                    callBack(null);

                return;
            }

            string bundleName = tableInfo.abn;
            bundleName = RemapVariantName(bundleName);

            AssetBundleLoader loadingLoader = GetLoaderFromLoadingDic(bundleName);
            if (loadingLoader != null)
            {
                loadingLoader.IsVaild = true;
                loadingLoader.loadCompleteCallback += callBack;
            }
            else
            {
                AssetBundleLoader loader = new AssetBundleLoader(this);
                loader.loadCompleteCallback = (obj) =>
                {
                    if (callBack != null)
                    {
#if REPLACE_SHADERS
                        EditorReplaceShader(ref obj);
#endif
                        callBack(obj);
                    }

                    RemoveFromLoadingDic(bundleName);
                };

                AddToLoadingDic(bundleName, loader);
                CoroutineRunner.Run(loader.LoadAssetAsync(bundleName, isSceneBundle, assetName));
            }
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>
        public void LoadSceneBundleAsync(string sceneName, System.Action callBack)
        {
            LoadAssetAsync(sceneName, true, (obj) =>
            {
                if (callBack != null)
                {
                    callBack();
                }
            });
        }
        #endregion

        #region 卸载ab相关
        /// <summary>
        /// 卸载全部AB
        /// </summary>
        /// <param name="isResetRootManifest">是否重置manifest文件, Manifest常驻，只在有art资源更新时重置一次</param>
        public void UnLoadAllAssetBundle(bool isResetRootManifest = false)
        {
            if (isResetRootManifest)
            {
                m_RootManifest = null;
                UnloadResidentBundle();
            }
            else
            {
                if (m_AllAssetBundleDict == null || m_AllAssetBundleDict.Count <= 0)
                {
                    return;
                }
                List<AssetBundleInfo> list = new List<AssetBundleInfo>();
                var enumerator = m_AllAssetBundleDict.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, AssetBundleInfo> pair = enumerator.Current;

                    if (isResetRootManifest == false && pair.Value.DontDestroyOnLoad)
                    {
                        continue;
                    }
                    list.Add(pair.Value);
                }
                enumerator.Dispose();

                for (int i = 0; i < list.Count; i++)
                {
                    AssetBundleInfo info = list[i];
                    if (info == null)
                        continue;

                    RemoveBundleInfo(info.BundleName);
                    info.UnLoadBundle();
                }
                list.Clear();
            }

            var loadingEnumerator = m_LoadingDic.GetEnumerator();//正在加载中的ab 设置为无效。
            while (loadingEnumerator.MoveNext())
            {
                KeyValuePair<string, AssetBundleLoader> pair = loadingEnumerator.Current;
                pair.Value.IsVaild = false;
            }
        }

        /// <summary>
        /// 卸载指定的ab
        /// </summary>
        /// <param name="assetID">ab的assetid</param>
        public bool UnLoadAssetBundle(string assetID)
        {
            AssetBundleTable.BundleTableInfo tableInfo = m_BundleTable.GetBundleTableInfo(assetID);
            if (tableInfo == null)
            {
                return false;
            }

            string bundleName = tableInfo.abn;
            bundleName = RemapVariantName(bundleName);

            AssetBundleInfo info = GetAssetBundleByBundleName(bundleName);
            if (info != null)
            {
                RemoveBundleInfo(bundleName);
                return info.UnLoadBundle();
            }

            return false;
        }
        /// <summary>
        /// 卸载常驻的bundle及其引用--有art资源更新之后调用。
        /// </summary>
        private void UnloadResidentBundle()
        {
            var enumerator = m_AllAssetBundleDict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, AssetBundleInfo> pair = enumerator.Current;

                pair.Value.UnLoadBundle();
            }
            enumerator.Dispose();
            m_AllAssetBundleDict.Clear();
        }

        public void SetAssetBundleDontDestroyOnLoad(string assetKey)
        {
            AssetBundleTable.BundleTableInfo info = m_BundleTable.GetBundleTableInfo(assetKey);
            if (info == null)
            {
                Debug.LogError("==bundle log:==BundleTableInfo为空!!! assetID = " + assetKey.ToString());
                return;
            }

            string bundleName = info.abn;
            bundleName = RemapVariantName(bundleName);

            SetAssetBundleDontDestroyOnLoadByBundleName(bundleName);
        }

        private void SetAssetBundleDontDestroyOnLoadByBundleName(string bundleName)
        {
            //设置依赖的AB不卸载
            string[] depPaths = m_RootManifest.GetAllDependencies(bundleName);
            for (int i = 0; i < depPaths.Length; i++)
            {
                string path = depPaths[i];
                path = RemapVariantName(path);

                AssetBundleInfo depInfo = GetAssetBundleByBundleName(path);
                if (depInfo == null)
                {
                    Debug.LogError("==bundle log:==AssetBundleInfo!!! path = " + path);
                    continue;
                }

                depInfo.SetDontDestroyOnLoad();
            }

            //设置本身AB不卸载
            AssetBundleInfo abInfo = GetAssetBundleByBundleName(bundleName);
            if (abInfo == null)
            {
                Debug.LogError("==bundle log:==AssetBundleInfo!!! bundleName = " + bundleName);
                return;
            }
            abInfo.SetDontDestroyOnLoad();
        }

        /// <summary>
        /// 更新资源时要先清掉之前加载的ab，总的mRootManifest要清掉以便重新初始化
        /// </summary>
        public void CleanCacheBeforeResUpdate()
        {
            try
            {
                UnLoadAllAssetBundle(true);
                m_BundleTable.CleanAssetBundleTable();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("===bundle log:CleanCacheBeforeResUpdate() ERROR " + ex.StackTrace);
            }
        }

        #endregion


#if REPLACE_SHADERS
        /// <summary>
        /// 编辑器下替换ab中已编译到平台的shader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        private void EditorReplaceShader<T>(ref T obj) where T : UnityEngine.Object
        {
            GameObject go = obj as GameObject;
            if (go != null)
            {
                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null)
                        {
                            continue;
                        }
                        if (material.shader == null)
                        {
                            continue;
                        }

                        Shader replaceShader = Shader.Find(material.shader.name);
                        if (replaceShader == null)
                        {
                            continue;
                        }
                        material.shader = replaceShader;
                    }
                }

                foreach (var image in go.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                {
                    Material material = image.material;
                    if (material == null)
                    {
                        continue;
                    }
                    if (material.shader == null)
                    {
                        continue;
                    }

                    Shader replaceShader = Shader.Find(material.shader.name);
                    if (replaceShader == null)
                    {
                        continue;
                    }
                    material.shader = replaceShader;
                }

                return;
            }

            Material mat = obj as Material;
            if (mat != null)
            {
                if (mat.shader == null)
                {
                    return;
                }
                Shader replaceShader = Shader.Find(mat.shader.name);
                if (replaceShader == null)
                {
                    return;
                }
                mat.shader = replaceShader;
                return;
            }

            Shader shader = obj as Shader;
            if (shader != null)
            {
                Shader replaceShader = Shader.Find(shader.name);
                if (replaceShader == null)
                {
                    return;
                }
                obj = replaceShader as T;
                return;
            }
        }
#endif

        #region 变体相关
        /// <summary>
        /// 激活变体
        /// </summary>
        /// <param name="variants"></param>
        public void ActivateVariants(string[] variants)
        {
            m_VariantMapper.ActivateVariants(variants);
        }

        /// <summary>
        /// 重新映射变体名 - 变体激活顺序匹配，匹配失败则返回无变体
        /// </summary>
        /// <returns></returns>
        public string RemapVariantName(string bundleName)
        {
            return m_VariantMapper.RemapVariantName(bundleName);
        }
        #endregion
    }
}
