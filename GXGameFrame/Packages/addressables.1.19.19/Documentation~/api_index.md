---
uid: addressables-script-ref
---

# Addressables Script Reference

This section of the documentation contains details of the scripting API that Unity provides for the Addressables package. 

The scripting reference is organized according to the classes available to scripts, which are described along with their methods, properties, and any other information relevant to their use.

API are grouped by namespaces they belong to, and can be selected from the sidebar to the left. You can use the Filter control above the sidebar to filter the table of contents so that it only shows entries containing the string you enter. You can also use the Search control at the top of the window to perform a full-text search of the Addressables documentation.

## Commonly used classes

The following lists include the most commonly used classes you will encounter when using the Addressables API:

In runtime code:

* [Addressables]: contains the main API for interacting with the Addressables system at runtime, from initialization, to loading assets, to releasing them.
* [AsyncOperationHandle]: a handle for Addressables operations. Provides access to the assets loaded, operation progress, and other results.
* [AsyncOperationBase]: a base class for implementing your own operations.
* [AssetReference]: a type you can use in MonoBehaviours to easily reference Addressable assets via an Inspector window.
* [IResourceLocation]: an interface to objects that contain information needed to load an asset.
* [ResourceManager]: manages Addressable resources and operations.
* [CacheInitializationSettings]: a specialized object for initializing your cache settings.
* [InternalIdTransformFunc]: A function you can implement to dynamically transform asset URLs.

In Unity Editor code:

* [AddressableAssetSettings]: defines the Addressable settings.
* [AddressableAssetSettingsDefaultObject]: provides access to the asset containing your Addressables settings.
* [AnalyzeRule]: a base class for adding rules to the Analyze tool.
* [IHostingService]: an interface for creating your own hosting service implementations.
* [IDataBuilder]: an interface for creating your own build implementations.


[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[AsyncOperationHandle]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle
[AsyncOperationBase]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1
[IResourceLocation]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation
[CacheInitializationSettings]: xref:UnityEditor.AddressableAssets.Settings.CacheInitializationSettings
[InternalIdTransformFunc]: xref:UnityEngine.ResourceManagement.ResourceManager.InternalIdTransformFunc
[AssetReference]: xref:UnityEngine.AddressableAssets.AssetReference
[AddressableAssetSettings]: xref:UnityEditor.AddressableAssets.Settings.AddressableAssetSettings
[AddressableAssetSettingsDefaultObject]: xref:UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject
[AnalyzeRule]: xref:UnityEditor.AddressableAssets.Build.AnalyzeRules.AnalyzeRule
[IHostingService]: xref:UnityEditor.AddressableAssets.HostingServices.IHostingService
[IDataBuilder]: xref:UnityEditor.AddressableAssets.Build.IDataBuilder
[ResourceManager]: xref:UnityEngine.ResourceManagement.ResourceManager
