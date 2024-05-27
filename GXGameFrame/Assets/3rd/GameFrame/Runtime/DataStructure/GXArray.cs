using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class GXArray<T> : IReference where T : class, IReference, new()
    {
        private T[] m_Item;

        public List<int> Indexs { get; private set; }

        public T this[int index] => m_Item[index];

        public void Init(int arrayMaxCount)
        {
            if (m_Item == null || m_Item.Length != arrayMaxCount)
            {
                m_Item = new T[arrayMaxCount];
            }

            Indexs = new List<int>(arrayMaxCount);
            Indexs.Clear();
        }

        public T Add(int index, Type type)
        {
            if (m_Item[index] != null)
            {
                return default(T);
            }

            var t = (T) ReferencePool.Acquire(type);
            Indexs.Add(index);
            m_Item[index] = t;
            return t;
        }


        public void Remove(int index)
        {
            if (m_Item[index] == null)
            {
                return;
            }

            m_Item[index] = default(T);
            Indexs.RemoveAtSwapBack(index);
        }

        public void Clear()
        {
            foreach (var index in Indexs)
            {
                ReferencePool.Release(m_Item[index]);
                m_Item[index] = default(T);
            }
            Indexs.Clear();
        }
    }
}