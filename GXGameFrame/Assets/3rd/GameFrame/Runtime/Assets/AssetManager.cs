using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace GameFrame
{
    public sealed class AssetManager : Singleton<AssetManager>
    {
        private const float InfinityLifeTime = float.PositiveInfinity;
        private const float ReleaseDelayTime = 60;

        private class AssetHandleCache
        {
            public readonly string path;
            public AsyncOperationHandle handle;
            public Action<object> unloadHandle;
            public object unloadHandleParameter;
            public int referenceCount;
            public float lifeTime;
            public bool isScene;

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
            IAssetReference tempRef = reference;
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                assetHandleCaches.Add(path, cache);
            }

            cache.lifeTime = InfinityLifeTime;

            tempRef?.RefAsset(path);
            if (tempRef == null)
            {
                IncrementReferenceCount(path);
            }

            if (!cache.handle.IsValid())
                cache.handle = Addressables.LoadAssetAsync<T>(path);

            var tHandle = cache.handle.Convert<T>();
            if (tHandle.IsDone)
                return tHandle.Result;

            var result = await tHandle.ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();
            if (result.IsCanceled)
            {
                cache.handle = default;
                return default(T);
            }

            tempRef?.LoadLater();
            return result.Result;
        }

        /// <summary>
        /// 当资源销毁的时候执行的操作
        /// </summary>
        /// <param name="path"></param>
        /// <param name="action"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool SetUnloadHandle(string path, Action<object> action, object param)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache.unloadHandle = action;
                cache.unloadHandleParameter = param;
                return true;
            }

            return false;
        }


        public async UniTask<Scene> LoadSceneAsync(string path, IAssetReference reference = null, System.Threading.CancellationToken token = default)
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                cache.isScene = true;
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
            cache.handle = handle;
            var result = await handle.ToUniTask(cancellationToken: token).SuppressCancellationThrow();
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

        public void DecrementReferenceCount(string path, bool immediately = false)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                Assert.AreNotEqual(0, cache.referenceCount, "Bad Asset Reference Count " + path);
                if (--cache.referenceCount == 0)
                {
                    cache.lifeTime = immediately ? 0 : ReleaseDelayTime;
                    dyingAssetHandles.Add(cache);
                }
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = dyingAssetHandles.Count - 1; i >= 0; i--)
            {
                var cache = dyingAssetHandles[i];
                if (float.IsPositiveInfinity(cache.lifeTime))
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
                        cache.unloadHandle?.Invoke(cache.unloadHandleParameter);
                        if (cache.handle.IsValid())
                            if (!cache.isScene)
                                Addressables.Release(cache.handle);
                            else
                                Addressables.UnloadSceneAsync(cache.handle);
                    }
                }
            }
        }
    }
}