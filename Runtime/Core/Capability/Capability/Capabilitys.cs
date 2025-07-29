using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        private GXArray<CapabilityBase>[] capabilitiesUpdateList;

        private GXArray<CapabilityBase>[] capabilitiesFixUpdateList;

        private int estimatedNumberPlayer;

        private SHWorld shWorld;

        public void Init(SHWorld shWorld, int capabilityCount, int estimatedNumberPlayer)
        {
            this.shWorld = shWorld;
            this.estimatedNumberPlayer = estimatedNumberPlayer;
            capabilitiesUpdateList = new GXArray<CapabilityBase>[capabilityCount];
            capabilitiesFixUpdateList = new GXArray<CapabilityBase>[capabilityCount];
        }

        public void OnUpdate(float delatTime)
        {
            ConvenientCapabilitys(capabilitiesUpdateList, delatTime);
        }

        public void OnFixedUpdate(float delatTime)
        {
            ConvenientCapabilitys(capabilitiesFixUpdateList, delatTime);
        }

        private void ConvenientCapabilitys(GXArray<CapabilityBase>[] arrays, float delatTime)
        {
            int count = arrays.Length;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = arrays[i];
                if (capabilityArray == null)
                    continue;
#if UNITY_EDITOR
                using (new Profiler(CapabilityID2Type.CapabilitysTyps[i].Name))
#endif
                    UpdateCapability(capabilityArray, delatTime);
            }
        }

        private void UpdateCapability(GXArray<CapabilityBase> capabilityBaseArray, float delatTime)
        {
            foreach (var capability in capabilityBaseArray)
            {
                var owner = capability.Owner;
                var capailty = (CapabiltyComponent) owner.GetComponent(ComponentsID<CapabiltyComponent>.TID);
                if (capability.Taglist != null && capailty.IsBlock(capability.Taglist))
                {
                    continue;
                }

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

                if (capability.IsActive)
                    capability.TickActive(delatTime);
            }
        }

        public void Dispose()
        {
            ClearCapabilities(capabilitiesUpdateList);
            ClearCapabilities(capabilitiesFixUpdateList);
            capabilitiesUpdateList = null;
            capabilitiesFixUpdateList = null;
        }

        private void ClearCapabilities(GXArray<CapabilityBase>[] arrays)
        {
            foreach (var array in arrays)
            {
                if (array == null)
                    continue;
                foreach (var item in array)
                {
                    item.OnDeactivated();
                }

                array.Dispose();
            }
        }
    }
}