using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    using Object = UnityEngine.Object;

    [InitializeOnLoad]
    internal static class AddressableAssetInspectorGUI
    {
        static GUIStyle s_ToggleMixed;
        static GUIContent s_AddressableAssetToggleText;

        static AddressableAssetInspectorGUI()
        {
            s_ToggleMixed = null;
            s_AddressableAssetToggleText = new GUIContent("Addressable", "Check this to mark this asset as an Addressable Asset, which includes it in the bundled data and makes it loadable via script by its address.");
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        static void SetAaEntry(AddressableAssetSettings aaSettings, List<TargetInfo> targetInfos, bool create)
        {
            if (create && aaSettings.DefaultGroup.ReadOnly)
            {
                Debug.LogError("Current default group is ReadOnly.  Cannot add addressable assets to it");
                return;
            }

            Undo.RecordObject(aaSettings, "AddressableAssetSettings");

            if (!create)
            {
	            List<AddressableAssetEntry> removedEntries = new List<AddressableAssetEntry>(targetInfos.Count);
	            for (int i = 0; i < targetInfos.Count; ++i)
	            {
		            AddressableAssetEntry e = aaSettings.FindAssetEntry(targetInfos[i].Guid);
		            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(e.parentGroup);
		            removedEntries.Add(e);
		            aaSettings.RemoveAssetEntry(removedEntries[i], false);
	            }
	            if (removedEntries.Count > 0)
					aaSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, removedEntries, true, false);
            }
            else
            {
	            AddressableAssetGroup parentGroup = aaSettings.DefaultGroup;
                var resourceTargets = targetInfos.Where(ti => AddressableAssetUtility.IsInResources(ti.Path));
                if (resourceTargets.Any())
                {
                    var resourcePaths = resourceTargets.Select(t => t.Path).ToList();
                    var resourceGuids = resourceTargets.Select(t => t.Guid).ToList();
                    AddressableAssetUtility.SafeMoveResourcesToGroup(aaSettings, parentGroup, resourcePaths, resourceGuids);
                }
                
                var otherTargetInfos = targetInfos.Except(resourceTargets);
                List<string> otherTargetGuids = new List<string>(targetInfos.Count);
                foreach (var info in otherTargetInfos)
					otherTargetGuids.Add(info.Guid);

                var entriesCreated = new List<AddressableAssetEntry>();
                var entriesMoved = new List<AddressableAssetEntry>();
                aaSettings.CreateOrMoveEntries(otherTargetGuids, parentGroup, entriesCreated, entriesMoved, false, false);
                
                bool openedInVC = false;
                if (entriesMoved.Count > 0)
                {
	                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(parentGroup);
	                openedInVC = true;
	                aaSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesMoved, true, false);
                }
            
                if (entriesCreated.Count > 0)
                {
	                if (!openedInVC)
		                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(parentGroup);
	                aaSettings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryAdded, entriesCreated, true, false);
	            } 
	        }
        }

        static void OnPostHeaderGUI(Editor editor)
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;

            if (editor.targets.Length > 0)
            {
                foreach (var t in editor.targets)
                {
                    if (t is AddressableAssetGroup || t is AddressableAssetGroupSchema)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Profile: " + AddressableAssetSettingsDefaultObject.GetSettings(true).profileSettings.
                            GetProfileName(AddressableAssetSettingsDefaultObject.GetSettings(true).activeProfileId));

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("System Settings", "MiniButton"))
                        {
                            EditorGUIUtility.PingObject(AddressableAssetSettingsDefaultObject.Settings);
                            Selection.activeObject = AddressableAssetSettingsDefaultObject.Settings;
                        }
                        GUILayout.EndHorizontal();
                        return;
                    }
                }
                
                List<TargetInfo> targetInfos = GatherTargetInfos(editor.targets, aaSettings);
                if (targetInfos.Count == 0)
                    return;

                bool targetHasAddressableSubObject = false;
                int mainAssetsAddressable = 0;
                int subAssetsAddressable = 0;
                foreach (TargetInfo info in targetInfos)
                {
                    if (info.MainAssetEntry == null)
                        continue;
                    if (info.MainAssetEntry.IsSubAsset)
                        subAssetsAddressable++;
                    else
                        mainAssetsAddressable++;
                    if (!info.IsMainAsset)
                        targetHasAddressableSubObject = true;
                }
                
                // Overrides a DisabledScope in the EditorElement.cs that disables GUI drawn in the header when the asset cannot be edited.
                bool prevEnabledState = UnityEngine.GUI.enabled;
                if (targetHasAddressableSubObject)
                    UnityEngine.GUI.enabled = false;
                else
                {
                    UnityEngine.GUI.enabled = true;
                    foreach (var info in targetInfos)
                    {
                        if (!info.IsMainAsset)
                        {
                            UnityEngine.GUI.enabled = false;
                            break;
                        }
                    }
                }

                int totalAddressableCount = mainAssetsAddressable + subAssetsAddressable;
                if (totalAddressableCount == 0) // nothing is addressable
                {
                    if (GUILayout.Toggle(false, s_AddressableAssetToggleText, GUILayout.ExpandWidth(false)))
                        SetAaEntry(AddressableAssetSettingsDefaultObject.GetSettings(true), targetInfos, true);
                }
                else if (totalAddressableCount == editor.targets.Length) // everything is addressable
                {
                    var entryInfo = targetInfos[targetInfos.Count - 1];
                    if (entryInfo == null || entryInfo.MainAssetEntry == null)
                        throw new NullReferenceException("EntryInfo incorrect for Addressables content.");
                    
                    GUILayout.BeginHorizontal();

                    if (mainAssetsAddressable > 0 && subAssetsAddressable > 0)
                    {
                        if (s_ToggleMixed == null)
                            s_ToggleMixed = new GUIStyle("ToggleMixed");
                        if (GUILayout.Toggle(false, s_AddressableAssetToggleText, s_ToggleMixed, GUILayout.ExpandWidth(false)))
                            SetAaEntry(aaSettings, targetInfos, true);
                    }
                    else if (mainAssetsAddressable > 0)
                    {
                        if (!GUILayout.Toggle(true, s_AddressableAssetToggleText, GUILayout.ExpandWidth(false)))
                        {
                            SetAaEntry(aaSettings, targetInfos, false);
                            UnityEngine.GUI.enabled = prevEnabledState;
                            GUIUtility.ExitGUI();
                        }
                    }
                    else if (GUILayout.Toggle(false, s_AddressableAssetToggleText, GUILayout.ExpandWidth(false)))
                        SetAaEntry(aaSettings, targetInfos, true);

                    if (editor.targets.Length == 1)
                    {
                        if (!entryInfo.IsMainAsset || entryInfo.MainAssetEntry.IsSubAsset)
                        {
                            bool preAddressPrevEnabledState = UnityEngine.GUI.enabled;
                            UnityEngine.GUI.enabled = false;
                            string address = entryInfo.Address + (entryInfo.IsMainAsset ? "" : $"[{entryInfo.TargetObject.name}]");
                            EditorGUILayout.DelayedTextField(address, GUILayout.ExpandWidth(true));
                            UnityEngine.GUI.enabled = preAddressPrevEnabledState;
                        }
                        else
                        {
                            string newAddress = EditorGUILayout.DelayedTextField(entryInfo.Address, GUILayout.ExpandWidth(true));
                            if (newAddress != entryInfo.Address)
                            {
                                if (newAddress.Contains("[") && newAddress.Contains("]"))
                                    Debug.LogErrorFormat("Rename of address '{0}' cannot contain '[ ]'.", entryInfo.Address);
                                else
                                {
                                    entryInfo.MainAssetEntry.address = newAddress;
                                    AddressableAssetUtility.OpenAssetIfUsingVCIntegration(entryInfo.MainAssetEntry.parentGroup, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        FindUniqueAssetGuids(targetInfos, out var uniqueAssetGuids, out var uniqueAddressableAssetGuids);
                        EditorGUILayout.LabelField(uniqueAddressableAssetGuids.Count + " out of " + uniqueAssetGuids.Count + " assets are addressable.");
                    }
                    
                    DrawSelectEntriesButton(targetInfos);
                    GUILayout.EndHorizontal();
                }
                else // mixed addressable selected
                {
                    GUILayout.BeginHorizontal();
                    if (s_ToggleMixed == null)
                        s_ToggleMixed = new GUIStyle("ToggleMixed");
                    if (GUILayout.Toggle(false, s_AddressableAssetToggleText, s_ToggleMixed, GUILayout.ExpandWidth(false)))
                        SetAaEntry(AddressableAssetSettingsDefaultObject.GetSettings(true), targetInfos, true);
                    FindUniqueAssetGuids(targetInfos, out var uniqueAssetGuids, out var uniqueAddressableAssetGuids);
                    EditorGUILayout.LabelField(uniqueAddressableAssetGuids.Count + " out of " + uniqueAssetGuids.Count + " assets are addressable.");
                    DrawSelectEntriesButton(targetInfos);
                    GUILayout.EndHorizontal();
                }
                UnityEngine.GUI.enabled = prevEnabledState;
            }
        }

        internal static List<TargetInfo> GatherTargetInfos(Object[] targets, AddressableAssetSettings aaSettings)
        {
            var targetInfos = new List<TargetInfo>();
            AddressableAssetEntry entry;
            foreach (var t in targets)
            {
                if (AddressableAssetUtility.TryGetPathAndGUIDFromTarget(t, out var path, out var guid))
                {
                    var mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                    // Is asset
                    if (mainAssetType != null && !BuildUtility.IsEditorAssembly(mainAssetType.Assembly))
                    {
                        bool isMainAsset = t is AssetImporter || AssetDatabase.IsMainAsset(t);
                        var info = new TargetInfo() {TargetObject = t, Guid = guid, Path = path, IsMainAsset = isMainAsset};

                        if (aaSettings != null)
                        {
                            entry = aaSettings.FindAssetEntry(guid, true);
                            if (entry != null)
                                info.MainAssetEntry = entry;
                        }

                        targetInfos.Add(info);
                    }
                }
            }

            return targetInfos;
        }

        internal static void FindUniqueAssetGuids(List<TargetInfo> targetInfos, out HashSet<string> uniqueAssetGuids, out HashSet<string> uniqueAddressableAssetGuids)
        {
            uniqueAssetGuids = new HashSet<string>();
            uniqueAddressableAssetGuids = new HashSet<string>();
            foreach (TargetInfo info in targetInfos)
            {
                uniqueAssetGuids.Add(info.Guid);
                if (info.MainAssetEntry != null)
                    uniqueAddressableAssetGuids.Add(info.Guid);
            }
        }

        static void DrawSelectEntriesButton(List<TargetInfo> targets)
        {
            var prevGuiEnabled = UnityEngine.GUI.enabled;
            UnityEngine.GUI.enabled = true;

            if (GUILayout.Button("Select"))
            {
                AddressableAssetsWindow.Init();
                var window = EditorWindow.GetWindow<AddressableAssetsWindow>();
                List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(targets.Count);
                foreach (TargetInfo info in targets)
                {
                    if (info.MainAssetEntry != null)
                    {
                        if (info.IsMainAsset == false && ProjectConfigData.ShowSubObjectsInGroupView)
                        {
                            List<AddressableAssetEntry> subs = new List<AddressableAssetEntry>();
                            info.MainAssetEntry.GatherAllAssets(subs, false, true, true);
                            foreach (AddressableAssetEntry sub in subs)
                            {
                                if (sub.TargetAsset == info.TargetObject)
                                {
                                    entries.Add(sub);
                                    break;
                                }
                            }
                        }
                        else
                            entries.Add(info.MainAssetEntry);
                    }
                }

                if (entries.Count > 0)
                    window.SelectAssetsInGroupEditor(entries);
            }
            UnityEngine.GUI.enabled = prevGuiEnabled;
        }

        internal class TargetInfo
        {
            public UnityEngine.Object TargetObject;
            public string Guid;
            public string Path;
            public bool IsMainAsset;
            public AddressableAssetEntry MainAssetEntry;

            public string Address
            {
                get
                {
                    if (MainAssetEntry == null)
                        throw new NullReferenceException("No Entry set for Target info with AssetPath " + Path);
                    return MainAssetEntry.address;
                }
            }
        }
    }
}
