namespace AddressableAssets.DocExampleCode
{
    #region doc_SetCustomBuilder
    #if UNITY_EDITOR
    using UnityEditor.AddressableAssets;
    using UnityEditor.AddressableAssets.Build;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    internal class CustomDataBuilder
    {
        public static void SetCustomDataBuilder(IDataBuilder builder) {
            AddressableAssetSettings settings
                = AddressableAssetSettingsDefaultObject.Settings;

            int index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
            if (index > 0)
                settings.ActivePlayerDataBuilderIndex = index;
            else if (AddressableAssetSettingsDefaultObject.Settings.AddDataBuilder(builder))
                settings.ActivePlayerDataBuilderIndex 
                    = AddressableAssetSettingsDefaultObject.Settings.DataBuilders.Count - 1;
            else
                Debug.LogWarning($"{builder} could not be found " +
                                 $"or added to the list of DataBuilders");
        }
    }
    #endif
    #endregion
}