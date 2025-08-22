using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    [CustomEditor(typeof(ViewEffBindEnitiy))]
    public class CustomViewEntity : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var viewEffBindEnitiy = (ViewEffBindEnitiy) target;
            if (GUILayout.Button($"capailityView"))
            {
                CapabilityView.Init(viewEffBindEnitiy.Entity);  
            }
            
            if (GUILayout.Button($"ComponentView"))
            {
                ComponentView.Init(viewEffBindEnitiy.Entity);  
            }
        }
    }
}