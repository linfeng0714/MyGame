using System.Collections.Generic;
using UnityEngine;
using GStore.ObjectPool;

namespace GStore
{
    /// <summary>
    /// 恢复默认的materail
    /// 只支持单Materail
    /// </summary>
    [DisallowMultipleComponent]
    public class MaterialRestore : MonoBehaviour, IRecycle
    {
        /// <summary>
        /// 共用临时字符串变量
        /// </summary>
        private static readonly System.Text.StringBuilder s_NamePathBuilder = new System.Text.StringBuilder();

        /// <summary>
        /// 数据结构
        /// </summary>
        private class PrefabData
        {
            /// <summary>
            /// 引用计数
            /// </summary>
            public int ref_count = 0;

            /// <summary>
            /// 最后使用时间
            /// </summary>
            public float last_use_time = 0f;

            /// <summary>
            /// 材质对应关系
            /// </summary>
            public readonly Dictionary<string, Material> prefab_material = new Dictionary<string, Material>();
        }

        /// <summary>
        /// 缓存prefab中的materail对应关系
        /// key为prefab的instance id
        /// </summary>
        private static readonly Dictionary<int, PrefabData> s_CachePrefabMaterial = new Dictionary<int, PrefabData>();

        /// <summary>
        /// 重用对象池
        /// </summary>
        private static readonly Stack<PrefabData> s_prefab_data_pool = new Stack<PrefabData>();

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private bool inited = false;

        /// <summary>
        /// Prefab的实例ID
        /// </summary>
        private int prefab_id = 0;

        /// <summary>
        /// prefab中的material列表
        /// </summary>
        private Dictionary<string, Material> prefab_material_dict = null;

        /// <summary>
        /// 缓存的renderer
        /// </summary>
        private Dictionary<string, Renderer> cache_renderer = null;


        #region Static Data Manager

        /// <summary>
        /// 释放未使用的prefab数据
        /// </summary>
        public static void UnloadUnusedPrefabData()
        {
            List<int> _remove_list = null;
            var _iter = s_CachePrefabMaterial.GetEnumerator();
            while (_iter.MoveNext())
            {
                if (_iter.Current.Value.ref_count <= 0)
                {
                    if (_remove_list == null)
                    {
                        _remove_list = ListPool<int>.Get();
                    }
                    _remove_list.Add(_iter.Current.Key);

                    // 回收
                    CollectPrefabData(_iter.Current.Value);
                }
            }
            _iter.Dispose();

            if (_remove_list != null)
            {
                for (int _i = 0; _i < _remove_list.Count; ++_i)
                {
                    s_CachePrefabMaterial.Remove(_remove_list[_i]);
                }

                ListPool<int>.Release(_remove_list);
            }
        }

        /// <summary>
        /// 请求一个PrefabData
        /// </summary>
        /// <returns></returns>
        private static PrefabData RequirePrefabData()
        {
            if (s_prefab_data_pool.Count > 0)
            {
                return s_prefab_data_pool.Pop();
            }
            return new PrefabData();
        }

        /// <summary>
        /// 回收PrefabData对象
        /// </summary>
        private static void CollectPrefabData(PrefabData _prefab_data)
        {
            _prefab_data.ref_count = 0;
            _prefab_data.last_use_time = 0;
            _prefab_data.prefab_material.Clear();
            s_prefab_data_pool.Push(_prefab_data);
        }


        private static Dictionary<string, Material> NewPrefabMaterial(GameObject _prefab, int _prefab_id)
        {

            Dictionary<string, Material> _out_dict = null;
            PrefabData _prefab_data;
            if (!s_CachePrefabMaterial.TryGetValue(_prefab_id, out _prefab_data))
            {
                // 新建
                _prefab_data = RequirePrefabData();
                CreatePrefabMaterial(_prefab, _prefab_data.prefab_material);
                s_CachePrefabMaterial.Add(_prefab_id, _prefab_data);
            }

            if (_prefab_data != null)
            {
                _prefab_data.ref_count += 1;
                _prefab_data.last_use_time = Time.realtimeSinceStartup;
                _out_dict = _prefab_data.prefab_material;
            }
            return _out_dict;
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="_prefab_id"></param>
        private static void ReleasePrefabMaterial(int _prefab_id)
        {
            PrefabData _prefab_data;
            if (s_CachePrefabMaterial.TryGetValue(_prefab_id, out _prefab_data))
            {
                if (_prefab_data.ref_count > 0)
                {
                    _prefab_data.ref_count -= 1;
                }
                else
                {
                    Debug.LogError("logic error");
                }
            }
        }

        #endregion // Static Data Manager

        /// <summary>
        /// 物体添加脚本的时候初始化材质信息缓存
        /// </summary>
        void Awake()
        {
            Init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_prefab"></param>
        public void Init()
        {
            if (inited)
            {
                return;
            }

            GameObject _prefab = AssetManager.Instance.poolManager.GetPrefab(gameObject);
            //PoolManager.Instance.AddMaterialPrefabs(gameObject, _prefab);
            if(_prefab != null)
            {
                //UnityEngine.Debug.Log("---prefab name---" + _prefab.name + "---prefab id---" + _prefab.GetInstanceID());
                prefab_id = _prefab.GetInstanceID();
                prefab_material_dict = NewPrefabMaterial(_prefab, prefab_id);
                inited = true;
            }
        }

        /// <summary>
        /// 创建Prefab的Materail
        /// </summary>
        /// <param name="_prefab"></param>
        /// <returns></returns>
        private static void CreatePrefabMaterial(GameObject _prefab, Dictionary<string, Material> _material_dict)
        {
            var _renderer_list = ListPool<Renderer>.Get();
            _prefab.GetComponentsInChildren<Renderer>(_renderer_list);
            _material_dict.Clear();
            Transform _root = _prefab.transform;
            string _path;
            for (int _i = 0; _i < _renderer_list.Count; ++_i)
            {
                var _mat = _renderer_list[_i].sharedMaterial;
                if (_mat == null)
                {
                    continue;
                }

                _path = GetTransformHierarchyPath(_root, _renderer_list[_i].transform);
#if UNITY_EDITOR
                // 不可能一样路径的
                if (_material_dict.ContainsKey(_path))
                {
                    Debug.LogErrorFormat("MaterialRestore 检测到有Renderer在同一路径。GO:{0} Path:{1}", _prefab.name, _path);
                }
#endif
                _material_dict[_path] = _mat;
            }
            ListPool<Renderer>.Release(_renderer_list);
        }


        /// <summary>
        /// 缓存renderer
        /// </summary>
        private void CacheRenderer()
        {
            if (prefab_material_dict == null)
            {
                return;
            }
            if (cache_renderer == null)
            {
                cache_renderer = new Dictionary<string, Renderer>();
                Transform _root = transform;
                var _iter = prefab_material_dict.GetEnumerator();
                while (_iter.MoveNext())
                {
                    Transform _child = _root.Find(_iter.Current.Key);
                    if (_child != null)
                    {
                        Renderer _renderer = _child.GetComponent<Renderer>();
                        if (_renderer != null)
                        {
                            cache_renderer[_iter.Current.Key] = _renderer;
                        }
                    }
                }
                _iter.Dispose();
            }
        }

        /// <summary>
        /// 获得tranform的路径
        /// </summary>
        /// <param name="_root"></param>
        /// <param name="_target"></param>
        /// <returns></returns>
        private static string GetTransformHierarchyPath(Transform _root, Transform _target)
        {
            s_NamePathBuilder.Length = 0;
            while (_target != null && _root != _target)
            {
                string _name = _target.name;
                if (!string.IsNullOrEmpty(_name))
                {
                    if (s_NamePathBuilder.Length > 0)
                    {
                        s_NamePathBuilder.Insert(0, '/');
                    }
                    s_NamePathBuilder.Insert(0, _name);
                }
                _target = _target.parent;
            }
            string _final_str = s_NamePathBuilder.ToString();
            s_NamePathBuilder.Length = 0;
            return _final_str;
        }


        #region MonoBehaviour

        /// <summary>
        /// 删除时
        /// </summary>
        private void OnDestroy()
        {
            if (inited)
            {
                ReleasePrefabMaterial(prefab_id);
            }
        }

        #endregion // MonoBehaviour

        #region IRecycle

        /// <summary>
        /// 物件启用时初始化相关逻辑
        /// </summary>
        void IRecycle.RecycleInit()
        {

        }

        /// <summary>
        /// 物件禁用时销毁相关逻辑
        /// </summary>
        void IRecycle.Recycled()
        {
            if (inited)
            {
                //UnityEngine.Debug.Log("---recycled obj---" + gameObject.name + "---obj id---" + gameObject.GetInstanceID());
#if ENABLE_G_PROFILER
            ProfilerTool.BeginSample("MaterialRestore - Recycled");
#endif
                CacheRenderer();
                if (cache_renderer != null && prefab_material_dict != null)
                {
                    var _iter = cache_renderer.GetEnumerator();
                    while (_iter.MoveNext())
                    {
                        Renderer _renderer = _iter.Current.Value;
                        if (_renderer != null)
                        {
                            Material _prefab_mat;
                            if (prefab_material_dict.TryGetValue(_iter.Current.Key, out _prefab_mat))
                            {
                                var _target_mat = _renderer.sharedMaterial;
                                if (_target_mat == null)
                                {
                                    continue;
                                }
                                if (_target_mat != _prefab_mat)
                                {
                                    // 这里不直接使用prefab替换pool里的对象，有可能是代码改变的material，如果替换了，就会有多出来的实例化资源，时间长了的话就会长留在内存里

                                    if (_target_mat.shader != _prefab_mat.shader)
                                    {
                                        //UnityEngine.Debug.Log("---current shader name---" + _target_mat.shader.name);
                                        _target_mat.shader = _prefab_mat.shader;
                                    }
                                    // keyword也可以复制
                                    _target_mat.CopyPropertiesFromMaterial(_prefab_mat);
                                }
                            }
                            else
                            {
                                // GameObject已被修改，路径找不到
                            }
                        }
                    }
                }

#if ENABLE_G_PROFILER
            ProfilerTool.EndSample();
#endif
            }
        }

        #endregion // IRecycle

    }
}
