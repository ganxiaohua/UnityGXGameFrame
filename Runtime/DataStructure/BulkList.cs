using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class BulkListIndex : IDisposable
    {
        public int Index { get; internal set; }
        public int key { get; internal set; }

        public virtual void Dispose()
        {
            Index = 0;
            key = 0;
        }
    }

    /// <summary>
    /// 散装容器
    /// </summary>
    public class BulkList<T> where T : BulkListIndex
    {
        private T[] datas;

        private Dictionary<int, List<T>> itemsWithKey;

        private List<int> nullIndex;
        public int Count { get; private set; }

        public T this[int index]
        {
            get { return datas[index]; }
            set { datas[index] = value; }
        }

        public BulkList(int count)
        {
            datas = new T[count];
            Count = count;
            itemsWithKey = new();
            nullIndex = new List<int>();
            for (int i = count - 1; i >= 0; i--)
            {
                nullIndex.Add(i);
            }
        }

        public int GetNullCount()
        {
            return nullIndex.Count;
        }

        public bool Add(int key, T t)
        {
            if (nullIndex.Count <= 0)
            {
                Debug.LogWarning("array count out");
                return false;
            }

            var index = nullIndex[^1];
            nullIndex.RemoveAt(nullIndex.Count - 1);
            datas[index] = t;
            t.Index = index;
            t.key = key;
            if (!itemsWithKey.TryGetValue(key, out var keys))
            {
                keys = ListPool<T>.Get();
                itemsWithKey.Add(key, keys);
            }

            keys.Add(t);
            return true;
        }

        public bool AddAt(int index, int key, T t)
        {
            if (datas[index] != null)
                return false;
            datas[index] = t;
            t.Index = index;
            t.key = key;
            nullIndex.Remove(index);
            if (!itemsWithKey.TryGetValue(t.key, out var keys))
            {
                keys = ListPool<T>.Get();
                itemsWithKey.Add(t.key, keys);
            }

            keys.Add(t);
            return true;
        }

        public T GetAt(int index)
        {
            return datas[index];
        }

        public List<T> Get(int key)
        {
            return itemsWithKey.GetValueOrDefault(key);
        }

        public T RemoveAt(int index)
        {
            if (datas[index] == null)
                return null;
            Assert.IsTrue(index < datas.Length, $"array out {index}");
            var t = datas[index];
            datas[index] = null;
            if (itemsWithKey.TryGetValue(t.key, out var keys))
            {
                keys.Remove(t);
            }

            if (keys.Count == 0)
            {
                ListPool<T>.Release(keys);
                itemsWithKey.Remove(t.key);
            }

            nullIndex.Add(index);
            nullIndex.Sort((a, b) => b.CompareTo(a));
            return t;
        }
    }
}