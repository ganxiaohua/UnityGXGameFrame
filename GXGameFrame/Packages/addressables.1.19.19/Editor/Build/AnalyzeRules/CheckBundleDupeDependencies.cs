using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    /// <summary>
    /// Rule class to check for duplicate bundle dependencies
    /// </summary>
    public class CheckBundleDupeDependencies : BundleRuleBase
    {
        /// <summary>
        /// Result for checking for duplicates
        /// </summary>
        protected internal struct CheckDupeResult
        {
            public AddressableAssetGroup Group;
            public string DuplicatedFile;
            public string AssetPath;
            public GUID DuplicatedGroupGuid;
        }

        /// <summary>
        /// Results for duplicate results inverted check
        /// </summary>
        protected internal struct ExtraCheckBundleDupeData
        {
            public bool ResultsInverted;
        }

        /// <inheritdoc />
        public override bool CanFix
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override string ruleName
        { get { return "Check Duplicate Bundle Dependencies"; } }

        [NonSerialized]
        internal readonly Dictionary<string, Dictionary<string, List<string>>> m_AllIssues = new Dictionary<string, Dictionary<string, List<string>>>();
        [SerializeField]
        internal HashSet<GUID> m_ImplicitAssets;

        /// <summary>
        /// Clear current analysis and rerun check for duplicates
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        /// <returns>List of the analysis results</returns>
        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            ClearAnalysis();
            return CheckForDuplicateDependencies(settings);
        }

        void RefreshDisplay()
        {
            var savedData = AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this);
            if (!savedData.ResultsInverted)
            {
                m_Results = (from issueGroup in m_AllIssues
                    from bundle in issueGroup.Value
                    from item in bundle.Value
                    select new AnalyzeResult
                    {
                        resultName = issueGroup.Key + kDelimiter +
                                     ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                                     item,
                        severity = MessageType.Warning
                    }).ToList();
            }
            else
            {
                m_Results = (from issueGroup in m_AllIssues
                             from bundle in issueGroup.Value
                             from item in bundle.Value
                             select new AnalyzeResult
                             {
                                 resultName = item + kDelimiter +
                                              ConvertBundleName(bundle.Key, issueGroup.Key) + kDelimiter +
                                              issueGroup.Key,
                                 severity = MessageType.Warning
                             }).ToList();
            }
            if (m_Results.Count == 0)
                m_Results.Add(noErrors);
        }

        internal override IList<CustomContextMenu> GetCustomContextMenuItems()
        {
            IList<CustomContextMenu> customItems = new List<CustomContextMenu>();
            customItems.Add(new CustomContextMenu("Organize by Asset", 
                () => InvertDisplay(), 
                AnalyzeSystem.AnalyzeData.Data[ruleName].Any(),
                AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this).ResultsInverted));
            return customItems;
        }

        void InvertDisplay()
        {
            List<AnalyzeResult> updatedResults = new List<AnalyzeResult>();
            foreach (var result in AnalyzeSystem.AnalyzeData.Data[ruleName])
            {
                updatedResults.Add(new AnalyzeResult()
                {
                    //start at index 1 because the first result is going to be the rule name which we want to remain where it is.
                    resultName = ReverseStringFromIndex(result.resultName, 1, kDelimiter),
                    severity = result.severity
                });
            }

            AnalyzeSystem.ReplaceAnalyzeData(this, updatedResults);
            var savedData = AnalyzeSystem.GetDataForRule<ExtraCheckBundleDupeData>(this);
            savedData.ResultsInverted = !savedData.ResultsInverted;
            AnalyzeSystem.SaveDataForRule(this, savedData);
            AnalyzeSystem.SerializeData();
            AnalyzeSystem.ReloadUI();
        }

        private string ReverseStringFromIndex(string data, int startingIndex, char delimiter)
        {
            string[] splitData = data.Split(delimiter);
            int i = startingIndex;
            int k = splitData.Length - 1;
            while (i < k)
            {
                string temp = splitData[i];
                splitData[i] = splitData[k];
                splitData[k] = temp;
                i++;
                k--;
            }

            return String.Join(kDelimiter.ToString(), splitData);
        }

        /// <summary>
        /// Check for duplicates among the dependencies and build implicit duplicates
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        /// <returns>List of results from analysis</returns>
        protected List<AnalyzeResult> CheckForDuplicateDependencies(AddressableAssetSettings settings)
        {
            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                m_Results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return m_Results;
            }

            CalculateInputDefinitions(settings);

            if (m_AllBundleInputDefs.Count > 0)
            {
                var context = GetBuildContext(settings);
                ReturnCode exitCode = RefreshBuild(context);
                if (exitCode < ReturnCode.Success)
                {
                    Debug.LogError("Analyze build failed. " + exitCode);
                    m_Results.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
                    return m_Results;
                }

                var implicitGuids = GetImplicitGuidToFilesMap();
                var checkDupeResults = CalculateDuplicates(implicitGuids, context);
                BuildImplicitDuplicatedAssetsSet(checkDupeResults);
            }

            RefreshDisplay();
            return m_Results;
        }
        
        /// <summary>
        /// Calculate duplicate dependencies 
        /// </summary>
        /// <param name="implicitGuids">Map of implicit guids to their bundle files</param>
        /// <param name="aaContext">The build context information</param>
        /// <returns>Enumerable of results from duplicates check</returns>
        protected internal IEnumerable<CheckDupeResult> CalculateDuplicates(Dictionary<GUID, List<string>> implicitGuids, AddressableAssetsBuildContext aaContext)
        {
            //Get all guids that have more than one bundle referencing them
            IEnumerable<KeyValuePair<GUID, List<string>>> validGuids =
                from dupeGuid in implicitGuids
                where dupeGuid.Value.Distinct().Count() > 1
                where IsValidPath(AssetDatabase.GUIDToAssetPath(dupeGuid.Key.ToString()))
                select dupeGuid;

            return
                from guidToFile in validGuids
                from file in guidToFile.Value

                //Get the files that belong to those guids
                let fileToBundle = m_ExtractData.WriteData.FileToBundle[file]

                    //Get the bundles that belong to those files
                    let bundleToGroup = aaContext.bundleToAssetGroup[fileToBundle]

                    //Get the asset groups that belong to those bundles
                    let selectedGroup = aaContext.Settings.FindGroup(findGroup => findGroup != null && findGroup.Guid == bundleToGroup)

                    select new CheckDupeResult
                {
                    Group = selectedGroup,
                    DuplicatedFile = file,
                    AssetPath = AssetDatabase.GUIDToAssetPath(guidToFile.Key.ToString()),
                    DuplicatedGroupGuid = guidToFile.Key
                };
        }

        internal void BuildImplicitDuplicatedAssetsSet(IEnumerable<CheckDupeResult> checkDupeResults)
        {
            m_ImplicitAssets = new HashSet<GUID>();

            foreach (var checkDupeResult in checkDupeResults)
            {
                Dictionary<string, List<string>> groupData;
                if (!m_AllIssues.TryGetValue(checkDupeResult.Group.Name, out groupData))
                {
                    groupData = new Dictionary<string, List<string>>();
                    m_AllIssues.Add(checkDupeResult.Group.Name, groupData);
                }

                List<string> assets;
                if (!groupData.TryGetValue(m_ExtractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], out assets))
                {
                    assets = new List<string>();
                    groupData.Add(m_ExtractData.WriteData.FileToBundle[checkDupeResult.DuplicatedFile], assets);
                }

                assets.Add(checkDupeResult.AssetPath);

                m_ImplicitAssets.Add(checkDupeResult.DuplicatedGroupGuid);
            }
        }

        /// <summary>
        /// Fix duplicates by moving to a new group
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        public override void FixIssues(AddressableAssetSettings settings)
        {
            if (m_ImplicitAssets == null)
                CheckForDuplicateDependencies(settings);

            if (m_ImplicitAssets.Count == 0)
                return;

            var group = settings.CreateGroup("Duplicate Asset Isolation", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;

            foreach (var asset in m_ImplicitAssets)
                settings.CreateOrMoveEntry(asset.ToString(), group, false, false);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
        }

        /// <inheritdoc />
        public override void ClearAnalysis()
        {
            m_AllIssues.Clear();
            m_ImplicitAssets = null;
            base.ClearAnalysis();
        }
    }


    [InitializeOnLoad]
    class RegisterCheckBundleDupeDependencies
    {
        static RegisterCheckBundleDupeDependencies()
        {
            AnalyzeSystem.RegisterNewRule<CheckBundleDupeDependencies>();
        }
    }
}
