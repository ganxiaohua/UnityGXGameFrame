using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        public CapabilitysUpdateMode Add<T>(EffEntity player) where T : CapabilityBase
        {
            var capability = ReferencePool.Acquire<T>();
            var updateMode = capability.UpdateMode;
            SetArray(GetCapabilityArray(updateMode), player, GetCapabilityId<T>(updateMode), capability);

            return updateMode;
        }

        internal static int GetCapabilityId<T>(CapabilitysUpdateMode updateMode) where T : CapabilityBase
        {
            return updateMode switch
            {
                CapabilitysUpdateMode.Update => CapabilityID<T, IUpdateSystem>.TID,
                CapabilitysUpdateMode.FixedUpdate => CapabilityID<T, IFixedUpdateSystem>.TID,
                CapabilitysUpdateMode.LateUpdate => CapabilityID<T, ILateUpdateSystem>.TID,
                CapabilitysUpdateMode.FixedUpdateForEndEcs => CapabilityID<T, IFixedUpdateForEndEcsSystem>.TID,
                _ => throw new System.ArgumentOutOfRangeException(nameof(updateMode), updateMode, null)
            };
        }

        protected virtual JumpIndexArray<CapabilityBase>[] GetCapabilityArray(CapabilitysUpdateMode updateMode)
        {
            return updateMode switch
            {
                CapabilitysUpdateMode.Update => capabilitiesUpdateList,
                CapabilitysUpdateMode.FixedUpdate => capabilitiesFixUpdateList,
                CapabilitysUpdateMode.LateUpdate => capabilitiesLateUpdateList,
                _ => throw new System.NotSupportedException($"{GetType().Name} does not support {updateMode} capabilities.")
            };
        }

        private void SetArray(JumpIndexArray<CapabilityBase>[] arrays, EffEntity player, int id, CapabilityBase capability)
        {
            if ((uint) id >= (uint) arrays.Length)
                throw new System.IndexOutOfRangeException($"{capability.GetType().Name} capability id {id} is outside {capability.UpdateMode} array length {arrays.Length}.");

            var array = arrays[id];
            if (array == null)
            {
                array = new JumpIndexArray<CapabilityBase>();
                array.Init(capabilityArrayCapacity);
                arrays[id] = array;
            }

            var cap = array.Set(player.Id, capability);
            cap.Init(id, eccWorld, player);
        }

        public void EnsureCapabilityCapacity(int entityId)
        {
            if (entityId < capabilityArrayCapacity)
                return;

            var newCapacity = System.Math.Max(capabilityArrayCapacity * 2, entityId + 1);
            ExpandCapabilityArrays(newCapacity);
            capabilityArrayCapacity = newCapacity;
        }

        protected virtual void ExpandCapabilityArrays(int capacity)
        {
            ExpandCapabilityArrays(capabilitiesUpdateList, capacity);
            ExpandCapabilityArrays(capabilitiesFixUpdateList, capacity);
            ExpandCapabilityArrays(capabilitiesLateUpdateList, capacity);
        }

        protected static void ExpandCapabilityArrays(JumpIndexArray<CapabilityBase>[] arrays, int capacity)
        {
            foreach (var array in arrays)
                array?.EnsureCapacity(capacity);
        }

        public void SetCapabilityBaseWithPlayer(EffEntity player, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate,
            List<CapabilityBase> lateUpdate)
        {
            // 旧接口仍返回全部能力，后置能力并入固定步列表。
            SetCapabilityBaseWithPlayer(player, update, fixedUpdate, lateUpdate, fixedUpdate);
        }

        public virtual void SetCapabilityBaseWithPlayer(EffEntity player, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate,
            List<CapabilityBase> lateUpdate, List<CapabilityBase> fixedUpdateForEndEcs)
        {
            void Get(EffEntity player, JumpIndexArray<CapabilityBase>[] scr, List<CapabilityBase> dst)
            {
                foreach (var capArray in scr)
                {
                    if (capArray == null || (uint) player.Id >= (uint) capArray.AllCount)
                    {
                        continue;
                    }

                    var capability = capArray[player.Id];
                    if (capability != null && capability.Owner == player)
                        dst.Add(capability);
                }
            }

            Get(player, capabilitiesUpdateList, update);
            Get(player, capabilitiesFixUpdateList, fixedUpdate);
            Get(player, capabilitiesLateUpdateList, lateUpdate);
        }

        public void Remove<T>(EffEntity player, CapabilitysUpdateMode updateMode) where T : CapabilityBase
        {
            Remove(player, GetCapabilityId<T>(updateMode), updateMode);
        }

        public void Remove(EffEntity player, int capabilitieId, CapabilitysUpdateMode updateMode)
        {
            var arrays = GetCapabilityArray(updateMode);
            if ((uint) capabilitieId >= (uint) arrays.Length)
                return;

            var array = arrays[capabilitieId];
            if (array != null)
            {
                RemoveArray(array, player);
            }
        }


        public virtual void RemoveEffEntitysAllCapability(EffEntity player)
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

        protected void RemoveArray(JumpIndexArray<CapabilityBase> array, EffEntity player)
        {
            if ((uint) player.Id >= (uint) array.AllCount) return;
            var capability = array[player.Id];
            if (capability == null || capability.Owner != player) return;

            array.Remove(player.Id);
            if (capability.IsActive)
                capability.OnDeactivated();
            ReferencePool.Release(capability);
        }
    }
}
