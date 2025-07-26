#if UNITY_EDITOR
using System.Collections.Generic;
using GameFrame;

namespace GameFrame.Runtime.SH
{
    public partial class CapabiltyComponent
    {
        private Dictionary<CapabilityBase, List<int>> instigatorsDic = new();

        private bool CanBlock(int index, CapabilityBase Instigators)
        {
            if (instigatorsDic.TryGetValue(Instigators, out var list))
            {
                list ??= new List<int>();
                instigatorsDic.Add(Instigators, list);
            }

            if (list.Contains(index))
            {
                Debugger.LogError($"{Instigators.GetType().Name} already block {index}");
                return false;
            }
            list.Add(index);
            return true;
        }

        private bool CanUnBlock(int index, CapabilityBase Instigators)
        {
            if (instigatorsDic.TryGetValue(Instigators, out var list))
            {
                Debugger.LogError($"{Instigators.GetType().Name}  not block {index}");
                return false;
            }

            if (!list.Contains(index))
            {
                Debugger.LogError($"{Instigators.GetType().Name} not Block {index}");
                return false;
            }
            list.Remove(index);
            return true;
        }
    }
}
#endif