using Framework.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class AssetBundleManager : IAssetLoader
    {
        /// <summary>
        /// 所有ab
        /// </summary>
        private Dictionary<string, AssetBundleInfo> _allAssetBundleDict = new Dictionary<string, AssetBundleInfo>();
        /// <summary>
        /// 正在加载中得ab
        /// </summary>
        public Dictionary<string, AssetBundleLoader> _allLoadingDict = new Dictionary<string, AssetBundleLoader>();

        public AssetBundleManifest _rootManifest = null;

        public const string BUNDLE_EXTESION = ".unity3d";

        //ab存放在 streamasset 的路径 
        private string _streamAssetBundleFolder = string.Empty;

        private string _downLoadAssetBundleFolder = string.Empty;

        private AssetBundleTable _bundleTable;
        /// <summary>
        /// 变体映射
        /// </summary>
        private VariantMapper _variantMapper = null;

        public Dictionary<string, AssetBundleInfo> GetAllAssetBundles()
        {
            return _allAssetBundleDict;
        }

        /// <summary>
        /// 退出场景时调用
        /// </summary>
        public void DoExitScene()
        {
            UnLoadAllAssetBundle();
        }
        /// <summary>
        /// 获取ab的路径，包含后缀名：优先读取下载路径--->然后查找StreamingAssets--->最后查找打包目录
        /// </summary>
        public string GetAssetsBundleFullPath(string bundleName)
        {
            string path = "";
            if (!GameSetting.developMode)
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
            else
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
            if (_allAssetBundleDict == null || _allAssetBundleDict.Count <= 0)
            {
                return null;
            }

            AssetBundleInfo bundleInfo = null;
            if (_allAssetBundleDict.TryGetValue(bundleName, out bundleInfo))
            {
                return bundleInfo;
            }
            return null;
        }

#if UNITY_EDITOR
        private string editorAssetBundleFolder = string.Empty;
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
        ///StreamingAssets路径,包含ab后缀名
        /// </summary>
        private string GetStreamingAssetsPath(string bundleName)
        {
            if (string.IsNullOrEmpty(_streamAssetBundleFolder))
            {
                InitAssetBundleFolder();
            }
            return System.IO.Path.Combine(_streamAssetBundleFolder, bundleName);
        }

        /// <summary>
        /// ab下载目录 包含后缀名
        /// </summary>
        private string GetAssetsBundleDownLoadPath(string bundleName)
        {
            if (string.IsNullOrEmpty(_downLoadAssetBundleFolder))
            {
                InitAssetBundleFolder();
            }

            string path = System.IO.Path.Combine(_downLoadAssetBundleFolder, bundleName);
            return path;
        }

        /// <summary>
        /// 初始化ab存放路径：减少每次调用时的拼接字符串
        /// </summary>
        private void InitAssetBundleFolder()
        {
            _downLoadAssetBundleFolder = AssetPathDefine.externalBundlePath;

#if UNITY_EDITOR
            _streamAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
#else
            if (Application.platform == RuntimePlatform.Android)
            {
                _streamAssetBundleFolder = System.IO.Path.Combine(Application.dataPath + "!assets", AssetPathDefine.assetBundleFolder);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _streamAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
            }
            else
            {
                //其他平台待定
                _streamAssetBundleFolder = System.IO.Path.Combine(Application.streamingAssetsPath, AssetPathDefine.assetBundleFolder);
            }
#endif
        }

        #region 卸载ab相关
        /// <summary>
        /// 卸载全部AB
        /// </summary>
        /// <param name="isResetRootManifest">是否重置manifest文件, Manifest常驻，只在有art资源更新时重置一次</param>
        public void UnLoadAllAssetBundle(bool isResetRootManifest = false)
        {
            if(isResetRootManifest)
            {
                _rootManifest = null;
                UnloadResidentBundle();
            }
            else
            {
                if (_allAssetBundleDict == null || _allAssetBundleDict.Count <= 0)
                    return;
                List<AssetBundleInfo> list = new List<AssetBundleInfo>();
                var enumerator = _allAssetBundleDict.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, AssetBundleInfo> pair = enumerator.Current;

                    if (pair.Value.DontDestroyOnLoad)
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
            var loadingEnumerator = _allLoadingDict.GetEnumerator();//正在加载中的ab 设置为无效。
            while (loadingEnumerator.MoveNext())
            {
                KeyValuePair<string, AssetBundleLoader> pair = loadingEnumerator.Current;
                pair.Value.IsVaild = false;
            }
        }

        /// <summary>
        /// 从列表移除一个bundle
        /// </summary>
        /// <param name="bundleName">Bundle name.</param>
        public void RemoveBundleInfo(string bundleName)
        {
            if (_allAssetBundleDict.ContainsKey(bundleName))
            {
                _allAssetBundleDict.Remove(bundleName);
            }
        }

        /// <summary>
        /// 添加一个bundle进列表缓存
        /// </summary>
        /// <param name="bundleName">Bundle name.</param>
        /// <param name="info">Info.</param>
        public void AddBundleInfo(string bundleName, AssetBundleInfo info)
        {
            _allAssetBundleDict[bundleName] = info;
        }

        /// <summary>
        /// bundle是否正在加载中
        /// </summary>
        /// <returns><c>true</c> if this instance is loading the specified assetid; otherwise, <c>false</c>.</returns>
        /// <param name="assetid">Assetid.</param>
        public bool IsInLoadingDic(string bundleName)
        {
            return _allLoadingDict.ContainsKey(bundleName);
        }

        /// <summary>
        /// 加到loading列表
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        /// <param name="bundleName">Bundle name.</param>
        public void AddToLoadingDic(string bundleName, AssetBundleLoader loader)
        {
            if (_allLoadingDict.ContainsKey(bundleName))
                return;

            _allLoadingDict.Add(bundleName, loader);
        }

        /// <summary>
        /// 从loading列表移除
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        public void RemoveFromLoadingDic(string bundleName)
        {
            if (_allLoadingDict.ContainsKey(bundleName))
                _allLoadingDict.Remove(bundleName);
        }

        /// <summary>
        /// 从loading列表获取loader信息
        /// </summary>
        /// <param name="assetId">Asset identifier.</param>
        public AssetBundleLoader GetLoaderFromLoadingDic(string bundleName)
        {
            AssetBundleLoader loader = null;
            if (_allLoadingDict.TryGetValue(bundleName, out loader))
            {
                return loader;
            }

            return null;
        }
        #endregion
        #region 初始化
        public void Init()
        {
            if (_rootManifest != null)
                return;
            _bundleTable = new AssetBundleTable();
            _bundleTable.Init(this);

            _allLoadingDict.Clear();
            _allAssetBundleDict.Clear();

            InitAssetBundleFolder();
            LoadRootAssetBundle();

            _variantMapper = new VariantMapper(_rootManifest);
        }

        /// <summary>
        ///加载root bundle
        /// </summary>
        private void LoadRootAssetBundle()
        {
            if (_rootManifest != null)
                return;

            string path = GetAssetsBundleFullPath(AssetPathDefine.assetBundleFolder);
            AssetBundle rootBundle = AssetBundle.LoadFromFile(path);
            if (rootBundle == null)
            {
                Debug.LogError("==bundle log: rootBundle == null");
                return;
            }

            _rootManifest = rootBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (_rootManifest == null)
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
                string bundleName = string.Format("{0}{1}", datas[i], BUNDLE_EXTESION);
                bundleName = RemapVariantName(bundleName);

                AssetBundleLoader loader = new AssetBundleLoader(this);
                loader.LoadBundle(bundleName);

                //设置不卸载
                SetAssetBundleDontDestroyOnLoadByBundleName(bundleName);
            }
        }

        private void SetAssetBundleDontDestroyOnLoadByBundleName(string bundleName)
        {
            //设置依赖的AB不卸载
            string[] depPaths = _rootManifest.GetAllDependencies(bundleName);
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


        #endregion


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

        /// <summary>
        /// 卸载常驻的bundle及其引用--有art资源更新之后调用。
        /// </summary>
        private void UnloadResidentBundle()
        {

            var enumerator = _allAssetBundleDict.GetEnumerator();
            while(enumerator.MoveNext())
            {
                KeyValuePair<string, AssetBundleInfo> pair = enumerator.Current;

                pair.Value.UnLoadBundle();
            }
            enumerator.Dispose();
            _allAssetBundleDict.Clear();
        }

        #region 变体相关
        /// <summary>
        /// 激活变体
        /// </summary>
        /// <param name="variants"></param>
        public void ActivateVariants(string[] variants)
        {
            _variantMapper.ActivateVariants(variants);
        }

        /// <summary>
        /// 重新映射变体名 - 变体激活顺序匹配，匹配失败则返回无变体
        /// </summary>
        /// <returns></returns>
        public string RemapVariantName(string bundleName)
        {
            return _variantMapper.RemapVariantName(bundleName);
        }
        #endregion

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
        /// 同步加载AB--总入口api
        /// </summary>
        /// <param name="iAssetID">AB的资源id</param>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (_rootManifest == null)
            {
                return null;
            }

            AssetBundleInfo bundleInfo = GetAssetBundleInfo(assetPath);

            if (bundleInfo == null)
            {
                return null;
            }

            AssetBundleTable.BundleTableInfo info = _bundleTable.GetBundleTableInfo(assetPath);

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
        /// 获取AssetBundleInfo
        /// </summary>
        /// <param name="assetKey"></param>
        /// <returns></returns>
        private AssetBundleInfo GetAssetBundleInfo(string assetKey)
        {
            AssetBundleTable.BundleTableInfo info = _bundleTable.GetBundleTableInfo(assetKey);
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
        /// 同步加载AB
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(string assetPath)
        {
            return LoadAsset<UnityEngine.Object>(assetPath);
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

        public T[] LoadAllAsset<T>(int assetId) where T : UnityEngine.Object
        {
            return LoadAllAsset<T>(AssetInfo.GetAssetPath(assetId));
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
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            if (_rootManifest == null)
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

        public T LoadAsset<T>(int assetId) where T : UnityEngine.Object
        {
            return LoadAsset<T>(AssetInfo.GetAssetPath(assetId));
        }

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
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public void LoadSceneBundle(string sceneName)
        {
            GetAssetBundleInfo(sceneName);
        }
    }
}
