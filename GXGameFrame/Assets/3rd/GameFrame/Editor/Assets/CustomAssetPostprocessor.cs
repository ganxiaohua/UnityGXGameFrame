using UnityEditor;
using System.IO;
using System;

namespace GameFrame.Editor
{
    public class CustomAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            PackAssets(importedAssets);
            PackAssets(movedAssets);
            if (Array.Exists(importedAssets, IsUIFolder) || Array.Exists(movedAssets, IsUIFolder) || Array.Exists(deletedAssets,IsUIFolder))
            {
                BuildScript.ProcessAssetGroupByName("UI");
            }
        }
        
        public static bool IsUIFolder(string assetPath)
        {
            return assetPath.StartsWith(EditorString.UIAssetPath);
        }
        
        private static void PackAssets(string[] assets)
        {
            foreach (var assetPath in assets)
            {
                // texture setting
                if (IsUIFolder(assetPath) && IsTexture(assetPath))
                {
                    TextureSetting(assetPath);
                }
            }
        }

        public static void TextureSetting(string assetPath)
        {
            var textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            textureImporter.mipmapEnabled = false;
            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
        }
        
        public static string[] TextrueExts = new string[] { ".png", ".jpg", ".tga", ".psd", ".bmp" };
        public static bool IsTexture(string assetPath)
        {
            for (int i = 0; i < TextrueExts.Length; i++)
            {
                if (assetPath.EndsWith(TextrueExts[i]))
                    return true;
            }
            return false;
        }
        
        //
        // [UnityEditor.Callbacks.DidReloadScripts]
        // public static void SystemBind()
        // {
        //     
        // }

    }
}
