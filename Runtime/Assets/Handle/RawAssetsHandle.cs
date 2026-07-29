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
                    result[i] = internalHandles[i].GetRawFileData();
                }

                return result;
            }
        }

        public IAssetReference AssetReference => null;

        private YooAsset.RawFileHandle[] internalHandles;

        public void Initialize(object paths, Type type)
        {
            try
            {
#if UNITY_EDITOR
                using (new Profiler("TextAssetsHandle.Initialize"))
#endif
                {
                    if (paths is string tag)
                    {
                        // tag
                        var package = PackageSearcher.SearchByAssetTag(tag, out var infos);
                        internalHandles = new YooAsset.RawFileHandle[infos.Length];
                        for (var i = infos.Length - 1; i >= 0; i--)
                        {
                            internalHandles[i] = package.LoadRawFileAsync(infos[i]);
                        }
                    }
                    else if (paths is IList<string> locations)
                    {
                        // asset locations
                        internalHandles = new YooAsset.RawFileHandle[locations.Count];
                        for (var i = locations.Count - 1; i >= 0; i--)
                        {
                            var package = PackageSearcher.SearchByAssetLocation(locations[i], out var info, null);
                            internalHandles[i] = package.LoadRawFileAsync(info);
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
                result[i] = internalHandles[i].GetRawFileText();
            }

            return result;
        }

        public void Release()
        {
            if (internalHandles == null)
                return;

            foreach (var handle in internalHandles)
            {
                if (handle == null)
                    continue;

                var assetInfo = handle.GetAssetInfo();
                if (handle.IsValid)
                    handle.Release();

                var package = PackageSearcher.SearchByAssetLocation(assetInfo.AssetPath);
                package?.TryUnloadUnusedAsset(assetInfo);
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
                if (handle == null || handle.Status != EOperationStatus.Succeed)
                    return false;
            }

            return true;
        }

        private static IEnumerator WhenAll(YooAsset.RawFileHandle[] handles)
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
