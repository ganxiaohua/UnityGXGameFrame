using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;

namespace UnityEditor.AddressableAssets.GUI
{
    using Object = UnityEngine.Object;

    [CustomPropertyDrawer(typeof(AssetReference), true)]
    class AssetReferenceDrawer : PropertyDrawer
    {
        public string newGuid;
        public string newGuidPropertyPath;
        internal string m_AssetName;
        internal Rect assetDropDownRect;
        internal const string noAssetString = "None (AddressableAsset)";
        internal const string forceAddressableString = "Make Addressable - ";
        internal AssetReference m_AssetRefObject;
        internal GUIContent m_label;
        internal bool m_ReferencesSame = true;
        internal List<AssetReferenceUIRestrictionSurrogate> m_Restrictions = null;
        SubassetPopup m_SubassetPopup;
        private Texture2D m_CaretTexture = null;
        internal const string k_FieldControlName = "AssetReferenceField";

        internal List<AssetReferenceUIRestrictionSurrogate> Restrictions => m_Restrictions;
        /// <summary>
        /// Validates that the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="path">The path to the asset in question.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public bool ValidateAsset(string path)
        {
            return AssetReferenceDrawerUtilities.ValidateAsset(m_AssetRefObject, Restrictions, path);
        }

        internal bool ValidateAsset(IReferenceEntryData entryData)
        {
            return AssetReferenceDrawerUtilities.ValidateAsset(m_AssetRefObject, Restrictions, entryData);
        }

        /*
        * The AssetReference class is not a Unity.Object or a base type so a lot of SerializedProperty's
        * functionalities doesn't work, because type-checking is done in the API to check whether an operation
        * can be executed or not. In the engine, one of the way changes are detected is when a new value is set
        * through the class' value setters (see MarkPropertyModified() in SerializedProperty.cpp). So in order to
        * trigger a change, we modify a sub-property instead.
        */
        void TriggerOnValidate(SerializedProperty property)
        {
            if (property != null)
            {
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();

                // This is actually what triggers the OnValidate() method.
                // Since 'm_EditorAssetChanged' is of a recognized type and is a sub-property of AssetReference, both
                // are flagged as changed and OnValidate() is called.
                property.FindPropertyRelative("m_EditorAssetChanged").boolValue = false;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || label == null)
            {
                Debug.LogError("Error rendering drawer for AssetReference property.");
                return;
            }

            string labelText = label.text;
            m_ReferencesSame = true;
            m_AssetRefObject = property.GetActualObjectForSerializedProperty<AssetReference>(fieldInfo, ref labelText);
            labelText = ObjectNames.NicifyVariableName(labelText);
            if (labelText != label.text || string.IsNullOrEmpty(label.text))
            {
                label = new GUIContent(labelText, label.tooltip);
            }
            m_label = label;

            if (m_AssetRefObject == null)
            {
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            if (m_Restrictions == null)
                m_Restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);
            string guid = m_AssetRefObject.AssetGUID;
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;

            var isNotAddressable = ApplySelectionChanges(property, aaSettings, ref guid);

            assetDropDownRect = EditorGUI.PrefixLabel(position, label);
            var nameToUse = AssetReferenceDrawerUtilities.GetNameForAsset(ref m_ReferencesSame, property, isNotAddressable, fieldInfo, m_label.text);

            DrawSubassets(property);

            bool isDragging = Event.current.type == EventType.DragUpdated && position.Contains(Event.current.mousePosition);
            bool isDropping = Event.current.type == EventType.DragPerform && position.Contains(Event.current.mousePosition);

            DrawControl(property, nameToUse, isNotAddressable, guid);

            HandleDragAndDrop(property, isDragging, isDropping, guid);

            EditorGUI.EndProperty();
        }

        private void DrawSubassets(SerializedProperty property)
        {
            if (m_AssetRefObject.editorAsset != null && m_ReferencesSame)
            {
                List<Object> subAssets = null;
                bool hasSubAssets = !string.IsNullOrEmpty(m_AssetRefObject.SubObjectName);
                if (!hasSubAssets)
                {
                    subAssets = AssetReferenceDrawerUtilities.GetSubAssetsList(m_AssetRefObject);
                    hasSubAssets = subAssets.Count > 1;
                }

                if (hasSubAssets)
                {
                    assetDropDownRect = DrawSubAssetsControl(property, subAssets);
                }
            }
        }

        bool ApplySelectionChanges(SerializedProperty property, AddressableAssetSettings aaSettings, ref string guid)
        {
            var checkToForceAddressable = string.Empty;
            if (!string.IsNullOrEmpty(newGuid) && newGuidPropertyPath == property.propertyPath)
            {
                if (newGuid == noAssetString)
                {
                    if (AssetReferenceDrawerUtilities.SetObject(ref m_AssetRefObject, ref m_ReferencesSame, property, null, fieldInfo, m_label.text, out guid))
                        TriggerOnValidate(property);
                    newGuid = string.Empty;
                }
                else if (newGuid == forceAddressableString)
                {
                    checkToForceAddressable = guid;
                    newGuid = string.Empty;
                }
                else if (guid != newGuid)
                {
                    if (AssetReferenceDrawerUtilities.SetObject(ref m_AssetRefObject, ref m_ReferencesSame, property, AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(newGuid)), fieldInfo, m_label.text, out guid))
                    {
                        checkToForceAddressable = newGuid;
                        TriggerOnValidate(property);
                    }
                    newGuid = string.Empty;
                }
            }

            bool isNotAddressable = false;
            m_AssetName = noAssetString;
            if (aaSettings != null && !string.IsNullOrEmpty(guid))
            {
                isNotAddressable = AssetReferenceDrawerUtilities.CheckForNewEntry(ref m_AssetName, aaSettings, guid, checkToForceAddressable);
            }

            return isNotAddressable;
        }

        private void DrawControl(SerializedProperty property, string nameToUse, bool isNotAddressable, string guid)
        {
            float pickerWidth = 20f;
            Rect pickerRect = assetDropDownRect;
            pickerRect.width = pickerWidth;
            pickerRect.x = assetDropDownRect.xMax - pickerWidth;

            bool isPickerPressed = Event.current.type == EventType.MouseDown && Event.current.button == 0 && pickerRect.Contains(Event.current.mousePosition);
            bool isEnterKeyPressed = Event.current.type == EventType.KeyDown && Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return);

            var asset = m_AssetRefObject?.editorAsset;
            if (asset != null && m_ReferencesSame)
            {
                float iconHeight = EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing * 3;
                Vector2 iconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(iconHeight, iconHeight));
                string assetPath = AssetDatabase.GUIDToAssetPath(m_AssetRefObject.AssetGUID);
                Texture2D assetIcon = AssetDatabase.GetCachedIcon(assetPath) as Texture2D;

                UnityEngine.GUI.SetNextControlName(k_FieldControlName);
                if (EditorGUI.DropdownButton(assetDropDownRect, new GUIContent(nameToUse, assetIcon), FocusType.Keyboard, EditorStyles.objectField))
                {
                    if (Event.current.clickCount == 1)
                    {
                        UnityEngine.GUI.FocusControl(k_FieldControlName);
                        EditorGUIUtility.PingObject(asset);
                    }
                    if (Event.current.clickCount == 2)
                    {
                        AssetDatabase.OpenAsset(asset);
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUIUtility.SetIconSize(iconSize);
            }
            else
            {
                UnityEngine.GUI.SetNextControlName(k_FieldControlName);
                if (EditorGUI.DropdownButton(assetDropDownRect, new GUIContent(nameToUse), FocusType.Keyboard, EditorStyles.objectField))
                    UnityEngine.GUI.FocusControl(k_FieldControlName);
            }

            DrawCaret(pickerRect);

            bool enterKeyRequestsPopup = isEnterKeyPressed && (k_FieldControlName == UnityEngine.GUI.GetNameOfFocusedControl());
            if (isPickerPressed || enterKeyRequestsPopup)
            {
                newGuidPropertyPath = property.propertyPath;
                var nonAddressedOption = isNotAddressable ? m_AssetName : string.Empty;
                PopupWindow.Show(assetDropDownRect, new AssetReferencePopup(this, guid, nonAddressedOption, enterKeyRequestsPopup));
            }
        }

        private void DrawCaret(Rect pickerRect)
        {
            if (m_CaretTexture == null)
            {
                string caretIconPath = EditorGUIUtility.isProSkin
                    ? @"Packages\com.unity.addressables\Editor\Icons\PickerDropArrow-Pro.png"
                    : @"Packages\com.unity.addressables\Editor\Icons\PickerDropArrow-Personal.png";

                if (File.Exists(caretIconPath))
                {
                    m_CaretTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(caretIconPath, typeof(Texture2D));
                }
            }

            if (m_CaretTexture != null)
            {
                UnityEngine.GUI.DrawTexture(pickerRect, m_CaretTexture, ScaleMode.ScaleToFit);
            }
        }

        private void HandleDragAndDrop(SerializedProperty property, bool isDragging, bool isDropping, string guid)
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            //During the drag, doing a light check on asset validity.  The in-depth check happens during a drop, and should include a log if it fails.
            var rejectedDrag = false;
            if (isDragging)
            {
                if (aaSettings == null)
                    rejectedDrag = true;
                else
                {
                    var aaEntries = DragAndDrop.GetGenericData("AssetEntryTreeViewItem") as List<AssetEntryTreeViewItem>;
                    rejectedDrag = AssetReferenceDrawerUtilities.ValidateDrag(m_AssetRefObject, Restrictions, aaEntries, DragAndDrop.objectReferences, DragAndDrop.paths);
                }
                DragAndDrop.visualMode = rejectedDrag ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Copy;
            }

            if (!rejectedDrag && isDropping)
            {
                var aaEntries = DragAndDrop.GetGenericData("AssetEntryTreeViewItem") as List<AssetEntryTreeViewItem>;
                if (aaEntries != null)
                {
                    if (aaEntries.Count == 1)
                    {
                        var item = aaEntries[0];
                        if (item.entry != null)
                        {
                            if (item.entry.IsInResources)
                                Addressables.LogWarning("Cannot use an AssetReference on an asset in Resources. Move asset out of Resources first.");
                            else
                            {
                                if (AssetReferenceDrawerUtilities.SetObject(ref m_AssetRefObject, ref m_ReferencesSame, property, item.entry.TargetAsset, fieldInfo, m_label.text, out guid))
                                    TriggerOnValidate(property);
                            }
                        }
                    }
                }
                else
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length == 1)
                    {
                        var path = DragAndDrop.paths[0];
                        DragAndDropNotFromAddressableGroupWindow(path, guid, property, aaSettings);
                    }
                }
            }
        }

        internal void DragAndDropNotFromAddressableGroupWindow(string path, string guid, SerializedProperty property, AddressableAssetSettings aaSettings)
        {
            if (AddressableAssetUtility.IsInResources(path))
                Addressables.LogWarning("Cannot use an AssetReference on an asset in Resources. Move asset out of Resources first. ");
            else if (!AddressableAssetUtility.IsPathValidForEntry(path))
                Addressables.LogWarning("Dragged asset is not valid as an Asset Reference. " + path);
            else
            {
                Object obj;
                if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length == 1)
                    obj = DragAndDrop.objectReferences[0];
                else
                    obj = AssetDatabase.LoadAssetAtPath<Object>(path);

                if (AssetReferenceDrawerUtilities.SetObject(ref m_AssetRefObject, ref m_ReferencesSame, property, obj, fieldInfo, m_label.text, out guid))
                {
                    TriggerOnValidate(property);
                    aaSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);
                    var entry = aaSettings.FindAssetEntry(guid);
                    if (entry == null && !string.IsNullOrEmpty(guid))
                    {
                        string assetName;
                        if (!aaSettings.IsAssetPathInAddressableDirectory(path, out assetName))
                        {
                            aaSettings.CreateOrMoveEntry(guid, aaSettings.DefaultGroup);
                            newGuid = guid;
                        }
                    }
                }
            }
        }

        private Rect DrawSubAssetsControl(SerializedProperty property, List<Object> subAssets)
        {
            assetDropDownRect = new Rect(assetDropDownRect.position, new Vector2(assetDropDownRect.width / 2, assetDropDownRect.height));
            var objRect = new Rect(assetDropDownRect.xMax, assetDropDownRect.y, assetDropDownRect.width, assetDropDownRect.height);
            float pickerWidth = 20f;
            Rect pickerRect = objRect;
            pickerRect.width = pickerWidth;
            pickerRect.x = objRect.xMax - pickerWidth;
            bool multipleSubassets = false;

            // Check if targetObjects have multiple different selected
            if (property.serializedObject.targetObjects.Length > 1)
                multipleSubassets = AssetReferenceDrawerUtilities.CheckTargetObjectsSubassetsAreDifferent(property, m_AssetRefObject.SubObjectName, fieldInfo, m_label.text);

            bool isPickerPressed = Event.current.type == EventType.MouseDown && Event.current.button == 0 && pickerRect.Contains(Event.current.mousePosition);
            if (isPickerPressed)
            {
                // Do custom popup with scroll to pick subasset
                if (m_SubassetPopup == null || m_SubassetPopup.m_property != property)
                {
                    m_SubassetPopup = CreateSubAssetPopup(property, subAssets ?? AssetReferenceDrawerUtilities.GetSubAssetsList(m_AssetRefObject));
                }

                PopupWindow.Show(objRect, m_SubassetPopup);
            }

            if (m_SubassetPopup != null && m_SubassetPopup.SelectionChanged)
            {
                m_SubassetPopup.UpdateSubAssets();
            }

            // Show selected name
            GUIContent nameSelected = new GUIContent("--");
            if (!multipleSubassets)
                nameSelected.text = AssetReferenceDrawerUtilities.FormatName(m_AssetRefObject.SubObjectName);
            UnityEngine.GUI.Box(objRect, nameSelected, EditorStyles.objectField);

            // Draw picker arrow
            DrawCaret(pickerRect);

            return assetDropDownRect;
        }

        internal void GetSelectedSubassetIndex(List<Object> subAssets, out int selIndex, out string[] objNames)
        {
            var subAssetNames = subAssets.Select(sa => sa == null ? "<none>" : $"{AssetReferenceDrawerUtilities.FormatName(sa.name)}:{sa.GetType()}").ToList();
            objNames = subAssetNames.ToArray();

            selIndex = subAssetNames.IndexOf($"{m_AssetRefObject.SubObjectName}:{m_AssetRefObject.SubOjbectType}");
            if (selIndex == -1)
                selIndex = 0;
        }

        SubassetPopup CreateSubAssetPopup(SerializedProperty property, List<Object> subAssets)
        {
            GetSelectedSubassetIndex(subAssets, out int selIndex, out string[] objNames);
            return new SubassetPopup(selIndex, objNames, subAssets, property, this);
        }
    }

    class SubassetPopup : PopupWindowContent
    {
        internal int SelectedIndex = 0;
        internal SerializedProperty m_property;
        private string[] m_objNames;
        private List<Object> m_subAssets;
        private AssetReferenceDrawer m_drawer;
        private Vector2 m_scrollPosition;
        bool selectionChanged = false;

        internal SubassetPopup(int selIndex, string[] objNames, List<Object> subAssets, SerializedProperty property, AssetReferenceDrawer drawer)
        {
            SelectedIndex = selIndex;
            m_objNames = objNames;
            m_property = property;
            m_drawer = drawer;
            m_subAssets = subAssets;
        }

        public bool SelectionChanged => selectionChanged;
        public void UpdateSubAssets()
        {
            if (selectionChanged)
            {
                if (!AssetReferenceDrawerUtilities.SetSubAssets(m_property, m_subAssets[SelectedIndex], m_drawer.fieldInfo, m_drawer.m_label.text))
                {
                    Debug.LogError("Unable to set all of the objects selected subassets");
                }
                else
                {
                    m_property.serializedObject.ApplyModifiedProperties();
                    m_property.serializedObject.Update();
                    m_property.FindPropertyRelative("m_EditorAssetChanged").boolValue = false;
                }
                selectionChanged = false;
            }
        }

        public override void OnGUI(Rect rect)
        {
            var buttonStyle = new GUIStyle();
            buttonStyle.fontStyle = FontStyle.Normal;
            buttonStyle.fontSize = 12;
            buttonStyle.contentOffset = new Vector2(10, 0);
            buttonStyle.normal.textColor = Color.white;

            EditorGUILayout.BeginVertical();
            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition, GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            for (int i = 0; i < m_objNames.Length; i++)
            {
                if (GUILayout.Button(m_objNames[i], buttonStyle))
                {
                    if (SelectedIndex != i)
                    {
                        SelectedIndex = i;
                        selectionChanged = true;
                    }
                    PopupWindow.focusedWindow.Close();
                    break;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    class AssetReferencePopup : PopupWindowContent
    {
        AssetReferenceTreeView m_Tree;
        TreeViewState m_TreeState;
        bool m_ShouldClose;

        void ForceClose()
        {
            m_ShouldClose = true;
        }

        string m_CurrentName = string.Empty;
        AssetReferenceDrawer m_Drawer;
        string m_GUID;
        string m_NonAddressedAsset;

        SearchField m_SearchField;
        bool m_OpenedByEnterKey;

        internal AssetReferencePopup(AssetReferenceDrawer drawer, string guid, string nonAddressedAsset, bool openedByEnterKey)
        {
            m_Drawer = drawer;
            m_GUID = guid;
            m_NonAddressedAsset = nonAddressedAsset;
            m_SearchField = new SearchField();
            m_ShouldClose = false;
            m_OpenedByEnterKey = openedByEnterKey;
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 result = base.GetWindowSize();
            result.x = m_Drawer.assetDropDownRect.width;
            return result;
        }

        public override void OnGUI(Rect rect)
        {
            int border = 4;
            int topPadding = 12;
            int searchHeight = 20;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainTop = topPadding + searchHeight + border;
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2, rect.height - remainTop - border);

            bool isEnterKeyPressed = Event.current.type == EventType.KeyDown && Event.current.isKey && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return);

            m_CurrentName = m_SearchField.OnGUI(searchRect, m_CurrentName);

            if (m_Tree == null)
            {
                if (m_TreeState == null)
                    m_TreeState = new TreeViewState();
                m_Tree = new AssetReferenceTreeView(m_TreeState, m_Drawer, this, m_GUID, m_NonAddressedAsset);
                m_Tree.Reload();
            }

            m_Tree.searchString = m_CurrentName;
            m_Tree.OnGUI(remainingRect);

            if (m_ShouldClose || (isEnterKeyPressed && m_OpenedByEnterKey))
            {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
            }
        }

        sealed class AssetRefTreeViewItem : TreeViewItem
        {
            public string AssetPath;

            private string m_Guid;
            public string Guid
            {
                get
                {
                    if (string.IsNullOrEmpty(m_Guid))
                        m_Guid = AssetDatabase.AssetPathToGUID(AssetPath);
                    return m_Guid;
                }
            }

            public AssetRefTreeViewItem(int id, int depth, string displayName, string path)
                : base(id, depth, displayName)
            {
                AssetPath = path;
                icon = AssetDatabase.GetCachedIcon(path) as Texture2D;
            }
        }

        internal class AssetReferenceTreeView : TreeView
        {
            AssetReferenceDrawer m_Drawer;
            AssetReferencePopup m_Popup;
            string m_GUID;
            string m_NonAddressedAsset;
            Texture2D m_WarningIcon;

            public AssetReferenceTreeView(TreeViewState state, AssetReferenceDrawer drawer, AssetReferencePopup popup, string guid, string nonAddressedAsset)
                : base(state)
            {
                m_Drawer = drawer;
                m_Popup = popup;
                showBorder = true;
                showAlternatingRowBackgrounds = true;
                m_GUID = guid;
                m_NonAddressedAsset = nonAddressedAsset;
                m_WarningIcon = EditorGUIUtility.FindTexture("console.warnicon");
            }

            protected override bool CanMultiSelect(TreeViewItem item)
            {
                return false;
            }

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                if (selectedIds != null && selectedIds.Count == 1)
                {
                    var assetRefItem = FindItem(selectedIds[0], rootItem) as AssetRefTreeViewItem;
                    if (assetRefItem != null && !string.IsNullOrEmpty(assetRefItem.AssetPath))
                    {
                        m_Drawer.newGuid = assetRefItem.Guid;
                        if (string.IsNullOrEmpty(m_Drawer.newGuid))
                            m_Drawer.newGuid = assetRefItem.AssetPath;
                    }
                    else
                    {
                        m_Drawer.newGuid = AssetReferenceDrawer.noAssetString;
                    }

                    m_Popup.ForceClose();
                }
            }

            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                if (string.IsNullOrEmpty(searchString))
                {
                    return base.BuildRows(root);
                }

                List<TreeViewItem> rows = new List<TreeViewItem>();

                foreach (var child in rootItem.children)
                {
                    if (child.displayName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                        rows.Add(child);
                }

                return rows;
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(-1, -1);

                var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
                if (aaSettings == null)
                {
                    var message = "Use 'Window->Addressables' to initialize.";
                    root.AddChild(new AssetRefTreeViewItem(message.GetHashCode(), 0, message, string.Empty));
                }
                else
                {
                    if (!string.IsNullOrEmpty(m_NonAddressedAsset))
                    {
                        var item = new AssetRefTreeViewItem(m_NonAddressedAsset.GetHashCode(), 0,
                            AssetReferenceDrawer.forceAddressableString + m_NonAddressedAsset, AssetReferenceDrawer.forceAddressableString);
                        item.icon = m_WarningIcon;
                        root.AddChild(item);
                    }

                    root.AddChild(new AssetRefTreeViewItem(AssetReferenceDrawer.noAssetString.GetHashCode(), 0, AssetReferenceDrawer.noAssetString, string.Empty));
                    var allAssets = new List<IReferenceEntryData>();
                    aaSettings.GatherAllAssetReferenceDrawableEntries(allAssets);
                    foreach (var entry in allAssets)
                    {
                        if (!entry.IsInResources &&
                            m_Drawer.ValidateAsset(entry))
                        {
                            var child = new AssetRefTreeViewItem(entry.AssetPath.GetHashCode(), 0, entry.address, entry.AssetPath);
                            root.AddChild(child);
                        }
                    }
                }

                return root;
            }
        }
    }

    /// <summary>
    /// Used to manipulate data from a serialized property.
    /// </summary>
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Used to extract the target object from a serialized property.
        /// </summary>
        /// <typeparam name="T">The type of the object to extract.</typeparam>
        /// <param name="property">The property containing the object.</param>
        /// <param name="field">The field data.</param>
        /// <param name="label">The label name.</param>
        /// <returns>Returns the target object type.</returns>
        public static T GetActualObjectForSerializedProperty<T>(this SerializedProperty property, FieldInfo field, ref string label)
        {
            try
            {
                if (property == null || field == null)
                    return default(T);
                var serializedObject = property.serializedObject;
                if (serializedObject == null)
                {
                    return default(T);
                }

                var targetObject = serializedObject.targetObject;

                if (property.depth > 0)
                {
                    var slicedName = property.propertyPath.Split('.').ToList();
                    List<int> arrayCounts = new List<int>();
                    for (int index = 0; index < slicedName.Count; index++)
                    {
                        arrayCounts.Add(-1);
                        var currName = slicedName[index];
                        if (currName.EndsWith("]"))
                        {
                            var arraySlice = currName.Split('[', ']');
                            if (arraySlice.Length >= 2)
                            {
                                arrayCounts[index - 2] = Convert.ToInt32(arraySlice[1]);
                                slicedName[index] = string.Empty;
                                slicedName[index - 1] = string.Empty;
                            }
                        }
                    }

                    while (string.IsNullOrEmpty(slicedName.Last()))
                    {
                        int i = slicedName.Count - 1;
                        slicedName.RemoveAt(i);
                        arrayCounts.RemoveAt(i);
                    }

                    if (property.propertyPath.EndsWith("]"))
                    {
                        var slice = property.propertyPath.Split('[', ']');
                        if (slice.Length >= 2)
                            label = "Element " + slice[slice.Length - 2];
                    }

                    return DescendHierarchy<T>(targetObject, slicedName, arrayCounts, 0);
                }

                var obj = field.GetValue(targetObject);
                return (T)obj;
            }
            catch
            {
                return default(T);
            }
        }

        static T DescendHierarchy<T>(object targetObject, List<string> splitName, List<int> splitCounts, int depth)
        {
            if (depth >= splitName.Count)
                return default(T);

            var currName = splitName[depth];

            if (string.IsNullOrEmpty(currName))
                return DescendHierarchy<T>(targetObject, splitName, splitCounts, depth + 1);

            int arrayIndex = splitCounts[depth];

            var newField = targetObject.GetType().GetField(currName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (newField == null)
            {
                Type baseType = targetObject.GetType().BaseType;
                while (baseType != null && newField == null)
                {
                    newField = baseType.GetField(currName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    baseType = baseType.BaseType;
                }
            }

            var newObj = newField.GetValue(targetObject);
            if (depth == splitName.Count - 1)
            {
                T actualObject = default(T);
                if (arrayIndex >= 0)
                {
                    if (newObj.GetType().IsArray && ((System.Array)newObj).Length > arrayIndex)
                        actualObject = (T)((System.Array)newObj).GetValue(arrayIndex);

                    var newObjList = newObj as IList;
                    if (newObjList != null && newObjList.Count > arrayIndex)
                    {
                        actualObject = (T)newObjList[arrayIndex];

                        //if (actualObject == null)
                        //    actualObject = new T();
                    }
                }
                else
                {
                    actualObject = (T)newObj;
                }

                return actualObject;
            }
            else if (arrayIndex >= 0)
            {
                if (newObj is IList)
                {
                    IList list = (IList)newObj;
                    newObj = list[arrayIndex];
                }
                else if (newObj is System.Array)
                {
                    System.Array a = (System.Array)newObj;
                    newObj = a.GetValue(arrayIndex);
                }
            }

            return DescendHierarchy<T>(newObj, splitName, splitCounts, depth + 1);
        }
    }

    /// <summary>
    /// Used to restrict a class to only allow items with specific labels.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AssetReferenceSurrogateAttribute : Attribute
    {
        /// <summary>
        /// The type of the attribute.
        /// </summary>
        public Type TargetType;

        /// <summary>
        /// Construct a new AssetReferenceSurrogateAttribute.
        /// </summary>
        /// <param name="type">The Type of the class in question.</param>
        public AssetReferenceSurrogateAttribute(Type type)
        {
            TargetType = type;
        }
    }

    /// <summary>
    /// Surrogate to AssetReferenceUIRestriction.
    /// This surrogate class provides the editor-side implementation of AssetReferenceUIRestriction attribute
    /// Used to restrict an AssetReference field or property to only allow items with specific labels. This is only enforced through the UI.
    /// </summary>
    [AssetReferenceSurrogate(typeof(AssetReferenceUIRestriction))]
    public class AssetReferenceUIRestrictionSurrogate : AssetReferenceUIRestriction
    {
        AssetReferenceUIRestriction data;
        /// <summary>
        /// Sets the AssetReferenceUIRestriction for this surrogate
        /// </summary>
        /// <param name="initData">To initialize AssetReferenceUIRestriction for surrogate</param>
        public virtual void Init(AssetReferenceUIRestriction initData)
        {
            data = initData;
        }

        /// <summary>
        /// Validates the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="obj">The Object to validate.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public override bool ValidateAsset(Object obj)
        {
            return data.ValidateAsset(obj);
        }

        internal virtual bool ValidateAsset(IReferenceEntryData entryData)
        {
            return data.ValidateAsset(entryData?.AssetPath);
        }
    }
    /// <summary>
    /// Surrogate to AssetReferenceUILabelRestriction
    /// This surrogate class provides the editor-side implementation of AssetReferenceUILabelRestriction attribute
    /// Used to restrict an AssetReference field or property to only allow items wil specific labels. This is only enforced through the UI.
    /// </summary>
    [AssetReferenceSurrogate(typeof(AssetReferenceUILabelRestriction))]
    public class AssetReferenceUILabelRestrictionSurrogate : AssetReferenceUIRestrictionSurrogate
    {
        AssetReferenceUILabelRestriction data;
        /// <summary>
        /// Sets the AssetReferenceUILabelRestriction for this surrogate
        /// </summary>
        /// <param name="initData">To initialize AssetReferenceUILabelRestriction field</param>
        public override void Init(AssetReferenceUIRestriction initData)
        {
            data = initData as AssetReferenceUILabelRestriction;
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(Object obj)
        {
            var path = AssetDatabase.GetAssetOrScenePath(obj);
            return ValidateAsset(path);
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(string path)
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
                return false;
            var guid = AssetDatabase.AssetPathToGUID(path);
            var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid, true);
            return ValidateAsset(entry);
        }

        internal override bool ValidateAsset(IReferenceEntryData entry)
        {
            if (entry != null)
            {
                foreach (var label in data.m_AllowedLabels)
                {
                    if (entry.labels.Contains(label))
                        return true;
                }
            }
            return false;
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return data.ToString();
        }
    }

    /// <summary>
    /// Utility Class
    /// </summary>
    public class AssetReferenceUtility
    {
        /// <summary>
        /// Finds surrogate class for an Assembly with a particular TargetType
        /// </summary>
        /// <param name="targetType">Target Type to search</param>
        /// <returns>Type of the surrogate found for the Assembly with a particular Target Type.</returns>
        public static Type GetSurrogate(Type targetType)
        {
            if (targetType == null)
            {
                Debug.LogError("targetType cannot be null");
                return null;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> typesList = new List<Type>();
            foreach (Assembly assem in assemblies)
            {
                var assemblyTypeList = GatherTargetTypesFromAssembly(assem, targetType, out bool concreteTypeFound);
                if (concreteTypeFound == true)
                    return assemblyTypeList[0];
                typesList.AddRange(assemblyTypeList);
            }

            if (typesList.Count == 0)
                return null;

            typesList.Sort(AssetReferenceUtility.CompareTypes);
            return typesList[0];
        }

        static int CompareTypes(object x, object y)
        {
            Type t1 = (Type)x;
            Type t2 = (Type)y;
            if (t1 == t2)
                return 0;
            else if (t1.IsAssignableFrom(t2))
                return 1;
            else
                return -1;
        }

        private static List<Type> GatherTargetTypesFromAssembly(Assembly assembly, Type targetType, out bool concreteTypeFound)
        {
            List<Type> assignableTypesList = new List<Type>();
            var typeList = assembly.GetTypes().Where(attrType => typeof(AssetReferenceUIRestrictionSurrogate).IsAssignableFrom(attrType)).ToList();
            foreach (var type in typeList)
            {
                var customAttribute = type.GetCustomAttribute<AssetReferenceSurrogateAttribute>();
                if (customAttribute == null)
                    continue;
                if (customAttribute.TargetType == targetType)
                {
                    assignableTypesList.Clear();
                    assignableTypesList.Add(type);
                    concreteTypeFound = true;
                    return assignableTypesList;
                }
                if (customAttribute.TargetType.IsAssignableFrom(targetType))
                {
                    assignableTypesList.Add(type);
                }
            }
            concreteTypeFound = false;
            return assignableTypesList;
        }
    }
}
