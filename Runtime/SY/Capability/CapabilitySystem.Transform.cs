using System;
using GameFrame;

namespace SH.GameFrame
{
    public partial class CapabilitySystem
    {
        public void Add<T>(int playerId) where T : CapabilityBase
        {
            var tid = bilityCapabilitiesList[CapabilityID<T, World>.TID];
            tid.Add(playerId, typeof(T));
        }

        public void Remove<T>(int playerId) where T : CapabilityBase
        {
            var tid = bilityCapabilitiesList[CapabilityID<T, World>.TID];
            tid.Remove(playerId);
        }
    }
}