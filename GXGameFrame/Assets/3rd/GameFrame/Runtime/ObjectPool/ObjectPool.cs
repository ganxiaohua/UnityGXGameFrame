using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class ObjectPool<T> : IObjectPoolBase where T :  ObjectBase

    {
        private Dictionary<string, List<T>> m_ActionObject;
        private Dictionary<string, List<T>> m_HideObject;

        private Dictionary<T, string> m_ActionObjectWithName;

        private List<T> NeedClearList;

        
        /// <summary>
        /// 是否是激活的
        /// </summary>
        public bool m_Activate;

        /// <summary>
        /// 如果说吐出操作比较耗时们可以使用异步突出
        /// </summary>
        private Queue<ObjectPoolHandle> m_SpawnAsyncQueue;

        /// <summary>
        /// 对象池最大数量
        /// </summary>
        private int m_MaxCacheNum;

        /// <summary>
        /// 对象池当前数量
        /// </summary>
        private int m_CurCacheNum;


        public TypeNamePair TypeName;

        /// <summary>
        /// 对象池需要携带的内容
        /// </summary>
        private object m_UserData;

        /// <summary>
        /// 对象池每一帧吐出的最大数量
        /// </summary>
        private static Dictionary<Type, int> s_MaxSpawnAsynDic = new Dictionary<Type, int>();
        

        /// <summary>
        /// 对象池当前自动施法时间
        /// </summary>
        private float m_CurAutoReleaseTime;

        /// <summary>
        /// 对象池轮训检查时间
        /// </summary>
        private float m_AutoReleaseInterval;
        
        /// <summary>
        /// 对象池内部对象进入HitObjet列表之后的到期事件
        /// </summary>
        private float m_ExpireTime;

        // private List<ObjectBase> m_ActionObjects;
        private Type m_ObjectType;

        private static string sDefaultKey = "";

        public static ObjectPool<T> Create(TypeNamePair typeName, Type objectType, int maxNum, int expireTime, object userData)
        {
            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            ObjectPool<T> objectPool = ReferencePool.Acquire(objectPoolType) as ObjectPool<T>;
            objectPool.NeedClearList = new List<T>();
            objectPool.m_SpawnAsyncQueue = new();
            objectPool.Initialize(typeName);
            objectPool.m_MaxCacheNum = maxNum;
            objectPool.m_UserData = userData;
            objectPool.m_ExpireTime = expireTime;
            objectPool.m_AutoReleaseInterval = 5;
            objectPool.m_CurAutoReleaseTime = 0;
            objectPool.m_Activate = true;
            return objectPool;
        }

        private void Initialize(TypeNamePair typeName)
        {
            m_CurCacheNum = 0;
            TypeName = typeName;
            m_ObjectType = typeof(T);
            m_ActionObjectWithName = new();
            m_ActionObject = new();
            m_HideObject = new();
        }

        /// <summary>
        /// 需要带入进去的userData
        /// </summary>
        /// <param name="userData"></param>
        public void SetUserData(object userData)
        {
            this.m_UserData = userData;
        }

        public void SetLuck(T obj,bool luck)
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
            s_MaxSpawnAsynDic[typeof(T)] = maxCount;
        }

        /// <summary>
        /// 设置 对象池的到期事件和检查时间
        /// </summary>
        /// <param name="autoReleaseInterval">定期检查时间</param>
        /// <param name="expireTime">进入不活跃对象池之后的到期时间</param>
        public void SetExamineTime(float autoReleaseInterval, float expireTime)
        {
            m_AutoReleaseInterval = autoReleaseInterval;
            m_ExpireTime = expireTime;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_CurAutoReleaseTime += realElapseSeconds;
            if (m_CurAutoReleaseTime > m_AutoReleaseInterval)
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
            if (m_SpawnAsyncQueue.Count == 0)
            {
                return;
            }
            int cur = 0;
            while (m_SpawnAsyncQueue.Count > 0 && cur < s_MaxSpawnAsynDic[typeof(T)])
            {
                cur++;
                ObjectPoolHandle objectPoolHandle = m_SpawnAsyncQueue.Dequeue();
                objectPoolHandle.Complete();
            }
            
        }


        /// <summary>
        /// 异步吐出
        /// </summary>
        /// <returns></returns>
        public async UniTask<T> SpawnAsync( System.Threading.CancellationToken token = default)
        {
            ObjectPoolHandle objectPoolHandle = ReferencePool.Acquire<ObjectPoolHandle>();
            objectPoolHandle.Init(token);
            m_SpawnAsyncQueue.Enqueue(objectPoolHandle);
            if (!s_MaxSpawnAsynDic.ContainsKey(typeof(T)))
            {
                SetAsyncMaxCount();
            }
            await objectPoolHandle;
            bool spawn = (objectPoolHandle.TaskState == TaskState.Succ && m_Activate);
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
            if (m_HideObject.TryGetValue(key, out List<T> poolobjectlist))
            {
                if (poolobjectlist.Count > 0)
                {
                    poolobject = poolobjectlist[0];
                    poolobjectlist.RemoveAt(0);
                }
            }

            if (poolobject == null)
            {
                poolobject = ReferencePool.Acquire(m_ObjectType) as T;
                poolobject.Initialize(m_UserData);
            }

            if (!m_ActionObject.TryGetValue(key, out List<T> objectList))
            {
                objectList = new List<T>();
                m_ActionObject.Add(key, objectList);
            }

            objectList.Add(poolobject);
            m_ActionObjectWithName.Add(poolobject, key);
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
            if (!m_ActionObjectWithName.TryGetValue(t, out string key))
            {
                throw new Exception($"{m_ObjectType.Name} not has");
            }

            m_ActionObject[key].Remove(t);
            m_ActionObjectWithName.Remove(t);
            if (!m_HideObject.TryGetValue(key, out List<T> objectList))
            {
                objectList = new List<T>();
                m_HideObject.Add(key, objectList);
            }

            m_CurCacheNum++;
            objectList.Add(t);
            t.LastUseTime = DateTime.UtcNow;
            t.OnUnspawn();
            AmountCheck();
        }

        /// <summary>
        /// 缓存池最大数目检查超过则减少一半
        /// </summary>
        public void AmountCheck()
        {
            if (m_CurCacheNum <= m_MaxCacheNum)
                return;
            NeedClearList.Clear();
            int clearNum = m_MaxCacheNum / 2;
            int curClearNum = 0;
            foreach (var item in m_HideObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if(list[i].Luck)
                        continue;
                    NeedClearList.Add(list[i]);
                    list.RemoveAt(i);
                    curClearNum++;
                    if (curClearNum == clearNum)
                    {
                        m_CurCacheNum -= curClearNum;
                        ClearPasdue(NeedClearList);
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
            NeedClearList.Clear();
            foreach (var item in m_HideObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    DateTime expireTime = DateTime.UtcNow.AddSeconds(-m_ExpireTime);
                    if (expireTime >= list[i].LastUseTime && !list[i].Luck)
                    {
                        NeedClearList.Add(list[i]);
                        list.RemoveAt(i);
                        m_CurCacheNum--;
                    }
                }
            }

            ClearPasdue(NeedClearList);
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

            m_CurAutoReleaseTime = 0;
        }


        public void Clear()
        {
            void ClearDic(Dictionary<string, List<T>> dic)
            {
                foreach (var objectListKV in dic)
                {
                    List<T> objectList = objectListKV.Value;
                    ClearPasdue(objectList);
                }
            }

            while (m_SpawnAsyncQueue.Count>0)
            {
                var spawnaSynce = m_SpawnAsyncQueue.Dequeue();
                spawnaSynce.Cancel();
            }
            
            m_Activate = false;
            m_SpawnAsyncQueue.Clear();
            m_CurCacheNum = 0;
            m_CurAutoReleaseTime = 0;
            ClearDic(m_ActionObject);
            ClearDic(m_HideObject);
            m_ActionObject.Clear();
            m_ActionObjectWithName.Clear();
            m_HideObject.Clear();
        }
    }
}