using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace GameFrame
{
    public sealed class AssetSystem : Singleton<AssetSystem>
    {
        private const float InfinityLifeTime = float.PositiveInfinity;
        private const float ReleaseDelayTime = 60;
        
        private class AssetHandleCache
        {
            public readonly string path;
            public AsyncOperationHandle handle;
            public int referenceCount;
            public float lifeTime;

            public AssetHandleCache(string _path)
            {
                path = _path;
            }
        }

        private readonly Dictionary<string, AssetHandleCache> assetHandleCaches =
            new Dictionary<string, AssetHandleCache>();

        private readonly List<AssetHandleCache> dyingAssetHandles = new List<AssetHandleCache>();

        public async UniTask<T> LoadAsync<T>(string path, IAssetReference reference = null, System.Threading.CancellationToken cancellationToken = default)
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                assetHandleCaches.Add(path, cache);
            }
            
            cache.lifeTime = InfinityLifeTime;

            reference?.RefAsset(path);
            if (reference == null)
            {
                IncrementReferenceCount(path);
            }

            if (!cache.handle.IsValid())
                cache.handle = Addressables.LoadAssetAsync<T>(path);
            
            var tHandle = cache.handle.Convert<T>();
            if (tHandle.IsDone)
                return tHandle.Result;

            var result = await tHandle.ToUniTask(cancellationToken:cancellationToken).SuppressCancellationThrow();
            if (result.IsCanceled)
            {
                cache.handle = default;
                return default(T);
            }
            
            return result.Result;
        }

        public void LoadAsync<T>(string path, IAssetReference reference = null, Action<AsyncOperationHandle<T>> callback = null)
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                assetHandleCaches.Add(path, cache);
            }
            
            cache.lifeTime = InfinityLifeTime;
            
            reference?.RefAsset(path);
            if (reference == null)
            {
                IncrementReferenceCount(path);
            }
            if (!cache.handle.IsValid())
                cache.handle = Addressables.LoadAssetAsync<T>(path);
            
            var tHandle = cache.handle.Convert<T>();
            if (tHandle.IsDone)
            {
                callback?.Invoke(tHandle);
                return;
            }

            tHandle.Completed += callback;
        } 
        
        public async UniTask<Scene> LoadSceneAsync(string path,IAssetReference reference = null, System.Threading.CancellationToken token = default)
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                assetHandleCaches.Add(path, cache);
            }
            
            cache.lifeTime = InfinityLifeTime;

            reference?.RefAsset(path);
            if (reference == null)
            {
                IncrementReferenceCount(path);
            }

            // if (!cache.handle.IsValid())
            var handle = Addressables.LoadSceneAsync(path, LoadSceneMode.Additive);
            var result = await handle.ToUniTask(cancellationToken:token).SuppressCancellationThrow();
            if (result.IsCanceled)
            {
                cache.handle = default;
                return default(Scene);
            }
            
            return result.Result.Scene;
        }
        
        public void IncrementReferenceCount(string path)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache.referenceCount++;
            }
        }

        public void DecrementReferenceCount(string path)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                Assert.AreNotEqual(0, cache.referenceCount, "Bad Asset Reference Count");
                if (--cache.referenceCount == 0)
                {
                    cache.lifeTime = ReleaseDelayTime;
                    dyingAssetHandles.Add(cache);
                }
            }
        }
        
        public void Update(float deltaTime)
        {
            for (int i = dyingAssetHandles.Count - 1; i >= 0; i--)
            {
                var cache = dyingAssetHandles[i];
                if (cache.lifeTime == InfinityLifeTime)
                {
                    dyingAssetHandles.RemoveAt(i);
                }
                else
                {
                    cache.lifeTime -= deltaTime;
                    if (cache.lifeTime <= 0f)
                    {
                        dyingAssetHandles.RemoveAt(i);
                        assetHandleCaches.Remove(cache.path);
                        if (cache.handle.IsValid())
                            Addressables.Release(cache.handle);
                    }
                }
            }
        }
    }
}