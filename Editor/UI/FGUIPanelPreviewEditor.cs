using FairyGUI;
using GameFrame.Editor;
using UnityEditor;
using UnityEngine;

namespace FairyGUIEditor
{
    [CustomEditor(typeof(UIPanel))]
    public class FGUIPanelPreviewEditor : UIPanelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var panel = (UIPanel) target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LabelField("Package Dependencies");
                if (panel.ui != null)
                {
                    EditorGUI.indentLevel++;
                    foreach (var dep in panel.ui.packageItem.owner.dependencies)
                    {
                        var dPppackageName = UIPackage.GetById(dep["name"]).name;
                        EditorGUILayout.TextField(dPppackageName);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.Separator();

            using (new EditorGUI.DisabledScope(panel.ui == null))
            {
                using (new ColorScope(FGUIPanelCodeGen.Exists(panel.packageName, panel.componentName) ? Color.yellow : Color.white))
                {
                    if (GUILayout.Button($"生成代码({panel.packageName}/{panel.componentName}Panel)"))
                    {
                        FGUIPanelCodeGen.Build(panel.packageName, panel.componentName);
                    }

                    if (GUILayout.Button($"导出或更新组件"))
                    {
                        FGUIPanelCodeGen.Export(panel.packageName, panel.componentName);
                    }
                }
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button($"更新所有已生成的代码"))
            {
                FGUIPanelCodeGen.UpdateAll();
            }

            EditorGUILayout.Separator();
        }
    }
}