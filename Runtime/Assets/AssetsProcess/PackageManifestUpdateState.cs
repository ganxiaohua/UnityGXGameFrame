using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageManifestUpdateState : FsmState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            UpdateManifest().Forget();
        }

        private async UniTask UpdateManifest()
        {
            var packageName = (string) GetData("packageName");
            var packageVersion = (string) GetData("packageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.LoadPackageManifestAsync(new LoadPackageManifestOptions(packageVersion, 60));
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeeded)
            {
                Debug.LogWarning(operation.Error);
                EventData.Instance.FireAssetEvent(AssetEventType.PackageManifestFail, packageName);
            }
            else
            {
                ChangeState<PackageDownloaderCreate>();
            }
        }
    }
}
