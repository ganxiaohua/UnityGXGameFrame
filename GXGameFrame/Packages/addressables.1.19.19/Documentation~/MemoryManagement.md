---
uid: addressables-memory-management
---

# Memory management

The Addressables system manages the memory used to load assets and bundles by keeping a reference count of every item it loads.

When an Addressable is loaded, the system increments the reference count; when the asset is released, the system decrements the reference count. When the reference count of an Addressable returns to zero, it is eligible to be unloaded. When you explicitly load an Addressable asset, you must also release the asset when you are done with it. 

The basic rule of thumb to avoid "memory leaks" (assets that remain in memory after they are no longer needed) is to mirror every call to a load function with a call to a release function. You can release an asset with a reference to the asset instance itself or with the result handle returned by the original load operation.

Note, however, that released assets are not necessarily unloaded from memory immediately. The memory used by an asset is not freed until the AssetBundle it belongs to is also unloaded. (Released assets can also be unloaded by calling [Resources.UnloadUnusedAssets], but that tends to be a slow operation which can cause frame rate hitches.)

AssetBundles have their own reference count (the system treats them like Addressables with the assets they contain as dependencies). When you load an asset from a bundle, the bundle's reference count increases and when you release the asset, the bundle reference count decreases. When a bundle's reference count returns to zero, that means none of the assets it contains are still in use and the bundle and all the assets it contains are unloaded from memory.

Use the [Event Viewer] to monitor your runtime memory management. The viewer shows when assets and their dependencies are loaded and unloaded. 

<a name="when-is-memory-cleared"></a>
## Understanding when memory is cleared

An asset no longer being referenced (indicated by the end of a blue section in the [Event Viewer]) does not necessarily mean that asset was unloaded. A common applicable scenario involves multiple assets in an AssetBundle. For example:

* You have three Assets (`tree`, `tank`, and `cow`) in an AssetBundle (`stuff`).
* When `tree` loads, the profiler displays a single ref-count for `tree`, and one for `stuff`.
* Later, when `tank` loads, the profiler displays a single ref-count for both `tree` and `tank`, and two ref-counts for the `stuff` AssetBundle.
* If you release `tree`, it's ref-count becomes zero, and the blue bar goes away.

In this example, the `tree` asset is not actually unloaded at this point. You can load an AssetBundle, or its partial contents, but you cannot partially unload an AssetBundle. No asset in stuff unloads until the AssetBundle itself is completely unloaded. The exception to this rule is the engine interface [Resources.UnloadUnusedAssets]. Executing this method in the above scenario causes `tree` to unload. Because the Addressables system cannot be aware of these events, the profiler graph only reflects the Addressables ref-counts (not exactly what memory holds). Note that if you choose to use [Resources.UnloadUnusedAssets], it is a very slow operation, and should only be called on a screen that won't show any hitches (such as a loading screen).


## Avoiding asset churn

Asset churn is a problem that can arise if you release an object that happens to be the last item in an AssetBundle and then immediately reload either that asset or another asset in the bundle.

For example, say you have two materials, `boat` and `plane` that share a texture, `cammo`, which has been pulled into its own AssetBundle. Level 1 uses `boat` and level 2 uses `plane`. As you exit level 1 you release `boat`, and immediately load `plane`. When you release `boat`, Addressables unloads texture `cammo`. Then, when you load `plane`, Addressables immediately reloads `cammo`.

You can use the [Event Viewer] to help detect asset churn by monitoring asset loading and unloading.  

## AssetBundle memory overhead

When you load an AssetBundle, Unity allocates memory to store the bundle's internal data, which is in addition to the memory used for the assets it contains. The main types of internal data for a loaded AssetBundle include: 

* Loading cache: Stores recently accessed pages of an AssetBundle file. Use [AssetBundle.memoryBudgetKB] to control its size.
* [TypeTrees]: Defines the serialized layout of your objects.
* [Table of contents]: Lists the assets in a bundle.
* [Preload table]: Lists the dependencies of each asset.

When you organize your Addressable groups and AssetBundles, you typically must make trade-offs between the size and the number of AssetBundles you create and load. On the one hand, fewer, larger bundles can minimize the total memory usage of your AssetBundles. On the other hand, using a larger number of small bundles can minimize the peak memory usage because you can unload assets and AssetBundles more easily.  

While the size of an AssetBundle on disk is not the same as its size at runtime, you can use the disk size as an approximate guide to the memory overhead of the AssetBundles in a build. You can get bundle size and other information you can use to help analyze your AssetBundles from the [Build Layout Report].

The following sections discuss the internal data used by AssetBundles and how you can minimize the amount of memory they require, where possible. 

### TypeTrees

A TypeTree describes the field layout of one of your data types.

Each serialized file in an AssetBundle contains a TypeTree for each object type within the file. The TypeTree information allows you to load objects that are deserialized slightly differently from the way they were serialized. TypeTree information is not shared between AssetBundles; each bundle has a complete set of TypeTrees for the objects it contains.

All the TypeTrees are loaded when the AssetBundle is loaded and held in memory for the lifetime of the AssetBundle. The memory overhead associated with TypeTrees is proportional to the number of unique types in the serialized file and the complexity of those types.

You can reduce the memory requirements of AssetBundle TypeTrees in the following ways:

* Keep assets of the same types together in the same bundles.  
* Turn off TypeTrees -- turning off TypeTrees makes your AssetBundles smaller by excluding this information from a bundle. However, without TypeTree information, you may encounter serialization errors or undefined behavior when loading older bundles with a newer version of Unity or after making even small script changes in your project. 
*  Prefer simpler data types to reduce TypeTree complexity.

You can test the impact that TypeTrees have on the size of your AssetBundles by building them with and without TypeTrees disabled and comparing the sizes. Use [BuildAssetBundleOptions.DisableWriteTypeTree] to disable TypeTrees in your AssetBundles. Note that not all platforms support TypeTrees and some platforms require TypeTrees (and ignore this setting).

If you disable TypeTrees in a project, always rebuild local Addressable groups before building a new player. If you are distributing content remotely, only update content using the same version (including patch number) of Unity that you used to produce your current player and don't make even minor code changes. (When you are juggling multiple player versions, updates, and versions of Unity, you might not find the memory savings from disabling TypeTrees to be worth the trouble.) 

### Table of contents

The table of contents is a map within the bundle that allows you to look up each explicitly included asset by name. It scales linearly with the number of assets and the length of the string names by which they are mapped.

The size of your table of contents data is based on the total number of assets. You can minimize the amount of memory dedicated to holding table of content data by minimizing the number of AssetBundles loaded at a given time. 

### Preload table

The preload table is a list of all the other objects that an asset references. Unity uses the preload table to load these referenced objects when you load an asset from the AssetBundle. 

For example, a Prefab has a preload entry for each of its components as well as any other assets it may reference (materials, textures, etc). Each preload entry is 64 bits and can reference objects in other AssetBundles.

When an asset references another asset that in turn references other assets, the preload table can become large because it contains the entries needed to load both assets. If two assets both reference a third asset, then the preload tables of both contain entries to load the third asset (whether or not the referenced asset is Addressable or in the same AssetBundle).   

As an example, consider a situation in which you have two assets in an AssetBundle (PrefabA and PrefabB) and both of these prefabs reference a third prefab (PrefabC), which is large and contains several components and references to other assets. This AssetBundle contains two preload tables, one for PrefabA and one for PrefabB. Those tables contain entries for all the objects of their respective prefab, but also entries for all the objects in PrefabC and any objects referenced by PrefabC. Thus the information required to load PrefabC ends up duplicated in both PrefabA and PrefabB. This happens whether or not PrefabC is explicitly added to an AssetBundle.

Depending on how you organize your assets, the preload tables in AssetBundles could become quite large and contain many duplicate entries. This is especially true if you have several loadable assets that all reference a complex asset, such as PrefabC in the situation above. If you determine that the memory overhead from the preload table is a problem, you can structure your loadable assets so that they have fewer complex loading dependencies.

## AssetBundle dependencies

Loading an Addressable asset also loads all of the AssetBundles containing its dependencies. An AssetBundle dependency occurs when an asset in one bundle references an asset in another bundle. An example of this is a material referencing a texture. 

Addressables calculates dependencies between bundles at the bundle level. If one asset references an object in another bundle, then the entire bundle has a dependency on that bundle. This means that even if you load an asset in the first bundle that has no dependencies of its own, the second AssetBundle is still loaded into memory.

To avoid loading more bundles than are required, you should strive to keep the dependencies between AssetBundles as simple as possible. 

> [!NOTE]
> Prior to Addressables 1.13.0, the dependency graph was not as thorough as it is now. In the example above, RootAsset1 would not have had a dependency on BundleB. This previous behavior resulted in references breaking when an AssetBundle being referenced by another AssetBundle was unloaded and reloaded. This fix may result in additional data remaining in memory if the dependency graph is complex enough.

[TypeTrees]: #typetrees
[Table of contents]: #table-of-contents
[Preload table]: #preload-table
[Build Layout Report]: xref:addressables-build-layout-report
[BuildAssetBundleOptions.DisableWriteTypeTree]: xref:UnityEditor.BuildAssetBundleOptions.DisableWriteTypeTree
[Event Viewer]: xref:addressables-event-viewer
[Resources.UnloadUnusedAssets]: xref:UnityEngine.Resources.UnloadUnusedAssets
[AssetBundle.memoryBudgetKB]: xref:UnityEngine.AssetBundle.memoryBudgetKB