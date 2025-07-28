using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageDownloaderCreate : FsmState
    {
        public override void OnEnter(FsmController fsmController)
        {
            base.OnEnter(fsmController);
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
                Debug.Log("Not found any download files !");
                ChangeState<PackageDoneState>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;
                SetData("downloader", downloader);
                ChangeState<PackageDownloadState>();
                // PatchEventDefine.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
            }
        }
    }
}