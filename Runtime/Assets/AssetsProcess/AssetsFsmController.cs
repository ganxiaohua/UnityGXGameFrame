using YooAsset;

namespace GameFrame
{
    public class AssetsFsmController : FsmTaskController
    {
#if UNITY_EDITOR
        private EPlayMode playMode = EPlayMode.EditorSimulateMode;
#else
        private EPlayMode playMode = EPlayMode.OfflinePlayMode;
#endif
        private EDefaultBuildPipeline pipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;

        private string packageName = "DefaultPackage";

        public override void OnInitialize()
        {
            base.OnInitialize();
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