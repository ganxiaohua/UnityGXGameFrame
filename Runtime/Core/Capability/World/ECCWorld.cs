using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract unsafe partial class ECCWorld : World
    {
        private int maxCapabilityTag;

        private Capabilitys capabilitys;

        private readonly Dictionary<System.Type, CapabilitysUpdateMode> capabilityUpdateModes = new Dictionary<System.Type, CapabilitysUpdateMode>();


        protected void InitCapabilitys(int updateCapabilityCount, int fixedUpdateCapabilityCount, int lateUpdateCapabilityCount, int maxTag,
            int estimatePlayer)
        {
            var scheduler = new Capabilitys();
            scheduler.Init(this, updateCapabilityCount, fixedUpdateCapabilityCount, lateUpdateCapabilityCount, estimatePlayer);
            InitCapabilitys(scheduler, maxTag);
        }

        // 派生世界提供已初始化的调度器，能力生命周期仍由本世界统一管理。
        protected void InitCapabilitys(Capabilitys scheduler, int maxTag)
        {
            maxCapabilityTag = maxTag;
            capabilitys = scheduler;
        }

        protected override void OnEntityCreated(EffEntity child)
        {
            capabilitys.EnsureCapabilityCapacity(child.Id);
            BindCapability<DestroyCapability>(child);
            var component = this.AddComp<CapabilityComponent>(child);
            component->Init(maxCapabilityTag);
        }

        protected override void OnEntityDestroying(EffEntity entity)
        {
            capabilitys.RemoveEffEntitysAllCapability(entity);
        }

        public void SetCapability(EffEntity eff, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate, List<CapabilityBase> LateUpdate)
        {
            RequireEntity(eff);
            capabilitys.SetCapabilityBaseWithPlayer(eff, update, fixedUpdate, LateUpdate);
        }

        public void BindCapability<T>(EffEntity effEntity) where T : CapabilityBase
        {
            RequireEntity(effEntity);
            capabilityUpdateModes[typeof(T)] = capabilitys.Add<T>(effEntity);
        }

        public void UnBindCapability<T>(EffEntity player) where T : CapabilityBase
        {
            if (!this.IsAlive(player)) return;
            if (!capabilityUpdateModes.TryGetValue(typeof(T), out var updateMode))
                return;

            capabilitys.Remove<T>(player, updateMode);
        }


        public bool IsBindCapability(EffEntity player, List<int> tagInts)
        {
            var capabiltyComponent = this.GetComp<CapabilityComponent>(player);
            return capabiltyComponent.IsBlock(tagInts);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            capabilitys.OnUpdate(DeltaTime, realElapseSeconds);
            OnUpdateSystem(DeltaTime, realElapseSeconds);
        }

        public override void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnFixedUpdate(elapseSeconds, realElapseSeconds);
            capabilitys.OnFixedUpdate(FixedDeltaTime, realElapseSeconds);
            OnFixedUpdateSystem(FixedDeltaTime, realElapseSeconds);
        }

        public override void OnLateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnLateUpdate(elapseSeconds, realElapseSeconds);
            capabilitys.OnLateUpdate(DeltaTime, realElapseSeconds);
            OnLateUpdateSystem(DeltaTime, realElapseSeconds);
        }

        protected override void OnDisposeSystems()
        {
            capabilitys.Dispose();
            capabilityUpdateModes.Clear();
            DisposeSystem();
            base.OnDisposeSystems();
        }
    }
}
