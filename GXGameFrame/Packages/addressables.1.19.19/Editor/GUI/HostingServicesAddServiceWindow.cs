using System;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    class HostingServicesAddServiceWindow : EditorWindow
    {
        MonoScript m_Script;
        string m_HostingName;
        bool m_UseCustomScript;
        AddressableAssetSettings m_Settings;
        Type[] m_ServiceTypes;
        string[] m_ServiceTypeNames;
        int m_ServiceTypeIndex;

        /// <summary>
        /// Initialize the dialog for the given <see cref="AddressableAssetSettings"/>
        /// </summary>
        /// <param name="settings"></param>
        public void Initialize(AddressableAssetSettings settings)
        {
            m_Settings = settings;
            m_HostingName = string.Format("My Hosting Service {0}(Custom)", m_Settings.HostingServicesManager.NextInstanceId);
        }

        void OnGUI()
        {
            if (m_Settings == null) return;
            EditorGUILayout.HelpBox("Select a script that implements the IHostingService interface.", MessageType.Info);
            var script =
                EditorGUILayout.ObjectField("Hosting Service Script", m_Script, typeof(MonoScript), false) as MonoScript;

            if (script != m_Script && script != null)
            {
                var scriptType = script.GetClass();
                if (scriptType == null)
                {
                    EditorUtility.DisplayDialog("Error", "Unable to find a valid type from the specified script.", "Ok");
                    m_Script = null;
                }
                else if (scriptType.IsAbstract)
                {
                    EditorUtility.DisplayDialog("Error", "Script cannot be an Abstract class", "Ok");
                    m_Script = null;
                }
                else if (!typeof(IHostingService).IsAssignableFrom(scriptType))
                {
                    EditorUtility.DisplayDialog("Error", "Selected script does not implement the IHostingService interface", "Ok");
                    m_Script = null;
                }
                else
                {
                    m_Script = script;
                }
            }

            m_HostingName = EditorGUILayout.TextField("Service Name", m_HostingName);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledGroupScope(m_Script == null))
                {
                    if (GUILayout.Button("Add", GUILayout.MaxWidth(75f)))
                    {
                        try
                        {
                            var t = m_Script.GetClass();

                            m_Settings.HostingServicesManager.AddHostingService(t, m_HostingName);
                        }
                        finally
                        {
                            Close();
                            FocusWindowIfItsOpen<HostingServicesWindow>();
                        }
                    }
                }

                if (GUILayout.Button("Cancel", GUILayout.MaxWidth(75f)))
                {
                    Close();
                    FocusWindowIfItsOpen<HostingServicesWindow>();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
