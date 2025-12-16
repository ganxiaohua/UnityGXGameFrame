using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    [Serializable]
    public sealed partial class DefaultAssetReference : IAssetReference
    {
        private const int MinHashSetCount = 8;

        [SerializeField, HideInPlayMode]
        private List<string> refAssets;

        private HashSet<string> refAssetSet;

        public float PercentComplete
        {
            get
            {
                if (refAssets == null) return 1;
                int n = refAssets.Count;
                if (n == 0) return 1;
                int k = 0;
                var assetSystem = AssetManager.Instance;
                foreach (var asset in refAssets)
                {
                    if (assetSystem.TryGetAssetHandle(asset, out var handle) && handle.IsDone)
                    {
                        k++;
                        if (handle.AssetReference is DefaultAssetReference indirectReference)
                        {
                            var refAssets2 = indirectReference.refAssets;
                            if (refAssets2 != null)
                            {
                                foreach (var asset2 in refAssets2)
                                {
                                    n++;
                                    if (assetSystem.IsAssetReady(asset2))
                                    {
                                        k++;
                                    }
                                }
                            }
                        }
                    }
                }

                return (float) k / n;
            }
        }

        public bool RefInitialize()
        {
            if (refAssets == null) return true;
            var assetSystem = AssetManager.Instance;
            foreach (var asset in refAssets)
            {
                if (!assetSystem.ContainsAssetHandle(asset))
                    return false; // too late to initialize, because asset handle had been destroyed
            }

            foreach (var asset in refAssets)
            {
                assetSystem.IncrementReferenceCount(asset);
            }

            return true;
        }

        public bool RefAsset(string asset)
        {
            if (refAssets == null) refAssets = new List<string>();
            if (refAssets.Count < MinHashSetCount)
            {
                if (refAssets.Contains(asset))
                    return false;
            }
            else
            {
                if (refAssetSet == null) refAssetSet = new HashSet<string>(refAssets);
                if (refAssetSet.Contains(asset))
                    return false;
                refAssetSet.Add(asset);
            }

            refAssets.Add(asset);
            AssetManager.Instance.IncrementReferenceCount(asset);
            return true;
        }

        public void UnrefAsset(string asset, bool releaseImmediately = false)
        {
            if (refAssets == null) return;
            if (!refAssets.RemoveSwapBack(asset)) return;
            if (refAssetSet != null)
            {
                refAssetSet.Remove(asset);
                if (refAssetSet.Count < MinHashSetCount)
                    refAssetSet = null;
            }

            AssetManager.Instance.DecrementReferenceCount(asset, releaseImmediately);
        }

        public void UnrefAssets(bool releaseImmediately = false)
        {
            if (refAssets == null) return;
            var assetSystem = AssetManager.Instance;
            foreach (var asset in refAssets)
            {
                assetSystem.DecrementReferenceCount(asset, releaseImmediately);
            }

            refAssets = null;
            refAssetSet = null;
        }

        public bool Has(string path)
        {
            if (refAssetSet == null)
                return false;
            return refAssetSet.Contains(path);
        }

        public void Dispose()
        {
            UnrefAssets();
        }
    }
}