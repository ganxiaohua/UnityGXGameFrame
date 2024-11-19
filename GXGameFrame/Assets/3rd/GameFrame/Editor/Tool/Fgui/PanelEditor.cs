using UnityEditor;
using UnityEngine;
#if UNITY_5_3_OR_NEWER
#endif

namespace FairyGUIEditor
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(FairyGUI.UIPanel))]
    public class CostomUIPanelEditor : UIPanelEditor
    {
        private CreateUICode createUICode;

        public override void OnInspectorGUI()
        {
            createUICode ??= new CreateUICode();
            base.OnInspectorGUI();
            if (GUILayout.Button($"导出"))
            {
                FairyGUI.UIPanel panel = target as FairyGUI.UIPanel;
                createUICode.ButtonCreateScript(panel);
            }

            if (GUILayout.Button($"回到主场景"))
            {
                FairyGUI.UIPanel panel = target as FairyGUI.UIPanel;
                createUICode.ButtonBackMain();
            }
        }
    }
}