using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public struct TextAssetsHandle : IAssetHandle
    {
        public bool IsValid => internalHandle.IsValid;

        public bool IsDone => internalHandle.IsDone;

        public object Result => internalHandle.Result;

        public IAssetReference AssetReference => null;

        private YooAssetsHandle internalHandle;

        public void Initialize(object paths,Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("TextAssetsHandle.Initialize"))
#endif
                {
                    AssetHandle[] handles;
                    if (paths is string tag)
                    {
                        // tag
                        var package = PackageSearcher.SearchByAssetTag(tag, out var infos);
                        handles = new AssetHandle[infos.Length];
                        for (var i = infos.Length - 1; i >= 0; i--)
                        {
                            handles[i] = package.LoadAssetAsync(infos[i]);
                        }
                    }
                    else if (paths is IList<string> locations)
                    {
                        // asset locations
                        handles = new AssetHandle[locations.Count];
                        for (var i = locations.Count - 1; i >= 0; i--)
                        {
                            var package = PackageSearcher.SearchByAssetLocation(locations[i], out var info,type);
                            handles[i] = package.LoadAssetAsync(info);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"{paths}");
                    }

                    internalHandle = new YooAssetsHandle(handles);
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
            if (internalHandle.IsValid)
            {
                internalHandle.Release();
                internalHandle = default;
            }
        }
    }
}