namespace GameFrame.Runtime
{
    public abstract class SHWorld : World, IFixedUpdateSystem
    {
        private int maxCapabilityCount;

        private int maxCapabilityTag;
        private Capabilitys capabilitys;


        public void InitCapabilitys(int maxCapabilityCount, int maxTag, int estimatePlayer)
        {
            maxCapabilityTag = maxTag;
            capabilitys = new Capabilitys();
            capabilitys.Init(this, maxCapabilityCount, estimatePlayer);
        }

        public override EffEntity AddChild()
        {
            var child = base.AddChild();
            var capabiltyComponet = child.AddComponent<CapabiltyComponent>();
            capabiltyComponet.Init(maxCapabilityTag);
            return child;
        }

        public void BindCapabilityUpdate<T>(EffEntity effEntity) where T : CapabilityBase
        {
            BindCapability<T>(effEntity, CapabilitysUpdateMode.Update);
        }

        public void BindCapabilityUFixedUpdate<T>(EffEntity effEntity) where T : CapabilityBase
        {
            BindCapability<T>(effEntity, CapabilitysUpdateMode.FixedUpdate);
        }

        private void BindCapability<T>(EffEntity effEntity, CapabilitysUpdateMode mode) where T : CapabilityBase
        {
            Assert.IsNotNull(effEntity, $"not have {effEntity.Name} ecsentity");
            capabilitys.Add<T>(effEntity, mode);
        }

        public void UnBindCapability<T>(EffEntity player) where T : CapabilityBase
        {
            int id = CapabilityID<T, IUpdateSystem>.TID;
            UnBindCapability(player, id);
        }

        public void UnBindCapability(EffEntity player, int capabilitieId)
        {
            capabilitys.Remove(player, capabilitieId);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            capabilitys.OnUpdate(DeltaTime,realElapseSeconds);
        }

        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            capabilitys.OnFixedUpdate(elapseSeconds * Multiple,realElapseSeconds);
        }

        public override void Dispose()
        {
            capabilitys.Dispose();
            base.Dispose();
        }
    }
}