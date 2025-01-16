using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class GXArray<T> : IDisposable where T : class, IDisposable, new()
    {
        public T[] Items { get; private set; }

        public List<int> indexList { get; private set; }


        public void Init(int arrayMaxCount)
        {
            if (Items == null || Items.Length != arrayMaxCount)
            {
                Items = new T[arrayMaxCount];
            }

            indexList = new List<int>(arrayMaxCount);
            indexList.Clear();
        }

        public T Add(int index, Type type)
        {
            if (Items[index] != null)
            {
                return default(T);
            }

            var t = (T) ReferencePool.Acquire(type);
            indexList.Add(index);
            Items[index] = t;
            return t;
        }


        public void Remove(int index)
        {
            if (Items[index] == null)
            {
                return;
            }

            ReferencePool.Release(Items[index]);
            Items[index] = null;
            indexList.RemoveSwapBack(index);
        }

        public void Dispose()
        {
            foreach (var index in indexList)
            {
                ReferencePool.Release(Items[index]);
                Items[index] = default(T);
            }

            indexList.Clear();
        }
    }
}