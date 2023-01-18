---
uid: addressable-runtime
---

# Using Addressables at runtime

Once you have your Addressable assets organized into groups and built into AssetBundles, you must still load, instantiate, and, in the end release them at runtime.

Addressables uses a reference counting system to make sure that assets are only kept in memory while they are needed. See [Memory management] for more information about reference counting and how you can minimize the amount of memory used by assets at any given time.

Addressables provides several options and APIs for loading and instantiating Addressable assets. See [Loading Addressable assets] for information and examples, including:

* [Loading an single asset]
* [Loading multiple assets]
* [Loading an AssetReference]
* [Loading Scenes]
* [Loading assets by location]
* [Instantiating objects from Addressables]
* [Releasing Addressable assets]
* [Using Addressables in a Scene]
* [Downloading dependencies in advance]

Addressables uses asynchronous operations for most loading tasks. See [Operations] for information on how to handle operations in your code, including:

* [Releasing AsyncOperationHandle instances]
* [Coroutine- and IEnumerator-based operation handling]
* [Event-based operation handling]
* [Task-based operation handling]
* [Using operations synchronously]
* [Custom operations]
* [Using typed versus untyped operation handles]
* [Reporting operation progress]

See the following for information about other runtime topics:

* [Customizing initialization]
* [Loading additional catalogs]
* [Updating catalogs]
* [Modifying resource URLs at runtime]
* [Getting the address of an asset at runtime]


[Loading Addressable assets]: xref:addressables-api-load-asset-async
[Loading an single asset]: xref:addressables-api-load-asset-async#loading-a-single-asset
[Loading multiple assets]: xref:addressables-api-load-asset-async#loading-multiple-assets
[Loading an AssetReference]: xref:addressables-api-load-asset-async#loading-an-assetreference
[Loading assets by location]: xref:addressables-api-load-asset-async#loading-assets-by-location
[Loading Scenes]: xref:addressables-api-load-asset-async#loading-scenes
[Instantiating objects from Addressables]:  xref:addressables-api-load-asset-async#instantiating-objects-from-addressables
[Releasing Addressable assets]: xref:addressables-api-load-asset-async#releasing-addressable-assets
[Using Addressables in a Scene]:  xref:addressables-api-load-asset-async#using-addressables-in-a-scene
[Downloading dependencies in advance]:  xref:addressables-api-download-dependencies-async
[Releasing AsyncOperationHandle instances]: xref:addressables-async-operation-handling#releasing-asyncoperationhandle-instances 
[Coroutine- and IEnumerator-based operation handling]:xref:addressables-async-operation-handling#coroutine-operation-handling
[Event-based operation handling]: xref:addressables-async-operation-handling#event-based-operation-handling
[Task-based operation handling]: xref:addressables-async-operation-handling#task-based-operation-handling
[Using operations synchronously]: xref:addressables-async-operation-handling#using-operations-synchronously
[Custom operations]: xref:addressables-async-operation-handling#custom-operations
[Using typed versus untyped operation handles]: xref:addressables-async-operation-handling#using-typed-versus-typeless-operation-handles
[Reporting operation progress]: xref:addressables-async-operation-handling#reporting-operation-progress
[Operations]: xref:addressables-async-operation-handling
[Customizing initialization]: xref:addressables-api-initialize-async
[Loading additional catalogs]: xref:addressables-api-load-content-catalog-async#loading-additional-catalogs
[Updating catalogs]: xref:addressables-api-load-content-catalog-async#updating-catalogs
[Modifying resource URLs at runtime]: xref:addressables-api-transform-internal-id
[Getting the address of an asset at runtime]: xref:addressables-get-address
[Memory management]: xref:addressables-memory-management