namespace GameFrame.Runtime
{
    public sealed partial class AssetManager
    {
        public bool IsAssetReady(string path)
        {
            return assetCaches.TryGetValue(path, out var cache) && cache.handle.IsDone;
        }

        public bool ContainsAssetHandle(string path)
        {
            return assetCaches.ContainsKey(path);
        }

        public bool TryGetAssetHandle(string path, out IAssetHandle handle)
        {
            if (assetCaches.TryGetValue(path, out var cache))
            {
                handle = cache.handle;
                return true;
            }

            handle = default;
            return false;
        }

        public bool TryGetAssetStatus(string path, out int referenceCount, out float lifeTime)
        {
            if (assetCaches.TryGetValue(path, out var cache))
            {
                referenceCount = cache.referenceCount;
                lifeTime = cache.lifeTime;
                return true;
            }

            referenceCount = default;
            lifeTime = default;
            return false;
        }
    }
}