using YooAsset;

namespace GameFrame.Runtime
{
    public enum AssetEventType
    {
        InitFail,
        LoadVersionFail,
        PackageManifestFail,
        PackageDownloader,
        PackageDownloadFail,
        DownloadUpdateCallback,
        Succ,
    }

    public struct DownLoadSize
    {
        public int Count;
        public long Size;
    }

    public interface IAssetEvent : IEvent
    {
        public void OnAssetEvent(AssetEventType assetEvent);
        public void OnAssetEvent(DownLoadSize obj);

        public void OnAssetEvent(DownloadUpdateData obj);
    }

    public partial class EventData
    {
        public void FireAssetEvent(AssetEventType eventType)
        {
            var allEntity = GetEntity(typeof(IAssetEvent));
            if (allEntity == null)
            {
                return;
            }

            foreach (var entity in allEntity)
            {
                if (entity.State == IEntity.EntityState.IsRunning)
                {
                    ((IAssetEvent) entity).OnAssetEvent(eventType);
                }
            }

            HashSetPool<IEntity>.Release(allEntity);
        }

        public void FireAssetEvent(DownLoadSize obj)
        {
            var allEntity = GetEntity(typeof(IAssetEvent));
            if (allEntity == null)
            {
                return;
            }

            foreach (var entity in allEntity)
            {
                if (entity.State == IEntity.EntityState.IsRunning)
                {
                    ((IAssetEvent) entity).OnAssetEvent(obj);
                }
            }

            HashSetPool<IEntity>.Release(allEntity);
        }

        public void FireAssetDownEvent(DownloadUpdateData obj)
        {
            var allEntity = GetEntity(typeof(IAssetEvent));
            if (allEntity == null)
            {
                return;
            }

            foreach (var entity in allEntity)
            {
                if (entity.State == IEntity.EntityState.IsRunning)
                {
                    ((IAssetEvent) entity).OnAssetEvent(obj);
                }
            }

            HashSetPool<IEntity>.Release(allEntity);
        }
    }
}