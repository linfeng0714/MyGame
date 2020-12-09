using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class AssetBundleInfo
    {
        public enum eAssetBundleState
        {
            State_Loaded,
            State_UnLoad,
        }

        public string BundleName { get; set; }
        public AssetBundle Bundle { get; set; }
        public eAssetBundleState BundleState { get; set; }

        private int RefCount { get; set; }

        /// <summary>
        /// 切换场景时不卸载
        /// </summary>
        public bool DontDestroyOnLoad { get; private set; }

        public AssetBundleInfo(string bundleName , AssetBundle bundle)
        {
            this.RefCount = 1;
            this.BundleName = bundleName;
            this.Bundle = bundle;
            this.BundleState = eAssetBundleState.State_Loaded;
        }

        public Object LaodAsset(string assetName)
        {
            if (this.Bundle == null)
                return null;
            Object ob = this.Bundle.LoadAsset(assetName);
            return ob;
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (this.Bundle == null)
                return null;

            T ob = this.Bundle.LoadAsset<T>(assetName);
            return ob;
        }

        public Object[] LaodAllAsset()
        {
            if (this.Bundle == null)
                return null;

            return this.Bundle.LoadAllAssets();
        }

        public T[] LoadAllAssets<T>() where T : Object
        {
            if (this.Bundle == null)
                return null;

            return this.Bundle.LoadAllAssets<T>();
        }

        public T[] LoadAllAsset<T>() where T : Object
        {
            if (this.Bundle == null)
                return null;

            return this.Bundle.LoadAllAssets<T>();
        }

        public IEnumerable LoadAssetAsync(string assetName, System.Action<Object> OnFinish)
        {
            if(this.Bundle == null)
            {
                Debug.LogError("bundle is null");
                yield break;
            }

            AssetBundleRequest req = this.Bundle.LoadAssetAsync(assetName);
            yield return req;

            if (OnFinish != null)
                OnFinish(req.asset);
        }

        public bool UnLoadBundle()
        {
            if(this.BundleState == eAssetBundleState.State_Loaded)
            {
                if(this.Bundle != null)
                {
                    this.Bundle.Unload(false);
                    this.Bundle = null;
                    this.BundleState = eAssetBundleState.State_UnLoad;
                }
                return true;
            }
            return false;
        }

        public void SetDontDestroyOnLoad()
        {
            DontDestroyOnLoad = true;
        }

    }
}
