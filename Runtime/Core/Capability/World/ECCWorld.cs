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
            maxCapabilityTag = maxTag;
            capabilitys = new Capabilitys();
            capabilitys.Init(this, updateCapabilityCount, fixedUpdateCapabilityCount, lateUpdateCapabilityCount, estimatePlayer);
        }

        public override EffEntity AddChild()
        {
            var child = base.AddChild();
            BindCapability<DestroyCapability>(child);
            var capabiltyComponet = child.AddComponent<CapabilityComponent>();
            capabiltyComponet->Init(maxCapabilityTag);
            return child;
        }

        public override void RemoveChild(EffEntity effEntity)
        {
            capabilitys.RemoveEffEntitysAllCapability(effEntity);
            base.RemoveChild(effEntity);
        }

        public void SetCapability(EffEntity eff, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate, List<CapabilityBase> LateUpdate)
        {
            capabilitys.SetCapabilityBaseWithPlayer(eff, update, fixedUpdate, LateUpdate);
        }

        public void BindCapability<T>(EffEntity effEntity) where T : CapabilityBase
        {
            Assert.IsNotNull(effEntity, $"not have {effEntity.Name} ecsentity");
            capabilityUpdateModes[typeof(T)] = capabilitys.Add<T>(effEntity);
        }

        public void UnBindCapability<T>(EffEntity player) where T : CapabilityBase
        {
            if (!capabilityUpdateModes.TryGetValue(typeof(T), out var updateMode))
                return;

            int id;
            switch (updateMode)
            {
                case CapabilitysUpdateMode.FixedUpdate:
                    id = CapabilityID<T, IFixedUpdateSystem>.TID;
                    break;
                case CapabilitysUpdateMode.LateUpdate:
                    id = CapabilityID<T, ILateUpdateSystem>.TID;
                    break;
                default:
                    id = CapabilityID<T, IUpdateSystem>.TID;
                    break;
            }

            capabilitys.Remove(player, id, updateMode);
        }


        public bool IsBindCapability(EffEntity player, List<int> tagInts)
        {
            var capabiltyComponent = player.GetComponent<CapabilityComponent>();
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

        public override void Dispose()
        {
            capabilitys.Dispose();
            capabilityUpdateModes.Clear();
            DisposeSystem();
            base.Dispose();
        }
    }
}
