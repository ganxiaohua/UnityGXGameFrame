using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;
#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
using Unity.Services.CCD.Management.Models;
#endif

namespace UnityEditor.AddressableAssets.GUI
{
    internal class ProfileWindow : EditorWindow
    {
        //Min and Max proportion of the window that ProfilePane can take up
        const float k_MinProfilePaneWidth = 0.10f;
        const float k_MaxProfilePaneWidth = 0.6f;

        private const float k_MinLabelWidth = 155f;
        private const float k_ApproxCharWidth = 8.5f;

        const float k_DefaultHorizontalSplitterRatio = 0.33f;
        const int k_SplitterThickness = 2;
        const int k_ToolbarHeight = 20;
        const int k_ItemRectPadding = 15;

        //amount of padding between variable items
        const float k_VariableItemPadding = 5f;

        //Default length of the Label within the Variables Pane
        private float m_LabelWidth = 155f;
        private float m_FieldBufferWidth = 0f;

        GUIStyle m_ItemRectPadding;

        float m_HorizontalSplitterRatio = k_DefaultHorizontalSplitterRatio;

        private ProfileDataSourceSettings m_ProfileDataSource;

        internal AddressableAssetSettings settings
        {
            get { return AddressableAssetSettingsDefaultObject.Settings; }
        }

        internal ProfileDataSourceSettings dataSourceSettings
        {
            get
            {
                if (m_ProfileDataSource == null)
                    m_ProfileDataSource = ProfileDataSourceSettings.GetSettings();
                return m_ProfileDataSource;
            }
        }

        private ProfileTreeView m_ProfileTreeView;

        private bool m_IsResizingHorizontalSplitter;
        internal static bool m_Reload = false;

        private Vector2 m_ProfilesPaneScrollPosition;
        private Vector2 m_VariablesPaneScrollPosition;

        private int m_ProfileIndex = -1;
        public int ProfileIndex
        {
            get { return m_ProfileIndex; }
            set { m_ProfileIndex = value; }
        }


        private GUIStyle m_ButtonStyle;


        private Dictionary<string, bool?> m_foldouts = new Dictionary<string, bool?>();
        private Dictionary<string, bool> m_CustomGroupTypes = new Dictionary<string, bool>();

        [MenuItem("Window/Asset Management/Addressables/Profiles", priority = 2051)]
        internal static void ShowWindow()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Error", "Attempting to open Addressables Profiles window, but no Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
                return;
            }
            GetWindow<ProfileWindow>().Show();
        }

        internal static void DrawOutline(Rect rect, float size)
        {
            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = UnityEngine.GUI.color;
            UnityEngine.GUI.color = UnityEngine.GUI.color * color;
            UnityEngine.GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
            UnityEngine.GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
            UnityEngine.GUI.DrawTexture(new Rect(rect.x, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);
            UnityEngine.GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);

            UnityEngine.GUI.color = orgColor;
        }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
        private async void Awake()
        {
            if (CloudProjectSettings.projectId != String.Empty) await ProfileDataSourceSettings.UpdateCCDDataSourcesAsync(CloudProjectSettings.projectId, false);
        }
#endif
            
        private void OnEnable()
        {
            Undo.undoRedoPerformed += MarkForReload;
            titleContent = new GUIContent("Addressables Profiles");
            m_ItemRectPadding = new GUIStyle();
            m_ItemRectPadding.padding = new RectOffset(k_ItemRectPadding, k_ItemRectPadding, k_ItemRectPadding, k_ItemRectPadding);
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= MarkForReload;
        }

        internal static void MarkForReload()
        {
            m_Reload = true;
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

        void TopToolbar(Rect toolbarPos)
        {
            if (m_ButtonStyle == null)
                m_ButtonStyle = GetStyle("ToolbarButton");

            m_ButtonStyle.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginArea(new Rect(0, 0, toolbarPos.width, k_ToolbarHeight));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var guiMode = new GUIContent("Create");
                Rect rMode = GUILayoutUtility.GetRect(guiMode, EditorStyles.toolbarDropDown);
                if (EditorGUI.DropdownButton(rMode, guiMode, FocusType.Passive, EditorStyles.toolbarDropDown))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Profile"), false, NewProfile);
                    menu.AddItem(new GUIContent("Variable (All Profiles)"), false, () => NewVariable(rMode));
                    menu.AddItem(new GUIContent("Build and Load Path Variables (All Profiles)"), false, () => NewPathPair(rMode));
                    menu.DropDown(rMode);
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void NewVariable(Rect displayRect)
        {
            try
            {
                displayRect.y += 22f;
                PopupWindow.Show(displayRect,
                    new ProfileNewVariablePopup(position.width, position.height, 0, m_ProfileTreeView, settings));
            }
            catch (ExitGUIException)
            {
                // Exception not being caught through OnGUI call
            }
        }

        void NewPathPair(Rect displayRect)
        {
            try
            {
                displayRect.y += 22f;
                PopupWindow.Show(displayRect,
                    new ProfileNewPathPairPopup(position.width, position.height, 0, m_ProfileTreeView, settings));
            }
            catch (ExitGUIException)
            {
                // Exception not being caught through OnGUI call
            }
        }

        //Contains all of the profile names, primarily implemented in ProfileTreeView
        void ProfilesPane(Rect profilesPaneRect)
        {
            DrawOutline(profilesPaneRect, 1);
            GUILayout.BeginArea(profilesPaneRect);
            {
                m_ProfilesPaneScrollPosition = GUILayout.BeginScrollView(m_ProfilesPaneScrollPosition);
                Rect r = new Rect(profilesPaneRect);
                r.y = 0;

                var profiles = settings.profileSettings.profiles;

                if (m_ProfileTreeView == null || m_ProfileTreeView.Names.Count != profiles.Count || m_Reload)
                {
                    m_Reload = false;
                    m_ProfileTreeView = new ProfileTreeView(new TreeViewState(), profiles, this, ProfileTreeView.CreateHeader());
                }

                m_ProfileTreeView.OnGUI(r);
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }

        //Displays all variables for the currently selected profile and initializes each variable's context menu
        void VariablesPane(Rect variablesPaneRect)
        {
            DrawOutline(variablesPaneRect, 1);
            Event evt = Event.current;
            AddressableAssetProfileSettings.BuildProfile selectedProfile = GetSelectedProfile();

            if (selectedProfile == null) return;
            if (evt.isMouse || evt.isKey)
            {
                m_ProfileTreeView.lastClickedProfile = ProfileIndex;
            }

            //ensures amount of visible text is not affected by label width
            float fieldWidth = variablesPaneRect.width - (2 * k_ItemRectPadding) + m_FieldBufferWidth;

            if (!EditorGUIUtility.labelWidth.Equals(m_LabelWidth))
                EditorGUIUtility.labelWidth = m_LabelWidth;

            int maxLabelLen = 0;
            int maxFieldLen = 0;

            GUILayout.BeginArea(variablesPaneRect);
            EditorGUI.indentLevel++;
            List<ProfileGroupType> groupTypes = ProfileGroupType.CreateGroupTypes(selectedProfile);
            HashSet<string> drawnGroupTypes = new HashSet<string>();

            //Displaying Path Groups
            foreach (ProfileGroupType groupType in groupTypes)
            {
                bool? foldout;
                m_foldouts.TryGetValue(groupType.GroupTypePrefix, out foldout);
                GUILayout.Space(5);
                Rect pathPairRect = EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(fieldWidth + k_VariableItemPadding - k_SplitterThickness), GUILayout.MinWidth(fieldWidth + k_VariableItemPadding - k_SplitterThickness) });
                m_foldouts[groupType.GroupTypePrefix] = EditorGUILayout.Foldout(foldout != null ? foldout.Value : true, groupType.GroupTypePrefix, true);
                Rect dsDropdownRect = EditorGUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(fieldWidth - m_LabelWidth), GUILayout.MinWidth(fieldWidth - m_LabelWidth) });
                string dropdownText = DetermineOptionString(groupType);
                bool dsDropdown = EditorGUILayout.DropdownButton(new GUIContent(dropdownText), FocusType.Keyboard, new GUILayoutOption[] { GUILayout.Width(fieldWidth - m_LabelWidth) });
                if (evt.type == EventType.ContextClick)
                    CreatePairPrefixContextMenu(variablesPaneRect, pathPairRect, groupType, evt);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();
                DrawDataSourceDropDowns(dsDropdownRect, groupType, dsDropdown);

                //Specific Grouped variables
                List<ProfileGroupType.GroupTypeVariable> pathVariables = new List<ProfileGroupType.GroupTypeVariable>();
                pathVariables.Add(groupType.GetVariableBySuffix(AddressableAssetSettings.kBuildPath));
                drawnGroupTypes.Add(groupType.GetName(groupType.GetVariableBySuffix(AddressableAssetSettings.kBuildPath)));
                pathVariables.Add(groupType.GetVariableBySuffix(AddressableAssetSettings.kLoadPath));
                drawnGroupTypes.Add(groupType.GetName(groupType.GetVariableBySuffix(AddressableAssetSettings.kLoadPath)));


                if (m_foldouts[groupType.GroupTypePrefix].Value)
                {
                    bool custom;
                    m_CustomGroupTypes.TryGetValue(groupType.GroupTypePrefix, out custom);
                    EditorGUI.BeginDisabledGroup(!custom);

                    EditorGUI.indentLevel++;

                    //Displaying Path Groups
                    foreach (var variable in pathVariables)
                    {
                        Rect newPathRect = EditorGUILayout.BeginVertical();
                        string newPath = EditorGUILayout.TextField(groupType.GetName(variable), variable.Value, new GUILayoutOption[] { GUILayout.Width(fieldWidth) });
                        EditorGUILayout.EndVertical();
                        if (newPath != variable.Value && ProfileIndex == m_ProfileTreeView.lastClickedProfile)
                        {
                            Undo.RecordObject(settings, "Variable value changed");
                            settings.profileSettings.SetValue(selectedProfile.id, groupType.GetName(variable), newPath);
                            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
                        }
                    }
                    EditorGUI.indentLevel--;

                    EditorGUI.EndDisabledGroup();
                }
            }

            //Display all other variables
            for (var i = 0; i < settings.profileSettings.profileEntryNames.Count; i++)
            {
                AddressableAssetProfileSettings.ProfileIdData curVariable = settings.profileSettings.profileEntryNames[i];
                if (!drawnGroupTypes.Contains(curVariable.ProfileName))
                {
                    GUILayout.Space(5);
                    Rect newValueRect = EditorGUILayout.BeginVertical();
                    string newValue = EditorGUILayout.TextField(curVariable.ProfileName, selectedProfile.values[i].value, new GUILayoutOption[] { GUILayout.Width(fieldWidth) });
                    EditorGUILayout.EndVertical();
                    if (newValue != selectedProfile.values[i].value && ProfileIndex == m_ProfileTreeView.lastClickedProfile)
                    {
                        Undo.RecordObject(settings, "Variable value changed");
                        settings.profileSettings.SetValue(selectedProfile.id, settings.profileSettings.profileEntryNames[i].ProfileName, newValue);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
                    }

                    if (evt.type == EventType.ContextClick)
                    {
                        CreateVariableContextMenu(variablesPaneRect, newValueRect, curVariable, evt);
                    }
                }
                maxLabelLen = Math.Max(maxLabelLen, curVariable.ProfileName.Length);
            }

            EditorGUI.indentLevel--;
            GUILayout.EndArea();

            //Update the label width to the maximum of the minimum acceptable label width and the amount of
            //space required to contain the longest variable name
            m_LabelWidth = Mathf.Max(maxLabelLen * k_ApproxCharWidth, k_MinLabelWidth);
            m_FieldBufferWidth = Mathf.Clamp((maxFieldLen * k_ApproxCharWidth) - fieldWidth, 0f, float.MaxValue);
        }

        void DrawDataSourceDropDowns(Rect dsDropdownRect, ProfileGroupType groupType, bool showDropdown)
        {
            Rect fixedDropdownRect = new Rect(
                //Determine correct position for dropdown window
                new Vector2(
                    dsDropdownRect.x,
                    dsDropdownRect.y
                ),
                new Vector2(dsDropdownRect.width, 120)
            );

            if (showDropdown)
            {
                ProfileDataSourceDropdownWindow dataSourceDropdownWindow = new ProfileDataSourceDropdownWindow(fixedDropdownRect, groupType);
                //TODO: Add Event Handler Here
                dataSourceDropdownWindow.ValueChanged += DataSourceDropdownValueChanged;
                PopupWindow.Show(dsDropdownRect, dataSourceDropdownWindow);
            }
        }

        internal void DataSourceDropdownValueChanged(object sender, ProfileDataSourceDropdownWindow.DropdownWindowEventArgs e)
        {
            m_CustomGroupTypes[e.GroupType.GroupTypePrefix] = e.IsCustom;
            if (!e.IsCustom)
            {
                var selectedProfile = GetSelectedProfile();
                Undo.RecordObject(settings, "Variable value changed");
                settings.profileSettings.SetValue(selectedProfile.id, e.GroupType.GetName(e.GroupType.GetVariableBySuffix(AddressableAssetSettings.kBuildPath)), e.Option.BuildPath);
                settings.profileSettings.SetValue(selectedProfile.id, e.GroupType.GetName(e.GroupType.GetVariableBySuffix(AddressableAssetSettings.kLoadPath)), e.Option.LoadPath);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
            }

        }

        private string DetermineOptionString(ProfileGroupType groupType)
        {

            ProfileGroupType selectedGroupType = dataSourceSettings.FindGroupType(groupType);
            if (selectedGroupType != null)
            {
                bool custom;
                m_CustomGroupTypes.TryGetValue(groupType.GroupTypePrefix, out custom);
                if (custom && ProfileIndex == m_ProfileTreeView.lastClickedProfile)
                    return "Custom";
                m_CustomGroupTypes[groupType.GroupTypePrefix] = false;

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
                //Could ERR if user has group type prefix that starts with CCD
                if (selectedGroupType.GroupTypePrefix.StartsWith("CCD"))
                {
                    var parts = selectedGroupType.GroupTypePrefix.Split(ProfileGroupType.k_PrefixSeparator);
                    var badgeName = String.Join(ProfileGroupType.k_PrefixSeparator.ToString(), parts, 3, parts.Length - 3);
                    var bucketName = selectedGroupType.GetVariableBySuffix($"{nameof(CcdBucket)}{nameof(CcdBucket.Name)}").Value;
                    return String.Join(ProfileGroupType.k_PrefixSeparator.ToString(), new string[]
                    {
                        "CCD",
                        bucketName,
                        badgeName
                    });
                }
#endif
                return selectedGroupType.GroupTypePrefix;
            }
            else
            {
                m_CustomGroupTypes[groupType.GroupTypePrefix] = true;
                return "Custom";
            }
        }

        void CreatePairPrefixContextMenu(Rect parentWindow, Rect menuRect, ProfileGroupType groupType, Event evt)
        {
            if (menuRect.Contains(evt.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddDisabledItem(new GUIContent(groupType.GroupTypePrefix));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rename Path Prefix (All Profiles)"), false, () => { RenamePathPair(groupType, parentWindow, menuRect); });
                menu.AddItem(new GUIContent("Delete Path Pair (All Profiles)"), false, () => { DeletePathPair(groupType); });
                menu.ShowAsContext();
                evt.Use();
            }
        }

        //Creates the context menu for the selected variable
        void CreateVariableContextMenu(Rect parentWindow, Rect menuRect, AddressableAssetProfileSettings.ProfileIdData variable, Event evt)
        {
            if (menuRect.Contains(evt.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                //Displays name of selected variable so user can be confident they're deleting/renaming the right one
                menu.AddDisabledItem(new GUIContent(variable.ProfileName));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rename Variable (All Profiles)"), false, () => { RenameVariable(variable, parentWindow, menuRect); });
                menu.AddItem(new GUIContent("Delete Variable (All Profiles)"), false, () => { DeleteVariable(variable); });
                menu.ShowAsContext();
                evt.Use();
            }
        }
        
        void RenamePathPair(ProfileGroupType groupType, Rect parentWindow, Rect displayRect)
        {
            try
            {
                //Determines the current variable rect location
                Rect variableRect = new Rect(parentWindow.x + 2.5f, displayRect.y + 1.5f, m_LabelWidth, k_ToolbarHeight * 2);
                PopupWindow.Show(variableRect, new PathPairRenamePopup(m_LabelWidth, groupType, settings));
            }
            catch (ExitGUIException)
            {
                // Exception not being caught through OnGUI call
            }
        }

        //Opens ProfileRenameVariablePopup
        void RenameVariable(AddressableAssetProfileSettings.ProfileIdData profileVariable, Rect parentWindow, Rect displayRect)
        {
            try
            {
                //Determines the current variable rect location
                Rect variableRect = new Rect(parentWindow.x + 2.5f, displayRect.y + 1.5f, m_LabelWidth, k_ToolbarHeight * 2);
                PopupWindow.Show(variableRect, new ProfileRenameVariablePopup(m_LabelWidth, profileVariable, settings));
            }
            catch (ExitGUIException)
            {
                // Exception not being caught through OnGUI call
            }
        }

        void DeletePathPair(ProfileGroupType groupType)
        {
            var buildPathData = settings.profileSettings.GetProfileDataByName(groupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kBuildPath);
            var loadPathData = settings.profileSettings.GetProfileDataByName(groupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kLoadPath);
            if (loadPathData == default(AddressableAssetProfileSettings.ProfileIdData) || buildPathData == default(AddressableAssetProfileSettings.ProfileIdData))
            {
                Debug.LogError("An error occured while getting one of the path pair variables.");
                return;
            }
            Undo.RecordObject(settings, "Profile Variable Deleted");
            settings.profileSettings.RemoveValue(buildPathData.Id);
            settings.profileSettings.RemoveValue(loadPathData.Id);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
        }

        void DeleteVariable(AddressableAssetProfileSettings.ProfileIdData toBeDeleted)
        {
            Undo.RecordObject(settings, "Profile Variable Deleted");
            settings.profileSettings.RemoveValue(toBeDeleted.Id);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
        }

        //Returns the BuildProfile currently selected in the ProfilesPane
        AddressableAssetProfileSettings.BuildProfile GetSelectedProfile()
        {
            return m_ProfileTreeView.GetSelectedProfile();
        }

        //Creates a new BuildProfile and reloads the ProfilesPane
        void NewProfile()
        {
            var uniqueProfileName = settings.profileSettings.GetUniqueProfileName("New Profile");
            if (!string.IsNullOrEmpty(uniqueProfileName))
            {
                Undo.RecordObject(settings, "New Profile Created");
                //Either copy values from the selected profile, or if there is no selected profile, copy from the default
                string idToCopyFrom = GetSelectedProfile() != null
                    ? GetSelectedProfile().id
                    : settings.profileSettings.profiles[0].id;
                settings.profileSettings.AddProfile(uniqueProfileName, idToCopyFrom);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(settings);
                m_ProfileTreeView.Reload();
            }
        }

        private void OnGUI()
        {
            if (settings == null) return;

            if (m_IsResizingHorizontalSplitter)
                m_HorizontalSplitterRatio = Mathf.Clamp(Event.current.mousePosition.x / position.width,
                    k_MinProfilePaneWidth, k_MaxProfilePaneWidth);

            var toolbarRect = new Rect(0, 0, position.width, position.height);
            var profilesPaneRect = new Rect(0, k_ToolbarHeight, (position.width * m_HorizontalSplitterRatio), position.height);
            var variablesPaneRect = new Rect(profilesPaneRect.width + k_SplitterThickness, k_ToolbarHeight,
                position.width - profilesPaneRect.width - k_SplitterThickness, position.height - k_ToolbarHeight);
            var horizontalSplitterRect = new Rect(profilesPaneRect.width, k_ToolbarHeight, k_SplitterThickness, position.height - k_ToolbarHeight);

            EditorGUIUtility.AddCursorRect(horizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && horizontalSplitterRect.Contains(Event.current.mousePosition))
                m_IsResizingHorizontalSplitter = true;
            else if (Event.current.type == EventType.MouseUp)
                m_IsResizingHorizontalSplitter = false;

            TopToolbar(toolbarRect);

            ProfilesPane(profilesPaneRect);

            VariablesPane(variablesPaneRect);

            if (m_IsResizingHorizontalSplitter)
                Repaint();
        }

        class PathPairRenamePopup : PopupWindowContent
        {
            internal Rect m_WindowRect;
            internal ProfileGroupType m_ProfileGroupType;
            internal List<ProfileGroupType> m_AllProfileGroupTypes;
            internal AddressableAssetSettings m_Settings;
            internal string m_NewName;
            internal float m_LabelWidth;
            public PathPairRenamePopup(float labelWidth, ProfileGroupType profileGroupType, AddressableAssetSettings settings)
            {
                m_LabelWidth = labelWidth;
                m_ProfileGroupType = profileGroupType;
                m_Settings = settings;
                m_NewName = profileGroupType.GroupTypePrefix;
                UnityEngine.GUI.enabled = true;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(m_LabelWidth, 40);
            }

            public override void OnGUI(Rect windowRect)
            {
                GUILayout.BeginArea(windowRect);

                Event evt = Event.current;
                bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);

                m_NewName = GUILayout.TextField(m_NewName);
                if (GUILayout.Button("Save") || hitEnter)
                {
                    if (string.IsNullOrEmpty(m_NewName))
                        Debug.LogError("Path pair prefix cannot be empty.");
                    else if (m_NewName == m_ProfileGroupType.GroupTypePrefix)
                        editorWindow.Close();
                    else if (VariableWithNewPrefixAlreadyExists())
                        Debug.LogError("One or more build or load path variables with prefix '" + m_NewName + "' already exist. Please rename them or pick a different prefix.");
                    else if (m_NewName.Trim().Length == 0) // new name cannot only contain spaces
                        Debug.LogError("Path pair prefix cannot be only spaces");
                    else
                    {
                        var loadPathVariableData = m_Settings.profileSettings.GetProfileDataByName(m_ProfileGroupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kLoadPath);
                        var buildPathVariableData = m_Settings.profileSettings.GetProfileDataByName(m_ProfileGroupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kBuildPath);
                        if (loadPathVariableData == default(AddressableAssetProfileSettings.ProfileIdData) || buildPathVariableData == default(AddressableAssetProfileSettings.ProfileIdData))
                            Debug.LogError("Valid path pair to rename not found.");
                        else
                        {
                            Undo.RecordObject(m_Settings, "Path pair prefix Renamed");
                            m_ProfileGroupType.GroupTypePrefix = m_NewName;
                            loadPathVariableData.SetName(m_ProfileGroupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kLoadPath, m_Settings.profileSettings);
                            buildPathVariableData.SetName(m_ProfileGroupType.GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kBuildPath, m_Settings.profileSettings);
                            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings, true);
                            editorWindow.Close();
                        }
                    }
                }
                GUILayout.EndArea();
            }

            bool VariableWithNewPrefixAlreadyExists()
            {
                bool loadPathAlreadyExists = m_Settings.profileSettings.GetProfileDataByName(m_NewName + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kLoadPath) 
                    != default(AddressableAssetProfileSettings.ProfileIdData);
                bool buildPathAlreadyExists = m_Settings.profileSettings.GetProfileDataByName(m_NewName + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kBuildPath)
                    != default(AddressableAssetProfileSettings.ProfileIdData);
                return loadPathAlreadyExists || buildPathAlreadyExists;
            }
        }

        class ProfileRenameVariablePopup : PopupWindowContent
        {
            internal float m_LabelWidth;
            internal AddressableAssetProfileSettings.ProfileIdData m_ProfileVariable;
            internal AddressableAssetSettings m_Settings;
            internal string m_NewName;
            public ProfileRenameVariablePopup(float labelWidth, AddressableAssetProfileSettings.ProfileIdData profileVariable, AddressableAssetSettings settings)
            {
                m_LabelWidth = labelWidth;
                m_ProfileVariable = profileVariable;
                m_Settings = settings;
                m_NewName = m_ProfileVariable.ProfileName;
                UnityEngine.GUI.enabled = true;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(m_LabelWidth, 40);
            }

            public override void OnGUI(Rect windowRect)
            {
                GUILayout.BeginArea(windowRect);

                Event evt = Event.current;
                bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);

                m_NewName = GUILayout.TextField(m_NewName);
                if (GUILayout.Button("Save") || hitEnter)
                {
                    if (string.IsNullOrEmpty(m_NewName))
                        Debug.LogError("Variable name cannot be empty.");
                    else if (m_NewName == m_ProfileVariable.ProfileName)
                        editorWindow.Close();
                    else if (m_NewName != m_Settings.profileSettings.GetUniqueProfileEntryName(m_NewName))
                        Debug.LogError("Profile variable '" + m_NewName + "' already exists.");
                    else if (m_NewName.Trim().Length == 0) // new name cannot only contain spaces
                        Debug.LogError("Name cannot be only spaces");
                    else
                    {
                        Undo.RecordObject(m_Settings, "Profile Variable Renamed");
                        m_ProfileVariable.SetName(m_NewName, m_Settings.profileSettings);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings, true);
                        editorWindow.Close();
                    }
                }
                GUILayout.EndArea();
            }
        }


        class ProfileNewVariablePopup : PopupWindowContent
        {
            internal float m_WindowWidth;
            internal float m_WindowHeight;
            internal float m_xOffset;
            internal string m_Name;
            internal string m_Value;
            internal bool m_NeedsFocus = true;
            internal AddressableAssetSettings m_Settings;

            ProfileTreeView m_ProfileTreeView;

            public ProfileNewVariablePopup(float width, float height, float xOffset, ProfileTreeView profileTreeView, AddressableAssetSettings settings)
            {
                m_WindowWidth = width;
                m_WindowHeight = height;
                m_xOffset = xOffset;
                m_Settings = settings;
                m_Name = m_Settings.profileSettings.GetUniqueProfileEntryName("New Entry");
                m_Value = Application.dataPath;

                m_ProfileTreeView = profileTreeView;
            }

            public override Vector2 GetWindowSize()
            {
                float width = Mathf.Clamp(m_WindowWidth * 0.375f, Mathf.Min(600, m_WindowWidth - m_xOffset), m_WindowWidth);
                float height = Mathf.Clamp(65, Mathf.Min(65, m_WindowHeight), m_WindowHeight);
                return new Vector2(width, height);
            }

            public override void OnGUI(Rect windowRect)
            {
                GUILayout.Space(5);
                Event evt = Event.current;
                bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);
                EditorGUIUtility.labelWidth = 90;
                m_Name = EditorGUILayout.TextField("Variable Name", m_Name);
                m_Value = EditorGUILayout.TextField("Default Value", m_Value);

                UnityEngine.GUI.enabled = m_Name.Length != 0;
                if (GUILayout.Button("Save") || hitEnter)
                {
                    if (string.IsNullOrEmpty(m_Name))
                        Debug.LogError("Variable name cannot be empty.");
                    else if (m_Name != m_Settings.profileSettings.GetUniqueProfileEntryName(m_Name))
                        Debug.LogError("Profile variable '" + m_Name + "' already exists.");
                    else
                    {
                        Undo.RecordObject(m_Settings, "Profile Variable " + m_Name + " Created");
                        m_Settings.profileSettings.CreateValue(m_Name, m_Value);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings);
                        m_ProfileTreeView.Reload();
                        editorWindow.Close();
                    }
                }
            }
        }

        class ProfileNewPathPairPopup : PopupWindowContent
        {
            internal float m_WindowWidth;
            internal float m_WindowHeight;
            internal float m_xOffset;
            internal string m_Name;
            internal string m_BuildPath;
            internal string m_LoadPath;
            internal bool m_NeedsFocus = true;
            internal AddressableAssetSettings m_Settings;

            ProfileTreeView m_ProfileTreeView;

            public ProfileNewPathPairPopup(float width, float height, float xOffset, ProfileTreeView profileTreeView, AddressableAssetSettings settings)
            {
                m_WindowWidth = width;
                m_WindowHeight = height;
                m_xOffset = xOffset;
                m_Settings = settings;
                m_Name = m_Settings.profileSettings.GetUniqueProfileEntryName("New Entry");
                m_BuildPath = Application.dataPath;
                m_LoadPath = Application.dataPath;

                m_ProfileTreeView = profileTreeView;
            }

            public override Vector2 GetWindowSize()
            {
                float width = Mathf.Clamp(m_WindowWidth * 0.375f, Mathf.Min(600, m_WindowWidth - m_xOffset), m_WindowWidth);
                float height = Mathf.Clamp(85, Mathf.Min(85, m_WindowHeight), m_WindowHeight);
                return new Vector2(width, height);
            }

            public override void OnGUI(Rect windowRect)
            {
                GUILayout.Space(5);
                Event evt = Event.current;
                bool hitEnter = evt.type == EventType.KeyDown && (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter);
                EditorGUIUtility.labelWidth = 120;
                m_Name = EditorGUILayout.TextField("Prefix Name", m_Name);
                m_BuildPath = EditorGUILayout.TextField("Build Path Value", m_BuildPath);
                m_LoadPath = EditorGUILayout.TextField("Load Path Value", m_LoadPath);

                UnityEngine.GUI.enabled = m_Name.Length != 0;
                if (GUILayout.Button("Save") || hitEnter)
                {
                    string buildPathName = m_Name + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kBuildPath;
                    string loadPathName = m_Name + ProfileGroupType.k_PrefixSeparator + AddressableAssetSettings.kLoadPath;
                    if (string.IsNullOrEmpty(m_Name))
                        Debug.LogError("Variable name cannot be empty.");
                    else if (buildPathName != m_Settings.profileSettings.GetUniqueProfileEntryName(buildPathName))
                        Debug.LogError("Profile variable '" + buildPathName + "' already exists.");
                    else if (loadPathName != m_Settings.profileSettings.GetUniqueProfileEntryName(loadPathName))
                        Debug.LogError("Profile variable '" + loadPathName + "' already exists.");
                    else
                    {
                        Undo.RecordObject(m_Settings, "Profile Path Pair Created");
                        m_Settings.profileSettings.CreateValue(buildPathName, m_BuildPath);
                        m_Settings.profileSettings.CreateValue(loadPathName, m_LoadPath);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Settings);
                        m_ProfileTreeView.Reload();
                        editorWindow.Close();
                    }
                }
            }
        }
    }
}
