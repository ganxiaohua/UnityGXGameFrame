using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public struct RawAssetsHandle : IAssetHandle
    {
        public bool IsValid
        {
            get
            {
                if (internalHandles == null)
                    return false;

                for (int i = 0; i < internalHandles.Length; i++)
                {
                    if (internalHandles[i] == null || !internalHandles[i].IsValid)
                        return false;
                }

                return true;
            }
        }

        public bool IsDone
        {
            get
            {
                if (internalHandles == null)
                    return true;

                for (int i = 0; i < internalHandles.Length; i++)
                {
                    if (internalHandles[i] != null && !internalHandles[i].IsDone)
                        return false;
                }

                return true;
            }
        }

        public object Result
        {
            get
            {
                if (!AllSucceeded())
                    return null;

                var result = new byte[internalHandles.Length][];
                for (int i = 0; i < internalHandles.Length; i++)
                {
                    result[i] = ((RawFileObject) internalHandles[i].AssetObject).GetBytes();
                }

                return result;
            }
        }

        public IAssetReference AssetReference => null;

        private AssetHandle[] internalHandles;

        public void Initialize(object paths, Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("RawAssetsHandle.Initialize"))
#endif
                {
                    if (paths is string tag)
                    {
                        // tag
                        var package = PackageSearcher.SearchByAssetTag(tag, out var infos);
                        if (package == null)
                            return;
                        internalHandles = new AssetHandle[infos.Length];
                        for (var i = infos.Length - 1; i >= 0; i--)
                        {
                            internalHandles[i] = package.LoadAssetAsync<RawFileObject>(infos[i].AssetPath);
                        }
                    }
                    else if (paths is IList<string> locations)
                    {
                        // asset locations
                        internalHandles = new AssetHandle[locations.Count];
                        for (var i = locations.Count - 1; i >= 0; i--)
                        {
                            var package = PackageSearcher.SearchByAssetLocation(locations[i], out var info, typeof(RawFileObject));
                            internalHandles[i] = package.LoadAssetAsync(info);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"{paths}");
                    }
                }
            }
            catch (Exception e)
            {
                Release();
                Debug.LogException(e);
            }
        }

        public UniTask GetTask(CancellationToken cancellationToken)
        {
            return WhenAll(internalHandles).ToUniTask(cancellationToken: cancellationToken);
        }

        public void Finish()
        {
     
        }

        public IList<string> GetTexts()
        {
            if (!AllSucceeded())
                return null;

            var result = new string[internalHandles.Length];
            for (int i = 0; i < internalHandles.Length; i++)
            {
                result[i] = ((RawFileObject) internalHandles[i].AssetObject).GetText() ?? string.Empty;
            }

            return result;
        }

        public void Release()
        {
            if (internalHandles == null)
                return;

            foreach (var handle in internalHandles)
            {
                if (handle == null || !handle.IsValid)
                    continue;

                var assetInfo = handle.GetAssetInfo();
                handle.Release();

                if (YooAssets.TryGetPackage(assetInfo.PackageName, out var package))
                    package.TryUnloadUnusedAsset(assetInfo);
            }

            internalHandles = null;
        }

        private bool AllSucceeded()
        {
            if (internalHandles == null)
                return false;

            for (int i = 0; i < internalHandles.Length; i++)
            {
                var handle = internalHandles[i];
                if (handle == null || !handle.IsValid || handle.Status != EOperationStatus.Succeeded)
                    return false;
            }

            return true;
        }

        private static IEnumerator WhenAll(AssetHandle[] handles)
        {
            int current = 0;
            while (handles != null && current < handles.Length)
            {
                var handle = handles[current];
                if (handle == null || !handle.IsValid)
                {
                    break;
                }

                if (handle.IsDone)
                {
                    current++;
                }
                else
                {
                    yield return null;
                }
            }
        }
    }
}
