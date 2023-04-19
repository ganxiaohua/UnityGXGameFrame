using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor.UI;

namespace GameFrame
{
    public static class DependentResourcesSystem
    {
        [SystemBind]
        public class DependentResourcesStartSystem : StartSystem<DependentResources, List<string>>
        {
            protected override void Start(DependentResources self, List<string> p1)
            {
                self.Path = p1;
                foreach (var path in p1)
                {
                    self.CurLoadAmount = 0;
                    self.LoadOver = false;
                    AssetManager.Instance.UILoader.AddPackage(path, self.LoadUIAssetOver);
                }
            }
        }


        [SystemBind]
        public class DependentResourcesClearSystem : ClearSystem<DependentResources>
        {
            protected override void Clear(DependentResources self)
            {
                foreach (var path in self.Path)
                {
                    AssetManager.Instance.UILoader.RemovePackages(path);
                }

                if (self.Task != null)
                {
                    self.Task.SetResult(false);
                }
            }
        }

        public static void LoadUIAssetOver(this DependentResources self)
        {
            if (++self.CurLoadAmount == self.Path.Count)
            {
                self.LoadOver = true;
                self.Task.SetResult(true);
            }
        }

        public static async UniTask WaitLoad(this DependentResources self)
        {
            if (!self.LoadOver)
            {
                self.Task = new TaskCompletionSource<bool>();
                await self.Task.Task;
                self.Task = null;
            }
        }
    }
}