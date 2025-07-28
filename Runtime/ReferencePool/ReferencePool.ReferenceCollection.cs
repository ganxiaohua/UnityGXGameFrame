using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IDisposable> references;
            private readonly Type referenceType;
            private int maxAcquireReferenceCount;
            private int usingReferenceCount;
            private int acquireReferenceCount;
            private int releaseReferenceCount;
            private int addReferenceCount;
            private int removeReferenceCount;


            /// <summary>
            /// 引用池内部对象进入HitObjet列表之后的到期事件
            /// </summary>
            private float expireTime;

            /// <summary>
            /// 最后一次使用对象池的事件
            /// </summary>
            public float ExpiredTime;

            public ReferenceCollection(Type referenceType)
            {
                references = new Queue<IDisposable>();
                this.referenceType = referenceType;
                maxAcquireReferenceCount = 10;
                usingReferenceCount = 0;
                acquireReferenceCount = 0;
                releaseReferenceCount = 0;
                addReferenceCount = 0;
                removeReferenceCount = 0;
                expireTime = 60; //如果这个池子超过一分钟没有使用则释放所有的引用
            }

            public Type ReferenceType
            {
                get { return referenceType; }
            }

            public int UnusedReferenceCount
            {
                get { return references.Count; }
            }

            public int UsingReferenceCount
            {
                get { return usingReferenceCount; }
            }

            public int AcquireReferenceCount
            {
                get { return acquireReferenceCount; }
            }

            public int ReleaseReferenceCount
            {
                get { return releaseReferenceCount; }
            }

            public int AddReferenceCount
            {
                get { return addReferenceCount; }
            }

            public int RemoveReferenceCount
            {
                get { return removeReferenceCount; }
            }

            public int MaxAcquireReferenceCount
            {
                get { return maxAcquireReferenceCount; }
            }

            public void SetMaxAcquireReferenceCount(int count)
            {
                maxAcquireReferenceCount = count;
            }

            public T Acquire<T>() where T : class, IDisposable
            {
                if (typeof(T) != referenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                return (T)Acquire();
            }

            public IDisposable Acquire()
            {
                usingReferenceCount++;
                acquireReferenceCount++;
                lock (references)
                {
                    if (references.Count > 0)
                    {
                        return references.Dequeue();
                    }
                }

                addReferenceCount++;
                return (IDisposable) Activator.CreateInstance(referenceType);
            }


            public void Release(IDisposable disposable)
            {
                lock (references)
                {
#if UNITY_EDITOR
                    if (references.Contains(disposable))
                    {
                        throw new Exception("The reference has been released.");
                    }
#endif
                    references.Enqueue(disposable);
                }

                disposable.Dispose();
                releaseReferenceCount++;
                usingReferenceCount--;
                if (releaseReferenceCount >= maxAcquireReferenceCount)
                {
                    Remove(releaseReferenceCount / 2);
                }

                ExpiredTime = Time.realtimeSinceStartup + expireTime;
            }

            public void Add<T>(int count) where T : class, IDisposable, new()
            {
                if (typeof(T) != referenceType)
                {
                    throw new Exception("Type is invalid.");
                }

                lock (references)
                {
                    addReferenceCount += count;
                    while (count-- > 0)
                    {
                        references.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (references)
                {
                    addReferenceCount += count;
                    while (count-- > 0)
                    {
                        references.Enqueue((IDisposable) Activator.CreateInstance(referenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (references)
                {
                    if (count > references.Count)
                    {
                        count = references.Count;
                    }

                    removeReferenceCount += count;
                    while (count-- > 0)
                    {
                        references.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (references)
                {
                    removeReferenceCount += references.Count;
                    references.Clear();
                }
            }

            public bool UnusedCheck()
            {
                if (usingReferenceCount == 0)
                {
                    if (Time.realtimeSinceStartup >= ExpiredTime)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}