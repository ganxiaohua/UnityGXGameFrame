using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 散装容器
    /// </summary>
    public class BulkList<K, T> : ArrayEx<T> where T : class
    {
        private Dictionary<K, T> dictionary;

        public BulkList(int count) : base(count)
        {
            dictionary = new Dictionary<K, T>(count);
        }

        public T GetForKey(K k)
        {
            return dictionary.GetValueOrDefault(k);
        }

        public bool Add(K k, T t)
        {
            if (!dictionary.TryAdd(k, t))
            {
                Debugger.LogError($"{k} already exists");
                return false;
            }

            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i] != null) continue;
                Data[i] = t;
                return true;
            }

            return false;
        }

        public bool AddForIndex(int index, K k, T t)
        {
            if (!dictionary.TryAdd(k, t))
            {
                Debugger.LogError($"{k} already exists");
                return false;
            }

            if (Data[index] != null)
            {
                Debugger.LogError($"{k} already exists");
                return false;
            }

            Data[index] = t;
            return true;
        }

        public bool Contains(K k)
        {
            return dictionary.ContainsKey(k);
        }

        public bool TryGetValue(K k, out T t)
        {
            return dictionary.TryGetValue(k, out t);
        }

        public bool Remove(K k)
        {
            if (dictionary.TryGetValue(k, out T t))
            {
                for (int i = 0; i < Data.Length; i++)
                {
                    if (Data[i] == t)
                    {
                        Data[i] = default;
                        dictionary.Remove(k);
                        return true;
                    }
                }
            }

            Debugger.LogError($"{k} not exists");
            return false;
        }
    }
}