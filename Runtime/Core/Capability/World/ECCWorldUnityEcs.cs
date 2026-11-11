using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public abstract partial class ECCWorldUnityEcs : ECCWorld, IFixedUpdateForEndEcsSystem
    {
        private CapabilitysUnityEcs capabilitysUnityEcs;

        public bool HasFixedUpdateForEndEcsCapabilities => capabilitysUnityEcs.HasFixedUpdateForEndEcsCapabilities;

        protected void InitCapabilitys(int updateCapabilityCount, int fixedUpdateCapabilityCount, int lateUpdateCapabilityCount,
            int fixedUpdateForEndEcsCapabilityCount, int maxTag, int estimatePlayer)
        {
            capabilitysUnityEcs = new CapabilitysUnityEcs();
            capabilitysUnityEcs.Init(this, updateCapabilityCount, fixedUpdateCapabilityCount, lateUpdateCapabilityCount,
                fixedUpdateForEndEcsCapabilityCount, estimatePlayer);
            base.InitCapabilitys(capabilitysUnityEcs, maxTag);
        }

        public void SetCapability(EffEntity eff, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate,
            List<CapabilityBase> lateUpdate, List<CapabilityBase> fixedUpdateForEndEcs)
        {
            RequireEntity(eff);
            capabilitysUnityEcs.SetCapabilityBaseWithPlayer(eff, update, fixedUpdate, lateUpdate, fixedUpdateForEndEcs);
        }

        public virtual void OnFixedUpdateForEndEcs(float elapseSeconds, float realElapseSeconds)
        {
            // 使用前置阶段已经缩放的当步时间，不再次推进世界或运行普通固定步系统。
            capabilitysUnityEcs.OnFixedUpdateForEndEcs(FixedDeltaTime, realElapseSeconds);
        }
    }
}
