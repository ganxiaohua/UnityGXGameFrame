using YooAsset;

namespace GameFrame.Runtime
{
    public class AssetsFsmController : FsmTaskController
    {
        private EDefaultBuildPipeline pipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;
        
        public override void OnInitialize()
        {
            base.OnInitialize();
            YooAssets.Initialize();
            var defPackage = YooConst.PackageSettings[0];
            SetData("packageName",defPackage.name);
            SetData("playMode", defPackage.playMode);
            SetData("pipeline", pipeline);
            AddState<PackageInitState>();
            AddState<PackageVersionUpdateState>();
            AddState<PackageManifestUpdateState>();
            AddState<PackageDownloaderCreate>();
            AddState<PackageDownloadState>();
            AddState<PackageDoneState>();
            ChangeState<PackageInitState>();
            SetCompleteTypes(typeof(PackageDoneState));
        }
    }
}