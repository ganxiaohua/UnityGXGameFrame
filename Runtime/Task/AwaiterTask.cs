using System;
using System.Runtime.CompilerServices;

namespace GameFrame
{
    public class AwaiterTask<T> : INotifyCompletion,IDisposable where T : IHandle
    {
        private  T task;
        
        public bool IsCompleted => task?.IsDone ?? false;
        

        public void SetTask(T task)
        {
            this.task = task;
        }

        public T GetResult()
        {
            return task;
        }
        
        public void OnCompleted(Action continuation)
        {
            if (task != null)
            {
                task.AsyncStateMoveNext += continuation;
            }
           
        }

        public void Dispose()
        {
            task = default;
        }
    }
}