using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public static class EditorGUIHelper
    {
        private static Func<Rect, string, string> _editorGUISearchField;

        private static Func<Rect, string, string> EditorGUISearchField
        {
            get
            {
                if (_editorGUISearchField == null)
                {
                    var method = typeof(EditorGUI).GetMethod("SearchField", BindingFlags.Static | BindingFlags.NonPublic);
                    _editorGUISearchField = Delegate.CreateDelegate(typeof(Func<Rect, string, string>), method) as Func<Rect, string, string>;
                }

                return _editorGUISearchField;
            }
        }

        public static string SearchField(Rect position, string text)
        {
            return EditorGUISearchField(position, text);
        }

        public static void DisplayCustomMenu(Rect position, string[] menus, EditorUtility.SelectMenuItemFunction callback)
        {
            var enables = new bool[menus.Length];
            var separators = new bool[menus.Length];
            for (int i = 0; i < enables.Length; i++)
            {
                enables[i] = true;
                separators[i] = false;
            }

            var selecteds = new int[] { };
            EditorUtility.DisplayCustomMenuWithSeparators(position, menus, enables, separators, selecteds, callback, null);
        }

        public static void DisplayCustomMenu(Rect position, string[] menus, bool[] enables, EditorUtility.SelectMenuItemFunction callback)
        {
            var separators = new bool[menus.Length];
            for (int i = 0; i < enables.Length; i++)
            {
                separators[i] = false;
            }

            var selecteds = new int[] { };
            EditorUtility.DisplayCustomMenuWithSeparators(position, menus, enables, separators, selecteds, callback, null);
        }
    }
}