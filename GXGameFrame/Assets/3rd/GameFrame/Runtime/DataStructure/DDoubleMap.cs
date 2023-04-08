using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GameFrame
{
    public class DDoubleMap<T, K, V>
    {
        private readonly Dictionary<T, DoubleMap<K, V>> DDTKV = new();

        public void Add(T t, K k, V v)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv))
            {
                kv = new();
                DDTKV.Add(t, kv);
            }

            if (kv.ContainsKey(k))
            {
                Debugger.LogWarning($"kv  have t:{typeof(K)}");
            }
            else
            {
                kv.Add(k, v);
            }
        }

        public DoubleMap<K, V> RemoveTkey(T t)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv))
            {
                return null;
            }

            DDTKV.Remove(t);
            return kv;
        }

        public V RemoveKkey(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv) || !kv.TryGetValueKv(k, out V value))
            {
                return default(V);
            }

            kv.RemoveByKey(k);
            return value;
        }

        public V GetVValue(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv) || !kv.TryGetValueKv(k, out V value))
            {
                return default(V);
            }

            return value;
        }

        public K GetVKey(T t, V k)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv) || !kv.TryGetValueVk(k, out K key))
            {
                return default(K);
            }

            return key;
        }


        public DoubleMap<K, V> GetValue(T t)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv))
            {
                return null;
            }

            return kv;
        }

        public bool ContainsTk(T t, K k)
        {
            if (!DDTKV.TryGetValue(t, out DoubleMap<K, V> kv) || !kv.ContainsKey(k))
            {
                return false;
            }

            return true;
        }

        public List<V> GetAllV(T t)
        {
            if (DDTKV.TryGetValue(t, out DoubleMap<K, V> kv))
            {
                List<V> temp = new List<V>();
                foreach (V itemv in kv.GetAllV())
                {
                    temp.Add(itemv);
                }

                return temp;
            }

            return null;
        }
    }
}