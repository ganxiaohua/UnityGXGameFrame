using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public interface ICapability : IDisposable
    {
        int ID { get; }
        SHWorld World { get; }
        EffEntity Owner { get; }
        List<int> TagList{ get; }
        bool IsActive { get; }
        int TickGroupOrder { get; }
        void Init(SHWorld world,EffEntity owner, int id);
        bool ShouldActivate();
        bool ShouldDeactivate();
        void OnActivated();
        void OnDeactivated();
        void TickActive(float delatTime,float realElapseSeconds);
    }
}