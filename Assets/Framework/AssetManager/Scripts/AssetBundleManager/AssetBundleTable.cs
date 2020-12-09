using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Framework.AssetManager
{
    public class AssetBundleTable
    {
        [Serializable]
        public class BundleTableInfo
        {
            [SerializeField]
            public string id;
            [SerializeField]
            public string abn;
            [SerializeField]
            public string path;

            public BundleTableInfo()
            {

            }

            public BundleTableInfo(string assetid, string bundleName, string path)
            {
                this.abn = bundleName;
                this.id = assetid;
                this.path = path;
            }
        }
        private Dictionary<string, BundleTableInfo> _allBundleDict = new Dictionary<string, BundleTableInfo>();

        private bool _isInit = false;

        private const string NAME = "bundletable";

        public bool Init(AssetBundleManager manager)
        {
            if (_isInit)
                return true;
            _allBundleDict.Clear();
            string fullPath = manager.GetAssetsBundleFullPath(NAME);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            BundleTableAsset bundleTableAsset = assetBundle.LoadAsset<BundleTableAsset>(NAME);
            if(bundleTableAsset == null)
            {
                Debug.LogError("Load Scriptable Object failed");
                return false;
            }
            try
            {
                foreach (var assetBundleTable in bundleTableAsset.listBundleTable)
                {
                    string assetid = assetBundleTable.id;
                    if (_allBundleDict.ContainsKey(assetid))
                    {
                        Debug.LogError("=====has the same key!!!==" + assetid);
                    }
                    else
                    {
                        _allBundleDict.Add(assetid, assetBundleTable);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            assetBundle.Unload(true);

            _isInit = true;

            return true;
        }

        // <summary>
        /// 根据id(bundle名字)获取assetname。全路径包含后缀名
        /// </summary>
        /// <returns>The asset name by I.</returns>
        /// <param name="bundleId">Bundle identifier.</param>
        public string GetAssetNameByID(string assetId)
        {
            BundleTableInfo info;
            if (_allBundleDict.TryGetValue(assetId, out info))
            {
                return info.path;
            }
		    Debug.LogError("===bundle log：找不到该bundle,bundleid = "+ assetId.ToString());
            return null;
        }

        /// <summary>
        /// 根据id(bundle名字)获取assetname。只是名字，不包含路径和后缀名
        /// </summary>
        /// <returns>The asset name by I.</returns>
        /// <param name="bundleId">Bundle identifier.</param>
        public string GetAssetNameWithoutExtensionByID(string assetId)
        {
            string _assetName = GetAssetNameByID(assetId);
            if (string.IsNullOrEmpty(_assetName))
            {
                return null;
            }
            return System.IO.Path.GetFileNameWithoutExtension(_assetName);

        }
        public BundleTableInfo GetBundleTableInfo(string assetId)
        {
            BundleTableInfo info;
            if (_allBundleDict.TryGetValue(assetId, out info))
            {
                return info;
            }

            return null;

        }

        /// <summary>
        /// 清理ab 表格的缓存
        /// </summary>
        public void CleanAssetBundleTable()
        {
            _isInit = false;
            if (_allBundleDict != null)
                _allBundleDict.Clear();
        }


    }

}

