using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GameFrame
{
    public sealed class AssetManager : Singleton<AssetManager>
    {
        private const float InfinityLifeTime = float.PositiveInfinity;
        private const float ReleaseDelayTime = 60;

        private class AssetHandleCache
        {
            public readonly string Path;
            public AssetHandle Handle;
            public Action<object> UnloadHandle;
            public object UnloadHandleParameter;
            public int ReferenceCount;
            public float LifeTime;

            public AssetHandleCache(string _path)
            {
                Path = _path;
            }
        }

        private readonly Dictionary<string, AssetHandleCache> assetHandleCaches =
            new Dictionary<string, AssetHandleCache>();

        private readonly List<AssetHandleCache> dyingAssetHandles = new List<AssetHandleCache>();

        public async UniTask<T> LoadAsync<T>(string path, IAssetReference reference = null, System.Threading.CancellationToken token = default)
            where T : UnityEngine.Object
        {
            if (!assetHandleCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetHandleCache(path);
                assetHandleCaches.Add(path, cache);
            }
            cache.LifeTime = InfinityLifeTime;
            reference.RefAsset(path);
            if (cache.Handle==null || !cache.Handle.IsValid)
                cache.Handle = YooAssets.LoadAssetAsync(path);
            if (cache.Handle.IsDone)
                return (T) cache.Handle.AssetObject;
            try
            {
                await cache.Handle.ToUniTask(cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                cache.Handle = default;
                return default(T);
            }
            reference.LoadLater();
            return (T) cache.Handle.AssetObject;
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


        public async UniTask<SceneHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode,
            System.Threading.CancellationToken token = default)
        {
            var handle = YooAssets.LoadSceneAsync(path, sceneMode);
            if (!handle.IsValid || handle.IsDone)
                return null;
            await handle.ToUniTask(cancellationToken:token);
            if (handle.Status == EOperationStatus.Succeed)
            {
                return handle;
            }
            return null;
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
                        if (cache.Handle.IsValid)
                            cache.Handle.Release();
                    }
                }
            }
        }
    }
}