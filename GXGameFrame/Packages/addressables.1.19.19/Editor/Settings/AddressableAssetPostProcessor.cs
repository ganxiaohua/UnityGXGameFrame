namespace UnityEditor.AddressableAssets.Settings
{
    internal class AddressablesAssetPostProcessor : AssetPostprocessor
    {
        private static AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]> s_OnPostProcessHandler = new AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]>();
        public static AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]> OnPostProcess => s_OnPostProcessHandler;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (s_OnPostProcessHandler != null)
            {
                s_OnPostProcessHandler.Invoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            }
            else if (AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                s_OnPostProcessHandler.BufferInvoke(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            }
        }
    }
}
