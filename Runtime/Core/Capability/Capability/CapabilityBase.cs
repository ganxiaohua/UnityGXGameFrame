using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract class CapabilityBase : ICapability
    {
        protected List<int> tagList;
        public IReadOnlyList<int> Taglist => tagList;
        public int ID { get; private set; }
        public EffEntity Owner { get; private set; }
        public bool IsActive { get; private set; }
        public int TickGroupOrder { get; protected set; }

        public virtual void Init(EffEntity owner, int id)
        {
            Owner = owner;
            ID = id;
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

        public abstract void TickActive(float delatTime);

        public virtual void Dispose()
        {
            IsActive = false;
            tagList = null;
            Owner = null;
        }
    }
}