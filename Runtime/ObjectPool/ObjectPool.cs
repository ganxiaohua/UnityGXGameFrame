using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame
{
    public class ObjectPool<T> : IObjectPoolBase where T : ObjectBase
    {
        [ShowInInspector] public TypeNamePair TypeName;
        
        [ShowInInspector]
        private List<T> hideObject = new List<T>();

        /// <summary>
        /// 是否是激活的
        /// </summary>
        [ShowInInspector] public bool Activate;
        

        /// <summary>
        /// 对象池最大数量
        /// </summary>
        [ShowInInspector] private int maxCacheCount;


        /// <summary>
        /// 对象池需要携带的内容
        /// </summary>
        [ShowInInspector]
        private object userData;

        /// <summary>
        /// 对象池每一帧吐出的最大数量
        /// </summary>
        [ShowInInspector]
        private int maxSpawnCount;


        /// <summary>
        /// 对象池当前自动施法时间
        /// </summary>
        [ShowInInspector]
        private float curAutoReleaseTime;

        /// <summary>
        /// 对象池轮训检查时间
        /// </summary>
        [ShowInInspector]
        private float autoReleaseInterval;

        /// <summary>
        /// 对象池内部对象进入HitObjet列表之后的到期事件
        /// </summary>
        [ShowInInspector]
        private float expireTime;

        // private List<ObjectBase> m_ActionObjects;
        [ShowInInspector]
        private Type objectType;

        private int frameVersion;

        private int frameSpawn;

        public static ObjectPool<T> Create(TypeNamePair typeName, Type objectType, int maxNum, int expireTime, object userData)
        {
            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            ObjectPool<T> objectPool = ReferencePool.Acquire(objectPoolType) as ObjectPool<T>;
            objectPool.Initialize(typeName);
            objectPool.maxCacheCount = maxNum;
            objectPool.userData = userData;
            objectPool.expireTime = expireTime;
            objectPool.autoReleaseInterval = 5;
            objectPool.curAutoReleaseTime = 0;
            objectPool.Activate = true;
            objectPool.maxSpawnCount = 0;
            return objectPool;
        }

        private void Initialize(TypeNamePair typeName)
        {
            TypeName = typeName;
            objectType = typeof(T);
        }

        /// <summary>
        /// 需要带入进去的userData
        /// </summary>
        /// <param name="userData"></param>
        public void SetUserData(object userData)
        {
            this.userData = userData;
        }

        public void SetLuck(T obj, bool luck)
        {
            if (obj == null)
            {
                return;
            }

            obj.Luck = luck;
        }


        /// <summary>
        /// 设置这一这个类型的对象最大每一帧吐出多少个对象
        /// </summary>
        /// <param name="MaxCount"></param>
        public void SetAsyncMaxCount(int maxCount = 20)
        {
            if (maxSpawnCount == 0)
                maxSpawnCount = maxCount;
        }

        /// <summary>
        /// 设置 对象池的到期事件和检查时间
        /// </summary>
        /// <param name="autoReleaseInterval">定期检查时间</param>
        /// <param name="expireTime">进入不活跃对象池之后的到期时间</param>
        public void SetExamineTime(float expireTime)
        {
            this.expireTime = expireTime;
        }

        public void SetMaxCount(int maxCount)
        {
            this.maxCacheCount = maxCount;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            curAutoReleaseTime += elapseSeconds;
            if (curAutoReleaseTime > autoReleaseInterval)
            {
                TimeCheck();
                curAutoReleaseTime = 0;
            }
        }
        
        /// <summary>
        /// 异步吐出
        /// </summary>
        /// <returns></returns>
        public async UniTask<T> SpawnAsync(System.Threading.CancellationToken token = default)
        {
            while (true)
            {
                if (token.IsCancellationRequested || !Activate)
                    return null;

                if (frameSpawn < maxSpawnCount || frameVersion != Time.frameCount)
                    break;
                
                await UniTask.Yield();
            }

            if (frameVersion != Time.frameCount)
            {
                frameVersion = Time.frameCount;
                frameSpawn = 0;
            }

            frameSpawn++;
            return Spawn();
        }

        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="t"></param>
        /// <exception cref="Exception"></exception>
        public T Spawn()
        {
            return SpawnKey();
        }

        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="key">同一种类型可以存在不同的对象池快</param>
        public T SpawnKey()
        {
            // m_CurWaitSpawnCount++;
            T poolobject = null;
            if (hideObject.Count > 0)
            {
                poolobject = hideObject[^1];
                hideObject.RemoveAt(hideObject.Count - 1);
            }

            if (poolobject == null)
            {
                poolobject = ReferencePool.Acquire(objectType) as T;
                poolobject.Initialize(userData);
            }

            poolobject.OnSpawn();
            return poolobject;
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="t"></param>
        /// <exception cref="Exception"></exception>
        public void UnSpawn(T t)
        {
#if UNITY_EDITOR
            foreach (var obj in hideObject)
            {
                if (obj == t)
                {
                    Debugger.LogError($"{t.GetType()} is Already recycling.");
                    return;
                }
            }
#endif
            hideObject.Add(t);
            t.ExpiredTime = Time.realtimeSinceStartup + expireTime;
            t.OnUnspawn();
            AmountCheck();
        }

        /// <summary>
        /// 缓存池最大数目检查超过则减少一半
        /// </summary>
        public void AmountCheck()
        {
            if (hideObject.Count <= maxCacheCount)
                return;
            int clearNum = maxCacheCount / 2;
            for (int i = hideObject.Count - 1; i >= clearNum; i--)
            {
                if (hideObject[i].Luck)
                    continue;
                hideObject.RemoveAt(i);
                ReferencePool.Release(hideObject[i]);
            }
        }

        /// <summary>
        /// 超时检查
        /// </summary>
        public void TimeCheck()
        {
            for (int i = hideObject.Count - 1; i >= 0; i--)
            {
                if (Time.realtimeSinceStartup >= hideObject[i].ExpiredTime && !hideObject[i].Luck)
                {
                    ReferencePool.Release(hideObject[i]);
                    hideObject.RemoveAt(i);
                }
            }
        }


        public void Dispose()
        {
            Activate = false;
            curAutoReleaseTime = 0;
            maxSpawnCount = 0;
            hideObject.Clear();
        }
    }
}