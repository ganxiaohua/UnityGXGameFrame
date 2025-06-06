using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace GameFrame
{
    public struct YooAssetsHandle
    {
        public bool IsValid => internalHandles != null && Array.Find(internalHandles, h => !h.IsValid) == null;

        public bool IsDone => internalHandles != null && Array.Find(internalHandles, h => !h.IsDone) == null;

        public object Result
        {
            get
            {
                var arr = new object[internalHandles.Length];
                for (var i = internalHandles.Length - 1; i >= 0; i--)
                {
                    var handle = internalHandles[i];
                    if (handle.Status != EOperationStatus.Succeed)
                        return null;
                    arr[i] = handle.AssetObject;
                }

                return arr;
            }
        }

        private AssetHandle[] internalHandles;

        public YooAssetsHandle(AssetHandle[] internalHandles)
        {
            this.internalHandles = internalHandles;
        }

        public UniTask ToUniTask(CancellationToken cancellationToken)
        {
            return WhenAll(internalHandles).ToUniTask(cancellationToken: cancellationToken);
        }

        private static IEnumerator WhenAll(AssetHandle[] handles)
        {
            var current = 0;
            while (handles != null && current < handles.Length)
            {
                var handle = handles[current];
                if (handle == null || !handle.IsValid)
                {
                    break;
                }
                else if (handle.IsDone)
                {
                    current++;
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void Release()
        {
            if (internalHandles != null)
            {
                foreach (var handle in internalHandles)
                {
                    handle.Release();
                    var assetInfo = handle.GetAssetInfo();
                    var package = PackageSearcher.SearchByAssetLocation(assetInfo.AssetPath);
                    package.TryUnloadUnusedAsset(assetInfo.AssetPath);
                }

                internalHandles = null;
            }
        }
    }
}