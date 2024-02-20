using System.Collections.Generic;

namespace GameFrame.DataStructureRef
{
    public class RefList<T> : IReference
    {
        public List<T> List = new List<T>(64);
        public void Clear()
        {
            List.Clear();
        }
    }
}