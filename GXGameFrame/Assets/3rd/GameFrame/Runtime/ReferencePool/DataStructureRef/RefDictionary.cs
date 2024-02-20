using System.Collections.Generic;

namespace GameFrame.DataStructureRef
{
    public class RefDictionary<T, K> : IReference
    {
        public Dictionary<T, K> Dictionary = new Dictionary<T, K>(64);

        public void Clear()
        {
            Dictionary.Clear();
        }
    }
}