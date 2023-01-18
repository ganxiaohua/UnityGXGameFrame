using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    [Serializable]
    class AnalyzeRuleGUI
    {
        [SerializeField]
        private TreeViewState m_TreeState;

        private AssetSettingsAnalyzeTreeView m_Tree;

        private const float k_ButtonHeight = 24f;

        internal void OnGUI(Rect rect)
        {
            if (m_Tree == null)
            {
                if (m_TreeState == null)
                    m_TreeState = new TreeViewState();

                m_Tree = new AssetSettingsAnalyzeTreeView(m_TreeState);
                m_Tree.Reload();
            }

            var treeRect = new Rect(rect.xMin, rect.yMin + k_ButtonHeight, rect.width, rect.height - k_ButtonHeight);
            m_Tree.OnGUI(treeRect);

            var buttonRect = new Rect(rect.xMin, rect.yMin, rect.width, rect.height);

            GUILayout.BeginArea(buttonRect);
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!m_Tree.SelectionContainsRuleContainer);
            if (GUILayout.Button("Analyze Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.RunAllSelectedRules();
            }

            if (GUILayout.Button("Clear Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.ClearAllSelectedRules();
            }

            EditorGUI.BeginDisabledGroup(!m_Tree.SelectionContainsFixableRule || !m_Tree.SelectionContainsErrors);
            if (GUILayout.Button("Fix Selected Rules"))
            {
                EditorApplication.delayCall += () => m_Tree.FixAllSelectedRules();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //TODO
            //if (GUILayout.Button("Revert Selected"))
            //{
            //    m_Tree.RevertAllActiveRules();
            //}
        }
    }
}
