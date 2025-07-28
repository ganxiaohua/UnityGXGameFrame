using System;

namespace GameFrame.Runtime
{
    public interface IAssetReference : IDisposable
    {
        bool IsDisposed { get; }

        float PercentComplete { get; }

        bool RefInitialize();

        bool RefAsset(string asset);

        void UnrefAsset(string asset, bool releaseImmediately = false);

        void UnrefAssets(bool releaseImmediately = false);
    }
}