using System;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 状态任务机和普通状态机的区别就是可以等待,执行任务使用
    /// </summary>
    public class FsmTaskController : FsmController, IHandle
    {
        private System.Threading.CancellationToken token;
        public TaskState TaskState { get; set; }
        public bool IsDone => TaskState == TaskState.Succ || TaskState == TaskState.Fail;
        public bool IsCancel => token.IsCancellationRequested;
        public Action AsyncStateMoveNext { get; set; }

        private Type completeStateType;


        private AwaiterTask<FsmTaskController> awaiterTask;

        public AwaiterTask<FsmTaskController> GetAwaiter()
        {
            awaiterTask = ReferencePool.Acquire<AwaiterTask<FsmTaskController>>();
            awaiterTask.SetTask(this);
            return awaiterTask;
        }

        public void SetCompleteTypes(Type type, System.Threading.CancellationToken token = default)
        {
            TaskState = TaskState.Ing;
            completeStateType = type;
            this.token = token;
        }

        public override void ChangeState<T>()
        {
            base.ChangeState<T>();
            if (typeof(T) == completeStateType)
            {
                Complete();
            }
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

        public override void Dispose()
        {
            base.Dispose();
            TaskState = TaskState.None;
            AsyncStateMoveNext -= AsyncStateMoveNext;
            token = default;
            if (awaiterTask != null)
                ReferencePool.Release(awaiterTask);
        }
    }
}