using System;

namespace GameFrame.Runtime
{
    public static class ComponentDisposeAction
    {
        public static Action<World, int>[] ComponentDisposeActions;
    }
}