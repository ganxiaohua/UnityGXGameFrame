using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

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
            Assert.IsTrue(index < m_MaxIndex, $"Array overreach");
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
            // Assert.IsTrue(index < m_MaxIndex, $"Array overreach");
            return m_Item[index];
        }

        public void Remove(int index)
        {
            Assert.IsTrue(index < m_MaxIndex, $"Array overreach");
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