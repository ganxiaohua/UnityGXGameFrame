using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace GameFrame.Runtime.Editor
{
    public static class MacroDefineHelper
    {
        public static bool Contains(string define)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
            return defines.Contains(define);
        }

        public static void Enable(string define)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
            if (!defines.Contains(define))
            {
                var list = new List<string>(defines);
                list.Add(define);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, list.ToArray());
            }
        }

        public static void Disable(string define)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out var defines);
            if (defines.Contains(define))
            {
                var list = new List<string>(defines);
                list.Remove(define);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, list.ToArray());
            }
        }
    }
}