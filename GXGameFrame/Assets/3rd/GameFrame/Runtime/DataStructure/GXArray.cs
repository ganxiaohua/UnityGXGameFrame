using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class GXArray<T> : IReference where T : class, IReference, new()
    {
        private T[] m_Item;

        public List<int> Indexs { get; private set; }

        private int m_MaxIndex;

        public T this[int index]
        {
            get { return Get(index); }
        }

        public void Init(int arrayMaxCount)
        {
            if (m_Item == null || m_Item.Length != arrayMaxCount)
            {
                m_Item = new T[arrayMaxCount];
            }

            Indexs = new List<int>(arrayMaxCount);
            m_MaxIndex = arrayMaxCount;
            Indexs.Clear();
        }

        public T Add(int index, Type type)
        {
            if (index >= m_MaxIndex)
            {
                Debugger.LogError($"array maxcount is{m_MaxIndex} but you want to {index}");
                return null;
            }

            if (m_Item[index] != null)
            {
                return default(T);
            }

            var t = (T) ReferencePool.Acquire(type);
            Indexs.Add(index);
            m_Item[index] = t;
            return t;
        }

        public T Get(int index)
        {
            if (index >= m_MaxIndex)
            {
                Debugger.LogError($"array maxcount is{m_MaxIndex} but you want to {index}");
                return null;
            }

            return m_Item[index];
        }

        public void Remove(int index)
        {
            if (index >= m_MaxIndex)
            {
                Debugger.LogError($"array maxcount is{m_MaxIndex} but you want to {index}");
                return;
            }

            if (m_Item[index] == null)
            {
                return;
            }

            m_Item[index] = default(T);
            RemoveAtSwapBack(index);
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

        private void RemoveAtSwapBack(int index)
        {
            int tail = Indexs.Count - 1;
            if (index != tail)
                Indexs[index] = Indexs[tail];
            Indexs.RemoveAt(tail);
        }
    }
}