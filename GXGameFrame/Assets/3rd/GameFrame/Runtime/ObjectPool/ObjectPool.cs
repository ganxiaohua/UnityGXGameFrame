using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace GameFrame
{
    public class ObjectPool<T> : IObjectPoolBase where T : ObjectBase
    {
        private Dictionary<string, List<T>> m_ActionObject;
        private Dictionary<string, List<T>> m_HitObject;

        private Dictionary<T, string> m_ActionObjectWithName;

        private int m_MaxCacheNum;

        private int m_CurCacheNum;

        public TypeNamePair TypeName;

        private object m_InitData;

        // private List<ObjectBase> m_ActionObjects;
        private Type m_ObjectType;

        public static ObjectPool<T> Create(TypeNamePair typeName, Type objectType, int maxNum,object initData)
        {
            Type objectPoolType = typeof(ObjectPool<>).MakeGenericType(objectType);
            ObjectPool<T> objectPool = ReferencePool.Acquire(objectPoolType) as ObjectPool<T>;
            objectPool.Initialize(typeName);
            objectPool.m_MaxCacheNum = maxNum;
            objectPool.m_InitData = initData;
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
            t.OnUnspawn();
            Check();
        }

        public void Check()
        {
            if (m_CurCacheNum <= m_MaxCacheNum)
                return;
            List<T> needClearList = new List<T>();
            int clearNum = m_MaxCacheNum / 3;
            int curClearNum = 0;
            foreach (var item in m_HitObject)
            {
                List<T> list = item.Value;
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    needClearList.Add(list[i]);
                    list.RemoveAt(i);
                    curClearNum++;
                    if (curClearNum == clearNum)
                    {
                        break;
                    }
                }

                if (curClearNum == clearNum)
                {
                    break;
                }
            }

            Clear(needClearList);
        }

        public void Clear(List<T> list)
        {
            foreach (var item in list)
            {
                ReferencePool.Release(item);
            }
        }


        public void Clear()
        {
            void ClearDic(Dictionary<string, List<T>> dic)
            {
                foreach (var objectListKV in dic)
                {
                    List<T> objectList = objectListKV.Value;
                    Clear(objectList);
                }
            }

            ClearDic(m_ActionObject);
            ClearDic(m_HitObject);
            m_ActionObject.Clear();
            m_HitObject.Clear();
        }
    }
}