using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.GUI
{
    class ContentUpdatePreviewWindow : EditorWindow
    {
        internal static bool PrepareForContentUpdate(AddressableAssetSettings settings, string buildPath)
        {
            var modifiedEntries = ContentUpdateScript.GatherModifiedEntriesWithDependencies(settings, buildPath);
            var previewWindow = GetWindow<ContentUpdatePreviewWindow>();
            previewWindow.Show(settings, modifiedEntries);
            return true;
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Content Update Preview");
        }

        class ContentUpdateTreeView : TreeView
        {
            class Item : TreeViewItem
            {
                internal AddressableAssetEntry entry;
                internal bool enabled;
                public Item(AddressableAssetEntry entry, int itemDepth = 1) : base(entry.guid.GetHashCode(), itemDepth)
                {
                    this.entry = entry;
                    enabled = true;
                }
            }

            ContentUpdatePreviewWindow m_Preview;
            public ContentUpdateTreeView(ContentUpdatePreviewWindow preview, TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
            {
                m_Preview = preview;
            }

            internal List<AddressableAssetEntry> GetEnabledEntries()
            {
                var result = new HashSet<AddressableAssetEntry>();
                foreach (var i in GetRows())
                {
                    var item = i as Item;
                    if (item != null && item.enabled)
                    {
                        result.Add(item.entry);
                        if (item.hasChildren)
                        {
                            foreach (var child in i.children)
                            {
                                var childItem = child as Item;
                                if (childItem != null && !result.Contains(childItem.entry))
                                    result.Add(childItem.entry);
                            }
                        }
                    }
                }
                return result.ToList();
            }

            protected override TreeViewItem BuildRoot()
            {
                columnIndexForTreeFoldouts = 1;

                var root = new TreeViewItem(-1, -1);
                root.children = new List<TreeViewItem>();
                foreach (var k in m_Preview.m_DepEntriesMap.Keys)
                {
                    var mainItem = new Item(k, 0);
                    root.AddChild(mainItem);

                    foreach (var dep in m_Preview.m_DepEntriesMap[k])
                        mainItem.AddChild(new Item(dep, mainItem.depth + 1));
                }

                return root;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                var item = args.item as Item;
                if (item == null)
                {
                    base.RowGUI(args);
                    return;
                }
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i));
                }
            }

            private const int kToggleOffset = 5;
            private const int kMainAssetXOffset = 20;
            private const int kDependencyAssetXOffset = 40;
            void CellGUI(Rect cellRect, Item item, int column)
            {
                if (column == 0)
                    cellRect.xMin = (cellRect.xMax / 2) - kToggleOffset;
                else //Only want this indent on every column that isn't 0
                {
                    if ((item.parent as Item) != null)
                        cellRect.xMin += kDependencyAssetXOffset;
                    else
                        cellRect.xMin += kMainAssetXOffset;
                }

                if (column == 0)
                {
                    if (item.entry != null)
                    {
                        if ((item.parent as Item) != null)
                            item.enabled = (item.parent as Item).enabled;
                        else
                            item.enabled = EditorGUI.Toggle(cellRect, item.enabled);
                    }
                }
                else if (column == 1)
                {
                    EditorGUI.LabelField(cellRect, item.entry.address);
                }
                else if (column == 2)
                {
                    EditorGUI.LabelField(cellRect, item.entry.AssetPath);
                }
                else if (column == 3)
                {
                    EditorGUI.LabelField(cellRect, item.entry.parentGroup.Name);
                }
            }

            internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
            {
                var retVal = new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Include", "Include change in Update"),
                        minWidth = 50,
                        width = 50,
                        maxWidth = 50,
                        headerTextAlignment = TextAlignment.Left,
                        canSort = true,
                        autoResize = true
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Address", "Data Value"),
                        minWidth = 300,
                        width = 300,
                        maxWidth = 1000,
                        headerTextAlignment = TextAlignment.Left,
                        canSort = true,
                        autoResize = true
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Path", "Asset Path"),
                        minWidth = 300,
                        width = 300,
                        maxWidth = 1000,
                        headerTextAlignment = TextAlignment.Left,
                        canSort = true,
                        autoResize = true
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent = new GUIContent("Modified Group", "The modified Addressable group"),
                        minWidth = 300,
                        width = 300,
                        maxWidth = 1000,
                        headerTextAlignment = TextAlignment.Left,
                        canSort = true,
                        autoResize = true
                    }
                };

                return new MultiColumnHeaderState(retVal);
            }
        }

        AddressableAssetSettings m_Settings;
        Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>> m_DepEntriesMap;
        Vector2 m_ScrollPosition;
        ContentUpdateTreeView m_Tree;
        [FormerlySerializedAs("treeState")]
        [SerializeField]
        TreeViewState m_TreeState;
        [FormerlySerializedAs("mchs")]
        [SerializeField]
        MultiColumnHeaderState m_Mchs;

        public void Show(AddressableAssetSettings settings, Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>> entryDependencies)
        {
            m_Settings = settings;
            m_DepEntriesMap = entryDependencies;
            Show();
        }

        public void OnGUI()
        {
            if (m_DepEntriesMap == null)
                return;
            Rect contentRect = new Rect(0, 0, position.width, position.height - 50);

            if (m_Tree == null)
            {
                if (m_TreeState == null)
                    m_TreeState = new TreeViewState();

                var headerState = ContentUpdateTreeView.CreateDefaultMultiColumnHeaderState();
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_Mchs, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_Mchs, headerState);
                m_Mchs = headerState;

                m_Tree = new ContentUpdateTreeView(this, m_TreeState, m_Mchs);
                m_Tree.Reload();
            }

            if (m_DepEntriesMap.Count == 0)
            {
                GUILayout.BeginArea(contentRect);
                GUILayout.BeginVertical();

                GUILayout.Label("No Addressable groups with a BundledAssetGroupSchema and ContentUpdateGroupSchema (with StaticContent enabled) appear to have been modified.");

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            else
                m_Tree.OnGUI(contentRect);

            GUILayout.BeginArea(new Rect(0, position.height - 50, position.width, 50));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                Close();
            using (new EditorGUI.DisabledScope(m_Tree.GetEnabledEntries().Count == 0))
            {
                if (GUILayout.Button("Apply Changes"))
                {
                    ContentUpdateScript.CreateContentUpdateGroup(m_Settings, m_Tree.GetEnabledEntries(), "Content Update");
                    Close();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
