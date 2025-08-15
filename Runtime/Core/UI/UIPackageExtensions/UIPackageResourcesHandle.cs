using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed class UIPackageResourcesHandle : IAssetHandle
    {
        public bool IsValid { get; private set; }

        public bool IsDone { get; private set; }

        public object Result => package;

        public IAssetReference AssetReference => null;

        public string Path { get; private set; }

        private ResourceRequest request;
        private UIPackage package;

        public void Initialize(object key, Type type)
        {
            IsValid = true;
            IsDone = false;
            Path = (string) key;
#if UNITY_EDITOR
            using (new Profiler("Resources.LoadAsync<TextAsset>"))
#endif
                request = Resources.LoadAsync<TextAsset>(Path);
        }

        public UniTask GetTask(CancellationToken cancellationToken)
        {
            return request.ToUniTask(cancellationToken: cancellationToken);
        }

        public void Finish()
        {
            IsDone = true;

            Assert.AreNotEqual(null, request.asset, $"no resources '{Path}' found");
            var buffer = ((TextAsset) request.asset).bytes;
            Resources.UnloadAsset(request.asset);
            request = default;
            // remove '_fui' suffix
            package = UIPackage.AddPackage(buffer, Path.Substring(0, Path.Length - 4), UIPackageHelper.OnLoadResFromResources);
        }

        public void Release()
        {
            IsValid = false;

            request = default;

            if (package != null)
            {
                UIPackage.RemovePackage(package.name);
                package = null;
            }
        }
    }
}