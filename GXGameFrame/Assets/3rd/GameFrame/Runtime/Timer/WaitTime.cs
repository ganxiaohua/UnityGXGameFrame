using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFrame.Runtime.Timer
{
    public class WaitTime : IDisposable
    {
        private int versions;

        public async UniTask WaitSec(float sec, CancellationToken cancellationToken = default)
        {
            int ver = ++versions;
            await UniTask.Delay((int) (sec * 1000), false, PlayerLoopTiming.Update, cancellationToken);
            if (ver != versions)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
        
        public async UniTask WaitFrame(float sec, CancellationToken cancellationToken = default)
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