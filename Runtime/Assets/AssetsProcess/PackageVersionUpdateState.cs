using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace GameFrame
{
    public class PackageVersionUpdateState :FsmState
    {
        public override void OnEnter(FsmController fsmController)
        {
            base.OnEnter(fsmController);
            UpdatePackageVersion().Forget();
        }
        
        private async UniTask UpdatePackageVersion()
        {
            var packageName = (string)GetData("packageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.ToUniTask();
            if (operation.Status != EOperationStatus.Succeed)
            {
                Debugger.LogWarning(operation.Error);
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