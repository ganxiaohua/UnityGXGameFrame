#if GAMEFRAME_USE_UNITYUI
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    internal sealed class ResourcesPrefabHandle : IAssetHandle
    {
        public bool IsValid { get; private set; }
        public bool IsDone { get; private set; }
        public object Result { get; private set; }
        public IAssetReference AssetReference => null;
        private ResourceRequest request;

        public void Initialize(object key, Type type)
        {
            IsValid = true;
            IsDone = false;
            request = Resources.LoadAsync<GameObject>((string)key);
        }

        public UniTask GetTask(CancellationToken cancellationToken) => request.ToUniTask(cancellationToken: cancellationToken);

        public void Finish()
        {
            Result = request.asset;
            IsDone = true;
            request = null;
        }

        public void Release()
        {
            // Resources.UnloadAsset cannot unload a GameObject prefab. Drop our reference;
            // Unity's normal UnloadUnusedAssets pass owns reclamation of Resources objects.
            Result = null;
            request = null;
            IsValid = false;
        }
    }
}
#endif
