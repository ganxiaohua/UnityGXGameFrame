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
            public readonly string Path;
            public AsyncOperationHandle Handle;
            public Action<object> UnloadHandle;
            public object UnloadHandleParameter;
            public int ReferenceCount;
            public float LifeTime;
            public bool IsScene;

            public AssetHandleCache(string _path)
            {
                Path = _path;
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

            cache.LifeTime = InfinityLifeTime;

            tempRef?.RefAsset(path);
            if (tempRef == null)
            {
                IncrementReferenceCount(path);
            }

            if (!cache.Handle.IsValid())
                cache.Handle = Addressables.LoadAssetAsync<T>(path);

            var tHandle = cache.Handle.Convert<T>();
            if (tHandle.IsDone)
                return tHandle.Result;


            try
            {
                await tHandle.ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                cache.Handle = default;
                return default(T);
            }

            tempRef?.LoadLater();
            return tHandle.Result;
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
                cache.UnloadHandle = action;
                cache.UnloadHandleParameter = param;
                return true;
            }

            return false;
        }


        public async UniTask<Scene> LoadSceneAsync(string path, IAssetReference reference = null, System.Threading.CancellationToken token = default)
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                cache.IsScene = true;
                assetHandleCaches.Add(path, cache);
            }

            cache.LifeTime = InfinityLifeTime;

            reference?.RefAsset(path);
            if (reference == null)
            {
                IncrementReferenceCount(path);
            }

            // if (!cache.handle.IsValid())
            var handle = Addressables.LoadSceneAsync(path, LoadSceneMode.Additive);
            cache.Handle = handle;
            var result = await handle.ToUniTask(cancellationToken: token).SuppressCancellationThrow();
            if (result.IsCanceled)
            {
                cache.Handle = default;
                return default(Scene);
            }

            return result.Result.Scene;
        }

        public void IncrementReferenceCount(string path)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache.ReferenceCount++;
            }
        }

        public void DecrementReferenceCount(string path, bool immediately = false)
        {
            if (assetHandleCaches.TryGetValue(path, out var cache))
            {
                Assert.AreNotEqual(0, cache.ReferenceCount, "Bad Asset Reference Count " + path);
                if (--cache.ReferenceCount == 0)
                {
                    cache.LifeTime = immediately ? 0 : ReleaseDelayTime;
                    dyingAssetHandles.Add(cache);
                }
            }
        }

        public void Update(float deltaTime)
        {
            for (int i = dyingAssetHandles.Count - 1; i >= 0; i--)
            {
                var cache = dyingAssetHandles[i];
                if (float.IsPositiveInfinity(cache.LifeTime))
                {
                    dyingAssetHandles.RemoveAt(i);
                }
                else
                {
                    cache.LifeTime -= deltaTime;
                    if (cache.LifeTime <= 0f)
                    {
                        dyingAssetHandles.RemoveAt(i);
                        assetHandleCaches.Remove(cache.Path);
                        cache.UnloadHandle?.Invoke(cache.UnloadHandleParameter);
                        if (cache.Handle.IsValid())
                            if (!cache.IsScene)
                                Addressables.Release(cache.Handle);
                            else
                                Addressables.UnloadSceneAsync(cache.Handle);
                    }
                }
            }
        }
    }
}