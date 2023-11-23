using System;
using System.Collections.Generic;
using UnityEngine;
using Common.Runtime;

namespace GameFrame
{
    [Serializable]
    public sealed class DefaultAssetReference : IAssetReference
    {
        private const int MinHashSetCount = 8;
        
        [SerializeField]
        private List<string> refAssets;
        private HashSet<string> refAssetSet;

        public void RefInitialize()
        {
            if (refAssets == null) return;
            foreach (var asset in refAssets)
            {
                AssetSystem.Instance.IncrementReferenceCount(asset);
            }
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
            AssetSystem.Instance.IncrementReferenceCount(asset);
            return true;
        }

        public void UnrefAsset(string asset)
        {
            if (refAssets == null) return;
            if (!refAssets.Remove(asset)) return;
            if (refAssetSet != null)
            {
                refAssetSet.Remove(asset);
                if (refAssetSet.Count < MinHashSetCount)
                    refAssetSet = null;      
            }
            AssetSystem.Instance.DecrementReferenceCount(asset);
        }

        public void UnrefAssets()
        {
            if (refAssets == null) return;
            foreach (var asset in refAssets)
            {
                AssetSystem.Instance.DecrementReferenceCount(asset);
            }
            refAssets = null;
            refAssetSet = null;
        }
    }
}