using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class AssetManager : Singleton<AssetManager>
    {
        //是否从Ab加载资源
        private bool _enableAssetBundleForEditor = false;

        public bool enableAssetBundleForEditor
        {
            get
            {
#if UNITY_EDITOR
                return _enableAssetBundleForEditor;
#else
                return true;
#endif
            }
#if UNITY_EDITOR
            set
            {
                _enableAssetBundleForEditor = value;
            }
#endif
        }

        private Dictionary<int, CacheInfo> _cacheAssetDict = new Dictionary<int, CacheInfo>();

        private Dictionary<string, SpriteCollection> _cacheSpriteDict = new Dictionary<string, SpriteCollection>();

        /// <summary>
        /// AssetBundle管理器
        /// </summary>
        private AssetBundleManager m_AssetBundleManager = null;
        public AssetBundleManager assetBundleManager { get { return m_AssetBundleManager; } }

//        /// <summary>
//        /// 资源管理器
//        /// </summary>
//        private ResourceManager m_ResourceManager = null;
//        public ResourceManager resourceManager { get { return m_ResourceManager; } }

//#if UNITY_EDITOR
//        /// <summary>
//        /// AssetDatabase管理器
//        /// </summary>
//        private AssetDatabaseManager m_AssetDatabaseManager = null;
//        public AssetDatabaseManager assetDatabaseManager { get { return m_AssetDatabaseManager; } }
//#endif
//        /// <summary>
//        /// 对象池管理器
//        /// </summary>
//        private PoolManager m_PoolManager = null;
//        public PoolManager poolManager { get { return m_PoolManager; } }

    }
}

