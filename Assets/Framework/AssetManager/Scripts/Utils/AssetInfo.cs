using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    /// <summary>
    /// 资源表信息
    /// </summary>
    public class AssetInfo
    {
        #region 静态方法
        /// <summary>
        /// 资源数据库
        /// key = 资源唯一ID
        /// value = 资源信息
        /// </summary>
        private static Dictionary<int, AssetInfo> m_AssetInfoDict = new Dictionary<int, AssetInfo>();

        /// <summary>
        /// 场景字典
        /// </summary>
        private static Dictionary<string, AssetInfo> m_SceneDict = new Dictionary<string, AssetInfo>(System.StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, AssetInfo>.Enumerator GetEnumerator()
        {
            return m_AssetInfoDict.GetEnumerator();
        }

        /// <summary>
        /// 获取打包的场景
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, AssetInfo> GetBundledScenes()
        {
            return m_SceneDict;
        }

        static AssetInfo()
        {
            AssetManagerSetup.Setup();
            Load();
        }

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private static bool IsInitialized
        {
            get { return m_AssetInfoDict.Count > 0; }
        }

        /// <summary>
        /// 资源表获取委托
        /// </summary>
        public delegate List<AssetInfo> GetAssetTableHandler();
        public static GetAssetTableHandler getAssetTable;

        /// <summary>
        /// 载入所有AssetInfo
        /// </summary>
        /// <param name="assetTableAdapter"></param>
        public static void Load(bool force = false)
        {
            if (force == false && IsInitialized)
            {
                return;
            }

            if (getAssetTable == null)
            {
                Debug.LogErrorFormat("getAssetTable == null, 请先配置获取资源表的方法！");
                return;
            }

            List<AssetInfo> assetInfoList = getAssetTable();
            if (assetInfoList.Count <= 0)
            {
                return;
            }

            Clear();

            var iter = assetInfoList.GetEnumerator();
            while (iter.MoveNext())
            {
                var data = iter.Current;
                int key = data.id;

#if UNITY_EDITOR
                if (m_AssetInfoDict.ContainsKey(key))
                {
                    Debug.LogError("相同Key = " + key);
                    continue;
                }
#endif
                m_AssetInfoDict.Add(key, data);

                //记录场景文件
                if (data.suffix == ".unity")
                {
                    m_SceneDict.Add(data.m_AssetName, data);
                }
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        public static void Clear()
        {
            m_AssetInfoDict.Clear();
            m_SceneDict.Clear();
        }

        /// <summary>
        /// 获取AssetPath
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public static string GetAssetPath(int assetId)
        {
            AssetInfo assetInfo = GetAssetInfo(assetId);
            return assetInfo == null ? null : assetInfo.assetPath;
        }

        /// <summary>
        /// 获取资源数据
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public static AssetInfo GetAssetInfo(int assetId)
        {
            AssetInfo info = null;
            if (m_AssetInfoDict.TryGetValue(assetId, out info) == false)
            {
                Debug.LogError("找不到资源ID = " + assetId);
            }
            return info;
        }

        /// <summary>
        /// 获取场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public static AssetInfo GetSceneInfo(string sceneName)
        {
            AssetInfo info = null;
            if (m_SceneDict.TryGetValue(sceneName, out info) == false)
            {
            }
            return info;
        }
        #endregion

        /// <summary>
        /// 资源ID
        /// </summary>
        public int m_Id;
        public int id { get { return m_Id; } }

        /// <summary>
        /// 路径
        /// </summary>
        private string m_FolderName;

        /// <summary>
        /// 资源名字
        /// </summary>
        private string m_AssetName;
        public string assetName { get { return m_AssetName; } }

        /// <summary>
        /// 后缀名
        /// </summary>
        private string m_Suffix;
        public string suffix { get { return m_Suffix; } }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="assetName"></param>
        /// <param name="suffix"></param>
        public AssetInfo(int id, string folder, string assetName, string suffix)
        {
            m_Id = id;
            m_FolderName = folder;
            m_AssetName = assetName;
            m_Suffix = suffix;
            if (string.IsNullOrEmpty(m_AssetName))
            {
                m_Suffix = string.Empty;
            }
        }

        /// <summary>
        /// 资源目录下的文件路径
        /// </summary>
        private string m_ResourcesPath = null;
        public string resourcesPath
        {
            get
            {
                if (m_ResourcesPath == null)
                {
                    if (string.IsNullOrEmpty(m_AssetName))
                    {
                        m_ResourcesPath = folderPath;
                    }
                    else
                    {
                        m_ResourcesPath = string.Format("{0}/{1}", folderPath, m_AssetName);
                    }
                }
                return m_ResourcesPath;
            }
        }

        /// <summary>
        /// Resource下的目录路径
        /// </summary>
        private string m_FolderPath = null;
        private string folderPath
        {
            get
            {
                if (m_FolderPath == null)
                {
                    m_FolderPath = m_FolderName.Replace('_', '/');
                }
                return m_FolderPath;
            }
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        private string m_AssetPath = null;
        public string assetPath
        {
            get
            {
                if (m_AssetPath == null)
                {
                    m_AssetPath = Utils.ToAssetPath(resourcesPath, m_Suffix);
                }
                return m_AssetPath;
            }
        }

        /// <summary>
        /// 打印
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("id={0}, assetPath={1}", id, assetPath);
        }
    }
}
