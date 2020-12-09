using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class AssetBundleLoader
    {
        private AssetBundleManager _manager;

        public AssetBundleLoader(AssetBundleManager manager)
        {
            _manager = manager;
        }

        public System.Action<Object> loadCpmpleteCallback;

        public bool IsOverLoad = false;

        public bool IsVaild = true;
        /// <summary>
        /// 依赖包加载数量
        /// </summary>
        private int _depLaodingCount = 0;


        public AssetBundleInfo LoadBundle(string bundleName,bool isMainBundle = true)
        {
            //if (isMainBundle)
            //    LoadDepBundle();
            //string fullPath = _manager.
            return null;
        }
    }
}

