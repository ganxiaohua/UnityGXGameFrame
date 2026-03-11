#if UNITY_EDITOR
using System.Collections.Generic;
using GameFrame.Runtime;

namespace GameFrame.Runtime
{
    public partial struct CapabilityComponent
    {
        private int dicIndex;

        public void InitDebug()
        {
            dicIndex = DicDatas<CapabilityBase, List<int>>.Instance.AddArrayDatas();
        }

        private bool CanBlock(int index, CapabilityBase instigators)
        {
            if (instigators == null)
                return true;
            var instigatorsDic = DicDatas<CapabilityBase, List<int>>.Instance.GetArrayDatas(index);
            if (!instigatorsDic.TryGetValue(instigators, out var list))
            {
                list = ListPool<int>.Get();
                instigatorsDic.Add(instigators, list);
            }

            if (list.Contains(index))
            {
                Debugger.LogError($"{instigators.GetType().Name} already block {index}");
                return false;
            }

            list.Add(index);
            return true;
        }

        private bool CanUnBlock(int index, CapabilityBase instigators)
        {
            if (instigators == null)
                return true;
            var instigatorsDic = DicDatas<CapabilityBase, List<int>>.Instance.GetArrayDatas(index);
            if (!instigatorsDic.TryGetValue(instigators, out var list))
            {
                // Debugger.LogError($"{Instigators.GetType().Name}  not block {index}");
                return false;
            }

            if (!list.Contains(index))
            {
                // Debugger.LogError($"{Instigators.GetType().Name} not Block {index}");
                return false;
            }

            list.Remove(index);
            return true;
        }

        private void DisposeDebug()
        {
            DicDatas<CapabilityBase, List<int>>.Instance.RemoveDatas(dicIndex);
        }
    }
}
#endif