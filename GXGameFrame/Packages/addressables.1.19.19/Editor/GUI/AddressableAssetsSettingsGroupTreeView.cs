using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;
using static UnityEditor.AddressableAssets.Settings.AddressablesFileEnumeration;

namespace UnityEditor.AddressableAssets.GUI
{
    using Object = UnityEngine.Object;

    internal class AddressableAssetEntryTreeView : TreeView
    {
        AddressableAssetsSettingsGroupEditor m_Editor;
        internal string customSearchString = string.Empty;
        string m_FirstSelectedGroup;
        private readonly Dictionary<AssetEntryTreeViewItem, bool> m_SearchedEntries = new Dictionary<AssetEntryTreeViewItem, bool>();
        private bool m_ForceSelectionClear = false;

        enum ColumnId
        {
            Id,
            Type,
            Path,
            Labels
        }
        ColumnId[] m_SortOptions =
        {
            ColumnId.Id,
            ColumnId.Type,
            ColumnId.Path,
            ColumnId.Labels
        };

        internal AddressableAssetEntryTreeView(AddressableAssetSettings settings)
            : this(new TreeViewState(), CreateDefaultMultiColumnHeaderState(), new AddressableAssetsSettingsGroupEditor(ScriptableObject.CreateInstance<AddressableAssetsWindow>()))
        {
            m_Editor.settings = settings;
        }

        public AddressableAssetEntryTreeView(TreeViewState state, MultiColumnHeaderState mchs, AddressableAssetsSettingsGroupEditor ed) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            m_Editor = ed;
            columnIndexForTreeFoldouts = 0;
            multiColumnHeader.sortingChanged += OnSortingChanged;

            BuiltinSceneCache.sceneListChanged += OnScenesChanged;
            AddressablesAssetPostProcessor.OnPostProcess.Register(OnPostProcessAllAssets, 1);
        }

        internal TreeViewItem Root => rootItem;

        void OnScenesChanged()
        {
            if (m_Editor.settings == null)
                return;
            Reload();
        }

        void OnSortingChanged(MultiColumnHeader mch)
        {
            SortChildren(rootItem);
            Reload();
        }

        void OnPostProcessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (Object obj in Selection.objects)
            {
                if (obj == null)
                    continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj.GetInstanceID(), out string guid, out long localId))
                {
                    if (obj is GameObject go)
                    {
#if UNITY_2021_2_OR_NEWER
                        if (UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go) != null)
                            return;
#else
                        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(go) != null)
                            return;
#endif
                        var containingScene = go.scene;
                        if (containingScene.IsValid() && containingScene.isLoaded)
                            return;
                    }
                    m_ForceSelectionClear = true;
                    return;
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 1)
            {
                var item = FindItemInVisibleRows(selectedIds[0]);
                if (item != null && item.group != null)
                {
                    m_FirstSelectedGroup = item.group.name;
                }
            }

            base.SelectionChanged(selectedIds);

            UnityEngine.Object[] selectedObjects = new UnityEngine.Object[selectedIds.Count];
            for (int i = 0; i < selectedIds.Count; i++)
            {
                var item = FindItemInVisibleRows(selectedIds[i]);
                if (item != null)
                {
                    if (item.group != null)
                        selectedObjects[i] = item.group;
                    else if (item.entry != null)
                        selectedObjects[i] = item.entry.TargetAsset;
                }
            }

            // Make last selected group the first object in the array
            if (!string.IsNullOrEmpty(m_FirstSelectedGroup) && selectedObjects.Length > 1)
            {
                for (int i = 0; i < selectedObjects.Length-1; ++i)
                {
                    if (selectedObjects[i] != null && selectedObjects[i].name == m_FirstSelectedGroup)
                    {
                        var temp = selectedObjects[i];
                        selectedObjects[i] = selectedObjects[selectedIds.Count - 1];
                        selectedObjects[selectedIds.Count - 1] = temp;
                    }
                }
            }

            Selection.objects = selectedObjects; // change selection
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            using (new AddressablesFileEnumerationScope(BuildAddressableTree(m_Editor.settings)))
            {
                foreach (var group in m_Editor.settings.groups)
                    AddGroupChildrenBuild(group, root);
            }
            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                var rows = base.BuildRows(root);
                SortHierarchical(rows);
                return rows;
            }
            if (!string.IsNullOrEmpty(customSearchString))
            {
                SortChildren(root);
                return Search(base.BuildRows(root));
            }

            SortChildren(root);
            return base.BuildRows(root);
        }

        internal IList<TreeViewItem> Search(string search)
        {
            if (ProjectConfigData.HierarchicalSearch)
            {
                customSearchString = search;
                Reload();
            }
            else
            {
                searchString = search;
            }

            return GetRows();
        }

        protected IList<TreeViewItem> Search(IList<TreeViewItem> rows)
        {
            if (rows == null)
                return new List<TreeViewItem>();

            m_SearchedEntries.Clear();
            List<TreeViewItem> items = new List<TreeViewItem>(rows.Count);
            foreach (TreeViewItem item in rows)
            {
                if (ProjectConfigData.HierarchicalSearch)
                {
                    if(SearchHierarchical(item, customSearchString))
                        items.Add(item);
                }
                else if (DoesItemMatchSearch(item, searchString))
                    items.Add(item);
            }
            return items;
        }

        /*
         * Hierarchical search requirements :
         * An item is kept if :
         * - it matches
         * - an ancestor matches
         * - at least one descendant matches
         */
        bool SearchHierarchical(TreeViewItem item, string search, bool? ancestorMatching = null)
        {
            var aeItem = item as AssetEntryTreeViewItem;
            if (aeItem == null || search == null)
                return false;

            if (m_SearchedEntries.ContainsKey(aeItem))
                return m_SearchedEntries[aeItem];

            if (ancestorMatching == null)
                ancestorMatching = DoesAncestorMatch(aeItem, search);

            bool isMatching = false;
            if (!ancestorMatching.Value)
                isMatching = DoesItemMatchSearch(aeItem, search);

            bool descendantMatching = false;
            if (!ancestorMatching.Value && !isMatching && aeItem.hasChildren)
            {
                foreach (var child in aeItem.children)
                {
                    descendantMatching = SearchHierarchical(child, search, false);
                    if (descendantMatching)
                        break;
                }
            }

            bool keep = isMatching || ancestorMatching.Value || descendantMatching;
            m_SearchedEntries.Add(aeItem, keep);
            return keep;
        }

        private bool DoesAncestorMatch(TreeViewItem aeItem, string search)
        {
            if (aeItem == null)
                return false;

            var ancestor = aeItem.parent as AssetEntryTreeViewItem;
            bool isMatching = DoesItemMatchSearch(ancestor, search);
            while (ancestor != null && !isMatching)
            {
                ancestor = ancestor.parent as AssetEntryTreeViewItem;
                isMatching = DoesItemMatchSearch(ancestor, search);
            }

            return isMatching;
        }

        internal void ClearSearch()
        {
            customSearchString = string.Empty;
            searchString = string.Empty;
            m_SearchedEntries.Clear();
        }

        internal void SwapSearchType()
        {
            string temp = customSearchString;
            customSearchString = searchString;
            searchString = temp;
            m_SearchedEntries.Clear();
        }

        void SortChildren(TreeViewItem root)
        {
            if (!root.hasChildren)
                return;
            foreach (var child in root.children)
            {
                if (child != null && IsExpanded(child.id))
                    SortHierarchical(child.children);
            }
        }

        void SortHierarchical(IList<TreeViewItem> children)
        {
            if (children == null)
                return;

            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0)
                return;

            List<AssetEntryTreeViewItem> kids = new List<AssetEntryTreeViewItem>();
            List<TreeViewItem> copy = new List<TreeViewItem>(children);
            children.Clear();
            foreach (var c in copy)
            {
                var child = c as AssetEntryTreeViewItem;
                if (child != null && child.entry != null)
                    kids.Add(child);
                else
                    children.Add(c);
            }

            ColumnId col = m_SortOptions[sortedColumns[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[0]);

            IEnumerable<AssetEntryTreeViewItem> orderedKids = kids;
            switch (col)
            {
                case ColumnId.Type:
                    break;
                case ColumnId.Path:
                    orderedKids = kids.Order(l => l.entry.AssetPath, ascending);
                    break;
                case ColumnId.Labels:
                    orderedKids = OrderByLabels(kids, ascending);
                    break;
                default:
                    orderedKids = kids.Order(l => l.displayName, ascending);
                    break;
            }

            foreach (var o in orderedKids)
                children.Add(o);


            foreach (var child in children)
            {
                if (child != null && IsExpanded(child.id))
                    SortHierarchical(child.children);
            }
        }

        IEnumerable<AssetEntryTreeViewItem> OrderByLabels(List<AssetEntryTreeViewItem> kids, bool ascending)
        {
            var emptyHalf = new List<AssetEntryTreeViewItem>();
            var namedHalf = new List<AssetEntryTreeViewItem>();
            foreach (var k in kids)
            {
                if (k.entry == null || k.entry.labels == null || k.entry.labels.Count < 1)
                    emptyHalf.Add(k);
                else
                    namedHalf.Add(k);
            }
            var orderedKids = namedHalf.Order(l => m_Editor.settings.labelTable.GetString(l.entry.labels, 200), ascending);

            List<AssetEntryTreeViewItem> result = new List<AssetEntryTreeViewItem>();
            if (ascending)
            {
                result.AddRange(emptyHalf);
                result.AddRange(orderedKids);
            }
            else
            {
                result.AddRange(orderedKids);
                result.AddRange(emptyHalf);
            }

            return result;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            var aeItem = item as AssetEntryTreeViewItem;
            if (aeItem == null)
                return false;

            //check if item matches.
            if (aeItem.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (aeItem.entry == null)
                return false;
            if (aeItem.entry.AssetPath.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            
            foreach (string label in aeItem.entry.labels)
            {
                if (label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        void AddGroupChildrenBuild(AddressableAssetGroup group, TreeViewItem root)
        {
            int depth = 0;

            AssetEntryTreeViewItem groupItem = null;
            if (ProjectConfigData.ShowGroupsAsHierarchy && group != null)
            {
                //// dash in name imitates hiearchy.
                TreeViewItem newRoot = root;
                var parts = group.Name.Split('-');
                string partialRestore = "";
                for (int index = 0; index < parts.Length - 1; index++)
                {
                    TreeViewItem folderItem = null;
                    partialRestore += parts[index];
                    int hash = partialRestore.GetHashCode();
                    if (!TryGetChild(newRoot, hash, ref folderItem))
                    {
                        folderItem = new AssetEntryTreeViewItem(parts[index], depth, hash);
                        newRoot.AddChild(folderItem);
                    }
                    depth++;
                    newRoot = folderItem;
                }

                groupItem = new AssetEntryTreeViewItem(group, depth);
                newRoot.AddChild(groupItem);
            }
            else
            {
                groupItem = new AssetEntryTreeViewItem(group, 0);
                root.AddChild(groupItem);
            }

            if (group != null && group.entries.Count > 0)
            {
                foreach (var entry in group.entries)
                {
                    AddAndRecurseEntriesBuild(entry, groupItem, depth + 1, IsExpanded(groupItem.id));
                }
            }
        }

        bool TryGetChild(TreeViewItem root, int childHash, ref TreeViewItem childItem)
        {
            if (root.children == null)
                return false;
            foreach (var child in root.children)
            {
                if (child.id == childHash)
                {
                    childItem = child;
                    return true;
                }
            }

            return false;
        }

        void AddAndRecurseEntriesBuild(AddressableAssetEntry entry, AssetEntryTreeViewItem parent, int depth, bool expanded)
        {
            var item = new AssetEntryTreeViewItem(entry, depth);
            parent.AddChild(item);
            if (!expanded)
            {
                item.checkedForChildren = false;
                return;
            }
            RecurseEntryChildren(entry, item, depth);
        }

        internal void RecurseEntryChildren(AddressableAssetEntry entry, AssetEntryTreeViewItem item, int depth)
        {
            item.checkedForChildren = true;
            var subAssets = new List<AddressableAssetEntry>();
            entry.GatherAllAssets(subAssets, false, entry.IsInResources, ProjectConfigData.ShowSubObjectsInGroupView);
            if (subAssets.Count > 0)
            {
                foreach (var e in subAssets)
                {
                    if (e.guid.Length > 0 && e.address.Contains("[") && e.address.Contains("]"))
                        Debug.LogErrorFormat("Subasset address '{0}' cannot contain '[ ]'.", e.address);
                    AddAndRecurseEntriesBuild(e, item, depth + 1, IsExpanded(item.id));
                }
            }
        }

        protected override void ExpandedStateChanged()
        {
            foreach (var id in state.expandedIDs)
            {
                var item = FindItem(id, rootItem);
                if (item != null && item.hasChildren)
                {
                    foreach (AssetEntryTreeViewItem c in item.children)
                        if (!c.checkedForChildren)
                            RecurseEntryChildren(c.entry, c, c.depth + 1);
                }
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            //TODO - this occasionally causes a "hot control" issue.
            if (m_ForceSelectionClear ||
                (Event.current.type == EventType.MouseDown &&
                 Event.current.button == 0 &&
                 rect.Contains(Event.current.mousePosition)))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
                if (m_ForceSelectionClear)
                    m_ForceSelectionClear = false;
            }
        }

        protected override void BeforeRowsGUI()
        {
            base.BeforeRowsGUI();

            if (Event.current.type == EventType.Repaint)
            {
                var rows = GetRows();
                if (rows.Count > 0)
                {
                    int first;
                    int last;
                    GetFirstAndLastVisibleRows(out first, out last);
                    for (int rowId = first; rowId <= last; rowId++)
                    {
                        var aeI = rows[rowId] as AssetEntryTreeViewItem;
                        if (aeI != null && aeI.entry != null)
                        {
                            DefaultStyles.backgroundEven.Draw(GetRowRect(rowId), false, false, false, false);
                        }
                    }
                }
            }
        }

        GUIStyle m_LabelStyle;
        protected override void RowGUI(RowGUIArgs args)
        {
            if (m_LabelStyle == null)
            {
                m_LabelStyle = new GUIStyle("PR Label");
                if (m_LabelStyle == null)
                    m_LabelStyle = UnityEngine.GUI.skin.GetStyle("Label");
            }

            var item = args.item as AssetEntryTreeViewItem;
            if (item == null || item.group == null && item.entry == null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    base.RowGUI(args);
                }
            }
            else if (item.group != null)
            {
                if (item.isRenaming && !args.isRenaming)
                    item.isRenaming = false;
                using (new EditorGUI.DisabledScope(item.group.ReadOnly))
                {
                    base.RowGUI(args);
                }
            }
            else if (item.entry != null && !args.isRenaming)
            {
                using (new EditorGUI.DisabledScope(item.entry.ReadOnly))
                {
                    for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                    {
                        CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                    }
                }
            }
        }

        void CellGUI(Rect cellRect, AssetEntryTreeViewItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch ((ColumnId)column)
            {
                case ColumnId.Id:
                {
                    // The rect is assumed indented and sized after the content when pinging
                    float indent = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
                    cellRect.xMin += indent;

                    if (Event.current.type == EventType.Repaint)
                        m_LabelStyle.Draw(cellRect, item.entry.address, false, false, args.selected, args.focused);
                }
                break;
                case ColumnId.Path:
                    if (Event.current.type == EventType.Repaint)
                    {
                        var path = item.entry.AssetPath;
                        if (string.IsNullOrEmpty(path))
                            path = item.entry.ReadOnly ? "" : "Missing File";
                        m_LabelStyle.Draw(cellRect, path, false, false, args.selected, args.focused);
                    }
                    break;
                case ColumnId.Type:
                    if (item.assetIcon != null)
                        UnityEngine.GUI.DrawTexture(cellRect, item.assetIcon, ScaleMode.ScaleToFit, true);
                    break;
                case ColumnId.Labels:
                    if (EditorGUI.DropdownButton(cellRect, new GUIContent(m_Editor.settings.labelTable.GetString(item.entry.labels, cellRect.width)), FocusType.Passive))
                    {
                        var selection = GetItemsForContext(args.item.id);
                        Dictionary<string, int> labelCounts = new Dictionary<string, int>();
                        List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
                        var newSelection = new List<int>();
                        foreach (var s in selection)
                        {
                            var aeItem = FindItem(s, rootItem) as AssetEntryTreeViewItem;
                            if (aeItem == null || aeItem.entry == null)
                                continue;

                            entries.Add(aeItem.entry);
                            newSelection.Add(s);
                            foreach (var label in aeItem.entry.labels)
                            {
                                int count;
                                labelCounts.TryGetValue(label, out count);
                                count++;
                                labelCounts[label] = count;
                            }
                        }
                        SetSelection(newSelection);
                        PopupWindow.Show(cellRect, new LabelMaskPopupContent(cellRect, m_Editor.settings, entries, labelCounts));
                    }
                    break;
            }
        }

        IList<int> GetItemsForContext(int row)
        {
            var selection = GetSelection();
            if (selection.Contains(row))
                return selection;

            selection = new List<int>();
            selection.Add(row);
            return selection;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new[]
            {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
                //new MultiColumnHeaderState.Column(),
            };

            int counter = 0;

            retVal[counter].headerContent = new GUIContent("Group Name \\ Addressable Name", "Address used to load asset at runtime");
            retVal[counter].minWidth = 100;
            retVal[counter].width = 260;
            retVal[counter].maxWidth = 10000;
            retVal[counter].headerTextAlignment = TextAlignment.Left;
            retVal[counter].canSort = true;
            retVal[counter].autoResize = true;
            counter++;

            retVal[counter].headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType"), "Asset type");
            retVal[counter].minWidth = 20;
            retVal[counter].width = 20;
            retVal[counter].maxWidth = 20;
            retVal[counter].headerTextAlignment = TextAlignment.Left;
            retVal[counter].canSort = false;
            retVal[counter].autoResize = true;
            counter++;

            retVal[counter].headerContent = new GUIContent("Path", "Current Path of asset");
            retVal[counter].minWidth = 100;
            retVal[counter].width = 150;
            retVal[counter].maxWidth = 10000;
            retVal[counter].headerTextAlignment = TextAlignment.Left;
            retVal[counter].canSort = true;
            retVal[counter].autoResize = true;
            counter++;

            retVal[counter].headerContent = new GUIContent("Labels", "Assets can have multiple labels");
            retVal[counter].minWidth = 20;
            retVal[counter].width = 160;
            retVal[counter].maxWidth = 1000;
            retVal[counter].headerTextAlignment = TextAlignment.Left;
            retVal[counter].canSort = true;
            retVal[counter].autoResize = true;

            return retVal;
        }

        protected string CheckForRename(TreeViewItem item, bool isActualRename)
        {
            string result = string.Empty;
            var assetItem = item as AssetEntryTreeViewItem;
            if (assetItem != null)
            {
                if (assetItem.group != null && !assetItem.group.ReadOnly)
                    result = "Rename";
                else if (assetItem.entry != null && !assetItem.entry.ReadOnly)
                    result = "Change Address";
                if (isActualRename)
                    assetItem.isRenaming = !string.IsNullOrEmpty(result);
            }
            return result;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return !string.IsNullOrEmpty(CheckForRename(item, true));
        }

        AssetEntryTreeViewItem FindItemInVisibleRows(int id)
        {
            var rows = GetRows();
            foreach (var r in rows)
            {
                if (r.id == id)
                {
                    return r as AssetEntryTreeViewItem;
                }
            }
            return null;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;
            
            var item = FindItemInVisibleRows(args.itemID);
            if (item != null)
            {
                item.isRenaming = false;
            }

            if (args.originalName == args.newName)
                return;

            if (item != null)
            {
                if (args.newName != null && args.newName.Contains("[") && args.newName.Contains("]"))
                {
                    args.acceptedRename = false;
                    Debug.LogErrorFormat("Rename of address '{0}' cannot contain '[ ]'.", args.originalName);
                }
                else if (item.entry != null)
                {
                    item.entry.address = args.newName;
                    AddressableAssetUtility.OpenAssetIfUsingVCIntegration(item.entry.parentGroup, true);
                }
                else if (item.group != null)
                {
                    if (m_Editor.settings.IsNotUniqueGroupName(args.newName))
                    {
                        args.acceptedRename = false;
                        Addressables.LogWarning("There is already a group named '" + args.newName + "'.  Cannot rename this group to match");
                    }
                    else
                    {
                        item.group.Name = args.newName;
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(item.group, true);
                        AddressableAssetUtility.OpenAssetIfUsingVCIntegration(item.group.Settings, true);
                    }
                }
                Reload();
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItemInVisibleRows(id);
            if (item != null)
            {
                Object o = null;
                if (item.entry != null)
                    o = AssetDatabase.LoadAssetAtPath<Object>(item.entry.AssetPath);
                else if (item.group != null)
                    o = item.group;

                if (o != null)
                {
                    EditorGUIUtility.PingObject(o);
                    Selection.activeObject = o;
                }
            }
        }

        bool m_ContextOnItem;
        protected override void ContextClicked()
        {
            if (m_ContextOnItem)
            {
                m_ContextOnItem = false;
                return;
            }

            GenericMenu menu = new GenericMenu();
            PopulateGeneralContextMenu(ref menu);
            menu.ShowAsContext();
        }

        void PopulateGeneralContextMenu(ref GenericMenu menu)
        {
            foreach (var templateObject in m_Editor.settings.GroupTemplateObjects)
            {
                Assert.IsNotNull(templateObject);
                menu.AddItem(new GUIContent("Create New Group/" + templateObject.name), false, CreateNewGroup, templateObject);
            }
        }

        void HandleCustomContextMenuItemGroups(object context)
        {
            var d = context as Tuple<string, List<AssetEntryTreeViewItem>>;
            AddressableAssetSettings.InvokeAssetGroupCommand(d.Item1, d.Item2.Select(s => s.group));
        }

        void HandleCustomContextMenuItemEntries(object context)
        {
            var d = context as Tuple<string, List<AssetEntryTreeViewItem>>;
            AddressableAssetSettings.InvokeAssetEntryCommand(d.Item1, d.Item2.Select(s => s.entry));
        }

        protected override void ContextClickedItem(int id)
        {
            List<AssetEntryTreeViewItem> selectedNodes = new List<AssetEntryTreeViewItem>();
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId); //TODO - this probably makes off-screen but selected items not get added to list.
                if (item != null)
                    selectedNodes.Add(item);
            }
            if (selectedNodes.Count == 0)
                return;

            m_ContextOnItem = true;

            bool isGroup = false;
            bool isEntry = false;
            bool hasReadOnly = false;
            int resourceCount = 0;
            bool isResourcesHeader = false;
            bool isMissingPath = false;
            foreach (var item in selectedNodes)
            {
                if (item.group != null)
                {
                    hasReadOnly |= item.group.ReadOnly;
                    isGroup = true;
                }
                else if (item.entry != null)
                {
                    if (item.entry.AssetPath == AddressableAssetEntry.ResourcesPath)
                    {
                        if (selectedNodes.Count > 1)
                            return;
                        isResourcesHeader = true;
                    }
                    else if (item.entry.AssetPath == AddressableAssetEntry.EditorSceneListPath)
                    {
                        return;
                    }
                    hasReadOnly |= item.entry.ReadOnly;
                    isEntry = true;
                    resourceCount += item.entry.IsInResources ? 1 : 0;
                    isMissingPath |= string.IsNullOrEmpty(item.entry.AssetPath);
                }
                else if (!string.IsNullOrEmpty(item.folderPath))
                {
                    hasReadOnly = true;
                }
            }
            if (isEntry && isGroup)
                return;

            GenericMenu menu = new GenericMenu();
            if (isResourcesHeader)
            {
                foreach (var g in m_Editor.settings.groups)
                {
                    if (!g.ReadOnly)
                        menu.AddItem(new GUIContent("Move ALL Resources to group/" + g.Name), false, MoveAllResourcesToGroup, g);
                }
            }
            else if (!hasReadOnly)
            {
                if (isGroup)
                {
                    var group = selectedNodes.First().group;
                    if (!group.IsDefaultGroup())
                        menu.AddItem(new GUIContent("Remove Group(s)"), false, RemoveGroup, selectedNodes);
                    menu.AddItem(new GUIContent("Simplify Addressable Names"), false, SimplifyAddresses, selectedNodes);
                    if (selectedNodes.Count == 1)
                    {
                        if (!group.IsDefaultGroup() && group.CanBeSetAsDefault())
                            menu.AddItem(new GUIContent("Set as Default"), false, SetGroupAsDefault, selectedNodes);
                        menu.AddItem(new GUIContent("Inspect Group Settings"), false, GoToGroupAsset, selectedNodes);
                    }
                    foreach (var i in AddressableAssetSettings.CustomAssetGroupCommands)
                        menu.AddItem(new GUIContent(i), false, HandleCustomContextMenuItemGroups, new Tuple<string, List<AssetEntryTreeViewItem>>(i, selectedNodes));
                }
                else if (isEntry)
                {
                    foreach (var g in m_Editor.settings.groups)
                    {
                        if (g != null && !g.ReadOnly)
                            menu.AddItem(new GUIContent("Move Addressables to Group/" + g.Name), false, MoveEntriesToGroup, g);
                    }

                    var groups = new HashSet<AddressableAssetGroup>();
                    foreach (var n in selectedNodes)
                        groups.Add(n.entry.parentGroup);
                    foreach (var g in groups)
                        menu.AddItem(new GUIContent("Move Addressables to New Group/With settings from: " + g.Name), false, MoveEntriesToNewGroup, new KeyValuePair<string, AddressableAssetGroup>(AddressableAssetSettings.kNewGroupName, g));


                    menu.AddItem(new GUIContent("Remove Addressables"), false, RemoveEntry, selectedNodes);
                    menu.AddItem(new GUIContent("Simplify Addressable Names"), false, SimplifyAddresses, selectedNodes);

                    if (selectedNodes.Count == 1)
                        menu.AddItem(new GUIContent("Copy Address to Clipboard"), false, CopyAddressesToClipboard, selectedNodes);

                    else if (selectedNodes.Count > 1)
                        menu.AddItem(new GUIContent("Copy " + selectedNodes.Count + " Addresses to Clipboard"), false, CopyAddressesToClipboard, selectedNodes);

                    foreach (var i in AddressableAssetSettings.CustomAssetEntryCommands)
                        menu.AddItem(new GUIContent(i), false, HandleCustomContextMenuItemEntries, new Tuple<string, List<AssetEntryTreeViewItem>>(i, selectedNodes));
                }
                else
                    menu.AddItem(new GUIContent("Clear missing references."), false, RemoveMissingReferences);
            }
            else
            {
                if (isEntry && !isMissingPath)
                {
                    if (resourceCount == selectedNodes.Count)
                    {
                        foreach (var g in m_Editor.settings.groups)
                        {
                            if (!g.ReadOnly)
                                menu.AddItem(new GUIContent("Move Resources to group/" + g.Name), false, MoveResourcesToGroup, g);
                        }
                    }
                    else if (resourceCount == 0)
                    {
                        foreach (var g in m_Editor.settings.groups)
                        {
                            if (!g.ReadOnly)
                                menu.AddItem(new GUIContent("Move Addressables to group/" + g.Name), false, MoveEntriesToGroup, g);
                        }
                    }

                    if (selectedNodes.Count == 1)
                        menu.AddItem(new GUIContent("Copy Address to Clipboard"), false, CopyAddressesToClipboard, selectedNodes);

                    else if (selectedNodes.Count > 1)
                        menu.AddItem(new GUIContent("Copy " + selectedNodes.Count + " Addresses to Clipboard"), false, CopyAddressesToClipboard, selectedNodes);
                }
            }

            if (selectedNodes.Count == 1)
            {
                var label = CheckForRename(selectedNodes.First(), false);
                if (!string.IsNullOrEmpty(label))
                    menu.AddItem(new GUIContent(label), false, RenameItem, selectedNodes);
            }

            PopulateGeneralContextMenu(ref menu);

            menu.ShowAsContext();
        }

        void GoToGroupAsset(object context)
        {
            List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
            if (selectedNodes == null || selectedNodes.Count == 0)
                return;
            var group = selectedNodes.First().group;
            if (group == null)
                return;
            EditorGUIUtility.PingObject(group);
            Selection.activeObject = group;
        }
        
        internal static void CopyAddressesToClipboard(object context)
        {
            List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
            string buffer = "";
            
            foreach (AssetEntryTreeViewItem item in selectedNodes)
                buffer += item.entry.address + ",";
            
            buffer = buffer.TrimEnd(',');
            GUIUtility.systemCopyBuffer = buffer;
        }
        
        void MoveAllResourcesToGroup(object context)
        {
            var targetGroup = context as AddressableAssetGroup;
            var firstId = GetSelection().First();
            var item = FindItemInVisibleRows(firstId);
            if (item != null && item.children != null)
            {
                SafeMoveResourcesToGroup(targetGroup, item.children.ConvertAll(instance => (AssetEntryTreeViewItem)instance));
            }
            else
                Debug.LogWarning("No Resources found to move");
        }

        void MoveResourcesToGroup(object context)
        {
            var targetGroup = context as AddressableAssetGroup;
            var itemList = new List<AssetEntryTreeViewItem>();
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId);
                if (item != null)
                    itemList.Add(item);
            }

            SafeMoveResourcesToGroup(targetGroup, itemList);
        }

        bool SafeMoveResourcesToGroup(AddressableAssetGroup targetGroup, List<AssetEntryTreeViewItem> itemList)
        {
            var guids = new List<string>();
            var paths = new List<string>();
            foreach (AssetEntryTreeViewItem child in itemList)
            {
                if (child != null)
                {
                    guids.Add(child.entry.guid);
                    paths.Add(child.entry.AssetPath);
                }
            }
            return AddressableAssetUtility.SafeMoveResourcesToGroup(m_Editor.settings, targetGroup, paths, guids);
        }

        void MoveEntriesToNewGroup(object context)
        {
            var k = (KeyValuePair<string, AddressableAssetGroup>)context;
            var g = m_Editor.settings.CreateGroup(k.Key, false, false, true, k.Value.Schemas);
            MoveEntriesToGroup(g);
        }

        void MoveEntriesToGroup(object context)
        {
            var targetGroup = context as AddressableAssetGroup;
            var entries = new List<AddressableAssetEntry>();
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId);
                if (item != null)
                    entries.Add(item.entry);
            }
            if (entries.Count > 0)
                m_Editor.settings.MoveEntries(entries, targetGroup);
        }

        internal void CreateNewGroup(object context)
        {
            var groupTemplate = context as AddressableAssetGroupTemplate;
            if (groupTemplate != null)
            {
                AddressableAssetGroup newGroup = m_Editor.settings.CreateGroup(groupTemplate.Name, false, false, true, null, groupTemplate.GetTypes());
                groupTemplate.ApplyToAddressableAssetGroup(newGroup);
            }
            else
            {
                m_Editor.settings.CreateGroup("", false, false, false, null);
                Reload();
            }
        }

        internal void SetGroupAsDefault(object context)
        {
            List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
            if (selectedNodes == null || selectedNodes.Count == 0)
                return;
            var group = selectedNodes.First().group;
            if (group == null)
                return;
            m_Editor.settings.DefaultGroup = group;
            Reload();
        }

        protected void RemoveMissingReferences()
        {
            RemoveMissingReferencesImpl();
        }

        internal void RemoveMissingReferencesImpl()
        {
            if (m_Editor.settings.RemoveMissingGroupReferences())
                m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, null, true, true);
        }

        protected void RemoveGroup(object context)
        {
            RemoveGroupImpl(context);
        }
        internal void RemoveGroupImpl(object context, bool forceRemoval = false)
        {
            if (forceRemoval || EditorUtility.DisplayDialog("Delete selected groups?", "Are you sure you want to delete the selected groups?\n\nYou cannot undo this action.", "Yes", "No"))
            {
                List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
                if (selectedNodes == null || selectedNodes.Count < 1)
                    return;
                var groups = new List<AddressableAssetGroup>();
                foreach (var item in selectedNodes)
                {
                    m_Editor.settings.RemoveGroupInternal(item == null ? null : item.group, true, false);
                    groups.Add(item.group);
                }
                m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupRemoved, groups, true, true);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Editor.settings);
            }
        }

        protected void SimplifyAddresses(object context)
        {
            SimplifyAddressesImpl(context);
        }

        internal void SimplifyAddressesImpl(object context)
        {
            List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
            if (selectedNodes == null || selectedNodes.Count < 1)
                return;
            var entries = new List<AddressableAssetEntry>();
            HashSet<AddressableAssetGroup> modifiedGroups = new HashSet<AddressableAssetGroup>();
            foreach (var item in selectedNodes)
            {
                if (item.IsGroup)
                {
                    foreach (var e in item.group.entries)
                    {
                        e.SetAddress(Path.GetFileNameWithoutExtension(e.address), false);
                        entries.Add(e);
                    }
                    modifiedGroups.Add(item.group);
                }
                else
                {
                    item.entry.SetAddress(Path.GetFileNameWithoutExtension(item.entry.address), false);
                    entries.Add(item.entry);
                    modifiedGroups.Add(item.entry.parentGroup);
                }
            }
            foreach (var g in modifiedGroups)
            {
                g.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, false, true);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(g);
            }
            m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, true, false);
        }

        protected void RemoveEntry(object context)
        {
            RemoveEntryImpl(context);
        }

        internal void RemoveEntryImpl(object context, bool forceRemoval = false)
        {
            if (forceRemoval || EditorUtility.DisplayDialog("Delete selected entries?", "Are you sure you want to delete the selected entries?\n\nYou cannot undo this action.", "Yes", "No"))
            {
                List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
                if (selectedNodes == null || selectedNodes.Count < 1)
                    return;
                var entries = new List<AddressableAssetEntry>();
                HashSet<AddressableAssetGroup> modifiedGroups = new HashSet<AddressableAssetGroup>();
                foreach (var item in selectedNodes)
                {
                    if (item.entry != null)
                    {
                        entries.Add(item.entry);
                        modifiedGroups.Add(item.entry.parentGroup);
                        m_Editor.settings.RemoveAssetEntry(item.entry.guid, false);
                    }
                }
                foreach (var g in modifiedGroups)
                {
                    g.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, false, true);
                    AddressableAssetUtility.OpenAssetIfUsingVCIntegration(g);
                }
                m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entries, true, false);
            }
        }

        protected void RenameItem(object context)
        {
            RenameItemImpl(context);
        }

        internal void RenameItemImpl(object context)
        {
            List<AssetEntryTreeViewItem> selectedNodes = context as List<AssetEntryTreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count >= 1)
            {
                var item = selectedNodes.First();
                if (CanRename(item))
                    BeginRename(item);
            }
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            var aeItem = item as AssetEntryTreeViewItem;
            if (aeItem != null && aeItem.group != null)
                return true;

            return false;
        }

        protected override void KeyEvent()
        {
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Delete && GetSelection().Count > 0)
            {
                List<AssetEntryTreeViewItem> selectedNodes = new List<AssetEntryTreeViewItem>();
                bool allGroups = true;
                bool allEntries = true;
                foreach (var nodeId in GetSelection())
                {
                    var item = FindItemInVisibleRows(nodeId);
                    if (item != null)
                    {
                        selectedNodes.Add(item);
                        if (item.entry == null)
                            allEntries = false;
                        else
                            allGroups = false;
                    }
                }
                if (allEntries)
                    RemoveEntry(selectedNodes);
                if (allGroups)
                    RemoveGroup(selectedNodes);
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            int resourcesCount = 0;
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItemInVisibleRows(id);
                if (item != null)
                {
                    if (item.entry != null)
                    {
                        //can't drag the root "EditorSceneList" entry
                        if (item.entry.guid == AddressableAssetEntry.EditorSceneListName)
                            return false;

                        //can't drag the root "Resources" entry
                        if (item.entry.guid == AddressableAssetEntry.ResourcesName)
                            return false;

                        //if we're dragging resources, we should _only_ drag resources.
                        if (item.entry.IsInResources)
                            resourcesCount++;

                        //if it's missing a path, it can't be moved.  most likely this is a sub-asset.
                        if (string.IsNullOrEmpty(item.entry.AssetPath))
                            return false;
                    }
                }
            }
            if ((resourcesCount > 0) && (resourcesCount < args.draggedItemIDs.Count))
                return false;

            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            var selectedNodes = new List<AssetEntryTreeViewItem>();
            foreach (var id in args.draggedItemIDs)
            {
                var item = FindItemInVisibleRows(id);
                if (item.entry != null || (item.parent == rootItem && item.@group != null))
                    selectedNodes.Add(item);
            }
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = new Object[] {};
            DragAndDrop.SetGenericData("AssetEntryTreeViewItem", selectedNodes);
            DragAndDrop.visualMode = selectedNodes.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            DragAndDrop.StartDrag("AssetBundleTree");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

            var target = args.parentItem as AssetEntryTreeViewItem;

            if (target != null && target.entry != null && target.entry.ReadOnly)
                return DragAndDropVisualMode.Rejected;

            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
            {
                visualMode = HandleDragAndDropPaths(target, args);
            }
            else
            {
                visualMode = HandleDragAndDropItems(target, args);
            }

            return visualMode;
        }

        DragAndDropVisualMode HandleDragAndDropItems(AssetEntryTreeViewItem target, DragAndDropArgs args)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

            var draggedNodes = DragAndDrop.GetGenericData("AssetEntryTreeViewItem") as List<AssetEntryTreeViewItem>;
            if (draggedNodes != null && draggedNodes.Count > 0)
            {
                visualMode = DragAndDropVisualMode.Copy;
                bool isDraggingGroup = draggedNodes.First().parent == rootItem;
                bool dropParentIsRoot = args.parentItem == rootItem || args.parentItem == null;
                bool parentGroupIsReadOnly = target?.@group != null && target.@group.ReadOnly;

                if (isDraggingGroup && !dropParentIsRoot || !isDraggingGroup && dropParentIsRoot || parentGroupIsReadOnly)
                    visualMode = DragAndDropVisualMode.Rejected;

                if (args.performDrop)
                {
                    if (args.parentItem == null || args.parentItem == rootItem && visualMode != DragAndDropVisualMode.Rejected)
                    {
                        // Need to insert groups in reverse order because all groups will be inserted at the same index
                        for (int i = draggedNodes.Count - 1; i >= 0; i--)
                        {
                            AssetEntryTreeViewItem node = draggedNodes[i];
                            AddressableAssetGroup group = node.@group;
                            int index = m_Editor.settings.groups.FindIndex(g => g == group);
                            if (index < args.insertAtIndex)
                                args.insertAtIndex--;

                            m_Editor.settings.groups.RemoveAt(index);

                            if (args.insertAtIndex < 0 || args.insertAtIndex > m_Editor.settings.groups.Count)
                                m_Editor.settings.groups.Insert(m_Editor.settings.groups.Count, group);
                            else
                                m_Editor.settings.groups.Insert(args.insertAtIndex, group);
                        }

                        m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupMoved, m_Editor.settings.groups, true, true);
                        Reload();
                    }
                    else
                    {
                        AddressableAssetGroup parent = null;
                        if (target.group != null)
                            parent = target.group;
                        else if (target.entry != null)
                            parent = target.entry.parentGroup;

                        if (parent != null)
                        {
                            if (draggedNodes.First().entry.IsInResources)
                            {
                                SafeMoveResourcesToGroup(parent, draggedNodes);
                            }
                            else
                            {
                                var entries = new List<AddressableAssetEntry>();
                                HashSet<AddressableAssetGroup> modifiedGroups = new HashSet<AddressableAssetGroup>();
                                modifiedGroups.Add(parent);
                                foreach (var node in draggedNodes)
                                {
                                    modifiedGroups.Add(node.entry.parentGroup);
                                    m_Editor.settings.MoveEntry(node.entry, parent, false, false);
                                    entries.Add(node.entry);
                                }
                                foreach (AddressableAssetGroup modifiedGroup in modifiedGroups)
                                    AddressableAssetUtility.OpenAssetIfUsingVCIntegration(modifiedGroup);
                                m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entries, true, false);
                            }
                        }
                    }
                }
            }

            return visualMode;
        }

        DragAndDropVisualMode HandleDragAndDropPaths(AssetEntryTreeViewItem target, DragAndDropArgs args)
        {
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.None;

            bool containsGroup = false;
            foreach (var path in DragAndDrop.paths)
            {
                if (PathPointsToAssetGroup(path))
                {
                    containsGroup = true;
                    break;
                }
            }

            bool parentGroupIsReadOnly = target?.@group != null && target.@group.ReadOnly;
            if (target == null && !containsGroup || parentGroupIsReadOnly)
                return DragAndDropVisualMode.Rejected;

            foreach (String path in DragAndDrop.paths)
            {
                if (!AddressableAssetUtility.IsPathValidForEntry(path) && (!PathPointsToAssetGroup(path) && target != rootItem))
                    return DragAndDropVisualMode.Rejected;
            }
            visualMode = DragAndDropVisualMode.Copy;

            if (args.performDrop && visualMode != DragAndDropVisualMode.Rejected)
            {
                if (!containsGroup)
                {
                    AddressableAssetGroup parent = null;
                    bool targetIsGroup = false;
                    if (target.group != null)
                    {
                        parent = target.group;
                        targetIsGroup = true;
                    }
                    else if (target.entry != null)
                        parent = target.entry.parentGroup;

                    if (parent != null)
                    {
                        var resourcePaths = new List<string>();
                        var nonResourceGuids = new List<string>();
                        foreach (var p in DragAndDrop.paths)
                        {
                            if (AddressableAssetUtility.IsInResources(p))
                                resourcePaths.Add(p);
                            else
                                nonResourceGuids.Add(AssetDatabase.AssetPathToGUID(p));
                        }

                        bool canMarkNonResources = true;
                        if (resourcePaths.Count > 0)
                            canMarkNonResources = AddressableAssetUtility.SafeMoveResourcesToGroup(m_Editor.settings, parent, resourcePaths, null);

                        if (canMarkNonResources)
                        {
                            if (nonResourceGuids.Count > 0)
                            {
                                var entriesMoved = new List<AddressableAssetEntry>();
                                var entriesCreated = new List<AddressableAssetEntry>();
                                m_Editor.settings.CreateOrMoveEntries(nonResourceGuids, parent, entriesCreated, entriesMoved, false, false);

                                if (entriesMoved.Count > 0)
                                    m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesMoved, true);
                                if (entriesCreated.Count > 0)
                                    m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesCreated, true);

                                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(parent);
                            }

                            if (targetIsGroup)
                            {
                                SetExpanded(target.id, true);
                            }
                        }
                    }
                }
                else
                {
                    bool modified = false;
                    foreach (var p in DragAndDrop.paths)
                    {
                        if (PathPointsToAssetGroup(p))
                        {
                            AddressableAssetGroup loadedGroup = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(p);
                            if (loadedGroup != null)
                            {
                                if (m_Editor.settings.FindGroup(g => g.Guid == loadedGroup.Guid) == null)
                                {
                                    m_Editor.settings.groups.Add(loadedGroup);
                                    modified = true;
                                }
                            }
                        }
                    }

                    if (modified)
                        m_Editor.settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupAdded,
                            m_Editor.settings, true, true);
                }
            }
            return visualMode;
        }

        private bool PathPointsToAssetGroup(string path)
        {
            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AddressableAssetGroup);
        }
    }

    class AssetEntryTreeViewItem : TreeViewItem
    {
        public AddressableAssetEntry entry;
        public AddressableAssetGroup group;
        public string folderPath;
        public Texture2D assetIcon;
        public bool isRenaming;
        public bool checkedForChildren = true;

        public AssetEntryTreeViewItem(AddressableAssetEntry e, int d) : base(e == null ? 0 : (e.address + e.guid).GetHashCode(), d, e == null ? "[Missing Reference]" : e.address)
        {
            entry = e;
            group = null;
            folderPath = string.Empty;
            assetIcon = entry == null ? null : AssetDatabase.GetCachedIcon(e.AssetPath) as Texture2D;
            isRenaming = false;
        }

        public AssetEntryTreeViewItem(AddressableAssetGroup g, int d) : base(g == null ? 0 : g.Guid.GetHashCode(), d, g == null ? "[Missing Reference]" : g.Name)
        {
            entry = null;
            group = g;
            folderPath = string.Empty;
            assetIcon = null;
            isRenaming = false;
        }

        public AssetEntryTreeViewItem(string folder, int d, int id) : base(id, d, string.IsNullOrEmpty(folder) ? "missing" : folder)
        {
            entry = null;
            group = null;
            folderPath = folder;
            assetIcon = null;
            isRenaming = false;
        }

        public bool IsGroup => group != null && entry == null;

        public override string displayName
        {
            get
            {
                if (!isRenaming && group != null && group.Default)
                    return base.displayName + " (Default)";
                return base.displayName;
            }

            set
            {
                base.displayName = value;
            }
        }
    }

    static class MyExtensionMethods
    {
        // Find digits in a string
        static Regex s_Regex = new Regex(@"\d+", RegexOptions.Compiled);

        public static IEnumerable<T> Order<T>(this IEnumerable<T> items, Func<T, string> selector, bool ascending)
        {
            if (EditorPrefs.HasKey("AllowAlphaNumericHierarchy") && EditorPrefs.GetBool("AllowAlphaNumericHierarchy"))
            {
                // Find the length of the longest number in the string
                int maxDigits = items
                    .SelectMany(i => s_Regex.Matches(selector(i)).Cast<Match>().Select(digitChunk => (int?)digitChunk.Value.Length))
                    .Max() ?? 0;

                // in the evaluator, pad numbers with zeros so they all have the same length
                var tempSelector = selector;
                selector = i => s_Regex.Replace(tempSelector(i), match => match.Value.PadLeft(maxDigits, '0'));
            }

            return ascending ? items.OrderBy(selector) : items.OrderByDescending(selector);
        }
    }
}
