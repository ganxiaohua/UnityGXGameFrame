using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace GameFrame
{
    public static class DependentUIResourcesSystem
    {
        [SystemBind]
        public class DependentUIResourcesStartSystem : StartSystem<DependentUIResources, List<string>>
        {
            protected override void Start(DependentUIResources self, List<string> p1)
            {
                Init(self, p1).Forget();
            }

            private async UniTaskVoid Init(DependentUIResources self, List<string> p1)
            {
                self.Task = new UniTaskCompletionSource();
                self.AssetPaths = p1;
                self.All = 0;
                self.Cur = 0;
                foreach (var path in p1)
                {
                    self.All += await UILoader.Instance.AddPackage(path, self.LoadUIAssetOver);
                }
            }
        }

        [SystemBind]
        public class DependentUIResourcesClearSystem : ClearSystem<DependentUIResources>
        {
            protected override void Clear(DependentUIResources self)
            {
                foreach (var path in self.AssetPaths)
                {
                    UILoader.Instance.RemovePackages(path);
                }

                if (self.Task != null)
                {
                    self.Task.TrySetCanceled();
                }
            }
        }

        public static void LoadUIAssetOver(this DependentUIResources self)
        {
            if (++self.Cur == self.All)
            {
                self.Task.TrySetResult();
            }
        }

        public static async UniTask<bool> WaitLoad(this DependentUIResources self)
        {
            if (self.Task == null)
            {
                return false;
            }

            await self.Task.Task.SuppressCancellationThrow();
            if (self.Task.GetStatus(0) == UniTaskStatus.Succeeded)
            {
                return true;
            }

            return false;
        }
    }
}