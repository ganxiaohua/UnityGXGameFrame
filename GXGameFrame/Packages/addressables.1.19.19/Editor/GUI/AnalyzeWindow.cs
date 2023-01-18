using System;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    /// <summary>
    /// Window used to execute AnalyzeRule sets.
    /// </summary>
    public class AnalyzeWindow : EditorWindow
    {
        private static AnalyzeWindow s_Instance = null;
        private static AnalyzeWindow instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetWindow<AnalyzeWindow>(false, "Addressables Analyze", false);
                return s_Instance;
            }
        }

        private AddressableAssetSettings m_Settings;

        [SerializeField]
        private AnalyzeRuleGUI m_AnalyzeEditor;

        private Rect displayAreaRect
        {
            get
            {
                return new Rect(0, 0, position.width, position.height);
            }
        }

        [MenuItem("Window/Asset Management/Addressables/Analyze", priority = 2052)]
        internal static void ShowWindow()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Error", "Attempting to open Addressables Analyze window, but no Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
                return;
            }

            instance.titleContent = new GUIContent("Addressables Analyze");
            instance.Show();
        }

        void OnEnable()
        {
            if (m_AnalyzeEditor == null)
                m_AnalyzeEditor = new AnalyzeRuleGUI();
        }

        void OnGUI()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;

            GUILayout.BeginArea(displayAreaRect);
            m_AnalyzeEditor.OnGUI(displayAreaRect);
            GUILayout.EndArea();
        }

        /// <summary>
        /// Obsolete - please use AnalyzeSystem.RegisterNewRule&lt;TRule&gt;()
        /// </summary>
        /// <typeparam name="TRule">The rule type.</typeparam>
        [Obsolete("Please use AnalyzeSystem.RegisterNewRule<TRule>()")]
        public static void RegisterNewRule<TRule>() where TRule : AnalyzeRule, new()
        {
            AnalyzeSystem.RegisterNewRule<TRule>();
        }
    }
}
