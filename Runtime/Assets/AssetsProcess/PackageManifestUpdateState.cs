using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageManifestUpdateState:FsmState
    {
        public override void OnEnter(FsmController fsmController)
        {
            base.OnEnter(fsmController);
            UpdateManifest().Forget();
        }
        
        private async UniTask UpdateManifest()
        {

            var packageName = (string)GetData("packageName");
            var packageVersion = (string)GetData("packageVersion");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.UpdatePackageManifestAsync(packageVersion);
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
            }
            else
            {
                ChangeState<PackageDownloaderCreate>();
            }
        }
    }
}