using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GStore;

namespace GStore
{
    public class AssetBundleLoader
    {
        /// <summary>
        /// 管理器
        /// </summary>
        private AssetBundleManager m_Manager;

        public AssetBundleLoader(AssetBundleManager manager)
        {
            m_Manager = manager;
        }

        public System.Action<UnityEngine.Object> loadCompleteCallback;

        /// <summary>
        /// 在异步加载中的ab 是否被同步加载了
        /// </summary>
        public bool IsOverLoad = false;

        /// <summary>
        /// 是否有效，对于正在加载中的ab，如果切换场景了，会卸载所有ab，他们就是无效的了，那么等他们加载完需要卸载掉。
        /// </summary>
        public bool IsVaild = true;

        /// <summary>
        /// 依赖包加载数量
        /// </summary>
        private int m_DepLoadingCount = 0;

        #region 同步加载
        /// <summary>
        /// 同步加载
        /// </summary>
        /// <returns>The bundle.</returns>
        /// <param name="bundleName">Bundle name.</param>
        /// <param name="assetName">Asset name.</param>
        /// <param name="isMainBundle: 是否是主bundle,如果是依赖bundle,不需要再LoadDepBundle() </param>
        public AssetBundleInfo LoadBundle(string bundleName, bool isMainBundle = true)
        {
            if (isMainBundle)
                LoadDepBundle(bundleName);

            string fullPath = m_Manager.GetAssetsBundleFullPath(bundleName);
            AssetBundle bundle = AssetBundle.LoadFromFile(fullPath);
            if (bundle == null)
            {
                return null;
            }

            AssetBundleInfo info = new AssetBundleInfo(bundleName, bundle);

            m_Manager.AddBundleInfo(bundleName, info);
            return info;
        }


        /// <summary>
        /// 加载依赖bundle
        /// </summary>
        /// <returns><c>true</c>, if dep bundle was loaded, <c>false</c> otherwise.</returns>
        /// <param name="mainBundeName">Main bunde name.</param>
        private void LoadDepBundle(string mainBundleName)
        {
            string[] dependPath = m_Manager.m_RootManifest.GetAllDependencies(mainBundleName);
            int count = dependPath.Length;
            for (int i = 0; i < count; i++)
            {
                string depPath = dependPath[i];
                depPath = m_Manager.RemapVariantName(depPath);

                AssetBundleInfo _info = m_Manager.GetAssetBundleByBundleName(depPath);
                if (_info != null && _info.Bundle != null)
                {
                    continue;
                }
                LoadBundle(depPath, false);
            }
        }
        #endregion


        #region 异步加载

        /// <summary>
        ///异步加载bundle
        /// </summary>
        /// <returns>The bundle async.</returns>
        /// <param name="assetID">Asset I.</param>
        /// <param name="isSceneBundle">If set to <c>true</c> is scene bundle.</param>
        public IEnumerator LoadAssetAsync(string bundleName, bool isSceneBundle, string assetName)
        {
            AssetBundleInfo _info = m_Manager.GetAssetBundleByBundleName(bundleName);
            if (_info != null && _info.Bundle != null)//ab已经在缓存中
            {
                UnityEngine.Object _ob = null;
                if (!isSceneBundle)
                {
                    AssetBundleRequest req = _info.Bundle.LoadAssetAsync(assetName);
                    yield return req;

                    _ob = req.asset;

                    if (_ob == null)
                    {
                        Debug.LogError("=======LoadBundleAsync======ob===null  assetID=" + bundleName);
                    }
                }

                if (loadCompleteCallback != null)
                    loadCompleteCallback(_ob);

                yield break;
            }

            string path = m_Manager.GetAssetsBundleFullPath(bundleName);

            //加载依赖ab
            yield return CoroutineRunner.Run(LoadDepBundleAsync(bundleName));


            if (IsOverLoad) //加载主ab前再检查一下该ab是否已经被 重新同步加载了，
            {
                if (loadCompleteCallback != null)
                    loadCompleteCallback(null);

                yield break;
            }

            //加载主ab
            AssetBundleCreateRequest _targetCreq = AssetBundle.LoadFromFileAsync(path);
            yield return _targetCreq;

            if (_targetCreq == null || _targetCreq.assetBundle == null)
            {
                if (loadCompleteCallback != null)
                    loadCompleteCallback(null);

                yield break;
            }

            if (!IsVaild) //info.UseAsynLoadingDate = false,表示触发了unload操作了，所以异步加载完成之后也需要将其unload
            {
                _targetCreq.assetBundle.Unload(false);

                if (loadCompleteCallback != null)
                    loadCompleteCallback(null);

                yield break;
            }

            AssetBundleInfo info = new AssetBundleInfo(bundleName, _targetCreq.assetBundle);
            m_Manager.AddBundleInfo(bundleName, info);

            UnityEngine.Object ob = null;
            if (!isSceneBundle)
            {
                AssetBundleRequest req = _targetCreq.assetBundle.LoadAssetAsync(assetName);
                yield return req;

                ob = req.asset;
            }

            if (loadCompleteCallback != null)
                loadCompleteCallback(ob);

        }

        /// <summary>
        /// 异步加载依赖bundle
        /// /// </summary>
        /// <returns>The dep bundle async.</returns>
        /// <param name="mainBundleName">Main bundle name.</param>
        public IEnumerator LoadDepBundleAsync(string mainBundleName)
        {
            string[] depPaths = m_Manager.m_RootManifest.GetAllDependencies(mainBundleName);
            int count = depPaths.Length;
            for (int i = 0; i < count; i++)
            {
                if (IsOverLoad) //主ab不用加载，依赖ab也停止加载
                {
                    yield break;
                }

                string depPath = depPaths[i];
                depPath = m_Manager.RemapVariantName(depPath);

                AssetBundleInfo info = m_Manager.GetAssetBundleByBundleName(depPath);//已经存在的不用在加载
                if (info != null && info.Bundle != null)
                {
                    continue;
                }

                if (m_Manager.IsInLoadingDic(depPath))
                    continue;

                AssetBundleLoader mainBundleLoader = m_Manager.GetLoaderFromLoadingDic(mainBundleName);
                m_Manager.AddToLoadingDic(depPath, mainBundleLoader);

                //依赖包改为并行加载，减少无谓的等待
                m_DepLoadingCount++;
                CoroutineRunner.Run(LoadDepBundleCoroutine(depPath));
            }

            //等待所有依赖加载完成
            while (m_DepLoadingCount > 0)
            {
                yield return null;
            }
        }

        /// <summary>
        /// 加载单个依赖包的协程
        /// </summary>
        /// <param name="depPath"></param>
        /// <returns></returns>
        private IEnumerator LoadDepBundleCoroutine(string depPath)
        {
            string path = m_Manager.GetAssetsBundleFullPath(depPath);

            AssetBundleCreateRequest creq = AssetBundle.LoadFromFileAsync(path);
            yield return creq;
            bool isLoaded = m_Manager.GetAssetBundleByBundleName(depPath) != null;

            m_Manager.RemoveFromLoadingDic(depPath);
            if (creq == null || creq.assetBundle == null)
            {
                //已被加载过是因为后执行的同步加载提前完成了，不影响后续加载，不需要报错
                if (isLoaded == false)
                {
                    Debug.LogError("====bundle log:load depend ab error path=" + path + "," + Time.frameCount);
                }
            }
            else
            {
                AssetBundleInfo depInfo = new AssetBundleInfo(depPath, creq.assetBundle);
                m_Manager.AddBundleInfo(depPath, depInfo);
            }
            m_DepLoadingCount--;
        }

        #endregion

    }
}
