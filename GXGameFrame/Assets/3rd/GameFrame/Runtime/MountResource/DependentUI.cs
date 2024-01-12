using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame
{
    public class DependentUI : Entity, IStartSystem<string,string>, IClearSystem
    {
        public DefaultAssetReference DefaultAssetReference;
        public string PackageName;
        public GObject Window;
        public UniTaskCompletionSource waitLoadTask;
        public UINode UINode;

        public void Start(string packageName,string windowName)
        {
            PackageName = packageName;
            DefaultAssetReference = new DefaultAssetReference();
            waitLoadTask = new UniTaskCompletionSource();
            Init(packageName, windowName).Forget();
        }

        private async UniTaskVoid Init(string packageName, string windowName)
        {
            await UILoaderNew.Instance.AddPackage(packageName, DefaultAssetReference);
            Window = UIPackage.CreateObject(packageName, windowName);
            var succ = await UILoaderNew.Instance.LoadOver(PackageName);
            if (succ)
            {
                waitLoadTask.TrySetResult();
                return;
            }
            waitLoadTask.TrySetCanceled();
        }
        
        public  async UniTask<bool> WaitLoad()
        {
            if (waitLoadTask == null)
            {
                return false;
            }

            await waitLoadTask.Task.SuppressCancellationThrow();
            if (waitLoadTask.GetStatus(0) == UniTaskStatus.Succeeded)
            {
                return true;
            }
            return false;
        }
        
        public override void Clear()
        {
            base.Clear();
            waitLoadTask?.TrySetCanceled();
        }
    }
}