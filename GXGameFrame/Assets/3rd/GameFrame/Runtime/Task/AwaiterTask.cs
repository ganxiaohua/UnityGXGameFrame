using System;
using System.Runtime.CompilerServices;

namespace GameFrame
{
    public class AwaiterTask<T> : INotifyCompletion,IReference where T : IHandle
    {
        private  T m_Task;
        
        public bool IsCompleted => m_Task?.IsDone ?? false;
        

        public void SetTask(T task)
        {
            this.m_Task = task;
        }

        public T GetResult()
        {
            return m_Task;
        }
        
        public void OnCompleted(Action continuation)
        {
            if (m_Task != null)
            {
                m_Task.AsyncStateMoveNext += continuation;
            }
           
        }

        public void Clear()
        {
            m_Task = default;
        }
    }
}