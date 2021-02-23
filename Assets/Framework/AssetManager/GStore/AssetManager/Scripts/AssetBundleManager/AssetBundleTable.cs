using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using GStore;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace GStore
{
    public class AssetBundleTable
    {
        [Serializable]
        public class BundleTableInfo
        {
            /// <summary>
            /// 资源id
            /// </summary>
            /// <value>The path.</value>
            [SerializeField]
            public string id;

            /// <summary>
            /// AssetBundleName
            /// </summary>
            /// <value>The I.</value>
            [SerializeField]
            public string abn;

            /// <summary>
            /// 资源原来的路径
            /// </summary>
            /// <value>The path.</value>
            [SerializeField]
            public string path;

            /// <summary>
            /// 空构造方法，用于反序列化
            /// </summary>
            public BundleTableInfo()
            {

            }

            public BundleTableInfo(string assetid, string bundleName, string path)
            {
                this.abn = bundleName;
                this.path = path;
                this.id = assetid;
            }
        }

        /// <summary>
        /// The m all bundle dict.
        /// </summary>
        private Dictionary<string, BundleTableInfo> m_AllBundleDict = new Dictionary<string, BundleTableInfo>();
        /// <summary>
        /// 是否初始化了
        /// </summary>
        private bool m_IsInit = false;

        private const string NAME = "bundletable";

        /// <summary>
        /// 初始化bundle表
        /// </summary>
        /// <returns><c>true</c>, if bundle table was inited, <c>false</c> otherwise.</returns>
        public bool Init(AssetBundleManager manager)
        {
            if (m_IsInit)
                return true;

            m_AllBundleDict.Clear();
            string fullPath = manager.GetAssetsBundleFullPath(NAME);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(fullPath);
            BundleTableAsset bundleTableAsset = assetBundle.LoadAsset<BundleTableAsset>(NAME);

            if (bundleTableAsset == null)
            {
                Debug.LogError("Load Scriptable Object failed");
                return false;
            }
            try
            {
                //Profiler.BeginSample("Load Bundle Table ScriptableObject");
                foreach (var assetBundleTable in bundleTableAsset.listBundleTable)
                {
                    string assetid = assetBundleTable.id;
                    if (m_AllBundleDict.ContainsKey(assetid))
                    {
                        Debug.LogError("=====has the same key!!!==" + assetid);
                    }
                    else
                    {
                        m_AllBundleDict.Add(assetid, assetBundleTable);
                    }
                }
                //Profiler.EndSample();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            assetBundle.Unload(true);

            m_IsInit = true;

            return true;
        }

        /// <summary>
        /// 根据id(bundle名字)获取assetname。全路径包含后缀名
        /// </summary>
        /// <returns>The asset name by I.</returns>
        /// <param name="bundleId">Bundle identifier.</param>
        public string GetAssetNameByID(string assetId)
        {
            BundleTableInfo info;
            if (m_AllBundleDict.TryGetValue(assetId, out info))
            {
                return info.path;
            }

#if WLOG
		Debug.LogError("===bundle log：找不到该bundle,bundleid = "+ assetId.ToString());
#endif
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
            if (m_AllBundleDict.TryGetValue(assetId, out info))
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
            m_IsInit = false;
            if (m_AllBundleDict != null)
                m_AllBundleDict.Clear();
        }
    }
}
