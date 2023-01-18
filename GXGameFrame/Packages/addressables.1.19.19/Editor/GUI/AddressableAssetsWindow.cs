using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.GUI
{
    class AddressableAssetsWindow : EditorWindow, IHasCustomMenu
    {
        private SearchRequest m_Request;
        private string m_HelpUrl;

        [FormerlySerializedAs("m_groupEditor")]
        [SerializeField]
        internal AddressableAssetsSettingsGroupEditor m_GroupEditor;

        [MenuItem("Window/Asset Management/Addressables/Settings", priority = 2051)]
        internal static void ShowSettingsInspector()
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            if (setting == null)
            {
                Debug.LogWarning("Attempting to inspect default Addressables Settings, but no settings file exists.  Open 'Window/Asset Management/Addressables/Groups' for more info.");
            }
            else
            {
                EditorApplication.ExecuteMenuItem("Window/General/Inspector");
                EditorGUIUtility.PingObject(setting);
                Selection.activeObject = AddressableAssetSettingsDefaultObject.Settings;
            }
        }

        [MenuItem("Window/Asset Management/Addressables/Groups", priority = 2050)]
        internal static void Init()
        {
            var window = GetWindow<AddressableAssetsWindow>();
            window.titleContent = new GUIContent("Addressables Groups");
            window.minSize = new Vector2(430, 250);
            window.Show();
        }

        public static Vector2 GetWindowPosition()
        {
            var window = GetWindow<AddressableAssetsWindow>();
            return new Vector2(window.position.x, window.position.y);
        }

        internal void SelectAssetsInGroupEditor(IList<AddressableAssetEntry> entries)
        {
            if (m_GroupEditor == null)
                m_GroupEditor = new AddressableAssetsSettingsGroupEditor(this);
            m_GroupEditor.SelectEntries(entries);
        }

        public void OnEnable()
        {
            m_GroupEditor?.OnEnable();
            if (m_Request == null || m_Request.Status == StatusCode.Failure)
            {
                m_Request = PackageManager.Client.Search("com.unity.addressables");
            }
        }

        public void OnDisable()
        {
            m_GroupEditor?.OnDisable();
        }

        internal void OfferToConvert(AddressableAssetSettings settings)
        {
            var bundleList = AssetDatabase.GetAllAssetBundleNames();
            if (settings != null && bundleList.Length > 0)
            {
                var displayChoice = EditorUtility.DisplayDialog("Legacy Bundles Detected",
                    "We have detected the use of legacy bundles in this project.  Would you like to auto-convert those into Addressables? \nThis will take each asset bundle you have defined (we have detected " +
                    bundleList.Length +
                    " bundles), create an Addressables group with a matching name, then move all assets from those bundles into corresponding groups.  This will remove the asset bundle assignment from all assets, and remove all asset bundle definitions from this project.  This cannot be undone.",
                    "Convert", "Ignore");
                if (displayChoice)
                {
                    AddressableAssetUtility.ConvertAssetBundlesToAddressables();
                }
            }
        }

        public void OnGUI()
        {
            if (AddressableAssetSettingsDefaultObject.Settings == null)
            {
                GUILayout.Space(50);
                if (GUILayout.Button("Create Addressables Settings"))
                {
                    m_GroupEditor = null;
                    AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
                    OfferToConvert(AddressableAssetSettingsDefaultObject.Settings);
                }
                //if (GUILayout.Button("Import Addressables Settings"))
                //{
                //    m_GroupEditor = null;
                //    var path = EditorUtility.OpenFilePanel("Addressables Settings Object", AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, "asset");
                //    if (!string.IsNullOrEmpty(path))
                //    {
                //        var i = path.ToLower().IndexOf("/assets/");
                //        if (i > 0)
                //        {
                //            path = path.Substring(i + 1);
                //            Addressables.LogFormat("Loading Addressables Settings from {0}", path);
                //            var obj = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
                //            if (obj != null)
                //                AddressableAssetSettingsDefaultObject.Settings = obj;
                //            else
                //                Debug.LogWarning("Unable to load asset settings from: "
                //                                 + path
                //                                 + "\nPlease ensure the location included in the project directory."
                //                );
                //        }
                //    }
                //}
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Space(50);
                UnityEngine.GUI.skin.label.wordWrap = true;
                GUILayout.Label("Click the \"Create\" button above or simply drag an asset into this window to start using Addressables.  Once you begin, the Addressables system will save some assets to your project to keep up with its data");
                GUILayout.Space(50);
                GUILayout.EndHorizontal();
                switch (Event.current.type)
                {
                    case EventType.DragPerform:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        foreach (var path in DragAndDrop.paths)
                        {
                            if (AddressableAssetUtility.IsPathValidForEntry(path))
                            {
                                var guid = AssetDatabase.AssetPathToGUID(path);
                                if (!string.IsNullOrEmpty(guid))
                                {
                                    if (AddressableAssetSettingsDefaultObject.Settings == null)
                                        AddressableAssetSettingsDefaultObject.Settings = AddressableAssetSettings.Create(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder, AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName, true, true);
                                    Undo.RecordObject(AddressableAssetSettingsDefaultObject.Settings, "AddressableAssetSettings");
                                    AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, AddressableAssetSettingsDefaultObject.Settings.DefaultGroup);
                                }
                            }
                        }
                        break;
                    case EventType.DragUpdated:
                    case EventType.DragExited:
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        break;
                }
            }
            else
            {
                Rect contentRect = new Rect(0, 0, position.width, position.height);

                if (m_GroupEditor == null)
                    m_GroupEditor = new AddressableAssetsSettingsGroupEditor(this);

                if (m_GroupEditor.OnGUI(contentRect))
                    Repaint();
            }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            if (m_Request != null && m_Request.Status == StatusCode.Success && m_Request.Result != null && m_Request.Result.Length == 1)
            {
                string[] parts = m_Request.Result[0].version.Split('.');
                if (parts.Length >= 2)
                {
                    // Major & minor
                    string vUrl = $"{parts[0]}.{parts[1]}";
                    m_HelpUrl = $"https://docs.unity3d.com/Packages/com.unity.addressables@{vUrl}";
                    menu.AddItem(new GUIContent("Help"), false, OnHelp);
                }
            }
        }

        void OnHelp()
        {
            if (!string.IsNullOrEmpty(m_HelpUrl))
            {
                Application.OpenURL(m_HelpUrl);
            }
        }
    }
}
