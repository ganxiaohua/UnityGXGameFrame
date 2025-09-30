using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace GameFrame.Runtime
{
    public class AsyncLoadAsset<T> where T : Object
    {
        private ArrayEx<T> assets;
        private IAssetReference assetReference;
        private Action<ArrayEx<T>> onAfterLoad;
        private int remainCount;
        private int version;

        public AsyncLoadAsset(Action<ArrayEx<T>> func)
        {
            assets = new ArrayEx<T>(16);
            assetReference = new DefaultAssetReference();
            onAfterLoad = func;
            version = 0;
        }

        public void LoadAssets(List<string> assetPaths)
        {
            UnLoad(false);
            remainCount = assetPaths.Count;
            foreach (var assetPath in assetPaths)
            {
                Load(assetPath).Forget();
            }
        }

        public void LoadAsset(string assetPath)
        {
            UnLoad(false);
            remainCount = 1;
            Load(assetPath).Forget();
        }

        private async UniTask Load(string assetPath)
        {
            var preVersion = version;
            var index = assets.Count;
            var asset = await AssetManager.Instance.LoadAsync<T>(assetPath, assetReference);
            if (preVersion != version)
                return;
            assets[index] = asset;
            if (--remainCount == 0)
                onAfterLoad?.Invoke(assets);
        }

        private void UnLoad(bool immediately)
        {
            version++;
            assetReference.UnrefAssets(immediately);
            assets.Clear();
        }

        public void Clear()
        {
            UnLoad(false);
        }

        public void ImmediatelyClear()
        {
            UnLoad(true);
        }
    }
}