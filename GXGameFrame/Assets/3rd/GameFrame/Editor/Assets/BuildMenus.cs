using UnityEngine;
using UnityEditor;

namespace GameFrame.Editor
{
    public static class BuildMenus
    {
        [MenuItem("Build/Open PersistentDataPath")]
        public static void OpenPersistentDataPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        [MenuItem("Build/Open Cache Path")]
        public static void OpenCachePath()
        {
            Application.OpenURL(Caching.currentCacheForWriting.path);
        }

        [MenuItem("Build/Build Bundles")]
        public static void BuildBundles()
        {
            BuildScript.BuildBundles();
        }

        [MenuItem("Build/ProcessAllAssetGroup")]
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

        [MenuItem("Build/Build Player/整包")]
        public static void BuildPlayerFullRes()
        {
            BuildScript.BuildPlayer(true);
        }

        [MenuItem("Build/Build Player/微端")]
        public static void BuildPlayer()
        {
            BuildScript.BuildPlayer(false);
        }
    }
}
