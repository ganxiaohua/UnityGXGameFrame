using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        public CapabilitysUpdateMode Add<T>(EffEntity player) where T : CapabilityBase
        {
            var capability = ReferencePool.Acquire<T>();
            var updateMode = capability.UpdateMode;
            if (updateMode == CapabilitysUpdateMode.Update)
            {
                int id = CapabilityID<T, IUpdateSystem>.TID;
                SetArray(capabilitiesUpdateList, player, id, capability);
            }
            else if (updateMode == CapabilitysUpdateMode.FixedUpdate)
            {
                int id = CapabilityID<T, IFixedUpdateSystem>.TID;
                SetArray(capabilitiesFixUpdateList, player, id, capability);
            }
            else if (updateMode == CapabilitysUpdateMode.LateUpdate)
            {
                int id = CapabilityID<T, ILateUpdateSystem>.TID;
                SetArray(capabilitiesLateUpdateList, player, id, capability);
            }

            return updateMode;
        }

        private void SetArray(JumpIndexArray<CapabilityBase>[] arrays, EffEntity player, int id, CapabilityBase capability)
        {
            if ((uint) id >= (uint) arrays.Length)
                throw new System.IndexOutOfRangeException($"{capability.GetType().Name} capability id {id} is outside {capability.UpdateMode} array length {arrays.Length}.");

            var array = arrays[id];
            if (array == null)
            {
                array = new JumpIndexArray<CapabilityBase>();
                array.Init(estimatedNumberPlayer);
                arrays[id] = array;
            }

            var cap = array.Set(player.ID, capability);
            cap.Init(id, eccWorld, player);
        }

        public void SetCapabilityBaseWithPlayer(EffEntity player, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate,
            List<CapabilityBase> lateUpdate)
        {
            void Get(EffEntity player, JumpIndexArray<CapabilityBase>[] scr, List<CapabilityBase> dst)
            {
                foreach (var capArray in scr)
                {
                    if (capArray == null)
                    {
                        continue;
                    }

                    if (capArray[player.ID] != null)
                        dst.Add(capArray[player.ID]);
                }
            }

            Get(player, capabilitiesUpdateList, update);
            Get(player, capabilitiesFixUpdateList, fixedUpdate);
            Get(player, capabilitiesLateUpdateList, lateUpdate);
        }

        public void Remove(EffEntity player, int capabilitieId, CapabilitysUpdateMode updateMode)
        {
            switch (updateMode)
            {
                case CapabilitysUpdateMode.FixedUpdate:
                    RemoveFixedUpdate(player, capabilitieId);
                    break;
                case CapabilitysUpdateMode.LateUpdate:
                    RemoveLatedUpdate(player, capabilitieId);
                    break;
                default:
                    RemoveUpdate(player, capabilitieId);
                    break;
            }
        }

        private void RemoveUpdate(EffEntity player, int capabilitieId)
        {
            if ((uint) capabilitieId >= (uint) capabilitiesUpdateList.Length)
                return;

            var array = capabilitiesUpdateList[capabilitieId];
            if (array != null)
            {
                RemoveArray(array, player);
            }
        }

        private void RemoveFixedUpdate(EffEntity player, int capabilitieId)
        {
            if ((uint) capabilitieId >= (uint) capabilitiesFixUpdateList.Length)
                return;

            var array = capabilitiesFixUpdateList[capabilitieId];
            if (array != null)
            {
                RemoveArray(array, player);
            }
        }

        private void RemoveLatedUpdate(EffEntity player, int capabilitieId)
        {
            if ((uint) capabilitieId >= (uint) capabilitiesLateUpdateList.Length)
                return;

            var array = capabilitiesLateUpdateList[capabilitieId];
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

            foreach (var array in capabilitiesLateUpdateList)
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