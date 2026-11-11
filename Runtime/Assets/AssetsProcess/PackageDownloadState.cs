using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageDownloadState : FsmState
    {
        private ResourceDownloaderOperation downloader;
        private bool isActive;

        public override void OnEnter()
        {
            base.OnEnter();
            isActive = true;
            BeginDownload().Forget();
        }

        public override void OnExit()
        {
            isActive = false;
            if (downloader != null)
            {
                downloader.DownloadError -= OnDownloadError;
                downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
                if (!downloader.IsDone)
                    downloader.CancelDownload();
            }
            base.OnExit();
        }

        private async UniTask BeginDownload()
        {
            var packageName = (string) GetData("packageName");
            downloader = (ResourceDownloaderOperation) GetData("downloader");
            downloader.DownloadError += OnDownloadError;
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            try
            {
                downloader.StartDownload();
                await downloader.ToUniTask();
                if (!isActive)
                    return;
                if (downloader.Status != EOperationStatus.Succeeded)
                {
                    EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloadFail, packageName);
                    return;
                }

                var package = YooAssets.GetPackage(packageName);
                var operation = package.ClearCacheAsync(new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles));
                await operation.ToUniTask();
                if (!isActive)
                    return;
                if (operation.Status != EOperationStatus.Succeeded)
                    Debug.LogWarning(operation.Error);

                EventData.Instance.FireAssetEvent(AssetEventType.Succ, packageName);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (isActive)
                    EventData.Instance.FireAssetEvent(AssetEventType.PackageDownloadFail, packageName);
            }
            finally
            {
                downloader.DownloadError -= OnDownloadError;
                downloader.DownloadProgressChanged -= OnDownloadProgressChanged;
            }
        }

        private void OnDownloadError(DownloadErrorEventArgs data)
        {
            Debugger.Log($"{data.FileName}下载失败 :error:{data.ErrorInfo}");
        }

        private void OnDownloadProgressChanged(DownloadProgressChangedEventArgs data)
        {
            EventData.Instance.FireAssetDownEvent(data);
        }
    }
}
