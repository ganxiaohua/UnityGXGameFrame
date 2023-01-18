using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests
{
    internal static class ResourcesTestUtility
    {
        public static int GetResourcesEntryCount(AddressableAssetSettings settings, bool recurseAll,
            Func<AddressableAssetEntry, bool> filter = null)
        {
            return GetResourcesEntries(settings, recurseAll, filter).Count;
        }

        public static List<AddressableAssetEntry> GetResourcesEntries(AddressableAssetSettings settings,
            bool recurseAll, Func<AddressableAssetEntry, bool> filter = null)
        {
            var group = settings.CreateGroup("FindResources", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(PlayerDataGroupSchema));
            group.GetSchema<PlayerDataGroupSchema>().IncludeResourcesFolders = true;
            var entry = settings.CreateEntry("empty", "empty", group, false);
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            entry.GatherResourcesEntries(entries, recurseAll, filter);
            group.RemoveAssetEntry(entry);
            settings.RemoveGroup(group);
            return entries;
        }
    }
}