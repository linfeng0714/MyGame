using System;
using System.Collections.Generic;
using UnityEngine;

namespace GStore.ObjectPool
{
    public static class DictionaryPool<K, V>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<Dictionary<K, V>> s_dictionary_pool = new ObjectPool<Dictionary<K, V>>(null, l => l.Clear());

        public static Dictionary<K, V> Get()
        {
            return s_dictionary_pool.Get();
        }

        public static void Release(Dictionary<K, V> _to_release)
        {
            s_dictionary_pool.Release(_to_release);
        }
    }
}
