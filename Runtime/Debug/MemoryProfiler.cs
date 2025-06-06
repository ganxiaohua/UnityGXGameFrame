using System;
#if UNITY_EDITOR
using Unity.Profiling;
#endif

namespace GameFrame
{
    public struct MemoryProfiler : IDisposable
    {
#if UNITY_EDITOR
        private string tag;
        private bool autoPrint;

        private long totalReservedMemory0;
        private long gcUsedMemory0;
        private long systemUsedMemory0;
#endif

        public string Stat
        {
            get
            {
#if UNITY_EDITOR
                var option = ProfilerRecorderOptions.Default;
                var totalReservedMemory1 = new ProfilerRecorder(ProfilerCategory.Memory, "Total Reserved Memory", 0, option).CurrentValue;
                var gcUsedMemory1 = new ProfilerRecorder(ProfilerCategory.Memory, "GC Used Memory", 0, option).CurrentValue;
                var systemUsedMemory1 = new ProfilerRecorder(ProfilerCategory.Memory, "System Used Memory", 0, option).CurrentValue;
                return $"{tag} " +
                       $"Total Reserved Memory Changed: {(totalReservedMemory1 - totalReservedMemory0).PrettyMemory()}, " +
                       $"GC Used Memory Changed: {(gcUsedMemory1 - gcUsedMemory0).PrettyMemory()}, " +
                       $"System Used Memory Changed: {(systemUsedMemory1 - systemUsedMemory0).PrettyMemory()}";
#else
                return $"not support";
#endif
            }
        }

        public MemoryProfiler(string tag, bool autoPrint = true)
        {
#if UNITY_EDITOR
            this.tag = tag;
            this.autoPrint = autoPrint;

            GC.Collect();
            var option = ProfilerRecorderOptions.Default;
            totalReservedMemory0 = new ProfilerRecorder(ProfilerCategory.Memory, "Total Reserved Memory", 0, option).CurrentValue;
            gcUsedMemory0 = new ProfilerRecorder(ProfilerCategory.Memory, "GC Used Memory", 0, option).CurrentValue;
            systemUsedMemory0 = new ProfilerRecorder(ProfilerCategory.Memory, "System Used Memory", 0, option).CurrentValue;
#endif
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (autoPrint)
                UnityEngine.Debug.Log(Stat);
#endif
        }
    }
}