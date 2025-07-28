using System;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

namespace GameFrame.Runtime
{
    public struct TimeProfiler : IDisposable
    {
#if UNITY_EDITOR
        private string tag;
        private bool autoPrint;

        private Stopwatch stopwatch;
#endif

        public string Stat
        {
            get
            {
#if UNITY_EDITOR
                return $"{tag} cost {stopwatch.Elapsed.TotalMilliseconds}ms";
#else
                return $"not support";
#endif
            }
        }

        public TimeProfiler(string tag, bool autoPrint = true)
        {
#if UNITY_EDITOR
            this.tag = tag;
            this.autoPrint = autoPrint;

            stopwatch = Stopwatch.StartNew();
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            stopwatch.Stop();
            if (autoPrint)
                UnityEngine.Debug.Log(Stat);
#endif
        }
    }
}