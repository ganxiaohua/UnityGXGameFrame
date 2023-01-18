using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    /// <summary>
    /// Rule class to check scene dependencies for duplicates
    /// </summary>
    public class CheckSceneDupeDependencies : BundleRuleBase
    {
        /// <inheritdoc />
        public override bool CanFix
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override string ruleName
        {
            get { return "Check Scene to Addressable Duplicate Dependencies"; }
        }

        /// <summary>
        /// Clear analysis and calculate built in resources and corresponding bundle dependencies for scenes
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        /// <returns>List of results from analysis</returns>
        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            ClearAnalysis();

            string[] scenePaths = (from editorScene in EditorBuildSettings.scenes 
                 where editorScene.enabled select editorScene.path).ToArray();

            return CalculateBuiltInResourceDependenciesToBundleDependecies(settings, scenePaths);
        }

        /// <inheritdoc />
        public override void FixIssues(AddressableAssetSettings settings)
        {
            //Do nothing.  There's nothing to fix.
        }
    }

    [InitializeOnLoad]
    class RegisterCheckSceneDupeDependencies
    {
        static RegisterCheckSceneDupeDependencies()
        {
            AnalyzeSystem.RegisterNewRule<CheckSceneDupeDependencies>();
        }
    }
}
