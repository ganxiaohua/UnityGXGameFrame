using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_6000_3_OR_NEWER
using UnityEditor.Toolbars;
#else
using System.Reflection;
using UnityEngine.UIElements;
#endif

namespace GameFrame.Editor
{
    [InitializeOnLoad]
    public static class ToolbarExtensions
    {
        private static List<(GUIContent, Action)> customRightButtons = new List<(GUIContent, Action)>();
        private static List<(GUIContent, Action)> customLeftButtons = new List<(GUIContent, Action)>();

#if UNITY_6000_3_OR_NEWER
        private const string LeftToolbarPath = "GX框架/左侧工具";
        private const string RightToolbarPath = "GX框架/右侧工具";

        // Unity 6.3 replaces the old toolbar visual tree. Enable these groups from
        // the main toolbar's context menu; Unity persists their visibility/layout.
        [MainToolbarElement(LeftToolbarPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        private static IEnumerable<MainToolbarElement> CreateLeftToolbar()
        {
            foreach (var (content, action) in customLeftButtons)
                yield return CreateButton(content, action);
        }

        // Dock immediately after the built-in Play/Pause/Step group in the middle.
        [MainToolbarElement(RightToolbarPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 0)]
        private static IEnumerable<MainToolbarElement> CreateRightToolbar()
        {
            yield return new MainToolbarButton(new MainToolbarContent("审查器"),
                DialogueGraphWindow.OpenDialogueGraphWindow);
            foreach (var (content, action) in customRightButtons)
                yield return CreateButton(content, action);
        }

        private static MainToolbarButton CreateButton(GUIContent content, Action action)
        {
            return new MainToolbarButton(
                new MainToolbarContent(content.text, content.image as Texture2D, content.tooltip), action);
        }

        private static void ScheduleToolbarRefresh()
        {
            // Registrations can arrive after the toolbar is first created. Coalesce
            // them and rebuild once InitializeOnLoad constructors have completed.
            EditorApplication.delayCall -= RefreshToolbars;
            EditorApplication.delayCall += RefreshToolbars;
        }

        private static void RefreshToolbars()
        {
            MainToolbar.Refresh(LeftToolbarPath);
            MainToolbar.Refresh(RightToolbarPath);
        }
#else
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
#endif

        public static void RegisterRightButton(string name, Action action)
        {
            RegisterRightButton(name, string.Empty, action);
        }

        public static void RegisterRightButton(string name, string icon, Action action)
        {
            customRightButtons.Add((new GUIContent(name, EditorGUIUtility.FindTexture(icon)), action));
#if UNITY_6000_3_OR_NEWER
            ScheduleToolbarRefresh();
#endif
        }

        public static void RegisterLeftButton(string name, Action action)
        {
            RegisterLeftButton(name, string.Empty, action);
        }

        public static void RegisterLeftButton(string name, string icon, Action action)
        {
            customLeftButtons.Add((new GUIContent(name, EditorGUIUtility.FindTexture(icon)), action));
#if UNITY_6000_3_OR_NEWER
            ScheduleToolbarRefresh();
#endif
        }
    }
}
