using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Base
{
    public class AssetPathDefine
    {
        public static string dataFolderName = "Data";
        public static string dataFolderLower
        {
            get
            {
                using (gstring.Block())
                {
                    return gstring.Format("{0}/", dataFolderName.ToLower());
                }
            }
        } 
        public static string NoHotUpdateDataZipName = "Data.zip";
        public static string HotUpdateDataZipNamePrefix = "GUData";

        public static string HotUpdateDataZipName
        {
            get
            {
                using (gstring.Block())
                {
                    return gstring.Format("{0}.zip", HotUpdateDataZipNamePrefix);
                }
            }
        }
        /// <summary>
        /// 项目中存放资源的目录
        /// </summary>
        public static string resFolder
        {
            get { return "Assets/Res/"; }
        }
        /// <summary>
        /// 资源存放的基本目录（持久化目录）
        /// </summary>
        public static string webBasePath
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                using (gstring.Block())
                {
                    return gstring.Format("{0}/../HotUpdate/", Application.dataPath);
                }
                
#else
                return Application.persistentDataPath;
#endif
            }
        }
        /// <summary>
        /// 存放下载资源的目录
        /// </summary>
        private static string _externalFilePath = string.Empty;
        public static string externalFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_externalFilePath))
                {
                    _externalFilePath = System.IO.Path.Combine(webBasePath, "http_res");
                }
                return _externalFilePath;
            }
        }

        /// <summary>
        /// 存放分包资源的目录
        /// </summary>
        private static string _subPackageFilePath = string.Empty;
        public static string subPackageFilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_subPackageFilePath))
                {
                    _subPackageFilePath = System.IO.Path.Combine(webBasePath, "SubPackage");
                }
                return _subPackageFilePath;
            }
        }

        /// <summary>
        /// 存放数据文件的目录
        /// </summary>
        private static string _externalDataPath = string.Empty;
        public static string externalDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_externalDataPath))
                {
                    _externalDataPath = System.IO.Path.Combine(externalFilePath, dataFolderName.ToLower());
                }

                return _externalDataPath;
            }
        }

        /// <summary>
        /// 存放数据文件的目录 - 开发模式
        /// </summary>
        private static string _developDataPath = string.Empty;
        public static string developDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_developDataPath))
                {
                    _developDataPath = Application.dataPath + "/../../content/data";
                }

                return _developDataPath;
            }
        }

        /// <summary>
        /// 项目中数据文件路径
        /// </summary>
        private static string _projectDataPath = string.Empty;
        public static string projectDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_projectDataPath))
                {
                    _projectDataPath = resFolder + dataFolderName;
                }
                return _projectDataPath;
            }
        }

        /// <summary>
        /// Resources中的数据文件路径
        /// </summary>
        public static string resourcesDataPath
        {
            get { return "Assets/Resources/Data"; }
        }

        /// <summary>
        /// 打包后的数据文件路径(非热更数据压缩包)
        /// </summary>
        private static string _packedDataPath = string.Empty;
        public static string packedDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_packedDataPath))
                {
                    _packedDataPath = dataFolder + "/" + NoHotUpdateDataZipName;
                }
                return _packedDataPath;
            }
        }

        /// <summary>
        /// 打包后的数据文件路径(非热更数据压缩包)
        /// </summary>
        private static string _GUPackedDataPath = string.Empty;
        /// <summary>
        /// 热更的数据压缩包
        /// </summary>
        public static string guPackedDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_GUPackedDataPath))
                {
                    _GUPackedDataPath = dataFolder + "/" + HotUpdateDataZipName;
                }
                return _GUPackedDataPath;
            }
        }

        /// <summary>
        /// 数据zip目录
        /// </summary>
        public static string dataFolder
        {
            get { return Application.streamingAssetsPath + "/" + dataFolderName; }
        }

        /// <summary>
        /// 存放bundle的文件夹名
        /// </summary>
        public static string assetBundleFolder
        {
            get { return "AssetBundles"; }
        }

        /// <summary>
        /// bundle后缀
        /// </summary>
        public static string assetBundleExtension
        {
            get { return ".unity3d"; }
        }

        /// <summary>
        /// bundle表命名
        /// </summary>
        public static string bundleTableFileName
        {
            get { return "BundleTable.json"; }
        }

        /// <summary>
        /// 依赖bundle表
        /// </summary>
        public static string depBundleTableFileName
        {
            get { return "DepBundleTable.json"; }
        }

        /// <summary>
        /// 常驻内存列表
        /// </summary>
        public static string residentBundleTableName
        {
            get { return "ResidentBundles.json"; }
        }

        /// <summary>
        /// 指定的Bundle外部路径
        /// </summary>
        private static string _externalBundlePath = string.Empty;
        public static string externalBundlePath
        {
            get
            {
                if (string.IsNullOrEmpty(_externalBundlePath))
                {
                    _externalBundlePath = System.IO.Path.Combine(externalFilePath, assetBundleFolder);
                }
                return _externalBundlePath;
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// 指定的Bundle外部路径
        /// </summary>
        private static string _editorPath = string.Empty;
        public static string editorPath
        {
            get
            {
                if (string.IsNullOrEmpty(_editorPath))
                {
                    _editorPath = Application.dataPath + "/../ExtraResources";
                }
                return _editorPath;
            }
        }

        /// <summary>
        /// 指定的Bundle外部路径
        /// </summary>
        private static string _editorBundlePath = string.Empty;
        public static string editorBundlePath
        {
            get
            {
                if (string.IsNullOrEmpty(_editorBundlePath))
                {
                    var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
                    _editorBundlePath = string.Format("{0}/Bundle/{1}/{2}", editorPath, buildTarget, assetBundleFolder);
                }
                return _editorBundlePath;
            }
        }

        /// <summary>
        /// 指定的热更的GUData外部路径
        /// </summary>
        private static string _externalEditorGUDataPath = string.Empty;
        public static string externalEditorGUDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_externalEditorGUDataPath))
                {
                    _externalEditorGUDataPath = System.IO.Path.Combine(editorPath, "HotUpdata_GUData");
                }
                return _externalEditorGUDataPath;
            }
        }
#endif

    }
}

