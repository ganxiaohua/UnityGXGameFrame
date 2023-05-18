using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public static class WaitComponentSystem
    {
        [SystemBind]
        public class WaitComponentStartSystem : StartSystem<WaitComponent, Type>
        {
            protected override void Start(WaitComponent self, Type type)
            {
                self.WaitType = type;
            }
        }

        [SystemBind]
        public class WaitComponentClearSystem : ClearSystem<WaitComponent>
        {
            protected override void Clear(WaitComponent self)
            {
                WaitTask.Notify(self.WaitType);
            }
        }

        public static async UniTask Wait(this WaitComponent self)
        {
            await WaitTask.Wait(self.WaitType);
        }
    }
}