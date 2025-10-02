using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 引用池。
    /// </summary>
    public static partial class ReferencePool
    {
        /// <summary>
        /// 引用池当前轮训的时间
        /// </summary>
        private static float sCurAutoReleaseTime;

        /// <summary>
        /// 引用池轮训检查时间
        /// </summary>
        private const float AutoReleaseInterval = 10;

        private static readonly Dictionary<Type, ReferenceCollection> sReferenceCollections = new Dictionary<Type, ReferenceCollection>();
        private static List<Type> sWaitDestroyList = new();


        /// <summary>
        /// 获取引用池的数量。
        /// </summary>
        public static int Count
        {
            get
            {
                lock (sReferenceCollections)
                {
                    return sReferenceCollections.Count;
                }
            }
        }

        /// <summary>
        /// 获取所有引用池的信息。
        /// </summary>
        /// <returns>所有引用池的信息。</returns>
        public static ReferencePoolInfo[] GetAllReferencePoolInfos()
        {
            int index = 0;
            ReferencePoolInfo[] results = null;
            lock (sReferenceCollections)
            {
                results = new ReferencePoolInfo[sReferenceCollections.Count];
                foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in sReferenceCollections)
                {
                    results[index++] = new ReferencePoolInfo(referenceCollection.Key, referenceCollection.Value.UnusedReferenceCount,
                            referenceCollection.Value.UsingReferenceCount, referenceCollection.Value.AcquireReferenceCount,
                            referenceCollection.Value.ReleaseReferenceCount, referenceCollection.Value.AddReferenceCount,
                            referenceCollection.Value.RemoveReferenceCount);
                }
            }

            return results;
        }

        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (realElapseSeconds >= sCurAutoReleaseTime)
            {
                sWaitDestroyList.Clear();
                sCurAutoReleaseTime = realElapseSeconds + AutoReleaseInterval;
                lock (sReferenceCollections)
                {
                    foreach (ReferenceCollection referencepool in sReferenceCollections.Values)
                    {
                        if (referencepool.UnusedCheck())
                        {
                            sWaitDestroyList.Add(referencepool.ReferenceType);
                            if (sWaitDestroyList.Count == 10)
                            {
                                break;
                            }
                        }
                    }
                }

                foreach (Type type in sWaitDestroyList)
                {
                    Debugger.Log($"clear {type.Name} ReleasePool");
                    RemoveAll(type);
                }

                sWaitDestroyList.Clear();
            }
        }

        /// <summary>
        /// 清除所有引用池。
        /// </summary>
        public static void ClearAll()
        {
            lock (sReferenceCollections)
            {
                foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in sReferenceCollections)
                {
                    referenceCollection.Value.RemoveAll();
                }

                sReferenceCollections.Clear();
            }
        }

        /// <summary>
        /// 设置当前引用池子中的对象数量
        /// </summary>
        /// <typeparam name="T">引用类型</typeparam>
        /// <returns></returns>
        public static void SetMaxReferenceCount<T>(int count) where T : class, IDisposable
        {
            lock (sReferenceCollections)
            {
                if (sReferenceCollections.TryGetValue(typeof(T), out ReferenceCollection referenceCollection))
                {
                    referenceCollection.SetMaxAcquireReferenceCount(count);
                    return;
                }
            }

            throw new Exception($"{typeof(T).Name} not have ");
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <returns>引用。</returns>
        public static T Acquire<T>() where T : class, IDisposable
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <returns>引用。</returns>
        public static IDisposable Acquire(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            return GetReferenceCollection(referenceType).Acquire();
        }


        /// <summary>
        /// 将引用归还引用池。
        /// </summary>
        /// <param name="disposable">引用。</param>
        public static void Release(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new Exception("Reference is invalid.");
            }

            Type referenceType = disposable.GetType();
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Release(disposable);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public static void Add<T>(int count) where T : class, IDisposable, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public static void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public static void Remove<T>(int count) where T : class, IDisposable
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public static void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static void RemoveAll<T>() where T : class, IDisposable
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
            sReferenceCollections.Remove(referenceType);
        }

        private static void InternalCheckReferenceType(Type referenceType)
        {
#if UNITY_EDITOR
            if (referenceType == null)
            {
                throw new Exception("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new Exception("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IDisposable).IsAssignableFrom(referenceType))
            {
                throw new Exception($"Reference type '{referenceType.FullName}' is invalid");
            }
#endif
        }

        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new Exception("ReferenceType is invalid.");
            }

            ReferenceCollection referenceCollection = null;
            lock (sReferenceCollections)
            {
                if (!sReferenceCollections.TryGetValue(referenceType, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(referenceType);
                    sReferenceCollections.Add(referenceType, referenceCollection);
                }
            }

            return referenceCollection;
        }
        //
    }
}