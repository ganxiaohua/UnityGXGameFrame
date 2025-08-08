using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtensions
    {
        private static List<(GUIContent, Action)> customRightButtons = new List<(GUIContent, Action)>();
        private static List<(GUIContent, Action)> customLeftButtons = new List<(GUIContent, Action)>();

        static ToolbarExtensions()
        {
            EditorApplication.delayCall += () =>
            {
                var type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
                var toolbars = Resources.FindObjectsOfTypeAll(type);
                var toolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;
                if (toolbar != null)
                {
                    var rootField = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var root = rootField?.GetValue(toolbar) as VisualElement;
                    var zone = root.Q("ToolbarZoneRightAlign");
                    var parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    var container = new IMGUIContainer();
                    container.onGUIHandler += OnRightToolbar;
                    parent.Add(container);
                    zone.Add(parent);
                    zone = root.Q("ToolbarZoneLeftAlign");
                    parent = new VisualElement()
                    {
                        style =
                        {
                            flexGrow = 1,
                            flexDirection = FlexDirection.Row,
                        }
                    };
                    container = new IMGUIContainer();
                    container.onGUIHandler += OnLeftToolbar;
                    parent.Add(container);
                    zone.Add(parent);
                }
            };
        }

        private static void OnRightToolbar()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("审查器"))
                DialogueGraphWindow.OpenDialogueGraphWindow();
            foreach (var (content, action) in customRightButtons)
            {
                if (GUILayout.Button(content))
                    action.Invoke();
            }

            GUILayout.EndHorizontal();
        }

        private static void OnLeftToolbar()
        {
            GUILayout.BeginHorizontal();
            foreach (var (content, action) in customLeftButtons)
            {
                if (GUILayout.Button(content))
                    action.Invoke();
            }

            GUILayout.EndHorizontal();
        }


        public static void RegisterRightButton(string name, Action action)
        {
            RegisterRightButton(name, string.Empty, action);
        }

        public static void RegisterRightButton(string name, string icon, Action action)
        {
            customRightButtons.Add((new GUIContent(name, EditorGUIUtility.FindTexture(icon)), action));
        }

        public static void RegisterLeftButton(string name, Action action)
        {
            RegisterLeftButton(name, string.Empty, action);
        }

        public static void RegisterLeftButton(string name, string icon, Action action)
        {
            customLeftButtons.Add((new GUIContent(name, EditorGUIUtility.FindTexture(icon)), action));
        }
    }
}