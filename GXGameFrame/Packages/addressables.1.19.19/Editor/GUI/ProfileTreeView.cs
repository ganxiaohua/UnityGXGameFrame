using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    internal class ProfileTreeView : TreeView
    {
        private List<string> m_Names;
        private Dictionary<int, AddressableAssetProfileSettings.BuildProfile> m_TreeIndexToBuildProfileMap;
        public List<string> Names => m_Names;
        private int m_LastClickedProfile;
        public int lastClickedProfile
        {
            get { return m_LastClickedProfile; }
            set { m_LastClickedProfile = value; }
        }

        private ProfileWindow m_Window;

        private List<AddressableAssetProfileSettings.BuildProfile> m_ProfileList;

        static Texture2D k_CheckMark;

        public static MultiColumnHeader CreateHeader()
        {
            k_CheckMark = EditorGUIUtility.isProSkin
                ? EditorGUIUtility.FindTexture("d_FilterSelectedOnly")
                : EditorGUIUtility.FindTexture("FilterSelectedOnly");

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

        internal ProfileTreeView(TreeViewState treeViewState, List<AddressableAssetProfileSettings.BuildProfile> profiles, ProfileWindow window,
                                 MultiColumnHeader header) : base(treeViewState, header)
        {
            m_Window = window;
            m_ProfileList = profiles;
            m_Names = new List<string>();
            m_TreeIndexToBuildProfileMap = new Dictionary<int, AddressableAssetProfileSettings.BuildProfile>();
            if (m_Window.ProfileIndex == -1)
            {
                m_Window.ProfileIndex = 0;
            }

            Reload();

            if (m_Window.ProfileIndex >= 0)
            {
                SetSelection(new List<int> { m_Window.ProfileIndex });
                m_LastClickedProfile = m_Window.ProfileIndex;
            }

        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };
            m_Names.Clear();
            m_TreeIndexToBuildProfileMap.Clear();

            for (int i = 0; i < m_ProfileList.Count; i++)
            {
                var profile = m_ProfileList[i];
                m_Names.Add(profile.profileName);
                m_TreeIndexToBuildProfileMap.Add(i, profile);
                root.AddChild(new TreeViewItem { id = i, displayName = profile.profileName });
            }

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            //Don't draw the background if the current item is being renamed
            if (args.isRenaming) return;

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
                    //Display checkmark next to the active profile
                    if (GetProfile(item.id).id.Equals(m_Window.settings.activeProfileId))
                        UnityEngine.GUI.DrawTexture(cellRect, k_CheckMark, ScaleMode.ScaleToFit);
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
                if (r.id == id) return r as TreeViewItem;
            }
            return null;
        }

        AddressableAssetProfileSettings.BuildProfile GetProfile(int id)
        {
            return m_TreeIndexToBuildProfileMap.ContainsKey(id) ? m_TreeIndexToBuildProfileMap[id] : default(AddressableAssetProfileSettings.BuildProfile);
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
            m_LastClickedProfile = m_Window.ProfileIndex;
            m_Window.ProfileIndex = selectedNodes[0].id;
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);

            List<TreeViewItem> selectedNodes = GetSelectedNodes();
            GenericMenu menu = new GenericMenu();

            if (selectedNodes.Count > 1)
            {
                menu.AddItem(new GUIContent("Remove Profiles"), false, () =>
                {
                    for (int i = 0; i < selectedNodes.Count; i++)
                    {
                        // Show dialog for first entry only
                        m_Window.settings.profileSettings.RemoveProfile(GetProfile(selectedNodes[i].id).id);
                    }
                });
            }
            else
            {
                menu.AddItem(new GUIContent("Set Active"), false, UseProfile, selectedNodes);

                if (selectedNodes[0].displayName == "Default")
                {
                    menu.AddDisabledItem(new GUIContent("Rename Profile"));
                    menu.AddDisabledItem(new GUIContent("Delete Profile"));
                }
                else
                {
                    menu.AddItem(new GUIContent("Rename Profile"), false, RenameProfile, selectedNodes);
                    menu.AddItem(new GUIContent("Delete Profile"), false, DeleteProfile, selectedNodes);
                }
            }

            menu.ShowAsContext();
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item.displayName != "Default";
        }

        protected void RenameProfile(object context)
        {
            List<TreeViewItem> selectedNodes = context as List<TreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count >= 1)
            {
                var item = selectedNodes.First();
                BeginRename(item);
            }
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename)
                return;
            
            var item = FindItemInVisibleRows(args.itemID);

            AddressableAssetProfileSettings.BuildProfile profile = GetProfile(item.id);

            Undo.RecordObject(m_Window.settings, "Profile renamed");

            bool renameSuccessful = m_Window.settings.profileSettings.RenameProfile(profile, args.newName);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Window.settings, true);

            if (renameSuccessful) Reload();
        }

        public Rect GetRow(int i)
        {
            return GetRowRect(i);
        }

        void UseProfile(object context)
        {
            List<TreeViewItem> selectedNodes = context as List<TreeViewItem>;
            if (selectedNodes != null && selectedNodes.Count >= 1)
            {
                Undo.RecordObject(m_Window.settings, "Active Profile Changed");
                var item = selectedNodes.First();
                string activeProfileId = m_TreeIndexToBuildProfileMap[item.id].id;
                m_Window.settings.activeProfileId = activeProfileId;
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Window.settings);
            }
        }

        void DeleteProfile(object context)
        {
            List<TreeViewItem> selectedNodes = context as List<TreeViewItem>;
            foreach (var item in selectedNodes)
            {
                var prof = m_TreeIndexToBuildProfileMap[item.id];
                if (prof != default)
                {
                    Undo.RecordObject(m_Window.settings, "Profile Deleted");
                    m_Window.settings.profileSettings.RemoveProfile(prof.id);
                    AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Window.settings);
                    AssetDatabase.SaveAssets();
                }
            }
            m_Window.ProfileIndex = -1;
            Reload();
        }

        internal AddressableAssetProfileSettings.BuildProfile GetSelectedProfile()
        {
            foreach (var nodeId in GetSelection())
            {
                var item = FindItemInVisibleRows(nodeId); //TODO - this probably makes off-screen but selected items not get added to list.
                if (item != null)
                {
                    return m_ProfileList[item.id];
                }
            }
            return default(AddressableAssetProfileSettings.BuildProfile);
        }
    }
}
