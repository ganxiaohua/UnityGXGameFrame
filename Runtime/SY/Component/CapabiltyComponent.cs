using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace SH.GameFrame
{
    public partial class CapabiltyComponent
    {
        private int[] tags;

        public void Init()
        {
            tags = new int[(int) CapabilityTags.Count];
        }

        public void Block(CapabilityTags index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            if (!CanBlock(index, instigators))
                return;
#endif
            tags[(int) index]++;
        }

        public void UnBlock(CapabilityTags index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            CanUnBlock(index, instigators);
#endif
            tags[(int) index]--;
        }
    }
}