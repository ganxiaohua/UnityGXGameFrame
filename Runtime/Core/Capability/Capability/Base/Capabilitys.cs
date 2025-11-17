using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        private JumpIndexArray<CapabilityBase>[] capabilitiesUpdateList;

        private JumpIndexArray<CapabilityBase>[] capabilitiesFixUpdateList;

        private int estimatedNumberPlayer;

        private ECCWorld eccWorld;

        public void Init(ECCWorld eccWorld, int capabilityCount, int estimatedNumberPlayer)
        {
            this.eccWorld = eccWorld;
            this.estimatedNumberPlayer = estimatedNumberPlayer;
            capabilitiesUpdateList = new JumpIndexArray<CapabilityBase>[capabilityCount];
            capabilitiesFixUpdateList = new JumpIndexArray<CapabilityBase>[capabilityCount];
        }

        public void OnUpdate(float delatTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesUpdateList, delatTime, realElapseSeconds);
        }

        public void OnFixedUpdate(float delatTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesFixUpdateList, delatTime, realElapseSeconds);
        }

        private void ConvenientCapabilitys(JumpIndexArray<CapabilityBase>[] arrays, float delatTime, float realElapseSeconds)
        {
            int count = arrays.Length;
            if (count == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = arrays[i];
                if (capabilityArray == null)
                    continue;
#if UNITY_EDITOR
                using (new Profiler(CapabilityID2Type.CapabilitysTyps[i].Name))
#endif
                    UpdateCapability(capabilityArray, delatTime, realElapseSeconds);
            }
        }

        private void UpdateCapability(JumpIndexArray<CapabilityBase> capabilityBaseArrayEx, float delatTime, float realElapseSeconds)
        {
            foreach (var capability in capabilityBaseArrayEx)
            {
                var owner = capability.Owner;
                if (capability.TagList != null)
                {
                    var capailty = (CapabiltyComponent) owner.GetComponent(ComponentsID<CapabiltyComponent>.TID);
                    if (capailty.IsBlock(capability.TagList))
                        continue;
                }

                if (capability.TryComponentChanges)
                {
                    if (!capability.IsActive)
                    {
                        bool succ = capability.ShouldActivate();
                        if (succ)
                        {
                            capability.OnActivated();
                        }
                    }
                    else
                    {
                        bool succ = capability.ShouldDeactivate();
                        if (succ)
                        {
                            capability.OnDeactivated();
                        }
                    }
                }

                if (capability.IsActive)
                    capability.TickActive(delatTime, realElapseSeconds);
            }
        }

        public void Dispose()
        {
            ClearCapabilities(capabilitiesUpdateList);
            ClearCapabilities(capabilitiesFixUpdateList);
            capabilitiesUpdateList = null;
            capabilitiesFixUpdateList = null;
        }

        private void ClearCapabilities(JumpIndexArray<CapabilityBase>[] arrays)
        {
            foreach (var array in arrays)
            {
                if (array == null)
                    continue;
                foreach (var item in array)
                {
                    ReferencePool.Release(item);
                }

                array.Dispose();
            }
        }
    }
}