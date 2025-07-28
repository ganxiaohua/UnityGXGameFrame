using System;
using System.Collections.Generic;

namespace GameFrame.Runtime.DataStructureRef
{
    public class RefQueue<T> :IDisposable
    {
        public Queue<T> Queue = new Queue<T>(64);
        public void Dispose()
        {
            Queue.Clear();
        }
    }
    
}