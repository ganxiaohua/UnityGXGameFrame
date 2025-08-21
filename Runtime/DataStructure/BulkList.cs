using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 散装容器
    /// </summary>
    public class BulkList<K, T> where T : class
    {
        private Dictionary<K, T> dictionary;
        private ArrayEx<T> array;
        public int Count => array.Count;

        public T this[int index]
        {
            get { return array[index]; }
        }

        public BulkList(int count)
        {
            dictionary = new Dictionary<K, T>(count);
            array = new ArrayEx<T>(count);
        }

        public T GetForKey(K k)
        {
            return dictionary.GetValueOrDefault(k);
        }

        public T GetForIndex(int index)
        {
            return array[index];
        }

        public bool Add(K k, T t)
        {
            if (!dictionary.TryAdd(k, t))
            {
                Debugger.LogError($"{k} already exists");
                return false;
            }

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] != null) continue;
                array[i] = t;
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

            if (array[index] != null)
            {
                Debugger.LogError($"{k} already exists");
                return false;
            }

            array[index] = t;
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
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] == t)
                    {
                        array[i] = default;
                        return true;
                    }
                }
            }

            Debugger.LogError($"{k} not exists");
            return false;
        }

        public void SetCapacity(int size)
        {
            array.SetCapacity(size);
        }


        public ArrayEx<T>.GXEnumerator GetEnumerator() => array.GetEnumerator();
    }
}