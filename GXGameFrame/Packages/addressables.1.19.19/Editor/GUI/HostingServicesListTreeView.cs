using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor.AddressableAssets.HostingServices;

namespace UnityEditor.AddressableAssets.GUI
{
    class HostingServicesListTreeView : TreeView
    {
        List<string> m_Names;
        public List<string> Names => m_Names;

        HostingServicesWindow m_Window;

        HostingServicesManager m_Manager;
        static Texture2D k_checkMark;

        public static MultiColumnHeader CreateHeader()
        {
            k_checkMark = EditorGUIUtility.isProSkin ? EditorGUIUtility.FindTexture("d_FilterSelectedOnly") : EditorGUIUtility.FindTexture("FilterSelectedOnly");

            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(""),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    autoResize = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(""),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 10000,
                    minWidth = 60,
                    autoResize = true
                }
            };

            var header = new MultiColumnHeader(new MultiColumnHeaderState(columns))
            {
                height = 0
            };

            return header;
        }

        public HostingServicesListTreeView(TreeViewState treeViewState, HostingServicesManager manager, HostingServicesWindow window, MultiColumnHeader header)
            : base(treeViewState, header)
        {
            m_Manager = manager;
            m_Window = window;
            m_Names = new List<string>();
            if (m_Window.ServiceIndex == -1)
            {
                m_Window.ServiceIndex = 0;
            }
            Reload();

            if (m_Window.ServiceIndex >= 0)
                SetSelection(new List<int> { m_Window.ServiceIndex });
        }

        /// <summary>
        /// Selects a row based on its index if the given index is valid.
        /// </summary>
        /// <param name="index">The index of the row.</param>
        public void SelectRow(int index)
        {
            IList<TreeViewItem> rows = GetRows();
            if (index >= 0 && index < rows.Count)
            {
                SelectionClick(rows[index], false);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            m_Names.Clear();
            foreach (var service in m_Manager.HostingServices)
            {
                m_Names.Add(service.DescriptiveName);
            }

            for (int i = 0; i < m_Names.Count; i++)
            {
                root.AddChild(new TreeViewItem { id = i, displayName = m_Names[i] });
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(ref args, i);
            }
        }

        void CellGUI(ref RowGUIArgs args, int i)
        {
            var cellRect = args.GetCellRect(i);
            CenterRectUsingSingleLineHeight(ref cellRect);
            var item = args.item;
            if (item == null) return;

            switch (args.GetColumn(i))
            {
                case 0:
                    if (GetService(item.displayName).IsHostingServiceRunning)
                        UnityEngine.GUI.DrawTexture(cellRect, k_checkMark, ScaleMode.ScaleToFit);
                    break;
                case 1:
                    EditorGUI.LabelField(cellRect, item.displayName);
                    break;
            }
        }

        TreeViewItem FindItemInVisibleRows(int id)
        {
            var rows = GetRows();
            foreach (var r in rows)
            {
                if (r.id == id)
                {
                    return r as TreeViewItem;
                }
            }
            return null;
        }

        int GetServiceIndex(string name)
        {
            int i = 0;
            foreach (var service in m_Manager.HostingServices)
            {
                if (name == service.DescriptiveName)
                    return i;
                ++i;
            }
            return -1;
        }

        IHostingService GetService(string name)
        {
            foreach (var service in m_Manager.HostingServices)
            {
                if (name == service.DescriptiveName)
                    return service;
            }
            return default(IHostingService);
        }

        List<TreeViewItem> GetSelectedNodes()
        {
            List<TreeViewItem> selectedNodes = new List<TreeViewItem>();
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId); //TODO - this probably makes off-screen but selected items not get added to list.
                if (item != null)
                {
                    selectedNodes.Add(item);
                }
            }
            return selectedNodes;
        }

        protected override void SingleClickedItem(int id)
        {
            List<TreeViewItem> selectedNodes = GetSelectedNodes();
            m_Window.ServiceIndex = GetServiceIndex(selectedNodes[0].displayName);
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);

            List<TreeViewItem> selectedNodes = GetSelectedNodes();
            GenericMenu menu = new GenericMenu();

            if (selectedNodes.Count > 1)
            {
                menu.AddItem(new GUIContent("Remove Services"), false, () =>
                {
                    for (int i = 0; i < selectedNodes.Count; i++)
                    {
                        // Show dialog for first entry only
                        m_Window.RemoveService(GetService(selectedNodes[i].displayName), i == 0);
                    }
                });
            }
            else
            {
                IHostingService service = GetService(selectedNodes[0].displayName);

                if (service.IsHostingServiceRunning)
                    menu.AddItem(new GUIContent("Disable Service"), false, service.StopHostingService);
                else
                    menu.AddItem(new GUIContent("Enable Service"), false, service.StartHostingService);

                menu.AddItem(new GUIContent("Remove Service"), false, () => m_Window.RemoveService(service));
            }


            menu.ShowAsContext();
        }
    }
}
