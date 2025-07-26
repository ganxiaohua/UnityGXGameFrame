using System;

namespace GameFrame.Runtime.SH
{
    public interface ICapability : IDisposable
    {
        int ID { get; }
        bool IsActive { get; }
        int TickGroupOrder { get; }
        void Init(int id);
        bool ShouldActivate();
        bool ShouldDeactivate();
        void OnActivated();
        void OnDeactivated();
        void TickActive(float delatTime);
    }
}