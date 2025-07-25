using System.Collections.Generic;
using GameFrame;

namespace GameFrame.Runtime.SH
{
    public partial class Capabilitys
    {
        private GXArray<CapabilityBase>[] capabilitiesUpdateList;
        
        private GXArray<CapabilityBase>[] capabilitiesFixUpdateList;

        public void Init()
        {
            int capabilityCount = 50;
            capabilitiesUpdateList = new GXArray<CapabilityBase>[capabilityCount];
            capabilitiesFixUpdateList = new GXArray<CapabilityBase>[capabilityCount];
        }


        public void OnUpdate(float delatTime)
        {
            int count = capabilitiesUpdateList.Length;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = capabilitiesUpdateList[i];
                UpdateCapability(capabilityArray, delatTime);
            }
        }

        public void OnFixedUpdate(float delatTime)
        {
            int count = capabilitiesFixUpdateList.Length;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = capabilitiesFixUpdateList[i];
                UpdateCapability(capabilityArray, delatTime);
            }
        }

        private void UpdateCapability(GXArray<CapabilityBase> capabilityBaseArray, float delatTime)
        {
            foreach (var capability in capabilityBaseArray)
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
                if(capability.IsActive)
                    capability.TickActive(delatTime);
            }
        }
    }
}