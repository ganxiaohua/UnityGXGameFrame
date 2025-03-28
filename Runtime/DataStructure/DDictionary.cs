using System.Collections;
using System.Collections.Generic;

namespace GameFrame
{
    public class DDictionary<T, K, V> : IEnumerable
    {
        private readonly Dictionary<T, Dictionary<K, V>> DDTKV = new();

        public void Add(T t, K k, V v)
        {
            if (!DDTKV.TryGetValue(t, out Dictionary<K, V> kv))
            {
                kv = new();
                DDTKV.Add(t, kv);
            }

            if (!kv.TryAdd(k, v))
            {
                Debugger.LogWarning($"kv  have t:{typeof(K)}");
            }
        }

        public Dictionary<K, V> RemoveTkey(T t)
        {
            return !DDTKV.Remove(t, out Dictionary<K, V> kv) ? null : kv;
        }

        public V RemoveKkey(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out Dictionary<K, V> kv) || !kv.Remove(k, out V value))
            {
                return default(V);
            }

            return value;
        }

        public V GetVValue(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out Dictionary<K, V> kv) || !kv.TryGetValue(k, out V value))
            {
                return default(V);
            }

            return value;
        }

        public Dictionary<K, V> GetValue(T t)
        {
            if (!DDTKV.TryGetValue(t, out Dictionary<K, V> kv))
            {
                return null;
            }

            return kv;
        }

        public bool ContainsTk(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out Dictionary<K, V> kv) || !kv.ContainsKey(k))
            {
                return false;
            }

            return true;
        }

        public bool ContainsT(T t)
        {
            if (!DDTKV.ContainsKey(t))
            {
                return false;
            }

            return true;
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<V> GetEnumerator()
        {
            foreach (var kv in DDTKV.Values)
            {
                foreach (var value in kv.Values)
                {
                    yield return value;
                }
            }
        }
    }
}