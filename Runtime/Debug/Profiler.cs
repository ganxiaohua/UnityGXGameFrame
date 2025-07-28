using System;
using System.Collections.Generic;
using UnityProfiler = UnityEngine.Profiling.Profiler;

namespace GameFrame.Runtime
{
    public struct Profiler : IDisposable
    {
#if UNITY_EDITOR
        private static Dictionary<Type, string> cacheTypeNames = new();
#endif

        public Profiler(string tag)
        {
#if UNITY_EDITOR
            UnityProfiler.BeginSample(string.IsNullOrEmpty(tag) ? "unknown" : tag);
#endif
        }

        public Profiler(object obj)
        {
#if UNITY_EDITOR
            var type = obj?.GetType();
            var tag = "unknown";
            if (type != null && !cacheTypeNames.TryGetValue(type, out tag))
            {
                tag = type.Name;
                cacheTypeNames.Add(type, tag);
            }

            UnityProfiler.BeginSample(tag);
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            UnityProfiler.EndSample();
#endif
        }
    }
}