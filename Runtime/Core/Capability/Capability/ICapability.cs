using System;

namespace GameFrame.Runtime
{
    public interface ICapability : IDisposable
    {
        int ID { get; }
        EffEntity Owner { get; }
        bool IsActive { get; }
        int TickGroupOrder { get; }
        void Init(EffEntity owner, int id);
        bool ShouldActivate();
        bool ShouldDeactivate();
        void OnActivated();
        void OnDeactivated();
        void TickActive(float delatTime);
    }
}