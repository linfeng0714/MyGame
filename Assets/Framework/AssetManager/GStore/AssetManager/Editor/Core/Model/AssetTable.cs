using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 封装AssetInfo，提供编辑器下专用的接口
    /// </summary>
    public class AssetTable
    {
        /// <summary>
        /// 表格信息
        /// </summary>
        private static readonly string s_TablePath = CSVHelper.Combine("assets.bytes");
        private static long s_TableTimestamp = 0;

        /// <summary>
        /// 资源表反向查询字典
        /// </summary>
        private static Dictionary<string, AssetInfo> m_AssetPathDict = new Dictionary<string, AssetInfo>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 文件夹查询字典
        /// </summary>
        private static Dictionary<string, AssetInfo> m_FolderDict = new Dictionary<string, AssetInfo>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<string, AssetInfo> AssetPathMap
        {
            get
            {
                //检查表格数据是否有更新
                if (CheckReloadTable())
                {
                    AssetInfo.Load(true);
                    Load();
                }

                return m_AssetPathDict;
            }
        }

        public static Dictionary<string, AssetInfo> FolderMap
        {
            get
            {
                //检查表格数据是否有更新
                if (CheckReloadTable())
                {
                    AssetInfo.Load(true);
                    Load();
                }

                return m_FolderDict;
            }
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, AssetInfo>.Enumerator GetEnumerator()
        {
            //检查表格数据是否有更新
            if (CheckReloadTable())
            {
                AssetInfo.Load(true);
                Load();
            }
            return AssetInfo.GetEnumerator();
        }

        private static void Load()
        {
            m_AssetPathDict.Clear();
            m_FolderDict.Clear();
            var itor = AssetInfo.GetEnumerator();
            while (itor.MoveNext())
            {
                AssetInfo assetInfo = itor.Current.Value;
                string assetPath = assetInfo.assetPath;
                AssetInfo existInfo;
                if (m_AssetPathDict.TryGetValue(assetPath, out existInfo))
                {
                    //有重复资源
                }
                else
                {
                    m_AssetPathDict[assetPath] = assetInfo;
                }

                if (Directory.Exists(assetPath))
                {
                    m_FolderDict[assetPath] = assetInfo;
                }
            }
            itor.Dispose();
        }


        /// <summary>
        /// 检查资源表是否时需要重载
        /// </summary>
        /// <returns></returns>
        private static bool CheckReloadTable()
        {
            string filePath = AssetManager.GetActualDataPath(s_TablePath);
            if (File.Exists(filePath) == false)
            {
                //工程内部路径
                filePath = AssetPathDefine.resFolder + s_TablePath;

                if (File.Exists(filePath) == false)
                {
                    Debug.LogErrorFormat("找不到资源表!");
                }
            }

            var fileInfo = new FileInfo(filePath);

            long timestamp = fileInfo.LastWriteTime.ToFileTime();
            if (s_TableTimestamp != timestamp)
            {
                s_TableTimestamp = timestamp;
                return true;
            }
            return false;
        }
    }
}
