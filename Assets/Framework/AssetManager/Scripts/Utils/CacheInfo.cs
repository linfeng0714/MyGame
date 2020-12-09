using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManager
{
    public class CacheInfo
    {
        
        private Object _asset;
        /// <summary>
        /// 缓存物件
        /// </summary>
        public Object asset { get { return _asset; } }

        private int _assetId;
        /// <summary>
        /// ID
        /// </summary>
        public int assetId { get { return _assetId; } }

        private float _useTime;
        /// <summary>
        /// 使用时间
        /// </summary>
        public float useTime { get { return _useTime; } }

        public CacheInfo(UnityEngine.Object asset, int assetId = -1)
        {
            _asset = asset;
            _assetId = assetId;

            _useTime = Time.time;
        }
        /// <summary>
        /// 设置使用
        /// </summary>
        public void Use()
        {
            _useTime = Time.time;
        }

        public static bool RemoveFreeCache(List<CacheInfo> list)
        {
            CacheInfo info = GetFreeCache(list);
            if (info == null)
            {
                return false;
            }

            list.Remove(info);
            return true;
        }

        private static CacheInfo GetFreeCache(List<CacheInfo> list)
        {
            if (list == null || list.Count <= 0)
                return null;
            CacheInfo cache = null;
            for(int i=0;i<list.Count;i++)
            {
                CacheInfo info = list[i];
                if (cache == null || info.useTime < cache.useTime)
                    cache = info;
            }
            return cache;
        }


    }
}
