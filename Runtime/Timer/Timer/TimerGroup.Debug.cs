#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed partial class TimerGroup
    {
        [ShowInInspector]
        private string GroupName => string.IsNullOrEmpty(Name) ? GetHashCode().ToString("x") : Name;

        [ShowInInspector]
        private int TimerCount => Count;

        [ShowInInspector]
        private List<string> TimerList
        {
            get
            {
                var list = new List<string>(timers.Count);
                var currentTime = Time.realtimeSinceStartup;
                for (var i = 0; i < timers.Count; i++)
                {
                    var timer = timers[i];
                    var name = string.IsNullOrEmpty(timer.Name) ? timer.GetHashCode().ToString("x") : timer.Name;
                    var lifetime = timer.ExpireTime - currentTime;
                    list.Add($"{name} lifetime:{lifetime.PrettySeconds()}");
                }
                return list;
            }
        }
    }
}
#endif
