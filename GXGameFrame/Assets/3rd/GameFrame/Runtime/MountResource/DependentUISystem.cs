using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame
{
    public static class DependentUISystem
    {
        [SystemBind]
        public class DependentUIStartSystem : StartSystem<DependentUI, string, string>
        {
            protected override void Start(DependentUI self, string packageName, string windowName)
            {
                self.PackageName = packageName;
                self.DefaultAssetReference = new DefaultAssetReference();
                self.waitLoadTask = new UniTaskCompletionSource();
                Init(self, packageName, windowName).Forget();
            }

            private async UniTaskVoid Init(DependentUI self, string packageName, string windowName)
            {
                await UILoaderNew.Instance.AddPackage(packageName, self.DefaultAssetReference);
                self.Window = UIPackage.CreateObject(packageName, windowName);
                var succ = await UILoaderNew.Instance.LoadOver(self.PackageName);
                if (succ)
                {
                    self.waitLoadTask.TrySetResult();
                    return;
                }
                self.waitLoadTask.TrySetCanceled();
            }
        }

        [SystemBind]
        public class DependentUIClearSystem : ClearSystem<DependentUI>
        {
            protected override void Clear(DependentUI self)
            {
                
                if (self.waitLoadTask != null)
                {
                    self.waitLoadTask.TrySetCanceled();
                }
            }
        }

        public static async UniTask<bool> WaitLoad(this DependentUI self)
        {
            if (self.waitLoadTask == null)
            {
                return false;
            }

            await self.waitLoadTask.Task.SuppressCancellationThrow();
            if (self.waitLoadTask.GetStatus(0) == UniTaskStatus.Succeeded)
            {
                return true;
            }

            return false;
        }
    }
}