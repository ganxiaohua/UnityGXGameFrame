using System;

namespace GameFrame.Runtime
{
    public interface ICapability : IDisposable
    {
        int ID { get; }
        int OwnerId { get; }
        bool IsActive { get; }
        int TickGroupOrder { get; }
        void Init(int ownerId, int id);
        bool ShouldActivate();
        bool ShouldDeactivate();
        void OnActivated();
        void OnDeactivated();
        void TickActive(float delatTime);
    }
}