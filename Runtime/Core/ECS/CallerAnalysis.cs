#if UNITY_EDITOR
using System;
using System.IO;

namespace GameFrame.Runtime
{
    public static class CallerAnalysis
    {
        public static (string scriptName, string line) Analysis(int line)
        {
            var str = Environment.StackTrace;
            var strs = str.Split('\n');
            var targetStr = strs[line];
            targetStr = targetStr.TrimEnd('\r');
            var startPos = targetStr.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            var keyEndPos = targetStr.LastIndexOf(':');
            var key = targetStr.AsSpan(startPos, keyEndPos - startPos);
            string splitStr = "] in ";
            startPos = targetStr.IndexOf(splitStr, StringComparison.Ordinal) + splitStr.Length;
            var value = targetStr.AsSpan(startPos, targetStr.Length - startPos);
            return (key.ToString(), value.ToString());
        }
    }
}
#endif