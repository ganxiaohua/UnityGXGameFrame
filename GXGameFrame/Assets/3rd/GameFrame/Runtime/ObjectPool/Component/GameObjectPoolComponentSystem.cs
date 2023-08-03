using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;

namespace GameFrame
{
    public static class GameObjectPoolComponentSystem
    {
        [SystemBind]
        public class GameObjectPoolComponentStartSystem : StartSystem<GameObjectPoolComponent>
        {
            protected override void Start(GameObjectPoolComponent self)
            {
                self.DefaultSize = 16;
                self.ExpireTime = 120;
                self.AllGameObjectPools = new();
            }
        }

        [SystemBind]
        public class GameObjectPoolComponentClearSystem : ClearSystem<GameObjectPoolComponent>
        {
            protected override void Clear(GameObjectPoolComponent self)
            {
                ObjectPoolManager.Instance.DeleteAll();
            }
        }

        public static ObjectPool<GameObjectObjectBase> CreatePool(this GameObjectPoolComponent gpc, string path, int defaultSize, int expireTime)
        {
            if (gpc.AllGameObjectPools.ContainsKey(path))
            {
                Debugger.LogWarning($"{path} already in pool");
                return null;
            }

            ObjectPool<GameObjectObjectBase> objectpool =
                ObjectPoolManager.Instance.CreateObjectPool<GameObjectObjectBase>(path, defaultSize, expireTime, path);
            gpc.AllGameObjectPools.Add(path, objectpool);
            return objectpool;
        }

        public static void RemovePool(this GameObjectPoolComponent self, string path)
        {
            if (!self.AllGameObjectPools.TryGetValue(path, out var pool))
            {
                Debugger.LogWarning($"{path} not already in pool");
            }
            ObjectPoolManager.Instance.DeleteObjectPool<GameObjectObjectBase>(path);
        }


        /// <summary>
        /// 获取一个对象
        /// </summary>
        /// <param name="self"></param>
        /// <param name="path">路径</param>
        /// <param name="size">对象池大小</param>
        /// <param name="expiretime">对象在对象池中存在的时间</param>
        /// <returns></returns>
        public static GameObjectObjectBase Get(this GameObjectPoolComponent self, string path,int size = 0,int expiretime = 0)
        {
            if (!self.AllGameObjectPools.TryGetValue(path, out var pool))
            {
                if (size == 0)
                {
                    size = self.DefaultSize;
                }
                if (expiretime == 0)
                {
                    expiretime = self.ExpireTime;
                }
                pool = CreatePool(self, path, size, expiretime);
            }
            GameObjectObjectBase gb = pool.Spawn();
            return gb;
        }

        public static void Recycle(this GameObjectPoolComponent self, GameObjectObjectBase obbase)
        {
            if (!self.AllGameObjectPools.TryGetValue(obbase.LoadPath, out var pool))
            {
                Debugger.LogWarning($"{obbase.LoadPath} not already in pool");
            }
            pool.UnSpawn(obbase);
        }
    }
}