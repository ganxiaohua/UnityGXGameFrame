using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageVersionUpdateState : FsmState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            UpdatePackageVersion().Forget();
        }

        private async UniTask UpdatePackageVersion()
        {
            var packageName = (string) GetData("packageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.ToUniTask();

            if (operation.Status != EOperationStatus.Succeed)
            {
                Debug.LogWarning(operation.Error);
                EventData.Instance.FireAssetEvent(AssetEventType.LoadVersionFail);
            }
            else
            {
                Debug.Log($"Request package version : {operation.PackageVersion}");
                SetData("packageVersion", operation.PackageVersion);
                ChangeState<PackageManifestUpdateState>();
            }
        }
    }
}