---
uid: addressables-api-load-content-catalog-async
---

# Managing catalogs at runtime

By default, the Addressables system manages the catalog automatically at runtime. If you built your application with a remote catalog, the Addressables system automatically checks to see if you have uploaded a new catalog, and, if so, downloads the new version and loads it into memory. 

You can load additional catalogs at runtime. For example, you could load a catalog produced by a separate, compatible project to load Addressable assets built by that project. (See [Loading Content from Multiple Projects].)

If you want to change the default catalog update behavior of the Addressables system, you can turn off the automatic check and check for updates manually. See [Updating catalogs].

## Loading additional catalogs

Use [Addressables.LoadContentCatalogAsync] to load additional content catalogs, either from your hosting service or from the local file system. After the operation to load the catalog is finished, you can call any Addressables loading functions using the keys in the new catalog.

If you provide the catalog hash file at the same URL as the catalog, Addressables caches the secondary catalog. When the client application loads the catalog in the future, it only downloads a new version of the catalog if the hash changes.

Once you load a catalog, you cannot unload it. You can, however, update a loaded catalog. You must release the operation handle for the operation that loaded the catalog before updating a catalog. See [Updating catalogs] for more information.

In general, there is no reason to hold on to the operation handle after loading a catalog. You can release it automatically by setting the `autoReleaseHandle` parameter to true when loading a catalog, as shown in the following example: 

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_LoadAdditionalCatalog)]

<!--
```csharp
public IEnumerator Start()
{
    //Load a catalog and automatically release the operation handle.
    AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync("path_to_secondary_catalog", true);
    yield return handle;

    //...
}
```
-->

> [!NOTE]
> You can use the [Catalog Download Timeout] property of your Addressables settings to specify a timeout for downloading catalogs.

## Updating catalogs

If the catalog hash file is available, Addressables checks the hash when loading a catalog to determine if the version at the provided URL is more recent than the cached version of the catalog. You can turn off the default catalog check, if desired, and call the [Addressables.UpdateCatalogs] function when you want to update the catalog. If you loaded a catalog manually with [LoadContentCatalogAsync], you must release the operation handle before you can update the catalog.

When you call the UpdateCatalog function, all other Addressable requests are blocked until the operation is finished. You can release the operation handle returned by UpdateCatalogs immediately after the operation finishes (or set the `autoRelease` parameter to true).

If you call UpdateCatalog without providing a list of catalogs, Addressables checks all of the currently loaded catalogs for updates. 

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_UpdateCatalog)]

<!--
```csharp
IEnumerator UpdateCatalogs()
{
    AsyncOperationHandle<List<IResourceLocator>> updateHandle 
        = Addressables.UpdateCatalogs();

    yield return updateHandle;
    Addressables.Release(updateHandle);
}
```
-->

You can also call [Addressables.CheckForCatalogUpdates] directly to get the list of catalogs that have updates and then perform the update:

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_CheckCatalog)]

<!--
```csharp
IEnumerator UpdateCatalogs()
{
    List<string> catalogsToUpdate = new List<string>();
    AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
    checkForUpdateHandle.Completed += op =>
    {
        catalogsToUpdate.AddRange(op.Result);
    };

    yield return checkForUpdateHandle;

    if (catalogsToUpdate.Count > 0)
    {
        AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
        yield return updateHandle;
        Addressables.Release(updateHandle);
    }

    Addressables.Release(checkForUpdateHandle);
}
```
-->

> [!IMPORTANT]
> If you update a catalog when you have already loaded content from the related AssetBundles, you can encounter conflicts between the loaded AssetBundles and the updated versions. You can enable the [Unique Bundle Ids] option in your Addressable settings to eliminate the possibility of bundle ID collisions at runtime. However, enabling this option also means that more AssetBundles must typically be rebuilt when you perform a content update. See [Content update builds] for more information. Another option is to first unload any content and AssetBundles that must be updated, which can be a slow operation.

[Loading Content from Multiple Projects]: xref:addressables-multiple-projects
[Addressables.CheckForCatalogUpdates]: xref:UnityEngine.AddressableAssets.Addressables.CheckForCatalogUpdates*
[Addressables.InitializeAsync]: xref:UnityEngine.AddressableAssets.Addressables.InitializeAsync*
[Addressables.LoadContentCatalogAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync*
[Addressables.ResourceManager]: xref:UnityEngine.AddressableAssets.Addressables.ResourceManager
[Addressables.UpdateCatalogs]: xref:UnityEngine.AddressableAssets.Addressables.UpdateCatalogs*
[Build Remote Catalog]: xref:addressables-asset-settings#catalog
[Cache]: xref:UnityEngine.Cache
[CacheInitializationSettings]: xref:UnityEditor.AddressableAssets.Settings.CacheInitializationSettings
[Caching]: xref:UnityEngine.Caching
[Catalog Download Timeout]: xref:addressables-asset-settings#downloads
[Content update builds]: xref:addressables-content-update-builds
[Custom certificate handler]: xref:addressables-asset-settings#downloads
[Custom URL transform function]: #id-transform-function
[Customizing initialization]: #customizing-initialization
[Disable Catalog Update on Startup]: xref:addressables-asset-settings#catalog
[Getting the address of an asset at runtime]: #getting-the-address-of-an-asset-at-runtime
[initialization object list]: xref:addressables-asset-settings#initialization-object-list
[initialization object]: xref:addressables-asset-settings#initialization-object-list
[InternalId]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation.InternalId
[IObjectInitializationDataProvider]: xref:UnityEngine.ResourceManagement.Util.IObjectInitializationDataProvider
[IResourceLocation]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation
[LoadContentCatalogAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadContentCatalogAsync*
[Loading additional catalogs]: #loading-additional-catalogs
[Loading Assets by Location]: xref:addressables-loading-assets#loading-assets-by-location
[Modifying resource URLs at runtime]: #modifying-resource-urls-at-runtime
[ObjectInitializationData]: xref:UnityEngine.ResourceManagement.Util.ObjectInitializationData
[PrimaryKey]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation.PrimaryKey
[Profile variable syntax]: xref:addressables-profiles#profile-variable-syntax
[Profile variables]: xref:addressables-profiles#profile-variable-syntax
[RemoteLoadPath Profile variable]: xref:addressables-profiles
[ResourceLocators]: xref:UnityEngine.AddressableAssets.ResourceLocators
[ResourceManager exception handler]: xref:UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler
[ResourceManager]: xref:UnityEngine.ResourceManagement.ResourceManager
[InternalIdTransformFunc]: xref:UnityEngine.ResourceManagement.ResourceManager.InternalIdTransformFunc
[Unique Bundle Ids]: xref:addressables-content-update-builds#unique-bundle-ids-setting
[Updating catalogs]: #updating-catalogs
