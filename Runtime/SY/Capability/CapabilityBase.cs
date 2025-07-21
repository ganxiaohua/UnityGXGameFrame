namespace SH.GameFrame
{
    public abstract class CapabilityBase : ICapability
    {
        public bool IsActive { get; private set; }
        public int TickGroupOrder { get; protected set; }

        public abstract void Init();
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