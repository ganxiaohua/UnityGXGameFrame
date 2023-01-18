using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Settings
{
    /// <summary>
    /// Contains a list of AddressableAssetEntries that can be included in the settings.  The purpose of this class is to provide a way of combining entries from external sources such as packages into your project settings.
    /// </summary>
    [Obsolete("Addressable AssetEntryCollection is Obsolete")]
    public class AddressableAssetEntryCollection : ScriptableObject
    {
        [FormerlySerializedAs("m_serializeEntries")]
        [SerializeField]
        List<AddressableAssetEntry> m_SerializeEntries = new List<AddressableAssetEntry>();
        /// <summary>
        /// The collection of entries.
        /// </summary>
        public List<AddressableAssetEntry> Entries { get { return m_SerializeEntries; } }
        
        internal static bool ConvertEntryCollectionToEntries(AddressableAssetEntryCollection collection, AddressableAssetSettings settings)
        {
            if (settings == null)
                settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(collection, out var collectionGuid, out long localId))
                return false;
            
            var collectionEntry = settings.FindAssetEntry(collectionGuid, true);

            var group = collectionEntry == null ? settings.DefaultGroup : collectionEntry.parentGroup;
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            foreach (AddressableAssetEntry assetEntry in collection.Entries)
            {
                if (assetEntry == null)
                    continue;
                var entry = settings.FindAssetEntry(assetEntry.guid);
                if (entry != null)
                    continue;
                entries.Add(assetEntry);
            }

            HashSet<string> collectionLabels = new HashSet<string>();
            if (collectionEntry != null)
            {
                collectionLabels = collectionEntry.labels;
                if (!settings.RemoveAssetEntry(collectionEntry))
                    return false;
            }

            foreach (AddressableAssetEntry entry in entries)
            {
                var newEntry = settings.CreateOrMoveEntry(entry.guid, group);
                newEntry.SetAddress(entry.address);
                foreach (string label in collectionLabels)
                    newEntry.SetLabel(label, true);
                foreach (string label in entry.labels)
                    newEntry.SetLabel(label, true);
            }

            return true;
        }
    }
}
