using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageDownloaderCreate : FsmState
    {
        public override void OnEnter()
        {
            CreateDownloader();
        }

        private void CreateDownloader()
        {
            var packageName = (string) GetData("packageName");
            var package = YooAssets.GetPackage(packageName);
            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
            if (downloader.TotalDownloadCount == 0)
            {
                EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloaderNoMsg);
            }
            else
            {
                EventData.Instance.FireAssetEvent(downloader);
                EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloaderMsgGetOver);
            }
        }
    }
}