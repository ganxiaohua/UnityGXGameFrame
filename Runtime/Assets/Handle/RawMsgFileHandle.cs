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
    public struct RawMsgFileHandle : IAssetHandle
    {
        public bool IsValid => operation != null;

        public bool IsDone => operation?.IsDone ?? true;

        public object Result => Path;

        public IAssetReference AssetReference => null;

        private EnsureBundleFileOperation operation;

        public string Path { get; private set; }

        public void Initialize(object key, Type type)
        {
            Path = null;
            operation = null;
            try
            {
#if UNITY_EDITOR
                using (new Profiler("RawMsgFileHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info, null);
                    operation = package.EnsureBundleFileAsync(new EnsureBundleFileOptions(info));
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public UniTask GetTask(CancellationToken cancellationToken)
        {
            return operation != null
                    ? operation.ToUniTask(cancellationToken: cancellationToken)
                    : UniTask.CompletedTask;
        }

        public void Finish()
        {
            Path = operation != null && operation.Status == EOperationStatus.Succeeded
                    ? operation.Detail.BundleFilePath
                    : null;
        }

        public void Release()
        {
            operation = null;
            Path = null;
        }
    }
}
