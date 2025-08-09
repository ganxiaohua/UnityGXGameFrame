#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed partial class TimerSystem
    {
        [ShowInInspector]
        private List<string> TimerList
        {
            get
            {
                var list = new List<string>();
                for (var i = 0; i < timers.Count; i++)
                {
                    var timer = timers[i];
                    var name = string.IsNullOrEmpty(timer.Name) ? timer.GetHashCode().ToString("x") : timer.Name;
                    var lifetime = timer.ExpireTime - Time.deltaTime;
                    list.Add($"{name} lifetime:{lifetime.PrettySeconds()}");
                }

                return list;
            }
        }
    }
}
#endif