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
            return World.HasComp<DestroyComp>(Owner);
        }

        public override void OnActivated()
        {
            base.OnActivated();
            World.DestroyEntity(Owner);
        }

        public override bool ShouldDeactivate()
        {
            return World.HasComp<DestroyComp>(Owner);
        }

        public override void TickActive(float delatTime, float realElapseSeconds)
        {
        }
    }
}