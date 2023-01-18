---
uid: addressables-modification-events
---

# Modification events
Modification events are used to signal to parts of the Addressables system when certain data is manipulated, such as an `AddressableAssetGroup` or an `AddressableAssetEntry` getting added or removed.

Modification events are triggered as part of `SetDirty` calls inside of Addressables.  `SetDirty` is used to indicate when an asset needs to be re-serialized by the `AssetDatabase`.  As part of this `SetDirty`, two modification event callbacks can trigger: 
- `public static event Action<AddressableAssetSettings, ModificationEvent, object> OnModificationGlobal`
- `public Action<AddressableAssetSettings, ModificationEvent, object> OnModification { get; set; }`

which can be found on [`AddressableAssetSettings`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetSettings) through a static, or instance, accessors respectively.

#### Code Samples
```
AddressableAssetSettings.OnModificationGlobal += (settings, modificationEvent, data) =>
        {
            if(modificationEvent == AddressableAssetSettings.ModificationEvent.EntryAdded)
            {
                //Do work
            }
        };

        AddressableAssetSettingsDefaultObject.Settings.OnModification += (settings, modificationEvent, data) =>
        {
            if (modificationEvent == AddressableAssetSettings.ModificationEvent.EntryAdded)
            {
                //Do work
            }
        };
```
Modification events pass in a generic `object` for the data associated with the event.  Below is a list of the modification events and the data types that are passed with them.

#### The Data Passed with Each ModificationEvent:
- GroupAdded
The data passed with this event is the [`AddressableAssetGroup`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup), or list of groups, that were added.
- GroupRemoved
The data passed with this event is the [`AddressableAssetGroup`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup), or list of groups, that were removed.
- GroupRenamed
The data passed with this event is the [`AddressableAssetGroup`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup), or list of groups, that were renamed.
- GroupSchemaAdded
The data passed with this event is the [`AddressableAssetGroup`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup), or list of groups, that had schemas added to them.
- GroupSchemaRemoved
The data passed with this event is the [`AddressableAssetGroup`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup), or list of groups, that had schemas removed from them.
- GroupSchemaModified
The data passed with this event is the [`AddressableAssetGroupSchema`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroupSchema) that was modified.
- GroupTemplateAdded
The data passed with this event is the `ScriptableObject`, typically one that implements [`IGroupTemplate`](xref:UnityEditor.AddressableAssets.Settings.IGroupTemplate), that was the added Group Template object.
- GroupTemplateRemoved
The data passed with this event is the `ScriptableObject`, typically one that implements [`IGroupTemplate`](xref:UnityEditor.AddressableAssets.Settings.IGroupTemplate), that was the removed Group Template object.
- GroupTemplateSchemaAdded
The data passed with this event is the [`AddressableAssetGroupTemplate`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroupTemplate) that had a schema added.
- GroupTemplateSchemaRemoved
The data passed with this event is the [`AddressableAssetGroupTemplate`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroupTemplate) that had a schema removed.
- EntryCreated
The data passed with this event is the [`AddressableAssetEntry`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetEntry) that was created.
- EntryAdded
The data passed with this event is the [`AddressableAssetEntry`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetEntry), or list of entries, that were added.
- EntryMoved
The data passed with this event is the [`AddressableAssetEntry`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetEntry), or list of entries, that were moved from one group to another.
- EntryRemoved
The data passed with this event is the [`AddressableAssetEntry`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetEntry), or list of entries, that were removed.
- LabelAdded
The data passed with this event is the `string` label that was added.
- LabelRemoved
The data passed with this event is the `string` label that was removed.
- ProfileAdded
The data passed with this event is [`BuildProfile`] that was added.
- ProfileRemoved
The data passed with this event is the `string` of the profile ID that was removed.
- ProfileModified
The data passed with this event is [`BuildProfile`] that was modified, or `null` if a batch of `BuildProfiles` were modified.
- ActiveProfileSet
The data passed with this event if the `string` of the profile ID that is set as the active profile.
- EntryModified
The data passed with this event is the [`AddressableAssetEntry`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetEntry), or list of entries, that were modified.
- BuildSettingsChanged
The data passed with this event is the [`AddressableAssetBuildSettings`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetBuildSettings) object that was modified.
- ActiveBuildScriptChanged
The data passed with this event is the [`IDataBuilder`](xref:UnityEditor.AddressableAssets.Build.IDataBuilder) build script that was set as the active builder.
- DataBuilderAdded
The data passed with this event is the `ScriptableObject`, typically one that implements [`IDataBuilder`](xref:UnityEditor.AddressableAssets.Build.IDataBuilder), that was added to the list of DataBuilders.
- DataBuilderRemoved
The data passed with this event is the `ScriptableObject`, typically one that implements [`IDataBuilder`](xref:UnityEditor.AddressableAssets.Build.IDataBuilder), that was removed from the list of DataBuilders.
- InitializationObjectAdded
The data passed with this event is the `ScriptableObject`, typically one that implements [`IObjectInitializationDataProvider`](xref:UnityEngine.ResourceManagement.Util.IObjectInitializationDataProvider), that was added to the list of InitializationObjects.
- InitializationObjectRemoved
The data passed with this event is the `ScriptableObject`, typically one that implements [`IObjectInitializationDataProvider`](xref:UnityEngine.ResourceManagement.Util.IObjectInitializationDataProvider), that was removed from the list of InitializationObjects.
- ActivePlayModeScriptChanged
The data passed with this event is the [`IDataBuilder`](xref:UnityEditor.AddressableAssets.Build.IDataBuilder) that was set as the new active play mode data builder.
- BatchModification
The data passed with this event is `null`.  This event is primarily used to indicate several modification events happening at the same time and the [`AddressableAssetSettings`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetSettings) object needed to be marked dirty.
- HostingServicesManagerModified
The data passed is either going to be the [`HostingServicesManager`](xref:UnityEditor.AddressableAssets.HostingServices.HostingServicesManager), or the [`HttpHostingService`](xref:UnityEditor.AddressableAssets.HostingServices.HttpHostingService) that were modified.
- GroupMoved
The data passed with this event is the full list of [`AddressableAssetGroups`](xref:UnityEditor.AddressableAssets.Settings.AddressableAssetGroup).
- CertificateHandlerChanged
The data passed with this event is the new `System.Type` of the Certificate Handler to be used.