using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    /// <summary>
    /// Configuration GUI for addressable labels in <see cref="T:UnityEditor.AddressableAssets.Settings" />
    /// </summary>
    public class LabelWindow : EditorWindow
    {
        ReorderableList m_LabelNamesRl;
        private AddressableAssetSettings m_Settings;
        private Vector2 m_ScrollPosition;
        private int m_BorderSpacing = 7;

        //Edit menu
        int m_ActiveIndex = -1;
        bool m_IsEditing = false;
        string m_CurrentEdit;
        string m_OldName;

        /// <summary>
        /// Creates a new LabelWindow instance and retrieves label names from the given settings object.
        /// </summary>
        /// <param name="settings">The settings object.</param>
        public void Intialize(AddressableAssetSettings settings)
        {
            titleContent = new GUIContent("Addressables Labels");
            m_Settings = settings;

            var labels = m_Settings.labelTable.labelNames;
            m_LabelNamesRl = new ReorderableList(labels, typeof(string), true, false, true, true);
            m_LabelNamesRl.drawElementCallback = DrawLabelNamesCallback;
            m_LabelNamesRl.onAddDropdownCallback = OnAddLabel;
            m_LabelNamesRl.onRemoveCallback = OnRemoveLabel;
            m_LabelNamesRl.onSelectCallback = list =>
            {
                m_ActiveIndex = list.index;
                EndEditMenu();
            };
            m_LabelNamesRl.headerHeight = 0; // hide header completely

            m_ActiveIndex = -1;
            m_IsEditing = false;
        }
        
        void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.label);
            GUILayout.Space(m_BorderSpacing);
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            m_LabelNamesRl.DoLayoutList();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            HandleEvent(Event.current);
        }

        void HandleEvent(Event current)
        {
            if (m_ActiveIndex < 0 || m_Settings.labelTable.labelNames.Count == 0)
                return;

            if (current.type == EventType.ContextClick)
            {
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Edit"), false, () =>
                {
                    m_IsEditing = true;
                    m_CurrentEdit = m_Settings.labelTable.labelNames[m_ActiveIndex];
                    Repaint();
                });
                contextMenu.ShowAsContext();
                Repaint();
            }
            else if (m_IsEditing && (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter))
            {
                m_Settings.RenameLabel(m_OldName, m_CurrentEdit);
                EndEditMenu();
            }
            else if(current.type == EventType.MouseDown && m_IsEditing)
                EndEditMenu();
        }

        void EndEditMenu()
        {
            m_IsEditing = false;
            m_CurrentEdit = string.Empty;
            m_OldName = string.Empty;
            Repaint();
        }

        void DrawLabelNamesCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var oldName = m_Settings.labelTable.labelNames[index];

            if (m_IsEditing && index == m_ActiveIndex)
            {
                m_OldName = oldName;
                UnityEngine.GUI.SetNextControlName(m_OldName);
                m_CurrentEdit = EditorGUI.TextField(rect, m_CurrentEdit);
                UnityEngine.GUI.FocusControl(m_OldName);
            }
            else
                EditorGUI.LabelField(rect, oldName);
        }

        void OnRemoveLabel(ReorderableList list)
        {
            m_Settings.RemoveLabel(m_Settings.labelTable.labelNames[list.index]);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings);
        } 

        void OnAddLabel(Rect buttonRect, ReorderableList list)
        {
            buttonRect.x = 6;
            buttonRect.y -= 13;
            PopupWindow.Show(buttonRect, new LabelNamePopup(position.width, m_LabelNamesRl.elementHeight, m_Settings));
        }

        internal class LabelNamePopup : PopupWindowContent
        {
            internal float windowWidth;
            internal float rowHeight;
            internal string name;
            internal bool needsFocus = true;
            internal AddressableAssetSettings settings;

            public LabelNamePopup(float width, float rowHeight, AddressableAssetSettings settings)
            {
                this.windowWidth = width;
                this.rowHeight = rowHeight;
                this.settings = settings;
                name = this.settings.labelTable.GetUniqueLabelName("New Label");
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(windowWidth - 13f, rowHeight * 2.25f);
            }

            public override void OnGUI(Rect windowRect)
            {
                GUILayout.Space(5);
                Event evt = Event.current;
                bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);
                UnityEngine.GUI.SetNextControlName("LabelName");
                EditorGUIUtility.labelWidth = 80;
                name = EditorGUILayout.TextField("Label Name", name);
                if (needsFocus)
                {
                    needsFocus = false;
                    EditorGUI.FocusTextInControl("LabelName");
                }

                UnityEngine.GUI.enabled = name.Length != 0;
                if (GUILayout.Button("Save") || hitEnter)
                {
                    if (string.IsNullOrEmpty(name))
                        Debug.LogError("Cannot add empty label to Addressables label list");
                    else if (name != settings.labelTable.GetUniqueLabelName(name))
                        Debug.LogError("Label name '" + name + "' is already in the labels list.");
                    else if (name.Contains("[") && name.Contains("]"))
                        Debug.LogErrorFormat("Label name '{0}' cannot contain '[ ]'.", name);
                    else
                    {
                        settings.AddLabel(name);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
                    }

                    editorWindow.Close();
                }
            }
        }
    }
}
