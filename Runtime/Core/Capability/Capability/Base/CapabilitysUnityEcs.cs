using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class CapabilitysUnityEcs : Capabilitys
    {
        private JumpIndexArray<CapabilityBase>[] capabilitiesFixUpdateForEndEcsList = Array.Empty<JumpIndexArray<CapabilityBase>>();

        public bool HasFixedUpdateForEndEcsCapabilities
        {
            get
            {
                foreach (var array in capabilitiesFixUpdateForEndEcsList)
                {
                    if (array != null && array.Count > 0)
                        return true;
                }

                return false;
            }
        }

        public void Init(ECCWorld eccWorld, int updateCapabilityCount, int fixedUpdateCapabilityCount, int lateUpdateCapabilityCount,
            int fixedUpdateForEndEcsCapabilityCount, int estimatedNumberPlayer)
        {
            base.Init(eccWorld, updateCapabilityCount, fixedUpdateCapabilityCount, lateUpdateCapabilityCount, estimatedNumberPlayer);
            capabilitiesFixUpdateForEndEcsList = new JumpIndexArray<CapabilityBase>[fixedUpdateForEndEcsCapabilityCount];
        }

        public void OnFixedUpdateForEndEcs(float deltaTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesFixUpdateForEndEcsList, deltaTime, realElapseSeconds);
        }

        protected override JumpIndexArray<CapabilityBase>[] GetCapabilityArray(CapabilitysUpdateMode updateMode)
        {
            return updateMode == CapabilitysUpdateMode.FixedUpdateForEndEcs
                ? capabilitiesFixUpdateForEndEcsList
                : base.GetCapabilityArray(updateMode);
        }

        protected override void ExpandCapabilityArrays(int capacity)
        {
            base.ExpandCapabilityArrays(capacity);
            ExpandCapabilityArrays(capabilitiesFixUpdateForEndEcsList, capacity);
        }

        public override void SetCapabilityBaseWithPlayer(EffEntity player, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate,
            List<CapabilityBase> lateUpdate, List<CapabilityBase> fixedUpdateForEndEcs)
        {
            base.SetCapabilityBaseWithPlayer(player, update, fixedUpdate, lateUpdate, fixedUpdateForEndEcs);
            foreach (var array in capabilitiesFixUpdateForEndEcsList)
            {
                if (array == null || (uint) player.Id >= (uint) array.AllCount)
                    continue;

                var capability = array[player.Id];
                if (capability != null && capability.Owner == player)
                    fixedUpdateForEndEcs.Add(capability);
            }
        }

        public override void RemoveEffEntitysAllCapability(EffEntity player)
        {
            base.RemoveEffEntitysAllCapability(player);
            foreach (var array in capabilitiesFixUpdateForEndEcsList)
            {
                if (array != null)
                    RemoveArray(array, player);
            }
        }

        public override void Dispose()
        {
            ClearCapabilities(capabilitiesFixUpdateForEndEcsList);
            capabilitiesFixUpdateForEndEcsList = Array.Empty<JumpIndexArray<CapabilityBase>>();
            base.Dispose();
        }
    }
}
