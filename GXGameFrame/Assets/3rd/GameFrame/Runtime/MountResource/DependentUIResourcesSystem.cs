using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public static class DependentUIResourcesSystem
    {
        [SystemBind]
        public class DependentUIResourcesStartSystem : StartSystem<DependentUIResources, List<string>>
        {
            protected override void Start(DependentUIResources self, List<string> p1)
            {
                self.Task = new UniTaskCompletionSource<bool>();
                self.AssetPaths = p1;
                foreach (var path in p1)
                {
                    self.CurLoadAmount = 0;
                    AssetManager.Instance.UILoader.AddPackage(path, self.LoadUIAssetOver);
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
                    AssetManager.Instance.UILoader.RemovePackages(path);
                }

                if (self.Task != null)
                {
                    self.Task.TrySetCanceled();
                }
            }
        }

        public static void LoadUIAssetOver(this DependentUIResources self)
        {
            if (++self.CurLoadAmount == self.AssetPaths.Count)
            {

                self.Task.TrySetResult(true);
            }
        }

        public static async UniTask WaitLoad(this DependentUIResources self)
        {
            await self.Task.Task;
            self.Task = null;
        }
    }
}