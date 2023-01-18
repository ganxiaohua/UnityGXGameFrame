using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    /// <summary>
    /// Base class for creating rules to analyze Addressables data.  Use AnalyzeWindow.RegisterNewRule&lt;T&gt;() to register.
    ///  a rule with the GUI window.
    /// </summary>
    [Serializable]
    public class AnalyzeRule
    {
        internal struct CustomContextMenu
        {
            public string MenuName;
            public Action MenuAction;
            public bool MenuEnabled;
            public bool ToggledOn;

            public CustomContextMenu(string name, Action action, bool enabled, bool toggledOn)
            {
                MenuName = name;
                MenuAction = action;
                MenuEnabled = enabled;
                ToggledOn = toggledOn;
            }
        }
        
        /// <summary>
        /// True if this rule can fix itself.  If child class sets this to true, class must override FixIssues
        /// </summary>
        public virtual bool CanFix { get; set; }

        [SerializeField]
        internal List<AnalyzeResult> m_Results = new List<AnalyzeResult>();

        /// <summary>
        /// Represents a state where no errors were found after analyzing Addressables data.
        /// </summary>
        [NonSerialized]
        protected AnalyzeResult noErrors = new AnalyzeResult { resultName = "No issues found" };

        /// <summary>
        /// Delimiter character used in analyze rule string names.  This is used when a rule result needs to display
        /// as a tree view hierarchy.  A rule result of A:B:C will end up in the tree view with:
        ///  - A
        ///  --- B
        ///  ----- C
        /// </summary>
        public const char kDelimiter = ':';

        /// <summary>
        /// Result data returned by rules.
        /// </summary>
        [Serializable]
        public class AnalyzeResult
        {
            [SerializeField]
            string m_ResultName;

            /// <summary>
            /// Name of result data.  This name uses AnalyzeRule.kDelimiter to signify breaks in the tree display.
            /// </summary>
            public string resultName
            {
                get { return m_ResultName; }
                set { m_ResultName = value; }
            }

            [SerializeField]
            MessageType m_Severity = MessageType.None;
            /// <summary>
            /// Severity of rule result
            /// </summary>
            public MessageType severity
            {
                get { return m_Severity; }
                set { m_Severity = value; }
            }
        }

        /// <summary>
        /// Display name for rule
        /// </summary>
        public virtual string ruleName
        {
            get { return GetType().ToString(); }
        }

        /// <summary>
        /// This method runs the actual analysis for the rule.
        /// </summary>
        /// <param name="settings">The settings object to analyze</param>
        /// <returns>A list of resulting information (warnings, errors, or info)</returns>
        public virtual List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            return new List<AnalyzeResult>();
        }

        /// <summary>
        /// Fixing method to be run on results of the RefreshAnalysis.  If CanFix returns true, this method must be
        /// overriden.  It is recommended that RefreshAnalysis caches any data that will be needed to fix.  Fix should
        /// not rerun RefreshAnalysis before fixing.
        /// </summary>
        /// <param name="settings">The settings object to analyze</param>
        public virtual void FixIssues(AddressableAssetSettings settings) {}

        /// <summary>
        /// Clears out the analysis results. When overriding, use to clear rule-specific data as well.
        /// </summary>
        public virtual void ClearAnalysis()
        {
            m_Results.Clear();
        }

        internal virtual IList<CustomContextMenu> GetCustomContextMenuItems() { return new List<CustomContextMenu>(); }
    }
}
