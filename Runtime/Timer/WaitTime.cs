using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFrame.Runtime.Runtime.Timer
{
    public class WaitTime : IDisposable
    {
        private int versions;

        public async UniTask WaitSecs(float sec, CancellationToken cancellationToken = default)
        {
            try
            {
                await Wait(sec, cancellationToken);
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                    Debugger.LogError(e);
            }
        }

        private async UniTask Wait(float sec, CancellationToken cancellationToken = default)
        {
            int ver = ++versions;
            await UniTask.Delay((int) (sec * 1000), false, PlayerLoopTiming.Update, cancellationToken);
            if (ver != versions)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }


        public async UniTask WaitFrame(CancellationToken cancellationToken = default)
        {
            try
            {
                await Wait(cancellationToken);
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                    Debugger.LogError(e);
            }
        }

        private async UniTask Wait(CancellationToken cancellationToken = default)
        {
            int ver = ++versions;
            await UniTask.Yield();
            if (ver != versions)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        public void Dispose()
        {
            versions++;
        }
    }
}