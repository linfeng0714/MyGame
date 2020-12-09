using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class AssetManager : Singleton<AssetManager>
    {
        //是否从Ab加载资源
        private bool _enableAssetBundleForEditor;

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


    }
}

