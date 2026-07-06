using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract unsafe partial class ECCWorld : World
    {
        private int maxCapabilityTag;

        private Capabilitys capabilitys;


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

        public void GetCapability(EffEntity eff, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate, List<CapabilityBase> LateUpdate)
        {
            capabilitys.GetCapabilityBaseWithPlayer(eff, update, fixedUpdate, LateUpdate);
        }

        public void BindCapability<T>(EffEntity effEntity) where T : CapabilityBase
        {
            Assert.IsNotNull(effEntity, $"not have {effEntity.Name} ecsentity");
            capabilitys.Add<T>(effEntity);
        }

        public void UnBindCapability<T>(EffEntity player) where T : CapabilityBase
        {
            var capability = ReferencePool.Acquire<T>();
            int id;
            var updateMode = capability.UpdateMode;
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

            ReferencePool.Release(capability);
            capabilitys.Remove(player, id, updateMode);
        }

        public void UnBindCapability(EffEntity player, int capabilitiyId)
        {
            capabilitys.Remove(player, capabilitiyId);
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
            DisposeSystem();
            base.Dispose();
        }
    }
}
