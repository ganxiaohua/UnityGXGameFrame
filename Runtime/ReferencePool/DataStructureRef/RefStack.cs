using System;
using System.Collections.Generic;

namespace GameFrame.Runtime.DataStructureRef
{
    public class RefStack<T> : IDisposable
    {
        public Stack<T> Stack = new Stack<T>(64);
        public void Dispose()
        {
            Stack.Clear();
        }
    }
}