using System;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        public void Add<T>(EffEntity player, CapabilitysUpdateMode mode) where T : CapabilityBase
        {
            int id = CapabilityID<T, IUpdateSystem>.TID;
            if (mode == CapabilitysUpdateMode.Update)
            {
                SetArray<T>(capabilitiesUpdateList, player, id);
            }
            else if (mode == CapabilitysUpdateMode.FixedUpdate)
            {
                SetArray<T>(capabilitiesFixUpdateList, player, id);
            }
        }

        private void SetArray<T>(GXArray<CapabilityBase>[]  arrays, EffEntity player, int id) where T : CapabilityBase
        {
            var array = arrays[id];
            if (array == null)
            {
                array = new GXArray<CapabilityBase>();
                array.Init(estimatedNumberPlayer);
                arrays[id] = array;
            }
            var cap = array.Add(player.ID, typeof(T));
            cap.Init(player, id);
        }

        public void Remove(EffEntity player, int capabilitieId)
        {
            var array = capabilitiesUpdateList[capabilitieId];
            bool succ = array.Remove(player.ID);
            if (succ)
                return;
            array = capabilitiesFixUpdateList[capabilitieId];
            array.Remove(player.ID);
        }
    }
}