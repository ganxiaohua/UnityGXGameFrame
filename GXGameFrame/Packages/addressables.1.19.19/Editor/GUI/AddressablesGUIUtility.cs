using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEditor.AddressableAssets.GUI
{
    internal struct FoldoutSessionStateValue
    {
        bool? m_Value;
        private string m_Key;

        public FoldoutSessionStateValue(string key)
        {
            m_Value = null;
            m_Key = key;
        }

        public bool IsActive
        {
            get
            {
                if (string.IsNullOrEmpty(m_Key))
                    throw new NullReferenceException("FoldoutSessionStateValue does not have a valid key set");
                
                if (m_Value.HasValue == false)
                    m_Value = SessionState.GetBool(m_Key, true);
                return m_Value.Value;
            }
            set
            {
                m_Value = value;
                SessionState.SetBool(m_Key, value);
            }
        }
    }
    
    internal class AddressablesGUIUtility
    {
        private static Dictionary<string, FoldoutSessionStateValue> m_CachedSessionStates = new Dictionary<string, FoldoutSessionStateValue>();
        
        

        internal static bool GetFoldoutValue(string stateKey)
        {
            if (m_CachedSessionStates.TryGetValue(stateKey, out var val))
                return val.IsActive;
            var foldoutState = new FoldoutSessionStateValue(stateKey);
            m_CachedSessionStates.Add(stateKey, foldoutState);
            return foldoutState.IsActive;
        }
        
        internal static void SetFoldoutValue(string stateKey, bool isActive)
        {
            if (m_CachedSessionStates.TryGetValue(stateKey, out var val))
            {
                val.IsActive = isActive;
                return;
            }
            var foldoutState = new FoldoutSessionStateValue(stateKey);
            foldoutState.IsActive = isActive;
            m_CachedSessionStates.Add(stateKey, foldoutState);
        }

        static Color HeaderBorderColor
        {
            get
            {
                float shade = EditorGUIUtility.isProSkin ? 0.12f : 0.6f;
                return new Color(shade, shade, shade, 1);
            }
        }
        
        static Color HeaderNormalColor
        {
            get
            {
                float shade = EditorGUIUtility.isProSkin ? 62f/255f : 205f/255f;
                return new Color(shade, shade, shade, 1);
            }
        }
        
        static Color HeaderHoverColor
        {
            get
            {
                float shade = EditorGUIUtility.isProSkin ? 70f/255f : 215f/255f;
                return new Color(shade, shade, shade, 1);
            }
        }
        
        public static bool FoldoutWithHelp(bool isActive, GUIContent content, Action helpAction = null)
        {
            Rect controlRect = EditorGUILayout.GetControlRect();
            GUIStyle iconStyle = UnityEngine.GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");
            if (helpAction != null)
            {
                Rect helpRect = controlRect;
                helpRect.x = controlRect.x + controlRect.width - helpRect.height;
                helpRect.width = helpRect.height;
                if (UnityEngine.GUI.Button(helpRect, EditorGUIUtility.IconContent("_Help"), iconStyle))
                    helpAction.Invoke();
            }

            bool isPressedDown = controlRect.Contains(UnityEngine.Event.current.mousePosition) 
                                 && UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown 
                                 && UnityEngine.Event.current.button == 0;
            if (isPressedDown)
            {
                isActive = !isActive;
                UnityEngine.Event.current.Use();
                UnityEngine.GUI.changed = true;
            }

            EditorGUI.Foldout(controlRect, isActive, content, false);
            return isActive;
        }

        public static bool BeginFoldoutHeaderGroupWithHelp(bool isActive, GUIContent content, Action helpAction = null, int indent = 0, Action<Rect> menuAction = null)
        {
            Rect headerRect = EditorGUILayout.GetControlRect();

            Rect bgRect = new Rect(headerRect);
            bgRect.x = 0;
            bgRect.width = EditorGUIUtility.currentViewWidth;
            bool isHover = bgRect.Contains(UnityEngine.Event.current.mousePosition);
            EditorGUI.DrawRect(bgRect, isHover ? HeaderHoverColor : HeaderNormalColor);
            
            bgRect.y = headerRect.y - 1;
            bgRect.height = 1;
            Color color = HeaderBorderColor;
            EditorGUI.DrawRect(bgRect, color);
            bgRect.y = headerRect.y + headerRect.height + 1;
            bgRect.height = 0.5f;
            EditorGUI.DrawRect(bgRect, color);
            headerRect.y += 1;
            
            if (indent > 0)
            {
                headerRect.x += indent;
                headerRect.width -= indent;
            }

            GUIStyle iconStyle = UnityEngine.GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");
            if (menuAction != null)
            {
                Rect menuButtonRect = headerRect;
                menuButtonRect.x = headerRect.x + headerRect.width - menuButtonRect.height;
                menuButtonRect.width = menuButtonRect.height;
                if (UnityEngine.GUI.Button(menuButtonRect, EditorGUIUtility.IconContent("_Popup"), iconStyle))
                    menuAction.Invoke(menuButtonRect);
            }
            if (helpAction != null)
            {
                Rect helpRect = headerRect;
                helpRect.x = headerRect.x + headerRect.width - helpRect.height;
                if (menuAction != null)
                    helpRect.x -= helpRect.height;
                helpRect.width = helpRect.height;
                if (UnityEngine.GUI.Button(helpRect, EditorGUIUtility.IconContent("_Help"), iconStyle))
                    helpAction.Invoke();
            }

            bool isPressedDown = isHover && UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown && UnityEngine.Event.current.button == 0;
            if (isPressedDown)
            {
                isActive = !isActive;
                UnityEngine.Event.current.Use();
                UnityEngine.GUI.changed = true;
            }

            EditorGUI.Foldout(headerRect, isActive, content, false);
            if (isActive)
                GUILayout.Space(6f);
            return isActive;
        }
    }
}