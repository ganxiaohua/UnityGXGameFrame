using System;
using GameFrame;

namespace GameFrame.Runtime.SH
{
    public partial class Capabilitys
    {
        public void Add<T>(int playerId, CapabilitysUpdateMode mode) where T : CapabilityBase
        {
            if (mode == CapabilitysUpdateMode.Update)
            {
                int id = CapabilityID<T, IUpdateSystem>.TID;
                var array = capabilitiesUpdateList[id];
                var cap =  array.Add(playerId, typeof(T));
                cap.Init(id);
            }
            else if (mode == CapabilitysUpdateMode.FixedUpdat)
            {
                int id = CapabilityID<T, IFixedUpdateSystem>.TID;
                var array = capabilitiesFixUpdateList[id];
                var cap =  array.Add(playerId, typeof(T));
                cap.Init(id);
            }
        }

        public void Remove<T>(int playerId) where T : CapabilityBase
        {
            int id = CapabilityID<T, IUpdateSystem>.TID;
            Remove(playerId,id);
        }

        public void Remove(int playerId, int capabilitieId)
        {
            var array = capabilitiesUpdateList[capabilitieId];
            if (array.Contains(playerId))
            {
                array.Remove(playerId);
                return;
            }

            array = capabilitiesFixUpdateList[capabilitieId];
            if (array.Contains(playerId))
            {
                array.Remove(playerId);
            }
        }
    }
}