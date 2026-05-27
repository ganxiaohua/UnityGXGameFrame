using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GameFrame.Runtime
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [ShowInInspector] private Dictionary<TypeNamePair, IObjectPoolBase> objectPoolBaseDict = new Dictionary<TypeNamePair, IObjectPoolBase>();
        private List<IObjectPoolBase> objectPoolBaseList = new List<IObjectPoolBase>();

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            int count = objectPoolBaseList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                if (i >= objectPoolBaseList.Count)
                    return;
                objectPoolBaseList[i].Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 创建一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxNum"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ObjectPool<T> CreateObjectPool<T>(string name, int maxNum, int expireTime, object initObject = null) where T : ObjectBase, new()
        {
            Type type = typeof(T);
            TypeNamePair typeNamePair = new TypeNamePair(type, name);
            if (HasPool(typeNamePair))
            {
                throw new Exception($"has {name} {type}");
            }

            ObjectPool<T> objectPoolBase = ObjectPool<T>.Create(typeNamePair, type, maxNum, expireTime, initObject);
            objectPoolBaseDict.Add(typeNamePair, objectPoolBase);
            objectPoolBaseList.Add(objectPoolBase);
            return objectPoolBase;
        }


        /// <summary>
        /// 获得一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ObjectPool<T> GetObjectPool<T>(string name) where T : ObjectBase, new()
        {
            Type type = typeof(T);
            TypeNamePair typeNamePair = new TypeNamePair(type, name);
            if (objectPoolBaseDict.TryGetValue(typeNamePair, out IObjectPoolBase objectbase))
            {
                return objectbase as ObjectPool<T>;
            }

            return null;
        }

        /// <summary>
        /// 删除一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxNum"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void DeleteObjectPool<T>(ObjectPool<T> objectpool) where T : ObjectBase, new()
        {
            if (objectpool == null)
            {
                throw new Exception("objectpool wei null");
            }

            ReferencePool.Release(objectpool);
            objectPoolBaseList.RemoveSwapBack(objectpool);
            objectPoolBaseDict.Remove(objectpool.TypeName);
        }

        /// <summary>
        /// 删除一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxNum"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void DeleteObjectPool<T>(string name) where T : ObjectBase, new()
        {
            var objectPool = GetObjectPool<T>(name);
            if (objectPool == null)
            {
                throw new Exception($"objectpool {name} null");
            }

            DeleteObjectPool(objectPool);
        }

        /// <summary>
        /// 删除所有对象池
        /// </summary>
        public void Disable()
        {
            foreach (KeyValuePair<TypeNamePair, IObjectPoolBase> objectPool in objectPoolBaseDict)
            {
                ReferencePool.Release(objectPool.Value);
            }

            objectPoolBaseDict.Clear();
            objectPoolBaseList.Clear();
        }


        public bool HasPool(TypeNamePair typeNamePair)
        {
            if (objectPoolBaseDict.ContainsKey(typeNamePair))
            {
                return true;
            }

            return false;
        }
    }
}