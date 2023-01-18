using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UnityEditor.AddressableAssets.GUI
{
    class AssetSettingsAnalyzeTreeView : TreeView
    {
        private int m_CurrentDepth;

        internal AssetSettingsAnalyzeTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            Reload();
        }

        private List<AnalyzeRuleContainerTreeViewItem> GatherAllInheritRuleContainers(TreeViewItem baseContainer)
        {
            List<AnalyzeRuleContainerTreeViewItem> retValue = new List<AnalyzeRuleContainerTreeViewItem>();
            if (!baseContainer.hasChildren)
                return new List<AnalyzeRuleContainerTreeViewItem>();

            foreach (var child in baseContainer.children)
            {
                if (child is AnalyzeRuleContainerTreeViewItem)
                {
                    retValue.AddRange(GatherAllInheritRuleContainers(child as AnalyzeRuleContainerTreeViewItem));
                    retValue.Add(child as AnalyzeRuleContainerTreeViewItem);
                }
            }

            return retValue;
        }

        private void PerformActionForEntireRuleSelection(Action<AnalyzeRuleContainerTreeViewItem> action)
        {
            List<AnalyzeRuleContainerTreeViewItem> activeSelection = (from id in GetSelection()
                let selection = FindItem(id, rootItem)
                    where selection is AnalyzeRuleContainerTreeViewItem
                    select selection as AnalyzeRuleContainerTreeViewItem).ToList();

            List<AnalyzeRuleContainerTreeViewItem> inheritSelection = new List<AnalyzeRuleContainerTreeViewItem>();
            foreach (var selected in activeSelection)
                inheritSelection.AddRange(GatherAllInheritRuleContainers(selected));

            List<AnalyzeRuleContainerTreeViewItem> entireSelection = activeSelection.Union(inheritSelection).ToList();

            foreach (AnalyzeRuleContainerTreeViewItem ruleContainer in entireSelection)
            {
                if (ruleContainer.analyzeRule != null)
                {
                    action(ruleContainer);
                }
            }
        }

        public void RunAllSelectedRules()
        {
            PerformActionForEntireRuleSelection((ruleContainer) =>
            {
                var results = AnalyzeSystem.RefreshAnalysis(ruleContainer.analyzeRule);

                BuildResults(ruleContainer, results);
                Reload();
                UpdateSelections(GetSelection());
            });
        }

        public void FixAllSelectedRules()
        {
            PerformActionForEntireRuleSelection((ruleContainer) =>
            {
                AnalyzeSystem.FixIssues(ruleContainer.analyzeRule);
                var results = AnalyzeSystem.RefreshAnalysis(ruleContainer.analyzeRule);

                BuildResults(ruleContainer, results);
                Reload();
                UpdateSelections(GetSelection());
            });
        }

        public void ClearAllSelectedRules()
        {
            PerformActionForEntireRuleSelection((ruleContainer) =>
            {
                AnalyzeSystem.ClearAnalysis(ruleContainer.analyzeRule);

                BuildResults(ruleContainer, new List<AnalyzeRule.AnalyzeResult>());
                Reload();
                UpdateSelections(GetSelection());
            });
        }

        public bool SelectionContainsFixableRule { get; private set; }
        public bool SelectionContainsRuleContainer { get; private set; }

        public bool SelectionContainsErrors { get; private set; }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            UpdateSelections(selectedIds);
        }

        void UpdateSelections(IList<int> selectedIds)
        {
            var allSelectedRuleContainers = (from id in selectedIds
                let ruleContainer = FindItem(id, rootItem) as AnalyzeRuleContainerTreeViewItem
                    where ruleContainer != null
                    select ruleContainer);

            List<AnalyzeRuleContainerTreeViewItem> allRuleContainers = new List<AnalyzeRuleContainerTreeViewItem>();
            foreach (var ruleContainer in allSelectedRuleContainers)
            {
                allRuleContainers.AddRange(GatherAllInheritRuleContainers(ruleContainer));
                allRuleContainers.Add(ruleContainer);
            }

            allRuleContainers = allRuleContainers.Distinct().ToList();

            SelectionContainsErrors = (from container in allRuleContainers
                from child in container.children
                where child is AnalyzeResultsTreeViewItem && (child as AnalyzeResultsTreeViewItem).IsError
                select child).Any();

            SelectionContainsRuleContainer = allRuleContainers.Any();

            SelectionContainsFixableRule = (from container in allRuleContainers
                where container.analyzeRule.CanFix
                select container).Any();
        }

        protected override void ContextClicked()
        {
            if (SelectionContainsRuleContainer)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Run Analyze Rule"), false, RunAllSelectedRules);
                menu.AddItem(new GUIContent("Clear Analyze Results"), false, ClearAllSelectedRules);

                if (SelectionContainsFixableRule && SelectionContainsErrors)
                    menu.AddItem(new GUIContent("Fix Analyze Rule"), false, FixAllSelectedRules);
                else
                    menu.AddDisabledItem(new GUIContent("Fix Analyze Rule"));

                IList<int> selectedIds = GetSelection();
                if (selectedIds.Count == 1)
                {
                    AnalyzeRuleContainerTreeViewItem analyzeRuleContainer = FindItem(selectedIds[0], rootItem) as AnalyzeRuleContainerTreeViewItem;
                    if (analyzeRuleContainer != null)
                    {
                        foreach (var customMenuItem in analyzeRuleContainer.analyzeRule.GetCustomContextMenuItems())
                        {
                            if(customMenuItem.MenuEnabled)
                                menu.AddItem(new GUIContent(customMenuItem.MenuName), customMenuItem.ToggledOn, () => customMenuItem.MenuAction());
                            else
                                menu.AddDisabledItem(new GUIContent(customMenuItem.MenuName));
                        }
                    }
                }

                menu.ShowAsContext();
                Repaint();
            }
            else
            {
                var selectedIds = this.GetSelection();
                List<AnalyzeResultsTreeViewItem> items = new List<AnalyzeResultsTreeViewItem>();
                foreach (int id in selectedIds)
                {
                    var item = FindItem(id, rootItem) as AnalyzeResultsTreeViewItem;
                    if (item != null)
                        items.Add(item);
                }
                
                if (items.Count > 0)
                    AnalyzeResultsTreeViewItem.ContextClicked(items);
            }
            
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AnalyzeResultsTreeViewItem;
            if (item != null)
                item.DoubleClicked();
        }

        protected override TreeViewItem BuildRoot()
        {
            m_CurrentDepth = 0;
            var root = new TreeViewItem(-1, -1);
            root.children = new List<TreeViewItem>();

            string baseName = "Analyze Rules";
            string fixableRules = "Fixable Rules";
            string unfixableRules = "Unfixable Rules";

            AnalyzeSystem.TreeView = this;

            AnalyzeRuleContainerTreeViewItem baseViewItem = new AnalyzeRuleContainerTreeViewItem(baseName.GetHashCode(), m_CurrentDepth, baseName);
            baseViewItem.children = new List<TreeViewItem>();
            baseViewItem.analyzeRule.CanFix = true;

            root.AddChild(baseViewItem);

            m_CurrentDepth++;

            var fixable = new AnalyzeRuleContainerTreeViewItem(fixableRules.GetHashCode(), m_CurrentDepth, fixableRules);
            var unfixable = new AnalyzeRuleContainerTreeViewItem(unfixableRules.GetHashCode(), m_CurrentDepth, unfixableRules);

            fixable.analyzeRule.CanFix = true;
            unfixable.analyzeRule.CanFix = false;

            baseViewItem.AddChild(fixable);
            baseViewItem.AddChild(unfixable);

            m_CurrentDepth++;

            for (int i = 0; i < AnalyzeSystem.Rules.Count; i++)
            {
                AnalyzeRuleContainerTreeViewItem ruleContainer = new AnalyzeRuleContainerTreeViewItem(
                    AnalyzeSystem.Rules[i].ruleName.GetHashCode(), m_CurrentDepth, AnalyzeSystem.Rules[i]);

                if (ruleContainer.analyzeRule.CanFix)
                    fixable.AddChild(ruleContainer);
                else
                    unfixable.AddChild(ruleContainer);
            }

            m_CurrentDepth++;

            int index = 0;
            var ruleContainers = GatherAllInheritRuleContainers(baseViewItem);
            foreach (var ruleContainer in ruleContainers)
            {
                if(ruleContainer == null)
                    continue;

                EditorUtility.DisplayProgressBar("Calculating Analyze Results...", ruleContainer.displayName, (index / (float)ruleContainers.Count));
                if (AnalyzeSystem.AnalyzeData.Data.ContainsKey(ruleContainer.analyzeRule.ruleName))
                    BuildResults(ruleContainer, AnalyzeSystem.AnalyzeData.Data[ruleContainer.analyzeRule.ruleName]);

                index++;
            }

            EditorUtility.ClearProgressBar();
            return root;
        }

        private readonly Dictionary<int, AnalyzeResultsTreeViewItem> hashToAnalyzeResults = new Dictionary<int, AnalyzeResultsTreeViewItem>();
        void BuildResults(TreeViewItem root, List<AnalyzeRule.AnalyzeResult> ruleResults)
        {
            hashToAnalyzeResults.Clear();
            int updateFrequency = Mathf.Max(ruleResults.Count / 10, 1);

            for (int index=0; index < ruleResults.Count; ++index)
            {
                var result = ruleResults[index];
                if (index == 0 || index % updateFrequency == 0)
                    EditorUtility.DisplayProgressBar("Building Results Tree...", result.resultName, (float)index / hashToAnalyzeResults.Keys.Count);
                
                var resPath = result.resultName.Split(AnalyzeRule.kDelimiter);
                string name = string.Empty;
                TreeViewItem parent = root;
                
                for (int i = 0; i < resPath.Length; i++)
                {
                    name += resPath[i];
                    int hash = name.GetHashCode();

                    if (!hashToAnalyzeResults.ContainsKey(hash))
                    {
                        AnalyzeResultsTreeViewItem item = new AnalyzeResultsTreeViewItem(hash, i + m_CurrentDepth, resPath[i], result.severity, result);
                        hashToAnalyzeResults.Add(item.id, item);
                        parent.AddChild(item);
                        parent = item;
                    }
                    else
                    {
                        var targetItem = hashToAnalyzeResults[hash];
                        targetItem.results.Add(result);
                        parent = targetItem;
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            List<TreeViewItem> allTreeViewItems = new List<TreeViewItem>();
            allTreeViewItems.Add(root);
            allTreeViewItems.AddRange(root.children);

            foreach (var node in allTreeViewItems)
                (node as AnalyzeTreeViewItemBase)?.AddIssueCountToName();

            AnalyzeSystem.SerializeData();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AnalyzeResultsTreeViewItem;
            if (item != null && item.severity != MessageType.None)
            {
                Texture2D icon = null;
                switch (item.severity)
                {
                    case MessageType.Info:
                        icon = GetInfoIcon();
                        break;
                    case MessageType.Warning:
                        icon = GetWarningIcon();
                        break;
                    case MessageType.Error:
                        icon = GetErrorIcon();
                        break;
                }

                UnityEngine.GUI.Label(
                    new Rect(args.rowRect.x + baseIndent, args.rowRect.y, args.rowRect.width - baseIndent,
                        args.rowRect.height), new GUIContent(icon, string.Empty));
            }

            base.RowGUI(args);
        }

        Texture2D m_ErrorIcon;
        Texture2D m_WarningIcon;
        Texture2D m_InfoIcon;

        Texture2D GetErrorIcon()
        {
            if (m_ErrorIcon == null)
                FindMessageIcons();
            return m_ErrorIcon;
        }

        Texture2D GetWarningIcon()
        {
            if (m_WarningIcon == null)
                FindMessageIcons();
            return m_WarningIcon;
        }

        Texture2D GetInfoIcon()
        {
            if (m_InfoIcon == null)
                FindMessageIcons();
            return m_InfoIcon;
        }

        void FindMessageIcons()
        {
            m_ErrorIcon = EditorGUIUtility.FindTexture("console.errorIcon");
            m_WarningIcon = EditorGUIUtility.FindTexture("console.warnicon");
            m_InfoIcon = EditorGUIUtility.FindTexture("console.infoIcon");
        }
    }

    class AnalyzeTreeViewItemBase : TreeViewItem
    {
        private string baseDisplayName;
        private string currentDisplayName;

        public override string displayName
        {
            get { return currentDisplayName; }
            set { baseDisplayName = value; }
        }

        public AnalyzeTreeViewItemBase(int id, int depth, string displayName) : base(id, depth,
                                                                                     displayName)
        {
            currentDisplayName = baseDisplayName = displayName;
        }

        public int AddIssueCountToName()
        {
            int issueCount = 0;
            if (children != null)
            {
                foreach (var child in children)
                {
                    var analyzeNode = child as AnalyzeResultsTreeViewItem;
                    if (analyzeNode != null)
                        issueCount += analyzeNode.AddIssueCountToName();
                }
            }

            if (issueCount == 0)
                return 1;

            currentDisplayName = baseDisplayName + " (" + issueCount + ")";
            return issueCount;
        }
    }

    class AnalyzeResultsTreeViewItem : AnalyzeTreeViewItemBase
    {
        public MessageType severity { get; set; }
        public HashSet<AnalyzeRule.AnalyzeResult> results { get; }

        public bool IsError
        {
            get { return !displayName.Contains("No issues found"); }
        }

        public AnalyzeResultsTreeViewItem(int id, int depth, string displayName, MessageType type)
            : base(id, depth, displayName)
        {
            severity = type;
            results = new HashSet<AnalyzeRule.AnalyzeResult>();
        }
        
        public AnalyzeResultsTreeViewItem(int id, int depth, string displayName, MessageType type, AnalyzeRule.AnalyzeResult analyzeResult)
            : base(id, depth, displayName)
        {
            severity = type;
            results = new HashSet<AnalyzeRule.AnalyzeResult>() {analyzeResult};
        }

        internal static void ContextClicked(List<AnalyzeResultsTreeViewItem> items)
        {
            HashSet<UnityEngine.Object> objects = new HashSet<Object>();
            
            foreach (AnalyzeResultsTreeViewItem viewItem in items)
            {
                foreach (var itemResult in viewItem.results)
                {
                    Object o = GetResultObject(itemResult.resultName);
                    if (o != null)
                        objects.Add(o);
                }
            }

            if (objects.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent(objects.Count > 0 ? "Select Assets" : "Select Asset"), false, () =>
                {
                    Selection.objects = objects.ToArray();
                    foreach (Object o in objects)
                        EditorGUIUtility.PingObject(o);
                });
                menu.ShowAsContext();
            }
        }

        static UnityEngine.Object GetResultObject(string resultName)
        {
            int li = resultName.LastIndexOf(AnalyzeRule.kDelimiter);
            if (li >= 0)
            {
                string assetPath = resultName.Substring(li + 1);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (!string.IsNullOrEmpty(guid))
                    return AssetDatabase.LoadMainAssetAtPath(assetPath);
            }

            return null;
        }

        internal void DoubleClicked()
        {
            HashSet<UnityEngine.Object> objects = new HashSet<Object>();
            foreach (var itemResult in results)
            {
                Object o = GetResultObject(itemResult.resultName);
                if (o != null)
                    objects.Add(o);
            }

            if (objects.Count > 0)
            {
                Selection.objects = objects.ToArray();
                foreach (Object o in objects)
                    EditorGUIUtility.PingObject(o);
            }
        }
    }

    class AnalyzeRuleContainerTreeViewItem : AnalyzeTreeViewItemBase
    {
        internal AnalyzeRule analyzeRule;

        public AnalyzeRuleContainerTreeViewItem(int id, int depth, AnalyzeRule rule) : base(id, depth, rule.ruleName)
        {
            analyzeRule = rule;
            children = new List<TreeViewItem>();
        }

        public AnalyzeRuleContainerTreeViewItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
            analyzeRule = new AnalyzeRule();
            children = new List<TreeViewItem>();
        }
    }
}
