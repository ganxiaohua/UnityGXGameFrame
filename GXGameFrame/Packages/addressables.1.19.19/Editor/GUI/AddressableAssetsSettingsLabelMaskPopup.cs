using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.GUI
{
    class LabelMaskPopupContent : PopupWindowContent
    {
        AddressableAssetSettings m_Settings;
        List<AddressableAssetEntry> m_Entries;
        Dictionary<string, int> m_LabelCount;
        List<string> m_Labels;

        GUIStyle m_ToggleMixed;

        GUIContent m_ManageLabelsButtonContent = EditorGUIUtility.TrIconContent("_Popup@2x", "Manage Labels");
        readonly GUIStyle m_ToolbarButtonStyle = "RL FooterButton";

        private GUIStyle m_HintLabelStyle;
        const string k_HintIdle = "Type to filter or add new label";
        const string k_HintCreateNewLabel = "<b>Return</b> to add '{0}'";
        const string k_HintSearchFoundIsEnabled = "<b>Return</b> to disable '{0}'";
        const string k_HintSearchFoundIsDisabled = "<b>Return</b> to enable '{0}'";
        
        SearchField m_SearchField;
        string m_SearchValue;
        List<GUIStyle> m_SearchStyles;
        Rect m_ActivatorRect;
        float m_LabelToggleControlRectHeight;
        float m_LabelToggleControlRectWidth;
        string m_ControlToFocus = null;

        int m_LastItemCount = -1;
        Vector2 m_Rect;
        
        public LabelMaskPopupContent(Rect activatorRect, AddressableAssetSettings settings, List<AddressableAssetEntry> e, Dictionary<string, int> count)
        {
            m_Settings = settings;
            m_Entries = e;
            m_LabelCount = count;
            m_SearchField = new SearchField();
            m_ActivatorRect = activatorRect;
            m_SearchStyles = new List<GUIStyle>();
            m_SearchStyles.Add(GetStyle("ToolbarSeachTextField"));
            m_SearchStyles.Add(GetStyle("ToolbarSeachCancelButton"));
            m_SearchStyles.Add(GetStyle("ToolbarSeachCancelButtonEmpty"));
            
            m_HintLabelStyle = new GUIStyle(UnityEngine.GUI.skin.label);
            m_HintLabelStyle.fontSize = 10;
            m_HintLabelStyle.fontStyle = FontStyle.Italic;
            m_HintLabelStyle.richText = true;
        }
        GUIStyle GetStyle(string styleName)
        {
            GUIStyle s = UnityEngine.GUI.skin.FindStyle(styleName);
            if (s == null)
                s = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (s == null)
            {
                Addressables.LogError("Missing built-in guistyle " + styleName);
                s = new GUIStyle();
            }
            return s;
        }

        public override Vector2 GetWindowSize()
        {
            if (m_Labels == null)
                m_Labels = GetLabelNamesOrderedSelectedFirst();
            
            if (m_LastItemCount != m_Labels.Count)
            {
                int maxLen = 0;
                string maxStr = "";
                for (int i = 0; i < m_Labels.Count; i++)
                {
                    var len = m_Labels[i].Length;
                    if (len > maxLen)
                    {
                        maxLen = len;
                        maxStr = m_Labels[i];
                    }
                }
                var content = new GUIContent(maxStr);
                var size = UnityEngine.GUI.skin.toggle.CalcSize(content);
                m_LabelToggleControlRectHeight = Mathf.Ceil(size.y + UnityEngine.GUI.skin.toggle.margin.top);
                m_LabelToggleControlRectWidth = size.x + 16;
                
                float width = Mathf.Clamp(Mathf.Max(size.x,m_ActivatorRect.width), 170, 500);
                float labelAreaHeight = m_Labels.Count * (m_LabelToggleControlRectHeight + UnityEngine.GUI.skin.toggle.margin.bottom);
                float toolbarAreaHeight = 30 + m_HintLabelStyle.CalcHeight(new GUIContent(maxStr), width);
                float paddingHeight = 6;
                float height = labelAreaHeight + toolbarAreaHeight + paddingHeight;
                height = Mathf.Clamp(height, 50, 300);
                m_Rect = new Vector2(width, height);
                m_LastItemCount = m_Labels.Count;
            }
            
            return m_Rect;
        }

        void SetLabelForEntries(string label, bool value)
        {
            m_Settings.SetLabelValueForEntries(m_Entries, label, value);
            m_LabelCount[label] = value ? m_Entries.Count : 0;
        }

        List<string> GetLabelNamesOrderedSelectedFirst()
        {
            int count;
            var labels = new List<string>(m_Settings.labelTable.labelNames.Count);
            List<string> disabledLabels = new List<string>();
            for (int i = 0; i < m_Settings.labelTable.labelNames.Count; ++i)
            {
                if (m_LabelCount != null)
                    m_LabelCount.TryGetValue(m_Settings.labelTable.labelNames[i], out count);
                else
                    count = m_Entries[0].labels.Contains(m_Settings.labelTable.labelNames[i]) ? m_Entries.Count : 0;
                
                if (count > 0)
                    labels.Add(m_Settings.labelTable.labelNames[i]);
                else
                    disabledLabels.Add(m_Settings.labelTable.labelNames[i]);
            }
            
            labels.AddRange(disabledLabels);
            return labels;
        }
        
        Vector2 m_ScrollPosition;
        public override void OnGUI(Rect fullRect)
        {
            if (m_Entries.Count == 0)
                return;
            int count = -1;
            
            if (m_Labels == null)
                m_Labels = GetLabelNamesOrderedSelectedFirst();

            var areaRect = new Rect(fullRect.xMin + 3, fullRect.yMin + 3, fullRect.width - 6, fullRect.height - 6);
            GUILayout.BeginArea(areaRect);
            
            GUILayoutUtility.GetRect(areaRect.width, 1);
            Rect barRect = EditorGUILayout.GetControlRect();
            var marginDown = GUILayoutUtility.GetRect(areaRect.width, 1);

            Rect plusRect = barRect;
            plusRect.width = plusRect.height;
            plusRect.x = (barRect.width - plusRect.width)+4;
            if (UnityEngine.GUI.Button(plusRect, m_ManageLabelsButtonContent, m_ToolbarButtonStyle))
            {
                EditorWindow.GetWindow<LabelWindow>(true).Intialize(m_Settings);
                editorWindow.Close();
            }

            Rect searchRect = barRect;
            float plusOffset = plusRect.width + 2;
            searchRect.width = searchRect.width - plusOffset;
            m_SearchValue = m_SearchField.OnGUI(searchRect, m_SearchValue, m_SearchStyles[0], m_SearchStyles[1], m_SearchStyles[2]);
            
            EditorGUI.BeginDisabledGroup(true);
            string labelText;
            int searchLabelIndex = m_Labels.IndexOf(m_SearchValue);
            if (searchLabelIndex >= 0)
            {
                if (m_LabelCount == null)
                    count = m_Entries[0].labels.Contains(m_SearchValue) ? m_Entries.Count : 0;
                else
                    m_LabelCount.TryGetValue(m_SearchValue, out count);
                labelText = string.Format(count == m_Entries.Count ? k_HintSearchFoundIsEnabled : k_HintSearchFoundIsDisabled, m_SearchValue);
            }
            else
                labelText = !string.IsNullOrEmpty(m_SearchValue) ? string.Format(k_HintCreateNewLabel, m_SearchValue) : k_HintIdle; 
            
            Rect hintRect = EditorGUILayout.GetControlRect(true, m_HintLabelStyle.CalcHeight(new GUIContent(labelText), fullRect.width), m_HintLabelStyle);
            hintRect.x -= 3;
            hintRect.width += 6;
            hintRect.y -= 3;
            EditorGUI.LabelField(hintRect, new GUIContent(labelText), m_HintLabelStyle);
            EditorGUI.EndDisabledGroup();
            
            if (Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) && m_SearchField.HasFocus())
            {
                if (!string.IsNullOrEmpty(m_SearchValue))
                {
                    if (searchLabelIndex >= 0)
                    {
                        if (count != m_Entries.Count)
                            SetLabelForEntries(m_SearchValue, true);
                        else
                            SetLabelForEntries(m_SearchValue, false);
                    }
                    else
                    {
                        m_Settings.AddLabel(m_SearchValue);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings);
                        SetLabelForEntries(m_SearchValue, true);
                        m_Labels.Insert(0, m_SearchValue);
                    }
                    
                    m_ControlToFocus = m_SearchValue;
                    UnityEngine.GUI.ScrollTo(new Rect(0,searchLabelIndex*19,0,0));
                    m_SearchValue = "";
                    m_LastItemCount = -1;
            
                    Event.current.Use();
                    GUIUtility.ExitGUI();
                    editorWindow.Repaint();
                }
            }
            
            var scrollViewHeight = areaRect.height - (hintRect.y + hintRect.height + 2);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, false, false);
            Vector2 yPositionDrawRange = new Vector2(m_ScrollPosition.y - 19, m_ScrollPosition.y + scrollViewHeight);

            for (int i = 0; i < m_Labels.Count; ++i)
            {
                var labelName = m_Labels[i];
                if (!string.IsNullOrEmpty(m_SearchValue))
                {
                    if (labelName.IndexOf(m_SearchValue, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                var toggleRect = EditorGUILayout.GetControlRect(GUILayout.Width(m_LabelToggleControlRectWidth), GUILayout.Height(m_LabelToggleControlRectHeight));
                if (toggleRect.height > 1)
                {
                    // only draw toggles if they are in view
                    if (toggleRect.y < yPositionDrawRange.x || toggleRect.y > yPositionDrawRange.y)
                        continue;
                }
                else continue;

                bool newState;
                if (m_LabelCount == null)
                    count = m_Entries[0].labels.Contains(labelName) ? m_Entries.Count : 0;
                else
                    m_LabelCount.TryGetValue(labelName, out count);

                bool oldState = count == m_Entries.Count;
                if (!(count == 0 || count == m_Entries.Count))
                    EditorGUI.showMixedValue = true;
                UnityEngine.GUI.SetNextControlName(labelName);
                newState = EditorGUI.ToggleLeft(toggleRect, new GUIContent(labelName), oldState);
                EditorGUI.showMixedValue = false;
                
                if (oldState != newState)
                    SetLabelForEntries(labelName, newState);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            
            if ( Event.current.type == EventType.Repaint &&
                m_Labels != null && m_ControlToFocus != null)
            {
                UnityEngine.GUI.FocusControl(m_ControlToFocus);
                m_ControlToFocus = null;
            }
        }
    }
}
