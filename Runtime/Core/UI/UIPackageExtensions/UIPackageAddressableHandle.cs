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
        private int loadNum = 0;

        public void Initialize(object key, Type type)
        {
            IsValid = true;
            IsDone = false;
            loadNum = 0;
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
            loadNum++;
            return bufferHandle.ToUniTask(cancellationToken: cancellationToken);
        }

        public void Finish()
        {
            if (!IsDone)
            {
                Assert.AreNotEqual(null, bufferHandle.AssetObject, $"no addressable asset '{bufferHandle}' found");
                var buffer = ((TextAsset) bufferHandle.AssetObject).bytes;
                package = UIPackage.AddPackage(buffer, string.Empty, UIPackageHelper.OnLoadResFromBundleAsync);
            }

            if (--loadNum == 0)
            {
                bufferHandle.Release();
                bufferHandle = default;
            }

            IsDone = true;
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