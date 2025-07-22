using System;
using NUnit.Framework;

namespace SH.GameFrame
{
    public interface ICapability : IDisposable
    {
        bool IsActive { get; }
        int TickGroupOrder { get; }
        void Init();
        bool ShouldActivate();
        bool ShouldDeactivate();
        void OnActivated();
        void OnDeactivated();
        void TickActive(float delatTime);
    }
}