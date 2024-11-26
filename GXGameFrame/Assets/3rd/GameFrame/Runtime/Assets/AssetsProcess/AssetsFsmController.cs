using YooAsset;

namespace GameFrame
{
    public class AssetsFsmController : FsmTaskController
    {
        private EPlayMode playMode = EPlayMode.EditorSimulateMode;
        private EDefaultBuildPipeline pipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;

        private string packageName = "DefaultPackage";

        public override void Initialize()
        {
            base.Initialize();
            YooAssets.Initialize();
            SetData("packageName", packageName);
            SetData("playMode", playMode);
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