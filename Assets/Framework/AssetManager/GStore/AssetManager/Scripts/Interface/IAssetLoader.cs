using UnityEngine;

namespace GStore
{
    /// <summary>
    /// 加载器接口
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// 同步加载资源接口
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        T LoadAsset<T>(int assetId) where T : Object;

        /// <summary>
        /// 同步加载资源接口 - 全部
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetId"></param>
        /// <returns></returns>
        T[] LoadAllAsset<T>(int assetId) where T : Object;

        /// <summary>
        /// 加载文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        byte[] LoadFileData(string filePath);
    }
}
