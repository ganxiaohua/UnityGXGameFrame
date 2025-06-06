using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFrame
{
    public sealed partial class AssetManager : Singleton<AssetManager>  , IDisposable
    {
        private const float InfinityLifeTime = float.PositiveInfinity;

        private const float DefaultDelayReleaseTime = 30;
        
        private float unloadUnusedAssetsTime;

        private float delayReleaseTime = DefaultDelayReleaseTime;

        private bool isAlive = true;

        private class AssetCache
        {
            public readonly string path;
            public IAssetHandle handle;
            public int referenceCount;
            public float lifeTime;

            public bool IsDying => !float.IsInfinity(lifeTime);

            public AssetCache(string _path)
            {
                path = _path;
                lifeTime = InfinityLifeTime;
            }
        }

        private readonly Dictionary<string, AssetCache> assetCaches = new Dictionary<string, AssetCache>();

        private readonly List<AssetCache> dyingAssetCaches = new List<AssetCache>();

        public async UniTask<T> LoadAsync<T>(string path, IAssetReference reference,
                CancellationToken cancellationToken = default)
        {
            Assert.IsNotNull(reference, "IAssetReference is null");
            Type type = typeof(T);
            var handle = await LoadAsync(path, type, typeof(DefaultAssetHandle), reference, cancellationToken);
            Assert.IsTrue(handle.Result == null || handle.Result is T, $"Type match fail from {handle.Result}, expect {typeof(T)}");
            return (T) handle.Result;
        }

        /// <summary>
        /// assetHandleType: <see cref="IAssetHandle"/>
        /// </summary>
        public async UniTask<IAssetHandle> LoadAsync(string path, Type assetType, Type assetHandleType, IAssetReference reference,
                CancellationToken cancellationToken = default)
        {
            if (!assetCaches.TryGetValue(path, out var cache))
            {
                cache = new AssetCache(path);
                assetCaches.Add(path, cache);
            }

            reference.RefAsset(path);

            var handle = cache.handle;

            if (handle == null || !handle.IsValid)
            {
                handle = (IAssetHandle) Activator.CreateInstance(assetHandleType);
                handle.Initialize(path, assetType);
                cache.handle = handle;
            }

            if (handle.IsDone)
                return handle;

            try
            {
                await handle.GetTask(cancellationToken);
                if (!handle.IsDone)
                    handle.Finish();
            }
            catch (OperationCanceledException)
            {
                handle.Release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return handle;
        }

        public async UniTask<IAssetHandle> LoadSceneAsync(string path, LoadSceneMode loadMode = LoadSceneMode.Single,
                bool activateOnLoad = true, int priority = 100, CancellationToken cancellationToken = default)
        {
            var handle = new SceneHandle(loadMode, activateOnLoad, priority);
            handle.Initialize(path, null);
            try
            {
                await handle.GetTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                handle.Release();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return handle;
        }

        public async UniTask LoadAssetCallBack<TObject>(string path, DefaultAssetReference reference, Action<TObject> callback)
                where TObject : UnityEngine.Object
        {
            var objectgo = await LoadAsync<TObject>(path, reference);
            if (objectgo == null)
            {
                return;
            }

            callback?.Invoke(objectgo);
        }

        public async UniTask<byte[]> LoadRawAsync(string path, CancellationToken cancellationToken = default)
        {
            var handle = new TextAssetHandle();
            handle.Initialize(path, null);
            try
            {
                await handle.GetTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                handle.Release();
                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }

            var result = (handle.Result as TextAsset)?.bytes;
            handle.Release();
            return result;
        }

        public async UniTask<string> LoadTextAsync(string path, CancellationToken cancellationToken = default)
        {
            var handle = new TextAssetHandle();
            handle.Initialize(path, null);
            try
            {
                await handle.GetTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                handle.Release();
                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }

            var result = (handle.Result as TextAsset)?.text;
            handle.Release();
            return result;
        }

        public async UniTask<IList<byte[]>> LoadRawsAsync(object paths, CancellationToken cancellationToken = default)
        {
            var handle = new TextAssetsHandle();
            handle.Initialize(paths, null);
            try
            {
                await handle.GetTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                handle.Release();
                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }

            var tas = handle.Result as IList;
            if (tas == null)
            {
                handle.Release();
                return default;
            }

            var result = new List<byte[]>(tas.Count);
            foreach (var ta in tas)
            {
                result.Add(((TextAsset) ta).bytes);
            }

            handle.Release();
            return result;
        }

        public async UniTask<IList<string>> LoadTextsAsync(object paths, CancellationToken cancellationToken = default)
        {
            var handle = new TextAssetsHandle();
            handle.Initialize(paths, null);
            try
            {
                await handle.GetTask(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                handle.Release();
                return default;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }

            var tas = handle.Result as IList;
            if (tas == null)
            {
                handle.Release();
                return default;
            }

            var result = new List<string>(tas.Count);
            foreach (var ta in tas)
            {
                result.Add(((TextAsset) ta).text);
            }

            handle.Release();
            return result;
        }

        public void IncrementReferenceCount(string path)
        {
            if (!isAlive) return;
            Assert.IsTrue(assetCaches.ContainsKey(path), $"Bad Asset{path} Reference Count Increment");
            if (assetCaches.TryGetValue(path, out var cache))
            {
                if (cache.IsDying)
                {
                    Assert.AreEqual(0, cache.referenceCount, $"Bad Asset{path} Reference Count");
                    dyingAssetCaches.RemoveSwapBack(cache);
                    cache.lifeTime = InfinityLifeTime;
                }

                cache.referenceCount++;
            }
        }

        public void DecrementReferenceCount(string path, bool releaseImmediately = false)
        {
            if (!isAlive) return;
            Assert.IsTrue(assetCaches.ContainsKey(path), $"Bad Asset{path} Reference Count Decrement");
            if (assetCaches.TryGetValue(path, out var cache))
            {
                Assert.AreNotEqual(0, cache.referenceCount, $"Bad Asset{path} Reference Count");
                if (--cache.referenceCount == 0)
                {
                    if (releaseImmediately)
                    {
                        assetCaches.Remove(cache.path);
                        cache.handle.Release();
                    }
                    else
                    {
                        cache.lifeTime = delayReleaseTime;
                        dyingAssetCaches.Add(cache);
                    }
                }
            }
        }

        public void Update(float deltaTime)
        {

            for (int i = dyingAssetCaches.Count - 1; i >= 0; i--)
            {
                var cache = dyingAssetCaches[i];
                cache.lifeTime -= deltaTime;
                if (cache.lifeTime <= 0f)
                {
                    dyingAssetCaches.RemoveAtSwapBack(i);
                    assetCaches.Remove(cache.path);
                    cache.handle.Release();
                }
            }
        }

        public void Dispose()
        {
            foreach (var kv in assetCaches)
            {
                kv.Value.handle.Release();
            }

            dyingAssetCaches.Clear();
            assetCaches.Clear();
            isAlive = false;
        }
        
    }
}