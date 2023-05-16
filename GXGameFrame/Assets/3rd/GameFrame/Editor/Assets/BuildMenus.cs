using UnityEngine;
using UnityEditor;

namespace GameFrame.Editor
{
    public static class BuildMenus
    {
        // [MenuItem("GX框架Build辅助器/Open PersistentDataPath")]
        // public static void OpenPersistentDataPath()
        // {
        //     Application.OpenURL(Application.persistentDataPath);
        // }

        // [MenuItem("GX框架Build辅助器/Open Cache Path")]
        // public static void OpenCachePath()
        // {
        //     Application.OpenURL(Caching.currentCacheForWriting.path);
        // }

        [MenuItem("GX框架Build辅助器/Build Bundles")]
        public static void BuildBundles()
        {
            BuildScript.BuildBundles();
        }

        [MenuItem("GX框架Build辅助器/ProcessAllAssetGroup")]
        public static void ProcessAllAssetGroup()
        {
            BuildScript.ProcessAllAssetGroup();
        }

        //[MenuItem("Build/ClearAssetBundleName")]
        public static void ClearAssetBundleName()
        {
            foreach (var item in AssetDatabase.GetAllAssetBundleNames())
            {
                var paths = AssetDatabase.GetAssetPathsFromAssetBundle(item);
                foreach (var path in paths)
                {
                    var importer = AssetImporter.GetAtPath(path);
                    importer.assetBundleName = "";
                    importer.SaveAndReimport();
                }

                var result = AssetDatabase.RemoveAssetBundleName(item, true);
                Debug.Log($"RemoveAssetBundleName:{item}, {result}");
            }
        }

        [MenuItem("GX框架Build辅助器/Build Player/含资源包(生成在Bin目录下)")]
        public static void BuildPlayerFullRes()
        {
            BuildScript.BuildBundles(true);
            BuildScript.BuildPlayer(true);
        }

        [MenuItem("GX框架Build辅助器/Build Player/不含资源包(生成在Bin目录下)")]
        public static void BuildPlayer()
        {
            BuildScript.BuildBundles(true);
            BuildScript.BuildPlayer(false);
        }
    }
}