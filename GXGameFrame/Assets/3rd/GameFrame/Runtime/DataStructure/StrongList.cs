﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrame
{
    public class StrongList<T> : IEnumerator<T>, IEnumerable<T>
    {
        private readonly List<T> m_Data;
        private int m_CurrentIndex;
        private T m_CurrentElement;

        private bool Lock => m_CurrentIndex >= 0;

        public T Current => m_CurrentElement;

        object IEnumerator.Current => m_CurrentElement;

        public IReadOnlyList<T> Data => m_Data;

        private bool m_keepOrder;

        public StrongList(int capacity = 0, bool keepOrder = false)
        {
            m_Data = new List<T>(capacity);
            m_CurrentIndex = -1;
            m_CurrentElement = default;
            m_keepOrder = keepOrder;
        }

        public void Add(T val)
        {
            m_Data.Add(val);
        }

        public bool Remove(T val)
        {
            if (Lock)
            {
                int index = m_Data.IndexOf(val);
                if (index == -1)
                    return false;

                if (index > m_CurrentIndex - 1)
                {
                    if (!m_keepOrder)
                        m_Data.RemoveAtSwapBack(index);
                    else
                    {
                        m_Data.RemoveAt(index);
                    }
                }
                else if (index < m_CurrentIndex - 1)
                {
                    m_Data[index] = m_Data[m_CurrentIndex - 1];
                    if (!m_keepOrder)
                        m_Data.RemoveAtSwapBack(m_CurrentIndex - 1);
                    else
                    {
                        m_Data.RemoveAt(index);
                    }

                    m_CurrentIndex--;
                }
                else
                {
                    if (!m_keepOrder)
                        m_Data.RemoveAtSwapBack(index);
                    else
                    {
                        m_Data.RemoveAt(index);
                    }

                    m_CurrentIndex--;
                }

                return true;
            }
            else
            {
                if (!m_keepOrder)
                    return m_Data.RemoveSwapBack(val);
                else
                {
                    return m_Data.Remove(val);
                }
            }
        }

        public bool Contains(T val)
        {
            return m_Data.Contains(val);
        }

        public bool MoveNext()
        {
            if (m_CurrentIndex == m_Data.Count)
            {
                m_CurrentElement = default;
                return false;
            }
            else
            {
                m_CurrentElement = m_Data[m_CurrentIndex++];
                return true;
            }
        }

        public void Reset()
        {
            if (Lock) throw new Exception($"Enumerator locked");
            m_CurrentIndex = 0;
            m_CurrentElement = default;
        }

        public void Dispose()
        {
            m_CurrentIndex = -1;
            m_CurrentElement = default;
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Reset();
            return this;
        }
    }
}