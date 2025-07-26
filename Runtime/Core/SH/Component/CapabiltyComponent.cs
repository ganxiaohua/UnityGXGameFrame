using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime.SH
{
    public partial class CapabiltyComponent : ECSComponent
    {
        private int[] tags;

        public void Init(int maxCount)
        {
            tags = new int[maxCount];
        }

        public void Block(int index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            if (!CanBlock(index, instigators))
                return;
#endif
            tags[index]++;
        }

        public void UnBlock(int index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            CanUnBlock(index, instigators);
#endif
            tags[index]--;
        }

        public bool IsBlock(IReadOnlyList<int> indexs)
        {
            foreach (var index in indexs)
            {
                if (tags[index] > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}