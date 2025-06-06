using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class ObjectPool<T> : IObjectPoolBase where T : ObjectBase
    {
        private Dictionary<string, List<T>> actionObject;
        private Dictionary<string, List<T>> hideObject;

        private Dictionary<T, string> actionObjectWithName;

        private List<T> needClearList;


        /// <summary>
        /// 是否是激活的
        /// </summary>
        public bool Activate;

        /// <summary>
        /// 如果说吐出操作比较耗时们可以使用异步突出
        /// </summary>
        private Queue<ObjectPoolHandle> spawnAsyncQueue;

        /// <summary>
        /// 对象池最大数量
        /// </summary>
        private int maxCacheCount;

        /// <summary>
        /// 对象池当前数量
        /// </summary>
        private int curCacheNum;


        public TypeNamePair TypeName;

        /// <summary>
        /// 对象池需要携带的内容
        /// </summary>
        private object userData;

        /// <summary>
        /// 对象池每一帧吐出的最大数量
        /// </summary>
        private int maxSpawnCount;


        /// <summary>
        /// 对象池当前自动施法时间
        /// </summary>
        private float curAutoReleaseTime;

        /// <summary>
        /// 对象池轮训检查时间
        /// </summary>
        private float autoReleaseInterval;

        /// <summary>
        /// 对象池内部对象进入HitObjet列表之后的到期事件
        /// </summary>
        private float expireTime;

        // private List<ObjectBase> m_ActionObjects;
        private Type objectType;

        private static string sDefaultKey = "";

        public static ObjectPool<T> Create(TypeNamePair typeName, Type objectType, int maxNum, int expireTime, object userData)
        {
            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            ObjectPool<T> objectPool = ReferencePool.Acquire(objectPoolType) as ObjectPool<T>;
            objectPool.needClearList = new List<T>();
            objectPool.spawnAsyncQueue = new();
            objectPool.Initialize(typeName);
            objectPool.maxCacheCount = maxNum;
            objectPool.userData = userData;
            objectPool.expireTime = expireTime;
            objectPool.autoReleaseInterval = 5;
            objectPool.curAutoReleaseTime = 0;
            objectPool.Activate = true;
            objectPool.maxSpawnCount = 0;
            objectPool.SetAsyncMaxCount();
            return objectPool;
        }

        private void Initialize(TypeNamePair typeName)
        {
            curCacheNum = 0;
            TypeName = typeName;
            objectType = typeof(T);
            actionObjectWithName = new();
            actionObject = new();
            hideObject = new();
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
            curAutoReleaseTime += realElapseSeconds;
            if (curAutoReleaseTime > autoReleaseInterval)
            {
                TimeCheck();
            }

            FrameSpawn();
        }

        /// <summary>
        /// 按帧吐出对象
        /// </summary>
        private void FrameSpawn()
        {
            if (spawnAsyncQueue.Count == 0)
            {
                return;
            }

            int cur = 0;
            while (spawnAsyncQueue.Count > 0 && cur < maxSpawnCount)
            {
                cur++;
                ObjectPoolHandle objectPoolHandle = spawnAsyncQueue.Dequeue();
                objectPoolHandle.Complete();
            }
        }


        /// <summary>
        /// 异步吐出
        /// </summary>
        /// <returns></returns>
        public async UniTask<T> SpawnAsync(System.Threading.CancellationToken token = default)
        {
            ObjectPoolHandle objectPoolHandle = ReferencePool.Acquire<ObjectPoolHandle>();
            objectPoolHandle.SetToken(token);
            spawnAsyncQueue.Enqueue(objectPoolHandle);
            await objectPoolHandle;
            bool spawn = (objectPoolHandle.TaskState == TaskState.Succ && Activate);
            ReferencePool.Release(objectPoolHandle);
            if (!spawn)
                return null;
            return Spawn();
        }

        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="t"></param>
        /// <exception cref="Exception"></exception>
        public T Spawn()
        {
            return SpawnKey(sDefaultKey);
        }

        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="key">同一种类型可以存在不同的对象池快</param>
        public T SpawnKey(string key)
        {
            // m_CurWaitSpawnCount++;
            if (key == null)
            {
                return null;
            }

            T poolobject = null;
            if (hideObject.TryGetValue(key, out List<T> poolobjectlist))
            {
                if (poolobjectlist.Count > 0)
                {
                    poolobject = poolobjectlist[poolobjectlist.Count-1];
                    poolobjectlist.RemoveAt(poolobjectlist.Count-1);
                }
            }

            if (poolobject == null)
            {
                poolobject = ReferencePool.Acquire(objectType) as T;
                poolobject.Initialize(userData);
            }

            if (!actionObject.TryGetValue(key, out List<T> objectList))
            {
                objectList = new List<T>();
                actionObject.Add(key, objectList);
            }

            objectList.Add(poolobject);
            actionObjectWithName.Add(poolobject, key);
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
            if (!actionObjectWithName.TryGetValue(t, out string key))
            {
                throw new Exception($"{objectType.Name} not has");
            }

            actionObject[key].Remove(t);
            actionObjectWithName.Remove(t);
            if (!hideObject.TryGetValue(key, out List<T> objectList))
            {
                objectList = new List<T>();
                hideObject.Add(key, objectList);
            }

            curCacheNum++;
            objectList.Add(t);
            t.ExpiredTime = Time.realtimeSinceStartup + expireTime;
            t.OnUnspawn();
            AmountCheck();
        }

        /// <summary>
        /// 缓存池最大数目检查超过则减少一半
        /// </summary>
        public void AmountCheck()
        {
            if (curCacheNum <= maxCacheCount)
                return;
            needClearList.Clear();
            int clearNum = maxCacheCount / 2;
            int curClearNum = 0;
            foreach (var item in hideObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Luck)
                        continue;
                    needClearList.Add(list[i]);
                    list.RemoveAt(i);
                    curClearNum++;
                    if (curClearNum == clearNum)
                    {
                        curCacheNum -= curClearNum;
                        ClearPasdue(needClearList);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 超时检查
        /// </summary>
        public void TimeCheck()
        {
            needClearList.Clear();
            foreach (var item in hideObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (Time.realtimeSinceStartup >= list[i].ExpiredTime && !list[i].Luck)
                    {
                        needClearList.Add(list[i]);
                        list.RemoveAt(i);
                        curCacheNum--;
                    }
                }
            }

            ClearPasdue(needClearList);
        }

        /// <summary>
        /// 清理对象 检查时间归零重新计时
        /// </summary>
        /// <param name="pasdue"></param>
        public void ClearPasdue(List<T> pasdue)
        {
            if (pasdue.Count == 0)
            {
                return;
            }

            foreach (var item in pasdue)
            {
                ReferencePool.Release(item);
            }

            curAutoReleaseTime = 0;
        }


        public void Dispose()
        {
            void ClearDic(Dictionary<string, List<T>> dic)
            {
                foreach (var objectListKV in dic)
                {
                    List<T> objectList = objectListKV.Value;
                    ClearPasdue(objectList);
                }
            }

            Activate = false;
            while (spawnAsyncQueue.Count > 0)
            {
                var spawnaSynce = spawnAsyncQueue.Dequeue();
                spawnaSynce.Cancel();
            }

            spawnAsyncQueue.Clear();
            curCacheNum = 0;
            curAutoReleaseTime = 0;
            maxSpawnCount = 0;
            ClearDic(actionObject);
            ClearDic(hideObject);
            actionObject.Clear();
            actionObjectWithName.Clear();
            hideObject.Clear();
        }
    }
}