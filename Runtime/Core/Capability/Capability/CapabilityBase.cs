using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract class CapabilityBase : ICapability
    {
        protected List<int> tagList;
        public IReadOnlyList<int> Taglist => tagList;
        public int ID { get; private set; }
        public int OwnerId { get; private set; }
        public bool IsActive { get; private set; }
        public int TickGroupOrder { get; protected set; }

        public virtual void Init(int ownerId,int id)
        {
            OwnerId = ownerId;
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

        public abstract void Dispose();
    }
}