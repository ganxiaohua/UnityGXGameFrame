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

        public override void Clear()
        {
            UniTaskCompletionSource.TrySetCanceled();
            base.Clear();
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