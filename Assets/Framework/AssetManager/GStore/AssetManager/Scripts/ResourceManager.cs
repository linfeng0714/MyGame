using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;

namespace GStore
{
    /// <summary>
    /// Resources资源加载管理器
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>,IAssetLoader
    {
        #region 异步相关

        private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// 待执行的任务队列
        /// </summary>
        private Queue<AssetLoaderTask> m_LoaderTaskQueue = new Queue<AssetLoaderTask>();

        /// <summary>
        /// 最大同时异步加载数量
        /// </summary>
        private const int MAX_ASYNC_LOADING_COUNT = 2;

        /// <summary>
        /// 当前加载数量
        /// </summary>
        private int m_CurLoadingCount = 0;

        #endregion

        /// <summary>
        /// 初始化资源管理器
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 结束场景时调用
        /// </summary>
        public void DoExitScene()
        {
            //TODO:停止正在异步加载的任务

            m_LoaderTaskQueue.Clear();
            m_CurLoadingCount = 0;
        }

        #region 同步加载

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

            return LoadAsset<T>(info.resourcesPath);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <returns></returns>
        public UnityEngine.Object LoadAsset(string resourcesPath)
        {
            return LoadAsset<UnityEngine.Object>(resourcesPath);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string resourcesPath) where T : UnityEngine.Object
        {
            return Resources.Load<T>(resourcesPath);
        }

        /// <summary>
        /// 加载指定全部资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAllAsset(int assetId)
        {
            return LoadAllAsset<UnityEngine.Object>(assetId);
        }

        /// <summary>
        /// 加载指定全部资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(int assetId) where T : UnityEngine.Object
        {
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            return Resources.LoadAll<T>(info.resourcesPath);
        }

        /// <summary>
        /// 加载指定全部资源
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <returns></returns>
        public UnityEngine.Object[] LoadAllAsset(string resourcesPath)
        {
            return LoadAllAsset<UnityEngine.Object>(resourcesPath);
        }

        /// <summary>
        /// 加载指定全部资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourcesPath"></param>
        /// <returns></returns>
        public T[] LoadAllAsset<T>(string resourcesPath) where T : UnityEngine.Object
        {
            return Resources.LoadAll<T>(resourcesPath);
        }

        /// <summary>
        /// 加载文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public byte[] LoadFileData(string resourcesPath)
        {
            if (string.IsNullOrEmpty(resourcesPath))
            {
                return null;
            }

            TextAsset asset = LoadAsset<TextAsset>(resourcesPath);
            if (asset == null)
            {
                return null;
            }

            return asset.bytes;
        }

        #endregion

        #region 异步加载        

        /// <summary>
        /// 异步加载Resources资源
        /// </summary>
        /// <param name="assetId"></param>
        /// <param name="callBack"></param>
        public void LoadAssetAsync(int assetId, ObjectCallback callBack, IsObjectOldFunc func)
        {
            AssetInfo info = AssetInfo.GetAssetInfo(assetId);
            if (info == null)
            {
                Utils.OnCallBack(callBack, null, false);
                return;
            }

            if (m_CurLoadingCount < MAX_ASYNC_LOADING_COUNT)
            {
                StartLoadAsync(info.resourcesPath, callBack, func);
            }
            else
            {
                AssetLoaderTask task = new AssetLoaderTask(info, callBack, func);
                //添加异步加载任务
                m_LoaderTaskQueue.Enqueue(task);
            }
        }

        /// <summary>
        /// 启动异步加载任务
        /// </summary>
        /// <param name="resourcesPath"></param>
        /// <param name="callBack"></param>
        /// <param name="func"></param>
        private void StartLoadAsync(string resourcesPath, ObjectCallback callBack, IsObjectOldFunc func)
        {
            m_CurLoadingCount++;

            CoroutineRunner.Run(LoadAssetCoroutine(resourcesPath, (asset, isOld) =>
            {
                Utils.OnCallBack(callBack, asset, isOld);
                OnLoadFinishAndCheckNext();
            }, func));
        }

        /// <summary>
        /// 加载完成并检查是否加载下一个
        /// </summary>
        private void OnLoadFinishAndCheckNext()
        {
            if (m_CurLoadingCount > 0)
                m_CurLoadingCount--;

            if (m_CurLoadingCount < MAX_ASYNC_LOADING_COUNT && m_LoaderTaskQueue.Count > 0)
            {
                AssetLoaderTask task = m_LoaderTaskQueue.Dequeue();

                StartLoadAsync(task.info.resourcesPath, task.callBack, task.func);
            }
        }

        /// <summary>
        /// 加载任务
        /// </summary>
        public class AssetLoaderTask
        {
            public AssetInfo info;
            public ObjectCallback callBack;
            public IsObjectOldFunc func;

            public AssetLoaderTask(AssetInfo info, ObjectCallback callBack, IsObjectOldFunc func)
            {
                this.info = info;
                this.callBack = callBack;
                this.func = func;
            }
        }

        /// <summary>
        /// 加载资源协程
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        private IEnumerator LoadAssetCoroutine(string path, ObjectCallback callBack, IsObjectOldFunc func)
        {
            ResourceRequest request = Resources.LoadAsync(path);
            float startTime = Time.time;

            while (request.isDone == false)
            {
                //10秒超时
                if ((Time.time - startTime) >= 10f)
                {
                    break;
                }
                yield return m_WaitForEndOfFrame;
            }
            UnityEngine.Object prefab = request.asset;
            Utils.OnCallBack(callBack, prefab, func);
        }
        #endregion
    }
}
