namespace GameFrame.Runtime
{
    public class DestroyCapability : CapabilityBase
    {
        public override int TickGroupOrder { get; protected set; } = int.MaxValue;

        protected override void OnInit()
        {
            Filter(ComponentsID<DestroyComp>.TID);
        }

        public override bool ShouldActivate()
        {
            return Owner.GetComponent(ComponentsID<DestroyComp>.TID) != null;
        }

        public override void OnActivated()
        {
            base.OnActivated();
            World.RemoveChild(Owner);
        }

        public override bool ShouldDeactivate()
        {
            return Owner.GetComponent(ComponentsID<DestroyComp>.TID) == null;
        }

        public override void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}