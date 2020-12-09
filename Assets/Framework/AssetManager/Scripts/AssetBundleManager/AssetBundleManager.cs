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

        private Ass

        public T[] LoadAllAsset<T>(int assetId) where T : Object
        {
           
        }

        public T LoadAsset<T>(int assetId) where T : Object
        {
            
        }

        public byte[] LoadFileData(string filePath)
        {
            
        }
    }
}
