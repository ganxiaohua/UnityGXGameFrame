using System;
using System.Collections.Generic;
using GameFrame.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class CapabilityView : OdinEditorWindow
    {
        private static CapabilityView sWindow;

        private EffEntity effEntity;
        private SHWorld shWorld;

        private List<CapabilityBase> capabilityBaseUpdateMode = new List<CapabilityBase>();
        private List<CapabilityBase> capabilityBaseFixUpdateMode = new List<CapabilityBase>();

        public static void Init(EffEntity effEntity)
        {
            sWindow ??= GetWindow<CapabilityView>();
            sWindow.titleContent.text = string.IsNullOrEmpty(effEntity.Name) ? "Entity" : effEntity.Name;
            sWindow.effEntity = effEntity;
            sWindow.shWorld = (SHWorld) effEntity.world;
            sWindow.Start();
            sWindow.Show();
        }

        private void Start()
        {
            capabilityBaseUpdateMode.Clear();
            capabilityBaseFixUpdateMode.Clear();
            shWorld.GetCapability(effEntity, capabilityBaseUpdateMode, capabilityBaseFixUpdateMode);
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            if (effEntity.State == IEntity.EntityState.IsClear)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            foreach (var capability in capabilityBaseUpdateMode)
            {
                EditorGUILayout.BeginHorizontal();
                var oldColor = GUI.contentColor;
                GUI.color = capability.IsActive ? Color.cyan : Color.white;
                if (capability.TagList != null)
                    GUI.color = shWorld.IsBindCapability(effEntity, capability.TagList) ? Color.yellow : GUI.color;
                EditorGUI.DrawRect(new Rect(0,0,200,50), new Color(0.15f, 0.15f, 0.15f));
                EditorGUILayout.LabelField(capability.GetType().Name);
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
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