using System;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;

namespace GameFrame
{
    public class ObjectPool<T> : IObjectPoolBase where T : ObjectBase
    {
        private Dictionary<string, List<T>> m_ActionObject;
        private Dictionary<string, List<T>> m_HitObject;

        private Dictionary<T, string> m_ActionObjectWithName;

        private List<T> NeedClearList;

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
        private object m_InitData;

        /// <summary>
        /// 对象池当前轮训的事件
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

        public static ObjectPool<T> Create(TypeNamePair typeName, Type objectType, int maxNum, object initData)
        {
            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            ObjectPool<T> objectPool = ReferencePool.Acquire(objectPoolType) as ObjectPool<T>;
            objectPool.NeedClearList = new List<T>();
            objectPool.Initialize(typeName);
            objectPool.m_MaxCacheNum = maxNum;
            objectPool.m_InitData = initData;
            objectPool.m_ExpireTime = 120;
            objectPool.m_AutoReleaseInterval = 5;
            objectPool.m_CurAutoReleaseTime = 0;
            return objectPool;
        }

        private void Initialize(TypeNamePair typeName)
        {
            m_CurCacheNum = 0;
            TypeName = typeName;
            m_ObjectType = typeof(T);
            m_ActionObjectWithName = new();
            m_ActionObject = new();
            m_HitObject = new();
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
        }


        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="t"></param>
        /// <exception cref="Exception"></exception>
        public T Spawn()
        {
            return SpawnKey("");
        }

        /// <summary>
        /// 吐出
        /// </summary>
        /// <param name="key"></param>
        public T SpawnKey(string key)
        {
            if (key == null)
            {
                return null;
            }

            T poolobject = null;
            if (m_HitObject.TryGetValue(key, out List<T> poolobjectlist))
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
                poolobject.Initialize(m_InitData);
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
            if (!m_HitObject.TryGetValue(key, out List<T> objectList))
            {
                objectList = new List<T>();
                m_HitObject.Add(key, objectList);
            }

            m_CurCacheNum++;
            objectList.Add(t);
            t.LastUseTime = DateTime.UtcNow;
            t.OnUnspawn();
            AmountCheck();
        }

        public void AmountCheck()
        {
            if (m_CurCacheNum <= m_MaxCacheNum)
                return;
            NeedClearList.Clear();
            int clearNum = m_MaxCacheNum / 2;
            int curClearNum = 0;
            foreach (var item in m_HitObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    NeedClearList.Add(list[i]);
                    list.RemoveAt(i);
                    curClearNum++;
                    if (curClearNum == clearNum)
                    {
                        ClearPasdue(NeedClearList);
                        return;
                    }
                }
            }
        }

        public void TimeCheck()
        {
            NeedClearList.Clear();
            foreach (var item in m_HitObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    DateTime expireTime = DateTime.UtcNow.AddSeconds(-m_ExpireTime);
                    if (expireTime >= list[i].LastUseTime)
                    {
                        NeedClearList.Add(list[i]);
                        list.RemoveAt(i);
                    }
                }
            }

            ClearPasdue(NeedClearList);
        }

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
            m_CurAutoReleaseTime = 0;
            ClearDic(m_ActionObject);
            ClearDic(m_HitObject);
            m_ActionObject.Clear();
            m_HitObject.Clear();
        }
    }
}