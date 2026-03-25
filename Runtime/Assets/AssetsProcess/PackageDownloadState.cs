using Cysharp.Threading.Tasks;
using YooAsset;

namespace GameFrame.Runtime
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
            downloader.DownloadErrorCallback = (DownloadErrorData data) =>
            {
                Debugger.Log($"{data.FileName}下载失败 :error:{data.ErrorInfo}");
                EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloadFail);
            };
            downloader.DownloadUpdateCallback = (DownloadUpdateData data) => { EventData.Instance.FireAssetDownEvent(data); };
            downloader.BeginDownload();
            await downloader.ToUniTask();
            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;
            var packageName = (string) GetData("packageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            operation.Completed += Operation_Completed;
        }

        private void Operation_Completed(YooAsset.AsyncOperationBase obj)
        {
            ChangeState<PackageDoneState>();
        }
    }
}