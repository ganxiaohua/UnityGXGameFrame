using System;
using System.Collections.Generic;
using Common.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public class GameObjectPool : Singleton<GameObjectPool>, IGameObjectPool, IDisposable
    {
        private Dictionary<object, ObjectPool<GameObjectPoolBaes>> objectPools = new();
        private Dictionary<GameObject, GameObjectPoolBaes> gameObjectPoolDic = new Dictionary<GameObject, GameObjectPoolBaes>();

        /// <summary>
        /// PoolObject类型吐出的最大数量,作用于所有的PoolObject的对象池
        /// </summary>
        private const int FrameMaxInstanceCount = 8;

        /// <summary>
        /// 对象池大小
        /// </summary>
        private const int DefaultSize = 64;

        /// <summary>
        /// 对象池检查时间间隔
        /// </summary>
        private const int ExpireTime = 30;

        /// <summary>
        /// 基础数据初始化
        /// </summary>
        /// <param name="defaultSize"></param>
        /// <param name="expireTime"></param>
        /// <param name="frameMaxInstanceCount"></param>
        public void SetPool(string assetName, int defaultSize, int expireTime, int frameMaxInstanceCount)
        {
            if (!objectPools.TryGetValue(assetName, out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectPoolBaes>(assetName, defaultSize, expireTime);
                objectPools.Add(assetName, objectPool);
            }

            objectPool.SetAsyncMaxCount(frameMaxInstanceCount);
            objectPool.SetExamineTime(expireTime);
            objectPool.SetMaxCount(defaultSize);
        }

        /// <summary>
        /// 异步获取GameObject with assetName
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<GameObject> GetAsync(string assetName, Transform parent = null, System.Threading.CancellationToken token = default)
        {
            if (!objectPools.TryGetValue(assetName, out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectPoolBaes>(assetName, DefaultSize, ExpireTime);
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

            GameObject go = await InstantiateGameObjectAsync(objectPool, assetName, prefab, defaultAssetReference, parent, token);
            //如果这个阶段发现被取消了,那就unload资源.
            if (go == null)
            {
                defaultAssetReference.UnrefAsset(assetName);
            }
            return go;
        }

        /// <summary>
        /// 异步获取GameObject with GameObject
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<GameObject> GetAsync(GameObject prefab, Transform parent = null, System.Threading.CancellationToken token = default)
        {
            DefaultAssetReference defaultAssetReference = null;
            ObjectPool<GameObjectPoolBaes> objectPool = null;
            string assetPath = null;
            SetPrefabMsg(prefab, ref defaultAssetReference, ref objectPool, ref assetPath);
            GameObject go = await InstantiateGameObjectAsync(objectPool, assetPath, prefab, defaultAssetReference, parent, token);
            if (go == null)
            {
                defaultAssetReference?.UnrefAsset(assetPath);
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
        private async UniTask<GameObject> InstantiateGameObjectAsync(ObjectPool<GameObjectPoolBaes> pool, string assetName, GameObject prefab,
            DefaultAssetReference assetReference,
            Transform parent = null,
            System.Threading.CancellationToken token = default)
        {
            pool.SetUserData(prefab);
            GameObjectPoolBaes gameObjectPoolBaes = await pool.SpawnAsync(token);
            if (gameObjectPoolBaes == null)
            {
                return null;
            }
            gameObjectPoolDic[gameObjectPoolBaes.Obj] = gameObjectPoolBaes;
            gameObjectPoolBaes.SetParent(parent);
            gameObjectPoolBaes.SetAssetPath(assetName);
            gameObjectPoolBaes.SetAssetReference(assetReference);
            return gameObjectPoolBaes.Obj;
        }

        /// <summary>
        /// 同步得到GameObjhect
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject Get(GameObject prefab, Transform parent = null)
        {
            DefaultAssetReference defaultAssetReference = null;
            ObjectPool<GameObjectPoolBaes> objectPool = null;
            string assetPath = null;
            SetPrefabMsg(prefab, ref defaultAssetReference, ref objectPool, ref assetPath);
            GameObjectPoolBaes gameObjectPoolBaes = objectPool.Spawn();
            gameObjectPoolBaes.SetParent(parent);
            gameObjectPoolBaes.SetAssetPath(assetPath);
            gameObjectPoolBaes.SetAssetReference(defaultAssetReference);
            gameObjectPoolDic[gameObjectPoolBaes.Obj] = gameObjectPoolBaes;
            return gameObjectPoolBaes.Obj;
        }

        
        private void SetPrefabMsg(GameObject prefab,ref DefaultAssetReference defaultAssetReference,ref ObjectPool<GameObjectPoolBaes> objectPool, ref string assetPath)
        {
            if (gameObjectPoolDic.TryGetValue(prefab, out GameObjectPoolBaes poolObject))
            {
                defaultAssetReference = new DefaultAssetReference();
                if (!string.IsNullOrEmpty(poolObject.assetName))
                {
                    objectPool = objectPools[poolObject.assetName];
                    assetPath = poolObject.assetName;
                    defaultAssetReference.RefAsset(assetPath);
                }
            }

            if (objectPool == null)
            {
                string prefabCode = prefab.GetHashCode().ToString();
                if (!objectPools.TryGetValue(prefabCode, out objectPool))
                {
                    objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectPoolBaes>(prefabCode, DefaultSize, ExpireTime);
                    objectPool.SetAsyncMaxCount(FrameMaxInstanceCount);
                    objectPools.Add(prefabCode, objectPool);
                }
            }
        }

        /// <summary>
        /// 清理Gameobejct
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        public void Release(string assetName, GameObject go)
        {
            if (gameObjectPoolDic == null || !gameObjectPoolDic.TryGetValue(go, out GameObjectPoolBaes poolObject) ||
                !objectPools.TryGetValue(assetName, out var objectPool))
            {
                Debugger.LogError($"{assetName}GameObjectPool is null");
                return;
            }

            gameObjectPoolDic.Remove(go);
            objectPool.UnSpawn(poolObject);
        }

        /// <summary>
        /// 清理Gameobejct
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="go"></param>
        public void Release(GameObject prefab, GameObject go)
        {
            string prefabCode = prefab.GetHashCode().ToString();
            if (gameObjectPoolDic == null || !gameObjectPoolDic.TryGetValue(go, out GameObjectPoolBaes poolObject) ||
                !objectPools.TryGetValue(prefabCode, out var objectPool))
            {
                Debugger.LogError($"{prefab.name}GameObjectPool is null");
                return;
            }

            gameObjectPoolDic.Remove(go);
            objectPool.UnSpawn(poolObject);
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
            gameObjectPoolDic.Clear();
        }
    }
}