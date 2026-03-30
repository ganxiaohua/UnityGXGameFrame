using YooAsset;

namespace GameFrame.Runtime
{
    public enum AssetEventType
    {
        InitFail,
        LoadVersionFail,
        PackageManifestFail,
        PackageDownloaderNoMsg,
        PackageDownloaderMsgGetOver,
        PackageDownloadFail,
        Succ,
    }
    

    public interface IAssetEvent : IEvent
    {
        public void OnAssetEvent(AssetEventType assetEvent,object obj);
        public void OnAssetEvent(ResourceDownloaderOperation obj);

        public void OnAssetEvent(DownloadUpdateData obj);
    }

    public partial class EventData
    {
        public void FireAssetEvent(AssetEventType eventType,object obj =null)
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
                    ((IAssetEvent) entity).OnAssetEvent(eventType,obj);
                }
            }

            HashSetPool<IEntity>.Release(allEntity);
        }

        public void FireAssetEvent(ResourceDownloaderOperation obj)
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