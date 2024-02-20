using System.Collections.Generic;

namespace GameFrame.DataStructureRef
{
    public class RefStack<T> : IReference
    {
        public Stack<T> Stack = new Stack<T>(64);
        public void Clear()
        {
            Stack.Clear();
        }
    }
}