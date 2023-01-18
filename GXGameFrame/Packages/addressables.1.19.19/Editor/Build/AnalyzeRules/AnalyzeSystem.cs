using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.Build
{
    /// <summary>
    /// Static system to manage Analyze functionality.
    /// </summary>
    [Serializable]
    public static class AnalyzeSystem
    {
        /// <summary>
        /// Method used to register any custom AnalyzeRules with the AnalyzeSystem.  This replaces calling into the AnalyzeWindow
        ///  directly to remove logic from the GUI.  The recommended pattern is to create
        /// your rules like so:
        /// <code>
        ///   class MyRule : AnalyzeRule {}
        ///   [InitializeOnLoad]
        ///   class RegisterMyRule
        ///   {
        ///       static RegisterMyRule()
        ///       {
        ///           AnalyzeSystem.RegisterNewRule&lt;MyRule&gt;();
        ///       }
        ///   }
        /// </code>
        /// </summary>
        /// <typeparam name="TRule">The rule type.</typeparam>
        public static void RegisterNewRule<TRule>() where TRule : AnalyzeRule, new()
        {
            foreach (var rule in Rules)
            {
                if (rule.GetType().IsAssignableFrom(typeof(TRule)))
                    return;
            }
            Rules.Add(new TRule());
        }

        internal static string AnalyzeRuleDataFolder
        {
            get
            {
                return $"{Addressables.LibraryPath}/AnalyzeData";
            }
        }

        internal static string AnalyzeRuleDataName => "AnalyzeRuleData.json"; 
        internal static string AnalyzeRuleDataPath => AnalyzeRuleDataFolder + "/" + AnalyzeRuleDataName;

        internal static string AnalyzeRuleDataAssetsFolderPath
        {
            get
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var path = AddressableAssetSettingsDefaultObject.kDefaultConfigFolder;
                if (settings != null && settings.IsPersisted)
                    path = settings.ConfigFolder;

                return path + "/AnalyzeData/";
            }
        }
        internal static AddressableAssetSettings Settings => AddressableAssetSettingsDefaultObject.Settings;

        internal static List<AnalyzeRule> Rules { get; } = new List<AnalyzeRule>();

        [SerializeField]
        private static AddressablesAnalyzeResultData m_AnalyzeData;

        internal static AssetSettingsAnalyzeTreeView TreeView { get; set; }

        internal static AddressablesAnalyzeResultData AnalyzeData
        {
            get
            {
                if (m_AnalyzeData == null)
                {
                    if (!Directory.Exists(AnalyzeRuleDataFolder))
                        Directory.CreateDirectory(AnalyzeRuleDataFolder);

                    if (!File.Exists(AnalyzeRuleDataPath))
                        File.WriteAllText(AnalyzeRuleDataPath, JsonUtility.ToJson(new AddressablesAnalyzeResultData()));

                    //Cleans up the previous result data
                    if (Directory.Exists(AnalyzeRuleDataAssetsFolderPath))
                        Directory.Delete(AnalyzeRuleDataAssetsFolderPath, true);

                    m_AnalyzeData = JsonUtility.FromJson<AddressablesAnalyzeResultData>(File.ReadAllText(AnalyzeRuleDataPath));
                    if(m_AnalyzeData == null)
                        Addressables.LogWarning($"Unable to load Analyze Result Data at {AnalyzeRuleDataPath}.");
                    else
                    {
                        if(m_AnalyzeData.Data == null)
                            m_AnalyzeData.Data = new Dictionary<string, List<AnalyzeRule.AnalyzeResult>>();

                        foreach (var rule in Rules)
                        {
                            if (rule == null)
                            {
                                Addressables.LogWarning("An unknown Analyze rule is being skipped because it is null.");
                                continue;
                            }

                            if (!m_AnalyzeData.Data.ContainsKey(rule.ruleName))
                                m_AnalyzeData.Data.Add(rule.ruleName, new List<AnalyzeRule.AnalyzeResult>());
                        }
                    }
                }

                return m_AnalyzeData;
            }
        }

        internal static void ReloadUI()
        {
            TreeView?.Reload();
        }

        internal static void SerializeData()
        {
            File.WriteAllText(AnalyzeRuleDataPath, JsonUtility.ToJson(m_AnalyzeData));
        }

        internal static void SaveDataForRule(AnalyzeRule rule, object data)
        {
            string jsonData = JsonUtility.ToJson(data);
            string path = $"{AnalyzeRuleDataFolder}/{rule.ruleName}Data.json";
            File.WriteAllText(path, jsonData);
        }

        internal static T GetDataForRule<T>(AnalyzeRule rule)
        {
            string path = $"{AnalyzeRuleDataFolder}/{rule.ruleName}Data.json";
            if (!File.Exists(path))
                return default;
            string fileRead = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(fileRead);
        }

        internal static void ReplaceAnalyzeData(AnalyzeRule rule, List<AnalyzeRule.AnalyzeResult> results)
        {
            m_AnalyzeData.Data[rule.ruleName] = results;
        }

        internal static List<AnalyzeRule.AnalyzeResult> RefreshAnalysis<TRule>() where TRule : AnalyzeRule
        {
            return RefreshAnalysis(FindRule<TRule>());
        }

        internal static List<AnalyzeRule.AnalyzeResult> RefreshAnalysis(AnalyzeRule rule)
        {
            if (rule == null)
                return null;

            if (!AnalyzeData.Data.ContainsKey(rule.ruleName))
                AnalyzeData.Data.Add(rule.ruleName, new List<AnalyzeRule.AnalyzeResult>());

            AnalyzeData.Data[rule.ruleName] = rule.RefreshAnalysis(Settings);

            return AnalyzeData.Data[rule.ruleName];
        }

        internal static void ClearAnalysis<TRule>() where TRule : AnalyzeRule
        {
            ClearAnalysis(FindRule<TRule>());
        }

        internal static void ClearAnalysis(AnalyzeRule rule)
        {
            if (rule == null)
                return;

            if (!AnalyzeData.Data.ContainsKey(rule.ruleName))
                AnalyzeData.Data.Add(rule.ruleName, new List<AnalyzeRule.AnalyzeResult>());

            rule.ClearAnalysis();;
            AnalyzeData.Data[rule.ruleName].Clear();
        }

        internal static void FixIssues<TRule>() where TRule : AnalyzeRule
        {
            FixIssues(FindRule<TRule>());
        }

        internal static void FixIssues(AnalyzeRule rule)
        {
            rule?.FixIssues(Settings);
        }

        private static AnalyzeRule FindRule<TRule>() where TRule : AnalyzeRule
        {
            var rule = Rules.FirstOrDefault(r => r.GetType().IsAssignableFrom(typeof(TRule)));
            if (rule == null)
                throw new ArgumentException($"No rule found corresponding to type {typeof(TRule)}");

            return rule;
        }
    }
}
