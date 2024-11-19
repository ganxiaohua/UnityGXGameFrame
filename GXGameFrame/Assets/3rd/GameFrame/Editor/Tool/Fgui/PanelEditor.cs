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
        private CreateUIScriptOdin createUIScriptOdin;

        public override void OnInspectorGUI()
        {
            createUIScriptOdin ??= new CreateUIScriptOdin();
            base.OnInspectorGUI();
            if (GUILayout.Button($"导出"))
            {
                FairyGUI.UIPanel panel = target as FairyGUI.UIPanel;
                createUIScriptOdin.ButtonCreateScript(panel);
            }

            if (GUILayout.Button($"回到主场景"))
            {
                FairyGUI.UIPanel panel = target as FairyGUI.UIPanel;
                createUIScriptOdin.ButtonBackMain();
            }
        }
    }
}