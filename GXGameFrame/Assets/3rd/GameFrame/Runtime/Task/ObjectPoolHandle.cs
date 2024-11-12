using System;

namespace GameFrame
{
    public class ObjectPoolHandle : IHandle, IDisposable
    {
        public TaskState TaskState { get; set; }
        public bool IsDone => TaskState == TaskState.Succ || TaskState == TaskState.Fail;

        public bool IsCancel => token.IsCancellationRequested;
        public Action AsyncStateMoveNext { get; set; }


        private System.Threading.CancellationToken token;
        

        private AwaiterTask<ObjectPoolHandle> awaiterTask;

        public AwaiterTask<ObjectPoolHandle> GetAwaiter()
        {
            awaiterTask = ReferencePool.Acquire<AwaiterTask<ObjectPoolHandle>>();
            awaiterTask.SetTask(this);
            return awaiterTask;
        }

        public void Init(System.Threading.CancellationToken token)
        {
            TaskState = TaskState.Ing;
            this.token = token;
        }

        public void Complete()
        {
            if (token != default && IsCancel)
            {
                Cancel();
                return;
            }
            TaskState = TaskState.Succ;
            AsyncStateMoveNext?.Invoke();
        }

        public void Cancel()
        {
            TaskState = TaskState.Fail;
            AsyncStateMoveNext?.Invoke();
        }

        public void Dispose()
        {
            TaskState = TaskState.None;
            AsyncStateMoveNext -= AsyncStateMoveNext;
            token = default;
            if (awaiterTask != null)
                ReferencePool.Release(awaiterTask);
        }
    }
}