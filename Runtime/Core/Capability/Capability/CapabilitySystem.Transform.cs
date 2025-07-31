using System;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        public void Add<T>(EffEntity player) where T : CapabilityBase
        {
            var capability = ReferencePool.Acquire<T>();
            if (capability.UpdateMode == CapabilitysUpdateMode.Update)
            {
                int id = CapabilityID<T, IUpdateSystem>.TID;
                SetArray(capabilitiesUpdateList, player, id, capability);
            }
            else if (capability.UpdateMode == CapabilitysUpdateMode.FixedUpdate)
            {
                int id = CapabilityID<T, IFixedUpdateSystem>.TID;
                SetArray(capabilitiesFixUpdateList, player, id, capability);
            }
        }

        private void SetArray(JumpIndexArray<CapabilityBase>[] arrays, EffEntity player, int id, CapabilityBase capability)
        {
            var array = arrays[id];
            if (array == null)
            {
                array = new JumpIndexArray<CapabilityBase>();
                array.Init(estimatedNumberPlayer);
                arrays[id] = array;
            }

            var cap = array.Add(player.ID, capability);
            cap.Init(shWorld, player);
        }

        public void Remove(EffEntity player, int capabilitieId)
        {
            RemoveUpdate(player, capabilitieId);
            RemoveFixedUpdate(player, capabilitieId);
        }
        
        private void RemoveUpdate(EffEntity player, int capabilitieId)
        {
            var array = capabilitiesUpdateList[capabilitieId];
            if (array != null)
            {
                RemoveArray(array, player);
            }
        }

        private void RemoveFixedUpdate(EffEntity player, int capabilitieId)
        {
            var array = capabilitiesFixUpdateList[capabilitieId];
            if (array != null)
            {
                RemoveArray(array, player);
            }
        }


        public void RemoveEffEntitysAllCapability(EffEntity player)
        {
            foreach (var array in capabilitiesUpdateList)
            {
                if (array != null)
                {
                    RemoveArray(array, player);
                }
            }
            foreach (var array in capabilitiesFixUpdateList)
            {
                if (array != null)
                {
                    RemoveArray(array, player);
                }
            }
        }
        
        private void RemoveArray(JumpIndexArray<CapabilityBase> array, EffEntity player)
        {
            var capability = array.Remove(player.ID);
            if (capability != null)
                ReferencePool.Release(capability);
        }
    }
}