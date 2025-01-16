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
                AssetManager.Instance.IncrementReferenceCount(asset);
            }
        }
        
        public bool RefAsset(string asset)
        {
            if (refAssets == null) refAssets = new List<string>(16);
            if (refAssets.Contains(asset))
                return false;
            refAssets.Add(asset);
            AssetManager.Instance.IncrementReferenceCount(asset);
            return true;
        }

        public void LoadLater()
        {
            loaded++;
        }

        public void UnrefAsset(string asset,bool immediately =false)
        {
            if (refAssets == null) return;
            if (!refAssets.Remove(asset)) return;
            AssetManager.Instance.DecrementReferenceCount(asset,immediately);
            --loaded;
        }

        public void UnrefAssets(bool immediately = false)
        {
            if (refAssets == null) return;
            foreach (var asset in refAssets)
            {
                AssetManager.Instance.DecrementReferenceCount(asset,immediately);
            }
            loaded = 0;
            refAssets = null;
        }
    }
}