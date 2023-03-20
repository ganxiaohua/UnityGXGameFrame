using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<TypeNamePair, IObjectPoolBase> s_ObjectPoolBase = new Dictionary<TypeNamePair, IObjectPoolBase>();

        
        public void Update(float elapseSeconds,float realElapseSeconds)
        {
            foreach (IObjectPoolBase objectpool in s_ObjectPoolBase.Values)
            {
                objectpool.Update(elapseSeconds,realElapseSeconds);
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
        public ObjectPool<T> CreateObjectPool<T>(string name, int maxNum,object initObject) where T : ObjectBase, new()
        {
            Type type = typeof(T);
            TypeNamePair typeNamePair = new TypeNamePair(type, name);
            if (HasPool(typeNamePair))
            {
                throw new Exception($"has {name} {type}");
            }

            ObjectPool<T> objectPoolBase = ObjectPool<T>.Create(typeNamePair, type, maxNum,initObject);
            s_ObjectPoolBase.Add(typeNamePair, objectPoolBase);
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
            if (s_ObjectPoolBase.TryGetValue(typeNamePair, out IObjectPoolBase objectbase))
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
        public void DeleteObjectPool<T>(ObjectPool<T> objectpool) where T : ObjectBase,new()
        {
            if (objectpool == null)
            {
                throw new Exception("objectpool wei null");
            }

            ReferencePool.Release(objectpool);
            s_ObjectPoolBase.Remove(objectpool.TypeName);
        }
        
        /// <summary>
        /// 删除一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="maxNum"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public void DeleteObjectPool<T>(string name) where T : ObjectBase,new()
        {
            var objectPool = GetObjectPool<T>(name);
            if (objectPool == null)
            {
                throw new Exception($"objectpool {name} null");
            }
            DeleteObjectPool(objectPool);
        }


        public bool HasPool(TypeNamePair typeNamePair)
        {
            if (s_ObjectPoolBase.ContainsKey(typeNamePair))
            {
                return true;
            }
            return false;
        }
        
    }
}