using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.GUI
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Configuration GUI for <see cref="T:UnityEditor.AddressableAssets.HostingServices.HostingServicesManager" />
    /// </summary>
    public class HostingServicesWindow : EditorWindow, ISerializationCallbackReceiver, ILogHandler
    {
        const float k_DefaultVerticalSplitterRatio = 0.67f;
        const float k_DefaultHorizontalSplitterRatio = 0.33f;
        const int k_SplitterThickness = 2;
        const int k_ToolbarHeight = 20;
        const int k_ItemRectPadding = 15;
        const int k_LogRectPadding = 5;

        GUIStyle m_ItemRectPadding;
        GUIStyle m_LogRectPadding;

        [FormerlySerializedAs("m_logText")]
        [SerializeField]
        string m_LogText;
        [FormerlySerializedAs("m_logScrollPos")]
        [SerializeField]
        Vector2 m_LogScrollPos;
        [FormerlySerializedAs("m_servicesScrollPos")]
        [SerializeField]
        Vector2 m_ServicesScrollPos;
        [FormerlySerializedAs("m_splitterRatio")]
        [SerializeField]
        float m_VerticalSplitterRatio = k_DefaultVerticalSplitterRatio;
        float m_HorizontalSplitterRatio = k_DefaultHorizontalSplitterRatio;
        [FormerlySerializedAs("m_settings")]
        [SerializeField]
        AddressableAssetSettings m_Settings;

        ILogger m_Logger;
        bool m_NewLogContent;
        bool m_IsResizingVerticalSplitter;
        bool m_IsResizingHorizontalSplitter;

        bool m_Reload = false;

        int m_serviceIndex = -1;
        /// <summary>
        /// Returns the index of the currently selected hosting service.
        /// </summary>
        public int ServiceIndex { get { return m_serviceIndex; } set { m_serviceIndex = value; } }

        readonly Dictionary<object, HostingServicesProfileVarsTreeView> m_ProfileVarTables =
            new Dictionary<object, HostingServicesProfileVarsTreeView>();

        readonly Dictionary<object, Dictionary<string, string>> m_TablePrevData =
            new Dictionary<object, Dictionary<string, string>>();

        private readonly Dictionary<object, Dictionary<string, string>> m_TablePrevManagerVariables =
            new Dictionary<object, Dictionary<string, string>>();
        
        readonly List<IHostingService> m_RemovalQueue = new List<IHostingService>();
        HostingServicesProfileVarsTreeView m_GlobalProfileVarTable;
        HostingServicesListTreeView m_ServicesList;

        Type[] m_ServiceTypes;

        Type[] ServiceTypes
        {
            get
            {
                if (m_ServiceTypes == null || m_ServiceTypes.Length == 0)
                    PopulateServiceTypes();
                return m_ServiceTypes;
            }
        }
        string[] m_ServiceTypeNames;

        /// <summary>
        /// Show the <see cref="HostingServicesWindow"/>, initialized with the given <see cref="AddressableAssetSettings"/>
        /// </summary>
        /// <param name="settings"></param>
        public void Show(AddressableAssetSettings settings)
        {
            Initialize(settings);
            Show();
        }

        void Initialize(AddressableAssetSettings settings)
        {
            if (m_Logger == null)
                m_Logger = new Logger(this);

            if (m_Settings == null)
                m_Settings = settings;

            if (m_Settings != null)
                m_Settings.HostingServicesManager.Logger = m_Logger;
        }

        void OnEnable()
        {
            PopulateServiceTypes();
            m_ItemRectPadding = new GUIStyle();
            m_ItemRectPadding.padding = new RectOffset(k_ItemRectPadding, k_ItemRectPadding, k_ItemRectPadding, k_ItemRectPadding);
            m_LogRectPadding = new GUIStyle();
            m_LogRectPadding.padding = new RectOffset(k_LogRectPadding, k_LogRectPadding, k_LogRectPadding, k_LogRectPadding);
        }

        [MenuItem("Window/Asset Management/Addressables/Hosting", priority = 2052)]
        static void InitializeWithDefaultSettings()
        {
            var defaultSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (defaultSettings == null)
            {
                EditorUtility.DisplayDialog("Error", "Attempting to open Addressables Hosting window, but no Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
                return;
            }

            GetWindow<HostingServicesWindow>().Show(defaultSettings);
        }

        void PopulateServiceTypes()
        {
            if (m_Settings == null) return;
            m_ServiceTypes = m_Settings.HostingServicesManager.RegisteredServiceTypes;
            m_ServiceTypeNames = new string[m_ServiceTypes.Length];
            for (var i = 0; i < m_ServiceTypes.Length; i++)
            {
                m_ServiceTypeNames[i] = m_ServiceTypes[i].Name;
            }
        }

        void Awake()
        {
            titleContent = new GUIContent("Addressables Hosting");

            Initialize(m_Settings);
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

        void OnGUI()
        {
            if (m_Settings == null)
            {
                if (AddressableAssetSettingsDefaultObject.Settings == null)
                    return;
                InitializeWithDefaultSettings();
            }
                

            if (m_IsResizingVerticalSplitter)
                m_VerticalSplitterRatio = Mathf.Clamp(Event.current.mousePosition.y / position.height, 0.2f, 0.9f);

            if (m_IsResizingHorizontalSplitter)
                m_HorizontalSplitterRatio = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.15f, 0.6f);

            var toolbarRect = new Rect(0, 0, position.width, position.height);
            var servicesRect = new Rect(0, k_ToolbarHeight, (position.width * m_HorizontalSplitterRatio), position.height);
            var itemRect = new Rect(servicesRect.width + k_SplitterThickness, k_ToolbarHeight, position.width - servicesRect.width - k_SplitterThickness, (position.height * m_VerticalSplitterRatio) - k_ToolbarHeight);
            var logRect = new Rect(servicesRect.width + k_SplitterThickness, k_ToolbarHeight + itemRect.height + k_SplitterThickness, position.width - servicesRect.width - k_SplitterThickness,
                position.height - itemRect.height - k_SplitterThickness);
            var verticalSplitterRect = new Rect(servicesRect.width + k_SplitterThickness, k_ToolbarHeight + itemRect.height, position.width, k_SplitterThickness);
            var horizontalSplitterRect = new Rect(servicesRect.width, k_ToolbarHeight, k_SplitterThickness, position.height - k_ToolbarHeight);


            //EditorGUI.LabelField(splitterRect, string.Empty, UnityEngine.GUI.skin.horizontalSlider);
            EditorGUIUtility.AddCursorRect(verticalSplitterRect, MouseCursor.ResizeVertical);
            EditorGUIUtility.AddCursorRect(horizontalSplitterRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && verticalSplitterRect.Contains(Event.current.mousePosition))
                m_IsResizingVerticalSplitter = true;
            else if (Event.current.type == EventType.MouseDown && horizontalSplitterRect.Contains(Event.current.mousePosition))
                m_IsResizingHorizontalSplitter = true;
            else if (Event.current.type == EventType.MouseUp)
            {
                m_IsResizingVerticalSplitter = false;
                m_IsResizingHorizontalSplitter = false;
            }

            GUILayout.BeginArea(toolbarRect);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                var guiMode = new GUIContent("Create");
                Rect rMode = GUILayoutUtility.GetRect(guiMode, EditorStyles.toolbarDropDown);
                if (EditorGUI.DropdownButton(rMode, guiMode, FocusType.Passive, EditorStyles.toolbarDropDown))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Local Hosting"), false, () => AddService(0, "Local Hosting"));
                    menu.AddItem(new GUIContent("Custom Service"), false, () => GetWindow<HostingServicesAddServiceWindow>(true, "Custom Service").Initialize(m_Settings));
                    menu.DropDown(rMode);
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            DrawOutline(servicesRect, 1);

            GUILayout.BeginArea(servicesRect);
            {
                Rect r = new Rect(servicesRect);
                r.y = 0;
                DrawServicesList(r);
            }
            GUILayout.EndArea();

            DrawOutline(itemRect, 1);
            GUILayout.BeginArea(itemRect, m_ItemRectPadding);
            {
                EditorGUILayout.Space();
                DrawServicesArea();
                EditorGUILayout.Space();
            }
            GUILayout.EndArea();

            DrawOutline(logRect, 1);
            GUILayout.BeginArea(logRect, m_LogRectPadding);
            {
                DrawLogArea(logRect);
            }
            GUILayout.EndArea();

            if (m_IsResizingVerticalSplitter || m_IsResizingHorizontalSplitter)
                Repaint();
        }

        void DrawServicesList(Rect rect)
        {
            var manager = m_Settings.HostingServicesManager;
            var svcList = manager.HostingServices;

            // Do removal queue
            if (m_RemovalQueue.Count > 0)
            {
                foreach (var svc in m_RemovalQueue)
                    manager.RemoveHostingService(svc);

                m_RemovalQueue.Clear();
            }

            if (svcList.Count == 0)
            {
                m_Reload = true;
                return;
            }

            if (m_ServicesList == null || m_ServicesList.Names.Count != svcList.Count || m_Reload)
            {
                m_ServicesList = new HostingServicesListTreeView(new TreeViewState(), manager, this, HostingServicesListTreeView.CreateHeader());

                if (m_Reload)
                    m_Reload = false;
            }
            m_ServicesList.OnGUI(rect);
        }

        void DrawServicesArea()
        {
            var manager = m_Settings.HostingServicesManager;
            m_ServicesScrollPos = EditorGUILayout.BeginScrollView(m_ServicesScrollPos);
            var svcList = manager.HostingServices;

            List<IHostingService> lst = new List<IHostingService>(svcList);
            if (lst.Count == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("No Hosting Services configured.");
                GUILayout.EndScrollView();
                return;
            }
            else if (m_serviceIndex >= lst.Count)
            {
                m_serviceIndex = lst.Count - 1;
            }
            DrawServiceElement(lst[m_serviceIndex], lst);

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Add a new hosting service to the HostingServicesManager. The service at index <paramref name="typeIndex"/> in ServiceTypes must implement the <see cref="IHostingService"/> interface, or an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="typeIndex">The index of the service stored in ServiceTypes. The service at this index must implement <see cref="IHostingService"/></param>
        /// <param name="serviceName">A descriptive name for the new service instance.</param>
        public void AddService(int typeIndex, string serviceName)
        {
            string hostingName = string.Format("{0} {1}", serviceName, m_Settings.HostingServicesManager.NextInstanceId);
            m_Settings.HostingServicesManager.AddHostingService(ServiceTypes[typeIndex], hostingName);
        }

        /// <summary>
        /// Add a hosting service to the removal queue.
        /// </summary>
        /// <param name="svc">The service type to be removed.</param>
        /// <param name="showDialog">Indicates whether or not a warning dialogue box is shown.</param>
        public void RemoveService(IHostingService svc, bool showDialog = true)
        {
            if (!showDialog)
                m_RemovalQueue.Add(svc);
            else if (EditorUtility.DisplayDialog("Remove Service", "Are you sure you want to remove " + svc.DescriptiveName + "? This action cannot be undone.", "Ok", "Cancel"))
                m_RemovalQueue.Add(svc);
        }

        void DrawServiceElement(IHostingService svc, List<IHostingService> svcList)
        {
            bool isDirty = false;
            string newName = EditorGUILayout.DelayedTextField("Service Name", svc.DescriptiveName);
            if (svcList.Find(s => s.DescriptiveName == newName) == default(IHostingService))
            {
                svc.DescriptiveName = newName;
                m_ServicesList.Reload();
                isDirty = true;
            }

            var typeAndId = string.Format("{0} ({1})", svc.GetType().Name, svc.InstanceId.ToString());
            EditorGUILayout.LabelField("Service Type (ID)", typeAndId, GUILayout.MinWidth(225f));

            // Allow service to provide additional GUI configuration elements
            svc.OnGUI();

            var newIsServiceEnabled = EditorGUILayout.Toggle("Enable", svc.IsHostingServiceRunning);
            if (newIsServiceEnabled != svc.IsHostingServiceRunning)
            {
                if (newIsServiceEnabled)
                    svc.StartHostingService();
                else
                    svc.StopHostingService();
                isDirty = true;
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!svc.IsHostingServiceRunning))
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Hosting Service Variables");

                var manager = m_Settings.HostingServicesManager;
                if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
                    manager.RefreshGlobalProfileVariables();

                GUILayout.EndHorizontal();

                DrawProfileVarTable(svc);
            }

            if (isDirty && m_Settings != null)
                m_Settings.SetDirty(AddressableAssetSettings.ModificationEvent.HostingServicesManagerModified, this, false, true);
        }

        void DrawLogArea(Rect rect)
        {
            if (m_NewLogContent)
            {
                var height = UnityEngine.GUI.skin.GetStyle("Label").CalcHeight(new GUIContent(m_LogText), rect.width);
                m_LogScrollPos = new Vector2(0f, height);
                m_NewLogContent = false;
            }

            m_LogScrollPos = EditorGUILayout.BeginScrollView(m_LogScrollPos);
            GUILayout.Label(m_LogText);
            EditorGUILayout.EndScrollView();
        }

        internal static bool DictsAreEqual(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            return a.Count == b.Count && !a.Except(b).Any();
        }

        void DrawProfileVarTable(IHostingService tableKey)
        {
            var manager = m_Settings.HostingServicesManager;
            var data = tableKey.ProfileVariables;
            
            HostingServicesProfileVarsTreeView table;
            
            if (!m_ProfileVarTables.TryGetValue(tableKey, out table))
            {
                table = new HostingServicesProfileVarsTreeView(new TreeViewState(),
                    HostingServicesProfileVarsTreeView.CreateHeader());
                m_ProfileVarTables[tableKey] = table;
                m_TablePrevData[tableKey] = new Dictionary<string, string>(data);
                m_TablePrevManagerVariables[tableKey] = new Dictionary<string, string>(manager.GlobalProfileVariables);
            }
            
            else if (!DictsAreEqual(data, m_TablePrevData[tableKey]) || !DictsAreEqual(manager.GlobalProfileVariables, m_TablePrevManagerVariables[tableKey]))
            {
                table.ClearItems();
                m_TablePrevData[tableKey] = new Dictionary<string, string>(data);
                m_TablePrevManagerVariables[tableKey] = new Dictionary<string, string>(manager.GlobalProfileVariables);
            }

            if (table.Count == 0)
            {
                foreach (var globalVar in manager.GlobalProfileVariables)
                    table.AddOrUpdateItem(globalVar.Key, globalVar.Value);
                
                foreach (var kvp in data)
                    table.AddOrUpdateItem(kvp.Key, kvp.Value);
            }
            
            var rowHeight = table.RowHeight;
            var tableHeight = table.multiColumnHeader.height + rowHeight + (rowHeight * (data.Count() + manager.GlobalProfileVariables.Count)); // header + 1 extra line
        
            table.OnGUI(EditorGUILayout.GetControlRect(false, tableHeight)); 
        }

        /// <inheritdoc/>
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            IHostingService svc = null;

            if (args.Length > 0)
                svc = args[args.Length - 1] as IHostingService;

            if (svc != null)
            {
                m_LogText += string.Format("[{0}] ", svc.DescriptiveName) + string.Format(format, args) + "\n";
                m_NewLogContent = true;
            }

            Debug.unityLogger.LogFormat(logType, context, format, args);
        }

        /// <inheritdoc/>
        public void LogException(Exception exception, Object context)
        {
            Debug.unityLogger.LogException(exception, context);
        }

        /// <inheritdoc/>
        public void OnBeforeSerialize()
        {
            // No implementation
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize()
        {
            // No implementation
        }
    }
}
