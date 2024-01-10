using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class GXArray<T> : IReference where T : class, IReference, new()
    {
        private T[] m_Item;

        private List<int> m_Indexs;

        private int MaxIndex;

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

            m_Indexs = new List<int>(arrayMaxCount);
            MaxIndex = arrayMaxCount;
            m_Indexs.Clear();
        }

        public T Add(int index, Type type)
        {
            if (index >= MaxIndex)
            {
                Debugger.LogError($"array maxcount is{MaxIndex} but you want to {index}");
                return null;
            }

            if (m_Item[index] != null)
            {
                return default(T);
            }

            var t = (T) ReferencePool.Acquire(type);
            m_Indexs.Add(index);
            m_Item[index] = t;
            return t;
        }

        public T Get(int index)
        {
            if (index >= MaxIndex)
            {
                Debugger.LogError($"array maxcount is{MaxIndex} but you want to {index}");
                return null;
            }

            return m_Item[index];
        }

        public void Remove(int index)
        {
            if (index >= MaxIndex)
            {
                Debugger.LogError($"array maxcount is{MaxIndex} but you want to {index}");
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
            foreach (var index in m_Indexs)
            {
                ReferencePool.Release(m_Item[index]);
                m_Item[index] = default(T);
            }

            m_Indexs.Clear();
        }

        private void RemoveAtSwapBack(int index)
        {
            int tail = m_Indexs.Count - 1;
            if (index != tail)
                m_Indexs[index] = m_Indexs[tail];
            m_Indexs.RemoveAt(tail);
        }
    }
}