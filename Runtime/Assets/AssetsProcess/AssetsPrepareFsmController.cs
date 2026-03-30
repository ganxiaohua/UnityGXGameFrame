using YooAsset;

namespace GameFrame.Runtime
{
    public class AssetsPrepareFsmController : FsmTaskController
    {

        public override void OnInitialize()
        {
            base.OnInitialize();
            AddState<PackageInitState>();
            AddState<PackageVersionUpdateState>();
            AddState<PackageManifestUpdateState>();
            AddState<PackageDownloaderCreate>();
        }

        public void Start()
        {
            ChangeState<PackageInitState>();
        }
    }
}