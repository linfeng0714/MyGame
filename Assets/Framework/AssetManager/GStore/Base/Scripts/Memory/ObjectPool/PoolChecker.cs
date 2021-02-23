using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
// 只在editor下使用

namespace GStore.ObjectPool
{
    public class PoolChecker
    {
        /// <summary>
        /// 检查基类
        /// </summary>
        protected static readonly System.Type POOL_TYPE = typeof(GStore.ObjectPool.ObjectPool<>);

        /// <summary>
        /// 命名空间
        /// </summary>
        protected static readonly string POOL_NAMESPACE = POOL_TYPE.Namespace;


        /// <summary>
        /// 所有对象
        /// </summary>
        protected static readonly HashSet<IObjectPool> s_debug_pool_instance = new HashSet<IObjectPool>();

     

        /// <summary>
        /// 加入ObjectPool
        /// </summary>
        /// <param name="_pool"></param>
        public static void CheckObjectPool(IObjectPool _pool)
        {
            if(!s_debug_pool_instance.Contains(_pool))
            {
                s_debug_pool_instance.Add(_pool);
            }
        }

        /// <summary>
        /// 检查
        /// </summary>
        public static void Check(Action<object> _on_fail)
        {
            if (s_debug_pool_instance.Count > 0)
            {
                var _iter = s_debug_pool_instance.GetEnumerator();
                while(_iter.MoveNext())
                {
                    var _pool = _iter.Current;
                    if (_pool.countActive > 0)
                    {
                        if (_on_fail != null)
                        {
                            _on_fail.Invoke(_pool);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 错误检查
        /// 请在Update的开始或结束调用
        /// </summary>
        /// <param name="_on_fail"></param>
        public static void CheckAndLog()
        {
            Check((_obj) =>
            {
                var _pool = _obj as GStore.ObjectPool.IObjectPool;
                if (_pool != null)
                {
                    Debug.LogError("程序请检查，使用ObjectPool方法有误，Get与Release在同一帧内需要匹配调用!(开启GSTORE_DEBUG_OBJECT_POOL宏可追踪调用堆栈)");

#if GSTORE_DEBUG_OBJECT_POOL
                    var _list = _pool.GetDebugTraceList();
                    for (int _i = 0; _i < _list.Count; ++_i)
                    {
                        Debug.LogError(_list[_i].ToString());
                    }
#else
                    Debug.LogError(_pool.GetType().ToString() + " 泄漏次数：" + _pool.countActive);
#endif


                }
            });

        }


        /// <summary>
        /// 找出对象池的类型
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private static FieldInfo GetObjectPoolField(System.Type _type)
        {
            if (!_type.IsGenericType)
            {
                return null;
            }
            if(_type.Namespace != POOL_NAMESPACE)
            {
                return null;
            }
            // 获得static成员
            var _fields = _type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            for(int _i = 0; _i < _fields.Length; ++_i)
            {
                var _field_type = _fields[_i].FieldType;
                if(IsPoolType(_field_type))
                {
                    return _fields[_i];
                }
            }
            return null;
        }

        /// <summary>
        /// 获得池类型
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private static bool IsPoolType(System.Type _type)
        {
            if (_type.IsGenericType)
            {
                if (_type.GetGenericTypeDefinition() == POOL_TYPE)
                {
                    return true;
                }
            }
            if (_type.BaseType != null)
            {
                return IsPoolType(_type.BaseType);
            }
            return false;
        }

    }
}

#endif