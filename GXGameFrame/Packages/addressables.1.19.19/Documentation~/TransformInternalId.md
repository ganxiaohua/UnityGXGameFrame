---
uid: addressables-api-transform-internal-id
---

<a name="modify-resoure-urls"></a>
# Transforming resource URLs

Addressables provides the following ways to modify the URLs it uses to load assets at runtime:

* Static properties in a Profile variable
* Implementing an ID transform function
* Implementing a WebRequestOverride method

## Static Profile variables

You can use a static property when defining the [RemoteLoadPath Profile variable] to specify all or part of the URL from which your application loads remote content, including catalogs, catalog hash files, and AssetBundles. See [Profile variable syntax] for information about specifying a property name in a Profile variable. The value of your static property must be set before Addressables initializes. Changing the value after initialization has no effect. 

## ID transform function

You can assign a function to the [Addressables.ResourceManager] object's [InternalIdTransformFunc] property to individually change the URLs from which Addressables loads assets. You must assign the function before the relevant operation starts, otherwise the default URL is used.

Using TransformInternalId grants a fair amount of flexibility, especially in regards to remote hosting. Given a single IResourceLocation, you can transform the ID to point towards a server specified at runtime. This is particularly useful if your server IP address changes or if you use different URLS to provide different variants of your application assets.

The ResourceManager calls your TransformInternalId  function when it looks up an asset, passing the [IResourceLocation] instance for the asset to your function. You can change the [InternalId] property of this IResourceLocation and return the modified object to the ResourceManager.

The following example illustrates how you could append a query string to all URLs for AssetBundles:

[!code-cs[sample](../Tests/Editor/DocExampleCode/IDTransformer.cs#doc_Transformer)]

<!--
```csharp
//Implement a method to transform the internal ids of locations
string MyCustomTransform(IResourceLocation location)
{
    if (location.ResourceType == typeof(IAssetBundleResource) && location.InternalId.StartsWith("http"))
        return location.InternalId + "?customQueryTag=customQueryValue";

    return location.InternalId;
}

//Override the Addressables transform method with your custom method.  This can be set to null to revert to default behavior.
[RuntimeInitializeOnLoadMethod]
static void SetInternalIdTransform()
{
    Addressables.InternalIdTransformFunc = MyCustomTransform;
}
```
-->

## WebRequest override

You can assign a function to the [Addressables] object's [WebRequestOverride] property to individually modify the [UnityWebRequest] from which is used to download files, such as an AssetBundle or catalog json file. You must assign the function before the relevant operation starts, otherwise the default UnityWebRequest is used.

The ResourceManager calls your [WebRequestOverride] function before [UnityWebRequest.SendWebRequest] is called. Passing the UnityWebRequest for the download to your function.

The following example illustrates how you could append a query string to all URLs for AssetBundles and catalogs:

[!code-cs[sample](../Tests/Editor/DocExampleCode/WebRequestOverride.cs#doc_TransformerWebRequest)]

<!--
```csharp
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

public class WebRequestOverride : MonoBehaviour
{
    //Register to override WebRequests Addressables creates
    //The UnityWebRequests will default to the standard behavior
    private void Start()
    {
        Addressables.WebRequestOverride = EditWebRequestURL;
    }
    
    //Override the url of the WebRequest
    private void EditWebRequestURL(UnityWebRequest request)
    {
         if (request.url.EndsWith(".bundle"))
            request.url = request.url + "?customQueryTag=customQueryValue";
        else if (request.url.EndsWith(".json") || request.url.EndsWith(".hash"))
            request.url = request.url + "?customQueryTag=customQueryValue";
    }
}
```
-->

[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[WebRequestOverride]: xref:UnityEngine.AddressableAssets.Addressables.WebRequestOverride
[UnityWebRequest]: xref:UnityEngine.Networking.UnityWebRequest
[UnityWebRequest.SendWebRequest]: xref:UnityEngine.Networking.UnityWebRequest.SendWebRequest
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