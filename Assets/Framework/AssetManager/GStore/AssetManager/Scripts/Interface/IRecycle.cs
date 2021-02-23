using UnityEngine;
using System.Collections;

namespace GStore
{
    /// <summary>
    /// 如果实例后的GameObject需要回收则继承该接口
    /// </summary>
    public interface IRecycle
    {
        /// <summary>
        /// 物件启用时初始化相关逻辑
        /// </summary>
        void RecycleInit();
        /// <summary>
        /// 物件禁用时销毁相关逻辑
        /// </summary>
        void Recycled();
    }
}
