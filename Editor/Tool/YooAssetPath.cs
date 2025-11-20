using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class YooAssetPath : UnityEditor.Editor
    {
        [MenuItem("Assets/CopyAssetPath", false, 0)]
        static void CopyFullPathFromHierarchy()
        {
            if (Selection.activeGameObject != null)
            {
                // 获取资源的相对路径
                string assetPath = AssetDatabase.GetAssetPath(Selection.activeGameObject);
                assetPath = GetAssetPath(assetPath);
                // 将路径复制到剪贴板
                EditorGUIUtility.systemCopyBuffer = assetPath;
            }
        }

        public static string GetAssetPath(string assetPath)
        {
            return assetPath.Substring(0, assetPath.LastIndexOf('.'));
        }
    }
}