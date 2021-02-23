using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GStore;

namespace GStore
{
    /// <summary>
    /// 资源管理器 - 对外统一的资源访问接口
    /// </summary>
    public class AssetManager : Singleton<AssetManager>
    {
        /// <summary>
        /// 是否开启从AB加载资源 - 真机强制开启
        /// </summary>
#if UNITY_EDITOR
        private bool m_EnableAssetBundleForEditor = false;
#endif
        public bool enableAssetBundle
        {
            get
            {
#if UNITY_EDITOR
                return m_EnableAssetBundleForEditor;
#else
                return true;
#endif
            }
#if UNITY_EDITOR
            set
            {
                m_EnableAssetBundleForEditor = value;
            }
#endif
        }

        /// <summary>
        /// 缓存资源列表
        /// key = 资源唯一ID
        /// value = 资源信息
        /// </summary>
        private Dictionary<int, CacheInfo> m_CacheAssetDict = new Dictionary<int, CacheInfo>();

        /// <summary>
        /// 缓存Sprite资源
        /// key = 资源AssetPath
        /// value = Sprite集合
        /// </summary>
        private readonly Dictionary<string, SpriteCollection> m_CacheSpriteDict = new Dictionary<string, SpriteCollection>();

        /// <summary>
        /// AssetBundle管理器
        /// </summary>
        private AssetBundleManager m_AssetBundleManager = null;
        public AssetBundleManager assetBundleManager { get { return m_AssetBundleManager; } }

        /// <summary>
        /// 资源管理器
        /// </summary>
        private ResourceManager m_ResourceManager = null;
        public ResourceManager resourceManager { get { return m_ResourceManager; } }

#if UNITY_EDITOR
        /// <summary>
        /// AssetDatabase管理器
        /// </summary>
        private AssetDatabaseManager m_AssetDatabaseManager = null;
        public AssetDatabaseManager assetDatabaseManager { get { return m_AssetDatabaseManager; } }
#endif

        /// <summary>
        /// 对象池管理器
        /// </summary>
        private PoolManager m_PoolManager = null;
        public PoolManager poolManager { get { return m_PoolManager; } }

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        public AssetManager()
        {
#if UNITY_EDITOR
            m_EnableAssetBundleForEditor = false;
#endif

            //AssetBundle与AssetDatabase互斥
            if (enableAssetBundle)
            {
                m_AssetBundleManager = new AssetBundleManager();
                m_AssetBundleManager.Init();
            }
#if UNITY_EDITOR
            else
            {
                m_AssetDatabaseManager = new AssetDatabaseManager();
                m_AssetDatabaseManager.Init();
            }
#endif

            //初始化Resources管理器
            m_ResourceManager = new ResourceManager();
            m_ResourceManager.Init();

            //对象池管理器
            m_PoolManager = new PoolManager();
            //m_PoolManager.Init();

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
        {
            DoExitScene();
        }

        private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            DoStartScene();
        }

        /// <summary>
        /// 激活变体
        /// </summary>
        /// <param name="variants"></param>
        public void ActivateVariants(string[] variants)
        {
            if (enableAssetBundle)
            {
                m_AssetBundleManager.ActivateVariants(variants);
            }
        }

        /// <summary>
        /// 开始场景时调用 - 为该场景资源加载/管理做好准备
        /// </summary>
        private void DoStartScene()
        {
            m_PoolManager.Init();
        }

        /// <summary>
        /// 结束场景时调用 - 为该场景释放所有的资源
        /// </summary>
        private void DoExitScene()
        {
            m_CacheAssetDict.Clear();
            m_CacheSpriteDict.Clear();

            m_PoolManager.DoDestroy();

            if (enableAssetBundle)
            {
                m_AssetBundleManager.DoExitScene();
            }

            m_ResourceManager.DoExitScene();
        }

        /// <summary>
        /// 卸载没有引用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 缓存列表
        /// </summary>
        /// <param name="list">资源ID列表</param>
        /// <param name="cacheNum">数量</param>
        public void CacheObjectList(List<int> list, int cacheNum = 1)
        {
            if (list.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                CacheObject(list[i], cacheNum);
            }
        }

        /// <summary>
        /// 缓存GameObject
        /// </summary>
        /// <param name="assetId">资源ID</param>
        /// <param name="_num">数量</param>
        public void CacheObject(int assetId, int cacheNum = 1)
        {
            if (cacheNum <= 0)
            {
                Debug.LogError("");
                return;
            }

            //缓存资源
            GameObject prefab = LoadAsset<Object>(assetId) as GameObject;
            if (prefab == null)
            {
                //非GameObject
                return;
            }

            for (int i = 0; i < cacheNum; i++)
            {
                GameObject instant = Instantiate(prefab, false);
                //回收到缓存池
                m_PoolManager.ReturnToPool(instant);
            }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public Object LoadAsset(int assetId)
        {
            return LoadAsset<Object>(assetId);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T LoadAsset<T>(int assetId) where T : Object
        {
            if (assetId <= 0)
            {
                Debug.LogError("资源ID无效 _asset_id=" + assetId);
                return null;
            }

            T asset = null;

            //从缓存获取
            asset = GetAssetFromCache(assetId) as T;

            if (enableAssetBundle)
            {
                //从Assetbundle获取
                asset = m_AssetBundleManager.LoadAsset<T>(assetId);
            }

            //从Resource获取
            if (asset == null)
            {
                asset = m_ResourceManager.LoadAsset<T>(assetId);
            }

#if UNITY_EDITOR
            //从AssetDataBase获取
            if (asset == null && enableAssetBundle == false)
            {
                asset = m_AssetDatabaseManager.LoadAsset<T>(assetId);
            }
#endif

            PutAssetToCache(assetId, asset);
            return asset;
        }

        /// <summary>
        /// 加载全部资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(int assetId) where T : UnityEngine.Object
        {
            if (assetId <= 0)
            {
                Debug.LogError("资源ID无效 _asset_id=" + assetId);
                return null;
            }
            T[] assets = null;

            if (enableAssetBundle)
            {
                //从Assetbundle获取
                assets = m_AssetBundleManager.LoadAllAsset<T>(assetId);
                if (Utils.IsCollectionNullOrEmpty(assets) == false)
                {
                    return assets;
                }
            }

            //Resources模式
            assets = m_ResourceManager.LoadAllAsset<T>(assetId);
            if (Utils.IsCollectionNullOrEmpty(assets) == false)
            {
                return assets;
            }

#if UNITY_EDITOR
            if (enableAssetBundle == false)
            {
                //AssetDatabase模式
                assets = m_AssetDatabaseManager.LoadAllAsset<T>(assetId);
            }
#endif
            if (Utils.IsCollectionNullOrEmpty(assets))
            {
                Debug.LogErrorFormat("资源加载失败！assetId={0}", assetId);
            }

            return assets;
        }

        #region 异步加载
        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="callBack"></param>
        public void LoadAssetAsync(int assetId, ObjectCallback callBack, IsObjectOldFunc func)
        {
            if (assetId <= 0)
            {
                Debug.LogError("资源ID无效 _asset_id=" + assetId);
                Utils.OnCallBack(callBack, null, false);
                return;
            }

            UnityEngine.Object prefab = null;

            //从缓存获取
            prefab = GetAssetFromCache(assetId);
            if (prefab != null)
            {
                Utils.OnCallBack(callBack, prefab, false);
                return;
            }

            if (enableAssetBundle)
            {
                //从Assetbundle加载
                m_AssetBundleManager.LoadAssetAsync(assetId, (asset, isOld) =>
                {
                    if (asset == null)
                    {
                        LoadAssetAsyncFromResourcesAndAssetDatabase(assetId, callBack, func);
                        return;
                    }
                    PutAssetToCache(assetId, asset);
                    Utils.OnCallBack(callBack, asset, func);
                }, func);
            }
            else
            {
                //从Resources加载
                LoadAssetAsyncFromResourcesAndAssetDatabase(assetId, callBack, func);
            }
        }

        /// <summary>
        /// 从Resources和AssetDatabase加载资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="callBack"></param>
        /// <param name="func"></param>
        private void LoadAssetAsyncFromResourcesAndAssetDatabase(int assetId, ObjectCallback callBack, IsObjectOldFunc func)
        {
            //从Resources加载
            m_ResourceManager.LoadAssetAsync(assetId, (asset, isOld) =>
            {
#if UNITY_EDITOR
                if (asset == null && enableAssetBundle == false)
                {
                    //编辑器下尝试从AssetDatabase加载
                    asset = m_AssetDatabaseManager.LoadAsset<Object>(assetId);
                }
#endif
                PutAssetToCache(assetId, asset);
                Utils.OnCallBack(callBack, asset, func);
            }, func);
        }
        #endregion

        #region 缓存
        /// <summary>
        /// 加载资源 - 从缓存中获取
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        private Object GetAssetFromCache(int assetId)
        {
            CacheInfo info = null;
            if (m_CacheAssetDict.TryGetValue(assetId, out info) == false)
            {
                return null;
            }

            info.Use();
            return info.asset;
        }

        /// <summary>
        /// 缓存预设体
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="asset"></param>
        private void PutAssetToCache(int assetId, Object asset)
        {
            if (asset == null)
            {
                return;
            }

            m_CacheAssetDict[assetId] = new CacheInfo(asset, assetId);
        }
        #endregion

        #region GameObject
        /// <summary>
        /// 实例化GameObject - 通过assetId
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="getFromPool"></param>
        /// <returns></returns>
        public GameObject LoadAssetAndInstantiate(int assetId, bool getFromPool = true)
        {
            return LoadAssetAndInstantiate(assetId, Vector3.zero, Quaternion.identity, getFromPool);
        }

        /// <summary>
        /// 实例化GameObject - 通过assetId
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="getFromPool"></param>
        /// <returns></returns>
        public GameObject LoadAssetAndInstantiate(int assetId, Vector3 position, Quaternion rotation, bool getFromPool = true)
        {
            GameObject go = null;

            //从缓存管理器中获取
            if (getFromPool)
            {
                AssetInfo info = AssetInfo.GetAssetInfo(assetId);
                if (info == null)
                {
                    return null;
                }
                go = m_PoolManager.GetFromPool(info.assetName);
                if (go != null)
                {
                    go.transform.SetPositionAndRotation(position, rotation);
                    return go;
                }
            }

            //需要加载资源
            GameObject prefab = LoadAsset<GameObject>(assetId);
            if (prefab == null)
            {
                return null;
            }

            return Instantiate(prefab, position, rotation, false);
        }

        /// <summary>
        /// 实例化GameObject
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="getFromPool"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject prefab, bool getFromPool = true)
        {
            return Instantiate(prefab, Vector3.zero, Quaternion.identity, getFromPool);
        }

        /// <summary>
        /// 实例化GameObject
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="getFromPool"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool getFromPool = true)
        {
            if (prefab == null)
            {
                return null;
            }

            string gameObjectName = prefab.name;
            GameObject go = null;

            //从缓存管理器中获取
            if (getFromPool)
            {
                go = m_PoolManager.GetFromPool(gameObjectName);
                if (go != null)
                {
                    go.transform.SetPositionAndRotation(position, rotation);
                    return go;
                }
            }

            //直接实例化
            go = UnityEngine.Object.Instantiate(prefab, position, rotation) as GameObject;
            //设置gameObject名与模版一致。
            go.name = gameObjectName;

            // 缓存到材质预设字典里头
            m_PoolManager.AddToPrefabMap(go, prefab);
            m_PoolManager.CallRecycleInterface(go, true);
            return go;
        }

        /// <summary>
        /// 异步加载资源物件并实例化
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="callBack"></param>
        /// <param name="func"></param>
        /// <param name="getFromPool"></param>
        public void LoadAssetAsyncAndInstantiate(int assetId, GameObjectCallback callBack, IsObjectOldFunc func, bool getFromPool = true)
        {
            LoadAssetAsyncAndInstantiate(assetId, Vector3.zero, Quaternion.identity, callBack, func, getFromPool);
        }

        /// <summary>
        /// 异步加载资源物件并实例化
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="position">初始位置</param>
        /// <param name="rotation">初始角度</param>
        /// <param name="getFromPool">是否从缓存中拿</param>
        /// <returns></returns>
        public void LoadAssetAsyncAndInstantiate(int assetId, Vector3 position, Quaternion rotation, GameObjectCallback callBack,
            IsObjectOldFunc func, bool getFromPool = true)
        {
            GameObject go = null;

            //从缓存管理器中获取
            if (getFromPool)
            {
                AssetInfo info = AssetInfo.GetAssetInfo(assetId);
                if (info == null)
                {
                    Utils.OnCallBack(callBack, null, false);
                    return;
                }
                go = m_PoolManager.GetFromPool(info.assetName);
                if (go != null)
                {
                    go.transform.SetPositionAndRotation(position, rotation);
                    Utils.OnCallBack(callBack, go, false);
                    return;
                }
            }

            //异步加载
            LoadAssetAsync(assetId, (prefab, isOld) =>
            {
                if (prefab == null)
                {
                    Utils.OnCallBack(callBack, null, isOld);
                    return;
                }

                //非GameObject资源
                go = prefab as GameObject;
                if (go == null)
                {
                    Utils.OnCallBack(callBack, null, isOld);
                    return;
                }

                //已过时
                if (isOld)
                {
                    Utils.OnCallBack(callBack, go, isOld);
                    return;
                }

                //实例化
                go = Instantiate(go, false);
                Utils.OnCallBack(callBack, go, isOld);

            }, func);
        }

        /// <summary>
        /// 回收GameObject，放回缓存池
        /// </summary>
        /// <param name="go"></param>
        public void RecycleGameObject(GameObject go)
        {
            m_PoolManager.ReturnToPool(go);
        }

        /// <summary>
        /// 销毁GameObject
        /// </summary>
        public void DestroyGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            //因为Destroy是延时销毁，所以安全起见，销毁前隐藏以及取消挂载节点
            //==================================//
            gameObject.SetActive(false);
            if (gameObject.transform != null)
            {
                gameObject.transform.SetParent(null);
            }
            //==================================//

            Object.Destroy(gameObject);
        }

        #endregion

        #region Sprite

        /// <summary>
        /// 加载Sprite
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite LoadSprite(int assetId, string spriteName = "")
        {
            //检查资源信息
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                return null;
            }
            //如果名字为null，表示Sprite的类型是Single，跟文件名一致
            if (spriteName == null)
            {
                spriteName = info.assetName;
            }

            Sprite resultSprite = null;
            SpriteCollection spriteCollection = LoadSpriteCollection(assetId);
            if (spriteCollection != null)
            {
                spriteCollection.TryGetSprite(spriteName, out resultSprite);
            }
            return resultSprite;
        }

        /// <summary>
        /// 加载Sprite
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="spriteName"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public Sprite LoadSprite(string resourcesPath, string spriteName = "", string suffix = ".png")
        {
            Sprite resultSprite = null;
            SpriteCollection spriteCollection = LoadSpriteCollection(resourcesPath, suffix);
            if (spriteCollection != null)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    resultSprite = spriteCollection.GetSingleSprite();
                }
                else
                {
                    spriteCollection.TryGetSprite(spriteName, out resultSprite);
                }
            }
            return resultSprite;
        }

        /// <summary>
        /// 加载全部Sprite
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public Sprite[] LoadAllSprite(int assetId)
        {
            SpriteCollection spriteCollection = LoadSpriteCollection(assetId);
            if (spriteCollection != null)
            {
                return spriteCollection.GetAllSprite();
            }
            return null;
        }

        /// <summary>
        /// 加载全部Sprite
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public Sprite[] LoadAllSprite(string resourcesPath, string suffix = ".png")
        {
            SpriteCollection spriteCollection = LoadSpriteCollection(resourcesPath, suffix);
            if (spriteCollection != null)
            {
                return spriteCollection.GetAllSprite();
            }
            return null;
        }

        /// <summary>
        /// 获取SpriteCollection
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public SpriteCollection LoadSpriteCollection(int assetId)
        {
            //检查资源信息
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                return null;
            }

            //从缓存中拿
            SpriteCollection spriteCollection = null;
            if (m_CacheSpriteDict.TryGetValue(info.assetPath, out spriteCollection))
            {
                return spriteCollection;
            }

            //从原始资源中拿
            return CacheSprite(assetId);
        }

        /// <summary>
        /// 获取SpriteCollection
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public SpriteCollection LoadSpriteCollection(string resourcesPath, string suffix = ".png")
        {
            string assetPath = Utils.ToAssetPath(resourcesPath, suffix);

            //从缓存中拿
            SpriteCollection spriteCollection = null;
            if (m_CacheSpriteDict.TryGetValue(assetPath, out spriteCollection))
            {
                return spriteCollection;
            }

            //从原始资源中拿
            return CacheSprite(resourcesPath, suffix);
        }

        /// <summary>
        /// 缓存Sprite
        /// </summary>
        /// <param name="assetId"></param>
        public SpriteCollection CacheSprite(int assetId)
        {
            //检查资源信息
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                return null;
            }

            Sprite[] allSprite = LoadAllAsset<Sprite>(assetId);
            if (allSprite == null || allSprite.Length <= 0)
            {
                Debug.LogErrorFormat("该资源没有Sprite！assetId={0}", assetId);
                return null;
            }

            //缓存资源文件
            SpriteCollection spriteCollection = new SpriteCollection(allSprite);
            m_CacheSpriteDict.Add(info.assetPath, spriteCollection);
            return spriteCollection;
        }

        /// <summary>
        /// 缓存Sprite
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public SpriteCollection CacheSprite(string resourcesPath, string suffix = ".png")
        {
            Sprite[] allSprite = LoadAllAsset<Sprite>(resourcesPath, suffix);
            if (allSprite == null || allSprite.Length <= 0)
            {
                Debug.LogErrorFormat("该资源没有Sprite！resourcesPath={0}", resourcesPath);
                return null;
            }

            string assetPath = Utils.ToAssetPath(resourcesPath, suffix);

            //缓存资源文件
            SpriteCollection spriteCollection = new SpriteCollection(allSprite);
            m_CacheSpriteDict.Add(assetPath, spriteCollection);
            return spriteCollection;
        }

        #endregion

        #region 不走AssetID流程的API
        /// <summary>
        /// 用于加载Data目录的数据文件
        /// </summary>
        /// <param name="originName"></param>
        /// <returns></returns>
        public byte[] LoadBytes(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            byte[] bytes = null;
            string filePath = path;

            //映射数据路径
            if (IsDataPath(path))
            {
                filePath = GetActualDataPath(filePath);
            }

            //从文件加载
            if (File.Exists(filePath))
            {
                bytes = File.ReadAllBytes(filePath);
            }

            if (bytes != null)
            {
                //解密
                if (GameSetting.encrypt && GameSetting.developMode == false)
                {
                    bytes = EncryptTool.Decrypt(bytes);
                }
            }

            //从内部加载
            if (bytes == null)
            {
                //确保去掉后缀名
                string resourcesPath = StringUtil.GetPrefix(path, ".");
                //注意:Resources.Load是unity内部接口，无论什么平台，目录连接都要使用"/"
                resourcesPath = resourcesPath.Replace("\\", "/");

                TextAsset asset = LoadAsset<TextAsset>(resourcesPath, StringUtil.GetSuffix(path, "."));

                if (asset == null)
                {
                    //Debug.LogErrorFormat("从内部加载文件数据失败！fileName={0}", resourcesPath);
                    return null;
                }
                bytes = asset.bytes;
            }
            return bytes;
        }

        /// <summary>
        /// 是否为数据路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsDataPath(string path)
        {
            return path.ToLower().StartsWith(AssetPathDefine.dataFolderLower);
        }

        /// <summary>
        /// 获取实际数据文件路径
        /// </summary>
        /// <param name="dataPath"></param>
        /// <returns></returns>
        public static string GetActualDataPath(string dataPath)
        {
            return (GameSetting.developMode ? AssetPathDefine.developDataPath : AssetPathDefine.externalDataPath) + dataPath.Substring(AssetPathDefine.dataFolderName.Length);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        public void LoadSceneBundle(string sceneName)
        {
            if (enableAssetBundle == false)
            {
                return;
            }

            m_AssetBundleManager.LoadSceneBundle(sceneName);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>
        public void LoadSceneBundleAsync(string sceneName, System.Action callBack)
        {
            if (enableAssetBundle == false)
            {
                return;
            }
            m_AssetBundleManager.LoadSceneBundleAsync(sceneName, callBack);
        }

        /// <summary>
        /// 通过路径加载资源
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public Object LoadAsset(string resourcesPath, string suffix = ".prefab")
        {
            return LoadAsset<Object>(resourcesPath, suffix);
        }

        /// <summary>
        /// 通过路径加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string resourcesPath, string suffix = ".prefab") where T : Object
        {
            T asset = null;

            if (enableAssetBundle)
            {
                //从Assetbundle获取
                string assetPath = Utils.ToAssetPath(resourcesPath, suffix);
                asset = m_AssetBundleManager.LoadAsset<T>(assetPath);
            }

#if UNITY_EDITOR
            //编辑器下从其它目录获取
            if (asset == null && enableAssetBundle == false)
            {
                string assetPath = Utils.ToAssetPath(resourcesPath, suffix);
                asset = m_AssetDatabaseManager.LoadAsset<T>(assetPath);
            }
#endif
            if (asset == null)
            {
                //从resources获取
                asset = m_ResourceManager.LoadAsset<T>(resourcesPath);
            }
            return asset;
        }

        /// <summary>
        /// 通过路径加载所有资源
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public Object[] LoadAllAsset(string resourcesPath, string suffix = ".prefab")
        {
            return LoadAllAsset<Object>(resourcesPath, suffix);
        }

        /// <summary>
        /// 通过路径加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourcesPath"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(string resourcesPath, string suffix = ".prefab") where T : UnityEngine.Object
        {
            T[] assets = null;

            if (enableAssetBundle)
            {
                //从Assetbundle获取
                string assetPath = Utils.ToAssetPath(resourcesPath, suffix);
                assets = m_AssetBundleManager.LoadAllAsset<T>(assetPath);
                if (Utils.IsCollectionNullOrEmpty(assets) == false)
                {
                    return assets;
                }
            }

            //Resources模式
            assets = m_ResourceManager.LoadAllAsset<T>(resourcesPath);
            if (Utils.IsCollectionNullOrEmpty(assets) == false)
            {
                return assets;
            }

#if UNITY_EDITOR
            if (enableAssetBundle == false)
            {
                //AssetDatabase模式
                string assetPath = Utils.ToAssetPath(resourcesPath, suffix);
                assets = m_AssetDatabaseManager.LoadAllAsset<T>(assetPath);
            }
#endif
            if (Utils.IsCollectionNullOrEmpty(assets))
            {
                Debug.LogErrorFormat("资源加载失败！path={0}", resourcesPath);
            }

            return assets;
        }

        #endregion
    }
}
