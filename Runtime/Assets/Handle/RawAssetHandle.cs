using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public struct RawAssetHandle : IAssetHandle
    {
        public bool IsValid => internalHandle?.IsValid ?? false;

        public bool IsDone => internalHandle?.IsDone ?? true;

        public object Result
        {
            get
            {
                if (IsValid && internalHandle.Status == EOperationStatus.Succeeded)
                    return ((RawFileObject) internalHandle.AssetObject).GetBytes();
                return null;
            }
        }

        public IAssetReference AssetReference => null;

        private AssetHandle internalHandle;

        public void Initialize(object key, Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("RawAssetHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info, typeof(RawFileObject));
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
            return internalHandle != null
                    ? internalHandle.ToUniTask(cancellationToken: cancellationToken)
                    : UniTask.CompletedTask;
        }

        public void Finish()
        {
       
        }

        public string GetText()
        {
            if (IsValid && internalHandle.Status == EOperationStatus.Succeeded)
                return ((RawFileObject) internalHandle.AssetObject).GetText() ?? string.Empty;
            return null;
        }

        public void Release()
        {
            if (internalHandle != null && internalHandle.IsValid)
            {
                var assetInfo = internalHandle.GetAssetInfo();
                internalHandle.Dispose();
                if (YooAssets.TryGetPackage(assetInfo.PackageName, out var package))
                    package.TryUnloadUnusedAsset(assetInfo);
            }

            internalHandle = null;
        }
    }
}
