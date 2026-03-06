using System;

namespace GameFrame.Runtime
{
    public class GoapState : IDisposable
    {
        public JumpIndexArrayEx<EffComponent> Components;

        public void Dispose()
        {
            Components?.Dispose();
        }
    }
}