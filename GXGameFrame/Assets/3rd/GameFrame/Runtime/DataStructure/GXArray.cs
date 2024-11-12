using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class GXArray<T> : IDisposable where T : class, IDisposable, new()
    {
        private T[] items;

        public List<int> indexList { get; private set; }

        public T this[int index] => items[index];

        public void Init(int arrayMaxCount)
        {
            if (items == null || items.Length != arrayMaxCount)
            {
                items = new T[arrayMaxCount];
            }

            indexList = new List<int>(arrayMaxCount);
            indexList.Clear();
        }

        public T Add(int index, Type type)
        {
            if (items[index] != null)
            {
                return default(T);
            }

            var t = (T) ReferencePool.Acquire(type);
            indexList.Add(index);
            items[index] = t;
            return t;
        }


        public void Remove(int index)
        {
            if (items[index] == null)
            {
                return;
            }
        
            ReferencePool.Release(items[index]);
            items[index] = null;
            indexList.RemoveSwapBack(index);
        }

        public void Dispose()
        {
            foreach (var index in indexList)
            {
                ReferencePool.Release(items[index]);
                items[index] = default(T);
            }
            indexList.Clear();
        }
    }
}