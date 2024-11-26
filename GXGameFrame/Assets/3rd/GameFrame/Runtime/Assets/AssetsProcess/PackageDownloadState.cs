using Cysharp.Threading.Tasks;
using YooAsset;

namespace GameFrame
{
    public class PackageDownloadState : FsmState
    {
        public override void OnEnter(FsmController fsmController)
        {
            base.OnEnter(fsmController);
            BeginDownload().Forget();
        }

        private async UniTask BeginDownload()
        {
            var downloader = (ResourceDownloaderOperation) GetData("downloader");
            downloader.OnDownloadErrorCallback = (string fileName, string error) => { Debugger.Log($"{fileName}下载失败 :error:{error}"); };
            downloader.OnDownloadProgressCallback = (int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes) =>
            {
                Debugger.Log($"{totalDownloadCount}/{currentDownloadCount}  |  {totalDownloadBytes}/{currentDownloadBytes}");
            };
            downloader.BeginDownload();
            await downloader.ToUniTask();
            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;
            var packageName = (string) GetData("packageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearUnusedBundleFilesAsync();
            operation.Completed += Operation_Completed;
        }

        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            ChangeState<PackageDoneState>();
        }
    }
}