using System;
using System.Collections.Generic;

namespace GameFrame
{
    internal static class ComponentsIdidGenerator<T> where T : ECSComponent
    {
        private static int next = 0;
        public static int Next => GetId();

        private static int GetId()
        {
            ComponentsID2Type.ComponentsTypes.Add(typeof(T));
            return next++;
        }
    }

    public static class ComponentsID<T> where T : ECSComponent
    {
        public static readonly int TID = ComponentsIdidGenerator<T>.Next;
    }

    public static class ComponentsID2Type
    {
        public static List<Type> ComponentsTypes = new List<Type>();

        public static int Count => ComponentsTypes.Count;
    }

}