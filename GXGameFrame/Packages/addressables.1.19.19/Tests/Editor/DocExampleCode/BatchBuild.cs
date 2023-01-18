namespace AddressableAssets.DocExampleCode
{
    #region doc_BatchBuild
#if UNITY_EDITOR
    using System;
    using UnityEditor;
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    internal class BatchBuild
    {
        public static string build_script 
            = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        public static string profile_name = "Default";
        public static void ChangeSettings() {
            string defines = "";
            string[] args = Environment.GetCommandLineArgs();

            foreach (var arg in args)
                if (arg.StartsWith("-defines=", System.StringComparison.CurrentCulture))
                    defines = arg.Substring(("-defines=".Length));

            var buildSettings = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildSettings, defines);
        }

        public static void BuildContentAndPlayer() {
            AddressableAssetSettings settings
                = AddressableAssetSettingsDefaultObject.Settings;

            settings.activeProfileId
                = settings.profileSettings.GetProfileId(profile_name);

            IDataBuilder builder
                = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

            settings.ActivePlayerDataBuilderIndex
                = settings.DataBuilders.IndexOf((ScriptableObject)builder);

            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

            if (!string.IsNullOrEmpty(result.Error))
                throw new Exception(result.Error);

            BuildReport buildReport
                = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,
                "d:/build/winApp.exe", EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None);

            if (buildReport.summary.result != BuildResult.Succeeded)
                throw new Exception(buildReport.summary.ToString());
        }
    }
#endif
    #endregion
}