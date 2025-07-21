using System.Collections.Generic;
using GameFrame;

namespace SH.GameFrame
{
    public partial class CapabilitySystem : Singleton<CapabilitySystem>
    {
        private GXArray<CapabilityBase>[] bilityCapabilitiesList;

        public void Init()
        {
            int capabilityCount = 50;
            bilityCapabilitiesList = new GXArray<CapabilityBase>[capabilityCount];
        }


        public void Update(float delatTime)
        {
            int count = bilityCapabilitiesList.Length;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = bilityCapabilitiesList[i];
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