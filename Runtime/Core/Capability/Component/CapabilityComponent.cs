using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameFrame.Runtime
{
    [StructLayout(LayoutKind.Auto)]
    public partial struct CapabilityComponent : EffComponent
    {
        private int arrayIndex;

        public void Init(int maxCount)
        {
            arrayIndex = ArrayDatas<int>.Instance.AddArrayDatas(maxCount);
#if UNITY_EDITOR
            InitDebug();
#endif
        }


        public void Block(int index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            if (!CanBlock(index, instigators))
                return;
#endif
            var tags = ArrayDatas<int>.Instance.GetArrayDatas(arrayIndex);
            tags[index]++;
        }

        public void UnBlock(int index, CapabilityBase instigators)
        {
#if UNITY_EDITOR
            CanUnBlock(index, instigators);
#endif
            var tags = ArrayDatas<int>.Instance.GetArrayDatas(arrayIndex);
            tags[index] = Mathf.Max(0, --tags[index]);
        }

        public bool IsBlock(List<int> indexs)
        {
            var tags = ArrayDatas<int>.Instance.GetArrayDatas(arrayIndex);
            foreach (var index in indexs)
            {
                if (tags[index] > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            ArrayDatas<int>.Instance.RemoveDatas(arrayIndex);
#if UNITY_EDITOR
            DisposeDebug();
#endif
        }

        public override bool Equals(object obj)
        {
            return obj is CapabilityComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(arrayIndex, dicIndex);
        }
    }
}