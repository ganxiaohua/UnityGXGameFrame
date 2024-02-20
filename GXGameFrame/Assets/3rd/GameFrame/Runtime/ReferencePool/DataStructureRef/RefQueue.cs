using System.Collections.Generic;

namespace GameFrame.DataStructureRef
{
    public class RefQueue<T> :IReference
    {
        public Queue<T> Queue = new Queue<T>(64);
        public void Clear()
        {
            Queue.Clear();
        }
    }
    
}