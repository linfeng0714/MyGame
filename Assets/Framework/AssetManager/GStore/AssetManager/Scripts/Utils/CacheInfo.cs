using UnityEngine;
using System.Collections.Generic;

namespace GStore
{
    /// <summary>
    /// 缓存信息
    /// </summary>
    public class CacheInfo
    {
        /// <summary>
        /// 缓存物件
        /// </summary>
        private UnityEngine.Object m_Asset;
        public UnityEngine.Object asset { get { return m_Asset; } }

        /// <summary>
        /// 物件ID
        /// </summary>
        private int m_AssetId;
        public int assetId { get { return m_AssetId; } }

        /// <summary>
        /// 使用时间
        /// </summary>
        private float m_UseTime;
        public float useTime { get { return m_UseTime; } }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_obj"></param>
        public CacheInfo(UnityEngine.Object asset, int assetId = -1)
        {
            m_Asset = asset;
            m_AssetId = assetId;

            m_UseTime = Time.time;
        }

        /// <summary>
        /// 设置使用
        /// </summary>
        public void Use()
        {
            m_UseTime = Time.time;
        }

        #region 静态方法
        /// <summary>
        /// 移除空闲的缓存物件
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取空闲的缓存物件
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static CacheInfo GetFreeCache(List<CacheInfo> list)
        {
            if (list == null || list.Count <= 0)
            {
                return null;
            }

            CacheInfo cache = null;
            for (int i = 0; i < list.Count; i++)
            {
                CacheInfo info = list[i];
                //先把第一个作为检查，之后每一个检查时间，值越小表示越久没用，应该作为空闲物件
                if (cache == null || info.useTime < cache.useTime)
                {
                    cache = info;
                }
            }

            return cache;
        }
        #endregion
    }
}
