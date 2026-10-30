using System;

namespace GameFrame.Runtime
{
    public unsafe delegate void ComponentDisposer(void* component);

    public static unsafe class ComponentDisposeAction
    {
        public static ComponentDisposer[] ComponentDisposeActions;

        internal static void Register<T>(int id, int capacity) where T : unmanaged, EffComponent
        {
            if (ComponentDisposeActions == null) ComponentDisposeActions = new ComponentDisposer[capacity];
            else if (ComponentDisposeActions.Length < capacity) Array.Resize(ref ComponentDisposeActions, capacity);
            ComponentDisposeActions[id] = Dispose<T>;
        }

        private static void Dispose<T>(void* component) where T : unmanaged, EffComponent => ((T*)component)->Dispose();
    }
}
