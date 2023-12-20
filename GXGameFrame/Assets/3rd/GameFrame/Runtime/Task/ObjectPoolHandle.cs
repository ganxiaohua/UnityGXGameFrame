using System;

namespace GameFrame
{
    public class ObjectPoolHandle : IHandle, IReference
    {
        public TaskState TaskState { get; set; }
        public bool IsDone => TaskState == TaskState.Succ || TaskState == TaskState.Fail;

        public bool IsCancell => Token.IsCancellationRequested;
        public Action AsyncStateMoveNext { get; set; }


        private System.Threading.CancellationToken Token;

        // public T Target{ get; private set; }

        private AwaiterTask<ObjectPoolHandle> AwaiterTask;

        public AwaiterTask<ObjectPoolHandle> GetAwaiter()
        {
            AwaiterTask = ReferencePool.Acquire<AwaiterTask<ObjectPoolHandle>>();
            AwaiterTask.SetTask(this);
            return AwaiterTask;
        }

        public void Init(System.Threading.CancellationToken token)
        {
            TaskState = TaskState.Ing;
            Token = token;
        }

        public void Complete()
        {
            if (Token != default && IsCancell)
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

        public void Clear()
        {
            TaskState = TaskState.None;
            AsyncStateMoveNext -= AsyncStateMoveNext;
            Token = default;
            if (AwaiterTask != null)
                ReferencePool.Release(AwaiterTask);
        }
    }
}