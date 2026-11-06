#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GameFrame.Runtime
{
    public sealed partial class TimerSystem
    {
        [ShowInInspector]
        private List<TimerGroup> TimerGroups => new(groups);
    }
}
#endif
