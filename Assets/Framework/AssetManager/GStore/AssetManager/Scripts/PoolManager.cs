using UnityEngine;
using System.Collections.Generic;

namespace GStore
{
    /// <summary>
    /// GameObject缓存池模块
    /// </summary>
    public class PoolManager
    {
        /// <summary>
        /// 缓存GameObject、Prefab的映射关系
        /// </summary>
        private Dictionary<GameObject, GameObject> m_PrefabMap = new Dictionary<GameObject, GameObject>();

        /// <summary>
        /// 管理物件
        /// </summary>
        private GameObject m_PoolGo;

        /// <summary>
        /// 缓存GameObject资源
        /// </summary>
        private readonly Dictionary<string, HashStack<GameObject>> m_CacheGoDict = new Dictionary<string, HashStack<GameObject>>();

        /// <summary>
        /// 缓存列表对象
        /// </summary>
        private readonly Stack<HashStack<GameObject>> m_PoolGoStack = new Stack<HashStack<GameObject>>();

        /// <summary>
        /// 临时列表，防止GC
        /// </summary>
        private readonly List<IRecycle> m_TempRecycleList = new List<IRecycle>(64);

        /// <summary>
        /// 初始化
        /// 策略：只针对每个场景进行缓存，切换场景则所有缓存丢失
        /// </summary>
        public void Init()
        {
            if (m_PoolGo != null)
            {
                return;
            }
            m_PoolGo = new GameObject("PoolManager");
            m_PoolGo.SetActive(false);
            // 设置容量，防止频繁分配
            m_PoolGo.transform.hierarchyCapacity = 4096;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void DoDestroy()
        {
            m_CacheGoDict.Clear();
            m_PrefabMap.Clear();

            //因为编辑器里也可能使用了PoolManager所以这里要区分一下
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(m_PoolGo);
            }
            else
            {
                UnityEngine.Object.Destroy(m_PoolGo);
            }
#else
            // 正常游戏
            UnityEngine.Object.Destroy(m_PoolGo);
#endif
            m_PoolGo = null;
        }

        #region GameObject       

        /// <summary>
        /// 获取GameObject
        /// </summary>
        /// <param name="_prefab"></param>
        /// <returns></returns>
        public GameObject GetFromPool(string key)
        {
            HashStack<GameObject> goQueue;
            if (!m_CacheGoDict.TryGetValue(key, out goQueue))
            {
                return null;
            }
            if (goQueue.Count <= 0)
            {
                return null;
            }
            GameObject go = goQueue.Pop();
            if (goQueue.Count <= 0)
            {
                // 回收列表
                m_PoolGoStack.Push(goQueue);
                m_CacheGoDict.Remove(key);
            }
            if (go == null)
            {
                return null;
            }

            go.transform.SetParent(null);

            CallRecycleInterface(go, true);

            return go;
        }

        /// <summary>
        /// 回收GameObject
        /// </summary>
        /// <param name="gameObject">物件</param>
        public void ReturnToPool(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            if (m_PoolGo == null)
            {
                //可能没有提前创建
                Init();
            }

            Transform trans = gameObject.transform;
            if (trans.parent == m_PoolGo)
            {
                // 已回收
                return;
            }

            //脚本清空工作
            CallRecycleInterface(gameObject, false);

            trans.SetParent(m_PoolGo.transform);

            string key = gameObject.name;

            HashStack<GameObject> goQueue;
            if (!m_CacheGoDict.TryGetValue(key, out goQueue))
            {
                if (m_PoolGoStack.Count > 0)
                {
                    goQueue = m_PoolGoStack.Pop();
                    Debug.Assert(goQueue.Count == 0);
                }
                else
                {
                    goQueue = new HashStack<GameObject>();
                }
                m_CacheGoDict.Add(key, goQueue);
                goQueue.Push(gameObject);
            }
            else
            {
                if (goQueue.Contains(gameObject))
                {
                    throw new AssetException("重复回收GameObject:" + gameObject.name);
                }
                else
                {
                    if (goQueue.Count > 0)
                    {
                        GameObject lastPrefab, nowPrefab;
                        if (m_PrefabMap.TryGetValue(goQueue.Peek(), out lastPrefab) && m_PrefabMap.TryGetValue(gameObject, out nowPrefab))
                        {
                            if (lastPrefab != nowPrefab)
                            {
                                throw new AssetException(string.Format("对象缓存池出错！不同的Prefab有相同的名字，prefabName={0}", key));
                            }
                        }
                    }
                    goQueue.Push(gameObject);
                }
            }
        }

        /// <summary>
        /// 调用回收接口
        /// </summary>
        /// <param name="go"></param>
        /// <param name="init"></param>
        public bool CallRecycleInterface(GameObject go, bool init)
        {
            go.GetComponentsInChildren(true, m_TempRecycleList);
            if (m_TempRecycleList.Count <= 0)
            {
                return false;
            }

            for (int i = 0; i < m_TempRecycleList.Count; ++i)
            {
                if (init)
                {
                    m_TempRecycleList[i].RecycleInit();
                }
                else
                {
                    m_TempRecycleList[i].Recycled();
                }
            }

            m_TempRecycleList.Clear();
            return true;
        }
        #endregion

        #region PrefabMap Module

        /// <summary>
        /// 添加材质物体映射预设
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="prefab"></param>
        public void AddToPrefabMap(GameObject gameObject, GameObject prefab)
        {
            m_PrefabMap[gameObject] = prefab;
        }

        /// <summary>
        /// 从缓存字典中获取对应存储的预设
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public GameObject GetPrefab(GameObject gameObject)
        {
            GameObject prefab;
            m_PrefabMap.TryGetValue(gameObject, out prefab);
            return prefab;
        }

        /// <summary>
        /// 移除材质物体映射预设
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="prefab"></param>
        public void RemoveFromPrefabMap(GameObject gameObject)
        {
            if (m_PrefabMap.ContainsKey(gameObject))
            {
                m_PrefabMap.Remove(gameObject);
            }
        }
        #endregion
    }

}
