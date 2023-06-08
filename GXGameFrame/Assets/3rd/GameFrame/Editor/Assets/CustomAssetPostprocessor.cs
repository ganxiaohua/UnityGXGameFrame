using UnityEditor;
using System.IO;
using System;

namespace GameFrame.Editor
{
    public class CustomAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            ABAssest(importedAssets);
            ABAssest(deletedAssets);
            ABAssest(movedAssets);
            ABAssest(movedFromAssetPaths);
        }
        
        /// <summary>
        /// 在配置中资源发生了变化就执行
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void ABAssest(string[] assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                foreach (var item in AssetGroupSettings.settings.data)
                {
                    foreach (var searchPath in item.searchPaths)
                    {
                        if (assetPath.StartsWith(searchPath))
                        {
                            BuildScript.ProcessAssetGroupByName(item.groupName);
                            break;
                        }
                    }
                }   
            }
        }
        
        //
        // [UnityEditor.Callbacks.DidReloadScripts]
        // public static void SystemBind()
        // {
        //     
        // }
    }
}
