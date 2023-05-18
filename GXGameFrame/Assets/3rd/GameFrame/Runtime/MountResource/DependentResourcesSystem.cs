using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace GameFrame
{
    public static class DependentResourcesSystem
    {
        [SystemBind]
        public class DependentResourcesStartSystem : StartSystem<DependentResources, string>
        {
            protected override async void Start(DependentResources self, string p1)
            {
                self.Task = new UniTaskCompletionSource();
                self.AssetPath = p1;
                Object obj =  await AssetManager.Instance.LoadAsyncTask<Object>(self.AssetPath);
                self.Asset = obj;
                self.Task.TrySetResult();
            }
        }


        [SystemBind]
        public class DependentResourcesClearSystem : ClearSystem<DependentResources>
        {
            protected override void Clear(DependentResources self)
            {
                AssetManager.Instance.UnLoad(self.AssetPath);
                if (self.Task != null)
                {
                    self.Task.TrySetCanceled();
                }
            }
        }


        public static async UniTask WaitLoad(this DependentResources self)
        {
            await self.Task.Task;
            self.Task = null;
        }
    }
}