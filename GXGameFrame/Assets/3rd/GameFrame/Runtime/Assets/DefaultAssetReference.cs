using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    [Serializable]
    public sealed class DefaultAssetReference : IAssetReference
    {
        [SerializeField]
        private List<string> refAssets;

        private int loaded;

        public bool IsLoadAll => refAssets.Count == loaded;

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
            if (refAssets == null) refAssets = new List<string>(16);
            if (refAssets.Contains(asset))
                return false;
            refAssets.Add(asset);
            AssetSystem.Instance.IncrementReferenceCount(asset);
            return true;
        }

        public void LoadLater()
        {
            loaded++;
        }

        public void UnrefAsset(string asset)
        {
            if (refAssets == null) return;
            if (!refAssets.Remove(asset)) return;
            AssetSystem.Instance.DecrementReferenceCount(asset);
            --loaded;
        }

        public void UnrefAssets()
        {
            if (refAssets == null) return;
            foreach (var asset in refAssets)
            {
                AssetSystem.Instance.DecrementReferenceCount(asset);
            }
            loaded = 0;
            refAssets = null;
        }
    }
}