using YooAsset;

namespace GameFrame.Runtime
{
    public class AssetsDownLoadFsmController : FsmTaskController
    {
        public override void OnInitialize()
        {
            base.OnInitialize();
            AddState<PackageDownloadState>();
        }

        public void Start()
        {
            ChangeState<PackageDownloadState>();
        }
    }
}