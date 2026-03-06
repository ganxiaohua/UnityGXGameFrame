using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public struct ComponentArray<T> where T : struct, EffComponent
    {
        private IntPtr[] Ptrs;
        public DestroyComp[] Array;

        public void Init(int size)
        {
            // Array = new T[size];
            Ptrs = new IntPtr[100];
            unsafe
            {
                fixed (DestroyComp* arrayPtr = Array)
                {
                    Ptrs[0] = (IntPtr) arrayPtr;
                }
            }
        }
    }
}