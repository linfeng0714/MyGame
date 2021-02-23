using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GStore
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

        public eAssetBundleState State { get; set; }

        private int RefCount { get; set; }

        /// <summary>
        /// 切场景时不卸载
        /// </summary>
        public bool DontDestroyOnLoad { get; private set; }

        public AssetBundleInfo(string bundleName, AssetBundle bundle)
        {
            this.RefCount = 1;
            this.BundleName = bundleName;
            this.Bundle = bundle;
            this.State = eAssetBundleState.State_Loaded;
        }

        public Object LoadAsset(string assetName)
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

        public Object[] LoadAllAsset()
        {
            if (this.Bundle == null)
                return null;

            return this.Bundle.LoadAllAssets();
        }

        public T[] LoadAllAsset<T>() where T : Object
        {
            if (this.Bundle == null)
                return null;

            return this.Bundle.LoadAllAssets<T>();
        }

        public IEnumerator LoadAssetAsync(string assetName, System.Action<Object> OnFinish)
        {
            if (this.Bundle == null)
            {
                Debug.LogError("====Bundle===mull!!!!!!!!!!");
                yield break;
            }

            AssetBundleRequest req = this.Bundle.LoadAssetAsync(assetName);
            yield return req;

            if (OnFinish != null)
                OnFinish(req.asset);

        }

        /// <summary>
        /// 卸载bundle
        /// </summary>
        public bool UnLoadBundle()
        {
            if (this.State == eAssetBundleState.State_Loaded)
            {
                if (this.Bundle != null)
                {
                    this.Bundle.Unload(false);
                    this.Bundle = null;
                    this.State = eAssetBundleState.State_UnLoad;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 切场景不卸载
        /// </summary>
        public void SetDontDestroyOnLoad()
        {
            DontDestroyOnLoad = true;
        }


        //   private  List<AssetBundleInfo> mDepbundleList;
        //   public List<AssetBundleInfo>DepbundleList{
        //       get
        //       { 
        //           if (mDepbundleList == null)
        //               mDepbundleList = new List<AssetBundleInfo>();

        //           return mDepbundleList;
        //       }
        //       set
        //       {
        //           mDepbundleList = value;
        //       }  
        //   }
        //   /// <summary>
        /////引用加1 
        ///// </summary>
        //public void RetainBundleRef()
        //   {
        //       this.RefCount++;
        //   }

        //   /// <summary>
        //   /// 引用减1
        //   /// </summary>
        //   public void ReleaseBundleRef()
        //   {
        //       if (this.RefCount > 0)
        //           this.RefCount--;
        //   }

    }
}
