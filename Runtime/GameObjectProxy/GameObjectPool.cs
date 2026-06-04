using System;
using System.Collections.Generic;
using Common.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class GameObjectPool : Singleton<GameObjectPool>, IGameObjectPool, IDisposable
    {
        private Dictionary<object, ObjectPool<GameObjectPoolBase>> objectPools = new();
        // private Dictionary<GameObject, GameObjectPoolBase> gameObjectPoolDic = new Dictionary<GameObject, GameObjectPoolBase>();

        /// <summary>
        /// PoolObject类型吐出的最大数量,作用于所有的PoolObject的对象池
        /// </summary>
        private const int FrameMaxInstanceCount = 10;

        /// <summary>
        /// 对象池大小
        /// </summary>
        private const int DefaultSize = 64;

        /// <summary>
        /// 对象池检查时间间隔
        /// </summary>
        private const int ExpireTime = 30;

        public static GameObject ObjectCacheArea;

        public GameObjectPool()
        {
            ObjectCacheArea = new GameObject();
            ObjectCacheArea.name = "ObjectCacheArea";
            ObjectCacheArea.transform.localScale = Vector3.one;
            ObjectCacheArea.transform.position = Vector3.zero;
            GameObject.DontDestroyOnLoad(ObjectCacheArea);
        }

        /// <summary>
        /// 基础数据初始化
        /// </summary>
        /// <param name="defaultSize"></param>
        /// <param name="expireTime"></param>
        /// <param name="frameMaxInstanceCount"></param>
        public ObjectPool<GameObjectPoolBase> SetPoolData(string assetName, int defaultSize, int expireTime, int frameMaxInstanceCount)
        {
            if (!objectPools.TryGetValue(assetName, out var objectPool))
            {
                Debugger.LogError($"not have {assetName}");
                return null;
            }

            objectPool.SetAsyncMaxCount(frameMaxInstanceCount);
            objectPool.SetExamineTime(expireTime);
            objectPool.SetMaxCount(defaultSize);
            return objectPool;
        }

        /// <summary>
        /// 异步获取GameObject with assetName
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<GameObjectPoolBase> GetAsync(string assetName, Transform parent = null, System.Threading.CancellationToken token = default)
        {
            if (!objectPools.TryGetValue(assetName, out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectPoolBase>(assetName, DefaultSize, ExpireTime);
                objectPool.SetAsyncMaxCount(FrameMaxInstanceCount);
                objectPools.Add(assetName, objectPool);
            }

            DefaultAssetReference defaultAssetReference = new DefaultAssetReference();
            var prefab = await AssetManager.Instance.LoadAsync<GameObject>(assetName, defaultAssetReference,
                token);
            if (prefab == null)
            {
                defaultAssetReference.UnrefAsset(assetName);
                return null;
            }

            var go = await InstantiateGameObjectAsync(objectPool, assetName, prefab, defaultAssetReference, parent, token);
            if (go == null)
            {
                defaultAssetReference.UnrefAsset(assetName);
            }

            return go;
        }


        /// <summary>
        /// 分帧吐出gameobejct
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="prefab"></param>
        /// <param name="assetReference"></param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask<GameObjectPoolBase> InstantiateGameObjectAsync(ObjectPool<GameObjectPoolBase> pool, string assetName, GameObject prefab,
            DefaultAssetReference assetReference,
            Transform parent = null,
            System.Threading.CancellationToken token = default)
        {
            pool.SetUserData(prefab);
            GameObjectPoolBase gameObjectPoolBase = await pool.SpawnAsync(null, token);
            if (gameObjectPoolBase == null)
            {
                return null;
            }

            gameObjectPoolBase.SetParent(parent);
            gameObjectPoolBase.SetAssetPath(assetName);
            gameObjectPoolBase.SetAssetReference(assetReference);
            return gameObjectPoolBase;
        }

        /// <summary>
        /// 同步得到GameObjhect
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObjectPoolBase InstantiateGameObject(GameObject prefab, Transform parent = null)
        {
            if (!objectPools.TryGetValue(prefab, out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectPoolBase>(prefab.GetHashCode().ToString(), DefaultSize, ExpireTime);
                objectPool.SetAsyncMaxCount(FrameMaxInstanceCount);
                objectPools.Add(prefab, objectPool);
            }

            objectPool.SetUserData(prefab);
            GameObjectPoolBase gameObjectPoolBase = objectPool.Spawn();
            gameObjectPoolBase.SetParent(parent);
            gameObjectPoolBase.SetAssetPath(prefab.name);
            return gameObjectPoolBase;
        }


        /// <summary>
        /// 清理Gameobejct
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        public void Release(object assetName, GameObjectPoolBase go)
        {
            if (objectPools.TryGetValue(assetName, out var pool))
            {
                pool.UnSpawn(go);
            }
        }


        /// <summary>
        /// 销毁
        /// </summary>
        public void Dispose()
        {
            foreach (var objectpool in objectPools.Values)
            {
                ObjectPoolManager.Instance.DeleteObjectPool(objectpool);
            }

            objectPools.Clear();
        }
    }
}