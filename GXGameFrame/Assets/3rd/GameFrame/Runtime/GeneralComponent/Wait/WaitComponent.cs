using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class WaitComponent : Entity, IStartSystem
    {
        public UniTaskCompletionSource UniTaskCompletionSource;

        public void Start()
        {
            UniTaskCompletionSource = new UniTaskCompletionSource();
        }

        public override void Dispose()
        {
            UniTaskCompletionSource.TrySetCanceled();
            base.Dispose();
        }

        public void WaitOver()
        {
            UniTaskCompletionSource.TrySetResult();
        }

        public async UniTask<bool> Wait()
        {
            await UniTaskCompletionSource.Task.SuppressCancellationThrow();
            return UniTaskCompletionSource.GetStatus(0) == UniTaskStatus.Succeeded;
        }
    }
}