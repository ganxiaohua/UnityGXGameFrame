using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public sealed class UIPackageAddressableHandle : IAssetHandle
    {
        public bool IsValid { get; private set; }

        public bool IsDone { get; private set; }

        public object Result => package;

        public IAssetReference AssetReference { get; private set; }

        private AssetHandle bufferHandle;
        private UIPackage package;

        public void Initialize(object key, Type type)
        {
            IsValid = true;
            IsDone = false;
            AssetReference = new DefaultAssetReference();
            try
            {
#if UNITY_EDITOR
                using (new Profiler("UIPackageAddressableHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info, type);
                    bufferHandle = package.LoadAssetAsync(info);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }


        public UniTask GetTask(CancellationToken cancellationToken)
        {
            return bufferHandle.ToUniTask(cancellationToken: cancellationToken);
        }

        public void Finish()
        {
            IsDone = true;

            Assert.AreNotEqual(null, bufferHandle.AssetObject, $"no addressable asset '{bufferHandle}' found");
            var buffer = ((TextAsset) bufferHandle.AssetObject).bytes;
            bufferHandle.Release();
            bufferHandle = default;
            package = UIPackage.AddPackage(buffer, string.Empty, UIPackageHelper.OnLoadResFromBundleAsync);
        }

        public void Release()
        {
            IsValid = false;
            if (package != null)
            {
                UIPackage.RemovePackage(package.name);
                package = null;
            }

            AssetReference.Dispose();

            if (bufferHandle != null)
            {
                bufferHandle.Release();
                bufferHandle = default;
            }
        }
    }
}