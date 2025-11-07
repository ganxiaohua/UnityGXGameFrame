using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract class CapabilityBase : IDisposable
    {
        public int ID { get; private set; }
        public List<int> TagList { get; protected set; }
        public ECCWorld World { get; private set; }
        public EffEntity Owner { get; private set; }
        public bool IsActive { get; private set; }
        public virtual CapabilitysUpdateMode UpdateMode { get; protected set; } = CapabilitysUpdateMode.Update;
        public virtual int TickGroupOrder { get; protected set; }

        public void Init(int id, ECCWorld world, EffEntity owner)
        {
            ID = id;
            World = world;
            Owner = owner;
            OnInit();
        }

        protected virtual void OnInit()
        {
        }


        public abstract bool ShouldActivate();
        public abstract bool ShouldDeactivate();

        public virtual void OnBlock(bool block)
        {
        }

        public virtual void OnActivated()
        {
            IsActive = true;
        }

        public virtual void OnDeactivated()
        {
            IsActive = false;
        }

        public virtual void TickActive(float delatTime, float realElapseSeconds)
        {
        }

        public virtual void Dispose()
        {
            IsActive = false;
            TagList = null;
            Owner = null;
            World = null;
        }
    }
}