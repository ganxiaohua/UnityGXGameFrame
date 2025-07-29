using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    internal static class CapabilityIDGenerator<TUpdateMode>
    {
        private static int next = 0;
        
        public static int GetId<T>(){
            CapabilityID2Type.CapabilitysTyps.Add(typeof(T));
            return next++;
        }
    }

    public static class CapabilityID<T,TUpdateMode> 
    {
        public static readonly int TID = CapabilityIDGenerator<TUpdateMode>.GetId<T>();
    }
    
    public static class CapabilityID2Type
    {
        public static List<Type> CapabilitysTyps = new List<Type>(128);

        public static int Count => CapabilitysTyps.Count;
    }
    
}