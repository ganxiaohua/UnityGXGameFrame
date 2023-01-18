namespace AddressableAssets.DocExampleCode
{
    using MenuItem = Dummy;
    #region doc_BuildLauncher
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using System;
    using UnityEngine;

    internal class BuildLauncher
    {
        public static string build_script 
            = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        public static string settings_asset 
            = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        public static string profile_name = "Default";
        private static AddressableAssetSettings settings;

        #region getSettingsObject
        static void getSettingsObject(string settingsAsset) {
            // This step is optional, you can also use the default settings:
            //settings = AddressableAssetSettingsDefaultObject.Settings;

            settings
                = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsAsset)
                    as AddressableAssetSettings;

            if (settings == null)
                Debug.LogError($"{settingsAsset} couldn't be found or isn't " +
                               $"a settings object.");
        }
        #endregion

        #region setProfile
        static void setProfile(string profile) {
            string profileId = settings.profileSettings.GetProfileId(profile);
            if (String.IsNullOrEmpty(profileId))
                Debug.LogWarning($"Couldn't find a profile named, {profile}, " +
                                 $"using current profile instead.");
            else
                settings.activeProfileId = profileId;
        }
        #endregion

        #region setBuilder
        static void setBuilder(IDataBuilder builder) {
            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);

            if (index > 0)
                settings.ActivePlayerDataBuilderIndex = index;
            else
                Debug.LogWarning($"{builder} must be added to the " +
                                 $"DataBuilders list before it can be made " +
                                 $"active. Using last run builder instead.");
        }
        #endregion

        #region buildAddressableContent
        static bool buildAddressableContent() {
            AddressableAssetSettings
                .BuildPlayerContent(out AddressablesPlayerBuildResult result);
            bool success = string.IsNullOrEmpty(result.Error);

            if (!success) {
                Debug.LogError("Addressables build error encountered: " + result.Error);
            }
            return success;
        }
        #endregion

        [MenuItem("Window/Asset Management/Addressables/Build Addressables only")]
        public static bool BuildAddressables() {
            getSettingsObject(settings_asset);
            setProfile(profile_name);
            IDataBuilder builderScript
              = AssetDatabase.LoadAssetAtPath<ScriptableObject>(build_script) as IDataBuilder;

            if (builderScript == null) {
                Debug.LogError(build_script + " couldn't be found or isn't a build script.");
                return false;
            }

            setBuilder(builderScript);

            return buildAddressableContent();
        }

        [MenuItem("Window/Asset Management/Addressables/Build Addressables and Player")]
        public static void BuildAddressablesAndPlayer() {
            bool contentBuildSucceeded = BuildAddressables();

            if (contentBuildSucceeded) {
                var options = new BuildPlayerOptions();
                BuildPlayerOptions playerSettings
                    = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);

                BuildPipeline.BuildPlayer(playerSettings);
            }
        }
    }
#endif
    #endregion
}