using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GameFrame.Runtime
{
    public sealed class SceneHandle : IAssetHandle
    {
        public bool IsValid => internalHandle?.IsValid ?? false;

        public bool IsDone => internalHandle?.IsDone ?? true;

        public object Result
        {
            get
            {
                if (internalHandle != null && internalHandle.Status == EOperationStatus.Succeed)
                    return internalHandle.SceneObject;
                return null;
            }
        }

        public IAssetReference AssetReference => null;

        private YooAsset.SceneHandle internalHandle;

        private LoadSceneMode loadMode;
        private bool activateOnLoad;
        private int priority;

        public SceneHandle()
        {
            this.loadMode = LoadSceneMode.Single;
            this.activateOnLoad = true;
            this.priority = 100;
        }

        public SceneHandle(LoadSceneMode loadMode, bool activateOnLoad, int priority)
        {
            this.loadMode = loadMode;
            this.activateOnLoad = activateOnLoad;
            this.priority = priority;
        }

        public void Initialize(object key,Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("SceneHandle.Initialize"))
#endif
                {
                    var package = PackageSearcher.SearchByAssetLocation((string) key, out var info,type);
                    internalHandle = package.LoadSceneAsync(info, loadMode,LocalPhysicsMode.None, !activateOnLoad, (uint) priority);
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
                //当加载新的主场景的时候，会自动释放之前加载的主场景以及附加场景。
                if (!internalHandle.IsMainScene())
                    internalHandle.UnloadAsync();
                internalHandle = default;
            }
        }
    }
}