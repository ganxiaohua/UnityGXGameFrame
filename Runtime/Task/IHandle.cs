using System;

namespace GameFrame.Runtime
{
    public enum TaskState
    {
        None,
        Ing,
        Fail,
        Succ,
    }

    public interface IHandle
    {
        TaskState TaskState {  get; }
        bool IsDone { get; }
        Action AsyncStateMoveNext { get; set; }

        void Complete();

        void Cancel();
    }
}