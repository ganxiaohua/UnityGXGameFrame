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
            var downloader = package.CreateResourceDownloader(new ResourceDownloaderOptions(downloadingMaxNum, failedTryAgain));
            if (downloader.TotalDownloadCount == 0)
            {
                EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloaderNoMsg, packageName);
            }
            else
            {
                EventData.Instance.FireAssetEvent(packageName, downloader);
                EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloaderMsgGetOver, packageName);
            }
        }
    }
}
