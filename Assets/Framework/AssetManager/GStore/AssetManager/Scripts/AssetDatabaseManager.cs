#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GStore;

namespace GStore
{
    /// <summary>
    /// 通过AssetDatabase加载的资源，仅Editor环境可用
    /// </summary>
    public class AssetDatabaseManager : IAssetLoader
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAllAsset(int assetId)
        {
            return LoadAllAsset<UnityEngine.Object>(assetId);
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(int assetId) where T : UnityEngine.Object
        {
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                return null;
            }

            return LoadAllAsset<T>(info.assetPath);
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

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T[] assets = null;

            if (Directory.Exists(assetPath))
            {
                assets = LoadAllFromFolder<T>(assetPath);
            }
            else
            {
                assets = LoadAllFromFile<T>(assetPath);
            }
            return assets;
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(int assetId)
        {
            return LoadAsset<UnityEngine.Object>(assetId);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T LoadAsset<T>(int assetId) where T : UnityEngine.Object
        {
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                return null;
            }

            return LoadAsset<T>(info.assetPath);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(string assetPath)
        {
            return LoadAsset<UnityEngine.Object>(assetPath);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset is GameObject)
            {
                string assetName = Path.GetFileNameWithoutExtension(assetPath);
                if (assetName != asset.name)
                {
                    Debug.LogWarningFormat("加载资源Asset名称和预设实际名称不一致, AssetDataBase 存在这个刷新bug, prefabName={0}, assetName={1}", asset.name, assetName);
                    //改正资源名，防止gameObject对象池出错
                    asset.name = assetName;
                }
            }

            return asset;
        }

        /// <summary>
        /// 从文件中加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private T[] LoadAllFromFile<T>(string assetPath) where T : UnityEngine.Object
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (objs is T[])
            {
                return objs as T[];
            }
            else
            {
                List<T> objList = new List<T>();
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] is T)
                    {
                        objList.Add(objs[i] as T);
                    }
                }
                return objList.ToArray();
            }
        }

        /// <summary>
        /// 从文件夹中加载所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folder"></param>
        /// <returns></returns>
        public T[] LoadAllFromFolder<T>(string folder) where T : UnityEngine.Object
        {
            string[] directoryEntries;
            List<T> objList = null;
            directoryEntries = System.IO.Directory.GetFileSystemEntries(folder);

            for (int i = 0; i < directoryEntries.Length; i++)
            {
                string path = directoryEntries[i].Replace("\\", "/");
                if (path.EndsWith(".meta"))
                {
                    continue;
                }
    ;
                T tempTex = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
                if (tempTex != null)
                {
                    if (objList == null)
                    {
                        objList = new List<T>();
                    }
                    objList.Add(tempTex);
                }
            }
            if (objList.Count > 0)
                return objList.ToArray();
            return null;
        }

        public void LoadAssetAsync(int assetId, ObjectCallback callBack, IsObjectOldFunc func)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 加载文件数据
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
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
    }
}
#endif
