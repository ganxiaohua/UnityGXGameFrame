using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame
{
    public struct TextAssetHandle : IAssetHandle
    {
        public bool IsValid => internalHandle?.IsValid ?? false;

        public bool IsDone => internalHandle?.IsDone ?? true;

        public object Result
        {
            get
            {
                if (internalHandle != null && internalHandle.Status == EOperationStatus.Succeed)
                    return internalHandle.AssetObject;
                return null;
            }
        }

        public IAssetReference AssetReference => null;

        private AssetHandle internalHandle;

        public void Initialize(object key,Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("TextAssetHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info,type);
                    internalHandle = package.LoadAssetAsync(info);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public UniTask GetTask(CancellationToken cancellationToken)
        {
            return internalHandle.ToUniTask(cancellationToken: cancellationToken);
        }

        public void Finish()
        {
            // never called
        }

        public void Release()
        {
            if (internalHandle != null && internalHandle.IsValid)
            {
                internalHandle.Dispose();
                var assetInfo = internalHandle.GetAssetInfo();
                var package = PackageSearcher.SearchByAssetLocation(assetInfo.AssetPath);
                package.TryUnloadUnusedAsset(assetInfo.AssetPath);
                internalHandle = default;
            }
        }
    }
}