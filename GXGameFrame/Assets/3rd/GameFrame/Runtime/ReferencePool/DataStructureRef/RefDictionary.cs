using System;
using System.Collections.Generic;

namespace GameFrame.DataStructureRef
{
    public class RefDictionary<T, K> : IDisposable
    {
        public Dictionary<T, K> Dictionary = new Dictionary<T, K>(64);

        public void Dispose()
        {
            Dictionary.Clear();
        }
    }
}