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
        public bool IsActive { get; internal set; }
        public virtual CapabilitysUpdateMode UpdateMode { get; protected set; } = CapabilitysUpdateMode.Update;
        public virtual int TickGroupOrder { get; protected set; }

        protected CapabilityCollector CapabilityCollector;

        internal bool ComponentChanges;

        internal bool TryComponentChanges
        {
            get
            {
                bool b = ComponentChanges;
                if (CapabilityCollector != null)
                    ComponentChanges = false;
                return b;
            }
        }

        public void Init(int id, ECCWorld world, EffEntity owner)
        {
            ID = id;
            World = world;
            Owner = owner;
            ComponentChanges = true;
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        protected void Filter(params int[] indexs)
        {
            Assert.IsNotNull(indexs, $"{this.GetType().Name} indexs is null ");
            CapabilityCollector = CapabilityCollector.CreateCollector(World, this, indexs);
            ComponentChanges = false;
        }


        public abstract bool ShouldActivate();
        public abstract bool ShouldDeactivate();

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
            if (CapabilityCollector != null)
                CapabilityCollector.Release(CapabilityCollector);
            CapabilityCollector = null;
            IsActive = false;
            TagList = null;
            Owner = null;
            World = null;
        }
    }
}