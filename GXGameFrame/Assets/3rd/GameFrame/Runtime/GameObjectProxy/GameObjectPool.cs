using System.Collections.Generic;
using Common.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame
{
    public  class GameObjectPool : Singleton<GameObjectPool>, IGameObjectPool, IReference
    {
        
        private Dictionary<string, ObjectPool<PoolObject>> mObjectPools = new();
        private Dictionary<GameObject, PoolObject> mGoPoolDic = new Dictionary<GameObject, PoolObject>();

        /// <summary>
        /// PoolObject类型吐出的最大数量,作用于所有的PoolObject的对象池
        /// </summary>
        private int mFrameMaxInstanceCount = 20;

        /// <summary>
        /// 对象池大小
        /// </summary>
        private int defaultSize = 16;

        /// <summary>
        /// 对象池检查时间间隔
        /// </summary>
        private int expireTime = 60;

        /// <summary>
        /// 基础数据初始化
        /// </summary>
        /// <param name="defaultSize"></param>
        /// <param name="expireTime"></param>
        /// <param name="frameMaxInstanceCount"></param>
        public void InitData(int defaultSize, int expireTime, int frameMaxInstanceCount)
        {
            mFrameMaxInstanceCount = frameMaxInstanceCount;
            this.defaultSize = defaultSize;
            this.expireTime = expireTime;
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
            if (!mObjectPools.TryGetValue(assetName,out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<PoolObject>(assetName, defaultSize, expireTime);
                objectPool.SetAsyncMaxCount(mFrameMaxInstanceCount);
                mObjectPools.Add(assetName,objectPool);
            }

            var prefab = await AssetManager.Instance.LoadAsync<GameObject>(assetName,null, token);//AssetManager.Instance.LoadAsyncTask<GameObject>(assetName, token);
            if (prefab == null)
            {
                return null;
            }
            GameObject go = await GoAsync(objectPool, prefab, parent, token);
            //如果这个阶段发现被取消了,那就unload资源.
            if (go == null)
            {
                AssetManager.Instance.DecrementReferenceCount(assetName);
                return null;
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
            string prefabCode = prefab.GetHashCode().ToString();
            if (!mObjectPools.TryGetValue(prefabCode,out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<PoolObject>(prefabCode, defaultSize, expireTime);
                objectPool.SetAsyncMaxCount(mFrameMaxInstanceCount);
                mObjectPools.Add(prefabCode,objectPool);
            }

            return await GoAsync(objectPool, prefab, parent, token);
        }

        /// <summary>
        /// 分帧吐出gameobejct
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<GameObject> GoAsync(ObjectPool<PoolObject> pool, GameObject prefab, Transform parent = null,
            System.Threading.CancellationToken token = default)
        {
            pool.SetUserData(prefab);
            PoolObject poolObject = await pool.SpawnAsync(token);
            if (poolObject == null)
            {
                return null;
            }

            mGoPoolDic[poolObject.Obj] = poolObject;
            poolObject.SetParent(parent);
            return poolObject.Obj;
        }

        /// <summary>
        /// 同步得到GameObjhect
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public GameObject Get(GameObject prefab, Transform parent = null)
        {
            string prefabCode = prefab.GetHashCode().ToString();
            if (!mObjectPools.TryGetValue(prefabCode,out var objectPool))
            {
                objectPool = ObjectPoolManager.Instance.CreateObjectPool<PoolObject>(prefabCode, defaultSize, expireTime);
                objectPool.SetAsyncMaxCount(mFrameMaxInstanceCount);
                mObjectPools.Add(prefabCode,objectPool);
            }

            PoolObject poolObject = objectPool.Spawn();
            mGoPoolDic[poolObject.Obj] = poolObject;
            return poolObject.Obj;
        }
        

        /// <summary>
        /// 清理Gameobejct
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="go"></param>
        public void Release(string assetName, GameObject go)
        {
            if (mGoPoolDic == null || !mGoPoolDic.TryGetValue(go, out PoolObject poolObject) || !mObjectPools.TryGetValue(assetName,out var objectPool))
            {
                Debugger.LogError($"{assetName}GameObjectPool is null");
                return;
            }
            mGoPoolDic.Remove(go);
            AssetManager.Instance.DecrementReferenceCount(assetName);
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
            if (mGoPoolDic == null || !mGoPoolDic.TryGetValue(go, out PoolObject poolObject) ||  !mObjectPools.TryGetValue(prefabCode,out var objectPool))
            {
                Debugger.LogError($"{prefab.name}GameObjectPool is null");
                return;
            }
            mGoPoolDic.Remove(go);
            objectPool.UnSpawn(poolObject);
        }


        /// <summary>
        /// 销毁
        /// </summary>
        public void Clear()
        {
            foreach (var objectpool in mObjectPools.Values)
            {
                ObjectPoolManager.Instance.DeleteObjectPool(objectpool);
            }
            mObjectPools.Clear();
            mGoPoolDic.Clear();
        }
    }
}