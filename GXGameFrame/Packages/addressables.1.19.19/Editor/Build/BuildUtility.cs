using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityEditor.AddressableAssets.Build
{
    /// <summary>
    /// Utility class for the Addressables Build Content process.
    /// </summary>
    public class BuildUtility
    {
        static HashSet<string> s_EditorAssemblies = null;
        static HashSet<string> editorAssemblies
        {
            get
            {
                if (s_EditorAssemblies == null)
                {
                    s_EditorAssemblies = new HashSet<string>();
                    foreach (var assembly in CompilationPipeline.GetAssemblies())
                    {
                        if ((assembly.flags & AssemblyFlags.EditorAssembly) != 0)
                            s_EditorAssemblies.Add(assembly.name);
                    }
                }

                return s_EditorAssemblies;
            }
        }

        /// <summary>
        /// Determines if the given assembly is an editor assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>Returns true if the assembly is an editor assembly. Returns false otherwise.</returns>
        public static bool IsEditorAssembly(System.Reflection.Assembly assembly)
        {
            var splitName = assembly.FullName.Split(',');
            return splitName.Length > 0 && editorAssemblies.Contains(splitName[0]);
        }

        /// <summary>
        /// Creates a new bundle name using its hash and a given naming style.
        /// </summary>
        /// <param name="schemaBundleNaming">The bundle naming style.</param>
        /// <param name="hash">The bundle hash.</param>
        /// <param name="sourceBundleName">The original bundle name.</param>
        /// <returns>Returns the new bundle name.</returns>
        public static string GetNameWithHashNaming(BundledAssetGroupSchema.BundleNamingStyle schemaBundleNaming, string hash, string sourceBundleName)
        {
            string result = sourceBundleName;
            switch (schemaBundleNaming)
            {
                case BundledAssetGroupSchema.BundleNamingStyle.AppendHash:
                    result = sourceBundleName.Replace(".bundle", "_" + hash + ".bundle");
                    break;
                case BundledAssetGroupSchema.BundleNamingStyle.NoHash:
                    break;
                case BundledAssetGroupSchema.BundleNamingStyle.OnlyHash:
                    result = hash + ".bundle";
                    break;
                case BundledAssetGroupSchema.BundleNamingStyle.FileNameHash:
                    result = HashingMethods.Calculate(result) + ".bundle";
                    break;
            }

            return result;
        }

        /// <summary>
        /// Used during the build to check for unsaved scenes and provide a user popup if there are any.
        /// </summary>
        /// <returns>True if there were no unsaved scenes, or if user hits "Save and Continue" on popup.
        /// False if any scenes were unsaved, and user hits "Cancel" on popup.</returns>
        public static bool CheckModifiedScenesAndAskToSave()
        {
            var dirtyScenes = new List<Scene>();

            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                {
                    dirtyScenes.Add(scene);
                }
            }

            if (dirtyScenes.Count > 0)
            {
                if (EditorUtility.DisplayDialog(
                    "Unsaved Scenes", "Modified Scenes must be saved to continue.",
                    "Save and Continue", "Cancel"))
                {
                    EditorSceneManager.SaveScenes(dirtyScenes.ToArray());
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
