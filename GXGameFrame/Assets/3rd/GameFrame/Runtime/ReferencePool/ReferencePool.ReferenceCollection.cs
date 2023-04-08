using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> m_References;
            private readonly Type m_ReferenceType;
            private int m_MaxAcquireReferenceCount;
            private int m_UsingReferenceCount;
            private int m_AcquireReferenceCount;
            private int m_ReleaseReferenceCount;
            private int m_AddReferenceCount;
            private int m_RemoveReferenceCount;


            /// <summary>
            /// 引用池内部对象进入HitObjet列表之后的到期事件
            /// </summary>
            private float m_ExpireTime;

            /// <summary>
            /// 最后一次使用对象池的事件
            /// </summary>
            public DateTime m_LastUseTime;

            public ReferenceCollection(Type referenceType)
            {
                m_References = new Queue<IReference>();
                m_ReferenceType = referenceType;
                m_MaxAcquireReferenceCount = 10;
                m_UsingReferenceCount = 0;
                m_AcquireReferenceCount = 0;
                m_ReleaseReferenceCount = 0;
                m_AddReferenceCount = 0;
                m_RemoveReferenceCount = 0;
                m_ExpireTime = 60; //如果这个池子超过一分钟没有使用则释放所有的引用
            }

            public Type ReferenceType
            {
                get { return m_ReferenceType; }
            }

            public int UnusedReferenceCount
            {
                get { return m_References.Count; }
            }

            public int UsingReferenceCount
            {
                get { return m_UsingReferenceCount; }
            }

            public int AcquireReferenceCount
            {
                get { return m_AcquireReferenceCount; }
            }

            public int ReleaseReferenceCount
            {
                get { return m_ReleaseReferenceCount; }
            }

            public int AddReferenceCount
            {
                get { return m_AddReferenceCount; }
            }

            public int RemoveReferenceCount
            {
                get { return m_RemoveReferenceCount; }
            }

            public int MaxAcquireReferenceCount
            {
                get { return m_MaxAcquireReferenceCount; }
            }

            public void SetMaxAcquireReferenceCount(int count)
            {
                m_MaxAcquireReferenceCount = count;
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T) m_References.Dequeue();
                    }
                }

                m_AddReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                m_UsingReferenceCount++;
                m_AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                m_LastUseTime = DateTime.UtcNow;
                m_AddReferenceCount++;
                return (IReference) Activator.CreateInstance(m_ReferenceType);
            }


            public void Release(IReference reference)
            {
                reference.Clear();
                lock (m_References)
                {
                    if (m_EnableStrictCheck && m_References.Contains(reference))
                    {
                        throw new Exception("The reference has been released.");
                    }

                    m_References.Enqueue(reference);
                }

                m_ReleaseReferenceCount++;
                m_UsingReferenceCount--;
                if (m_ReleaseReferenceCount >= m_MaxAcquireReferenceCount)
                {
                    Remove(m_ReleaseReferenceCount / 2);
                }

                m_LastUseTime = DateTime.UtcNow;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != m_ReferenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }

                m_LastUseTime = DateTime.UtcNow;
            }

            public void Add(int count)
            {
                lock (m_References)
                {
                    m_AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference) Activator.CreateInstance(m_ReferenceType));
                    }
                }

                m_LastUseTime = DateTime.UtcNow;
            }

            public void Remove(int count)
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    m_RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }

                m_LastUseTime = DateTime.UtcNow;
            }

            public void RemoveAll()
            {
                lock (m_References)
                {
                    m_RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }

            public bool UnusedCheck()
            {
                DateTime expireTime = DateTime.UtcNow.AddSeconds(-m_ExpireTime);
                if (expireTime >= m_LastUseTime)
                {
                    return true;
                }
                return false;
            }
        }
    }
}