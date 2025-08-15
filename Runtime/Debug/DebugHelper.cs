#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Runtime
{
    public static class DebugHelper
    {
        public static void OpenScript(string scriptName, bool selected = true)
        {
            var assets = AssetDatabase.FindAssets($"t:Script {scriptName}");
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == scriptName)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (selected)
                        Selection.activeObject = obj;
                    AssetDatabase.OpenAsset(obj);
                    break;
                }
            }
        }

        public static void SetGameObjectHierarchyExpanded(GameObject go, bool extpand)
        {
            var hierarchyType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            var method = hierarchyType.GetMethod("SetExpanded", BindingFlags.Instance | BindingFlags.NonPublic);
            var hierarchy = Resources.FindObjectsOfTypeAll(hierarchyType).First();
            method?.Invoke(hierarchy, new object[] {go.GetInstanceID(), extpand});
        }
    }
}
#endif