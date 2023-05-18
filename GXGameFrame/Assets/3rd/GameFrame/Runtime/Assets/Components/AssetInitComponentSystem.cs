using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;

namespace GameFrame
{
    public static class AssetInitComponentSystem
    {
        [SystemBind]
        public class AssetInitComponentStartSystem : StartSystem<AssetInitComponent>
        {
            protected override async void Start(AssetInitComponent self)
            {
                self.Task = new UniTaskCompletionSource();
                await self.CheckUpdate();
                self.Task.TrySetResult();
            }
        }

        [SystemBind]
        public class AssetInitComponentClearSystem : ClearSystem<AssetInitComponent>
        {
            protected override void Clear(AssetInitComponent self)
            {
            }
        }

        public static async UniTask CheckUpdate(this AssetInitComponent self)
        {
            await AddressablesHelper.InitializeAsync();
            //如果是在editor模式之下,且开启了资源分离(将资源拆到其他的工程里面去)  或者是 非Editor模式
#if (UNITY_EDITOR && RESSEQ) || !UNITY_EDITOR
            self.CheckUpdate = new CheckUpdate();
            await self.CheckUpdate.CheckVersions();
#endif
        }

        public static async UniTask WaitLoad(this AssetInitComponent self)
        {
            await self.Task.Task;
            self.Task = null;
        }
    }
}