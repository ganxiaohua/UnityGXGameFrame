using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtensions
    {
        private static List<(GUIContent, Action)> customButtons = new List<(GUIContent, Action)>();

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
                    container.onGUIHandler += OnToolbar;
                    parent.Add(container);
                    zone.Add(parent);
                }
            };
        }

        private static void OnToolbar()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Goto Main"))
                SaveCurrentSceneIfDirty("Assets/GXGame/Main.unity");
            foreach (var (content, action) in customButtons)
            {
                if (GUILayout.Button(content))
                    action.Invoke();
            }
            GUILayout.EndHorizontal();
        }


        private static void SaveCurrentSceneIfDirty(string gotoScene)
        {
            var currentScene = SceneManager.GetActiveScene();
            if (gotoScene.Contains(currentScene.name))
                return;
            if (currentScene.isDirty)
            {
                bool shouldSave = EditorUtility.DisplayDialog(
                        "保存场景",
                        "检测到场景有修改，是否保存？",
                        "保存",
                        "不保存"
                );

                if (shouldSave)
                {
                    EditorSceneManager.SaveScene(currentScene);
                }
            }

            EditorSceneManager.OpenScene(gotoScene);
        }

        public static void RegisterButton(string name, Action action)
        {
            RegisterButton(name, string.Empty, action);
        }

        public static void RegisterButton(string name, string icon, Action action)
        {
            customButtons.Add((new GUIContent(name, EditorGUIUtility.FindTexture(icon)), action));
        }
    }
}