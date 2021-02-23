using System;
using System.Collections.Generic;
using UnityEngine;

namespace GStore.ObjectPool
{
    public static class HashSetPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<HashSet<T>> s_hash_set_pool = new ObjectPool<HashSet<T>>(null, l => l.Clear());

        public static HashSet<T> Get()
        {
            return s_hash_set_pool.Get();
        }

        public static void Release(HashSet<T> _to_release)
        {
            s_hash_set_pool.Release(_to_release);
        }
    }
}
