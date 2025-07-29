using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    internal static class ComponentsIdidGenerator
    {
        private static int next = 0;

        public static int GetId<T>() where T : EffComponent
        {
            ComponentsID2Type.ComponentsTypes.Add(typeof(T));
            return next++;
        }
    }
    public static class ComponentsID<T> where T : EffComponent
    {
        public static readonly int TID = ComponentsIdidGenerator.GetId<T>();
    }

    public static class ComponentsID2Type
    {
        public static List<Type> ComponentsTypes = new List<Type>(128);

        public static int Count => ComponentsTypes.Count;
    }
}