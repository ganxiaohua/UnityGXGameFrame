using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public static class WaitComponentSystem
    {
        [SystemBind]
        public class WaitComponentStartSystem : StartSystem<WaitComponent>
        {
            protected override void Start(WaitComponent self)
            {
                self.UniTaskCompletionSource = new UniTaskCompletionSource();
            }
        }

        [SystemBind]
        public class WaitComponentClearSystem : ClearSystem<WaitComponent>
        {
            protected override void Clear(WaitComponent self)
            {
                self.UniTaskCompletionSource.TrySetCanceled();
            }
        }
        
        public static void WaitOver(this WaitComponent self)
        {
            self.UniTaskCompletionSource.TrySetResult();
        }

        public static async UniTask<bool> Wait(this WaitComponent self)
        {
            await self.UniTaskCompletionSource.Task.SuppressCancellationThrow();
            if (self.UniTaskCompletionSource.GetStatus(0) == UniTaskStatus.Succeeded)
            {
                return true;
            }
            return false;
        }
    }
}