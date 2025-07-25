using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime.SH
{
    public partial class CapabiltyComponent : ECSComponent
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