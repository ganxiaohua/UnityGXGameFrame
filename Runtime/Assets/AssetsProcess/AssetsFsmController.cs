using YooAsset;

namespace GameFrame.Runtime
{
    public class AssetsFsmController : FsmTaskController
    {
        // private EDefaultBuildPipeline pipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;

        public override void OnInitialize()
        {
            base.OnInitialize();
            YooAssets.Initialize();
            var defPackage = YooConst.PackageSettings[0];
            SetData("packageName", defPackage.Name);
            SetData("playMode", defPackage.PlayMode);
            AddState<PackageInitState>();
            AddState<PackageVersionUpdateState>();
            AddState<PackageManifestUpdateState>();
            AddState<PackageDownloaderCreate>();
            AddState<PackageDownloadState>();
            AddState<PackageDoneState>();
            ChangeState<PackageInitState>();
            SetCompleteTypes(typeof(PackageDoneState));
        }

        // public void OnAssetEvent(AssetEventType assetEvent, object obj)
        // {
        //     switch (assetEvent)
        //     {
        //         case AssetEventType.PackageDownloader:
        //              
        //             break;
        //     }
        // }
    }
}