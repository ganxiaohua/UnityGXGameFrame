using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    /// <summary>
    /// get raw path not load bytes
    /// </summary>
    public  struct RawMsgFileHandle : IAssetHandle
    {
        public bool IsValid => internalHandle?.IsValid ?? false;

        public bool IsDone => internalHandle?.IsDone ?? true;

        public object Result => Path;

        public IAssetReference AssetReference => null;

        private YooAsset.RawFileHandle internalHandle;

        public string Path { get; private set; }

        public void Initialize(object key, Type type)
        {
            Path = null;
            try
            {
#if UNITY_EDITOR
                using (new Profiler("RawMsgFileHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info, null);
                    internalHandle = package.LoadRawFileAsync(info);
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
            Path = internalHandle != null && internalHandle.Status == EOperationStatus.Succeed
                    ? internalHandle.GetRawFilePath()
                    : null;
        }

        public void Release()
        {
            if (internalHandle == null)
                return;

            var assetInfo = internalHandle.GetAssetInfo();
            if (internalHandle.IsValid)
                internalHandle.Dispose();

            var package = PackageSearcher.SearchByAssetLocation(assetInfo.AssetPath);
            package?.TryUnloadUnusedAsset(assetInfo);
            internalHandle = null;
        }
    }
}
