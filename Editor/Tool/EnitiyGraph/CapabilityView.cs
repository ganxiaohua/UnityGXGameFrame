using System.Collections.Generic;
using GameFrame.Runtime;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class CapabilityView : OdinEditorWindow
    {
        private static CapabilityView sWindow;

        private EffEntity effEntity;
        private ECCWorld eccWorld;

        private List<CapabilityBase> capabilityBaseUpdateMode = new List<CapabilityBase>();
        private List<CapabilityBase> capabilityBaseFixUpdateMode = new List<CapabilityBase>();

        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<CapabilityView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.eccWorld = (ECCWorld) effEntity.world;
            sWindow.Start();
            sWindow.Show();
        }

        private void Start()
        {
            capabilityBaseUpdateMode.Clear();
            capabilityBaseFixUpdateMode.Clear();
            eccWorld.GetCapability(effEntity, capabilityBaseUpdateMode, capabilityBaseFixUpdateMode);
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            if (effEntity.State == IEntity.EntityState.IsClear)
            {
                return;
            }

            EditorGUILayout.LabelField("蓝色表示激活，白色表示未激活，黄色表示被锁住了");
            EditorGUILayout.LabelField("___________________________update");
            Show(capabilityBaseUpdateMode);
            EditorGUILayout.LabelField("___________________________fixedUpdate");
            Show(capabilityBaseFixUpdateMode);
        }

        private void Show(List<CapabilityBase> baseMode)
        {
            var oldColor = GUI.contentColor;
            foreach (var capability in baseMode)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.color = capability.IsActive ? Color.cyan : Color.white;
                if (capability.TagList != null)
                    GUI.color = eccWorld.IsBindCapability(effEntity, capability.TagList) ? Color.yellow : GUI.color;
                // EditorGUI.DrawRect(new Rect(0,0,200,50), new Color(0.15f, 0.15f, 0.15f));
                EditorGUILayout.LabelField(capability.GetType().Name);
                EditorGUILayout.EndVertical();
            }

            GUI.color = oldColor;
        }


        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
        }

        public static void Destroy()
        {
            sWindow?.Close();
        }

        protected override void OnDestroy()
        {
            sWindow = null;
        }
    }
}