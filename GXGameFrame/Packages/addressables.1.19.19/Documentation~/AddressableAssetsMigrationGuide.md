---
uid: addressables-migration
---

# Upgrading to the Addressables system

This article covers how to modify your existing project to take advantage of Addressable assets. 

Outside of the Addressables system, Unity provides a few "traditional" ways to reference and load assets:

* __Scene data__: Assets you add directly to a Scene or to a component in a Scene, which the application loads automatically. Unity packages serialized scene data and the assets directly referenced by a scene into a single archive that it includes in your built player application. See [Converting Scenes] and [Using Addressable assets in non-Addressable Scenes].
* Prefabs: Assets you create using GameObjects and components, and save outside a Scene. See [Converting Prefabs].
* __Resources folders__: Assets you place in your project’s Resources folders and load using the Resources API. Unity packages assets in Resources files into a single archive that it includes in your built player application. The Resources archive is separate from the Scene data archive. See [Converting Resources folders].
* __AssetBundles__: Assets you package in AssetBundles and load with the AssetBundle API. See [Converting AssetBundles].
* __StreamingAssets__: Files you place in the StreamingAssets folder. Unity includes any files in the StreamingAssets folder in your built player application as is. See [Files in StreamingAssets]

## Converting to Addressables

Content built using Addressables only reference other assets built in that Addressables build. Content that is used or referenced to that is included within both Addressables, and the Player build through the __Scene data__ and __Resource folders__ is duplicated on disk and in memory if they are both loaded. Due to this limitation the recommended best practice is to convert all __Scene data__ and __Resource folders__ to the Addressables build system. Reducing the memory overhead due to duplication and allowing all content to be managed using Addressables. Allowing for the content to be either local or remote as well as updatable through [Content Update] builds.

### Converting Scenes

The easiest way to integrate Addressables into a project is to move your Scenes out of the __[Build Settings]__ list and make those scenes Addressable. You do need to have one Scene in the list, which is the Scene Unity loads at application startup. You can make a new Scene for this that does nothing else than load your first Addressable Scene.

To convert your Scenes:

1. Make a new "initialization" Scene.
2. Open the __Build Settings__ window (menu: __File > Build Settings__).
3. Add the initialization Scene to the Scene list.
4. Remove the other Scenes from the list.
5. Click on each Scene in the project list and check the Addressable option in its Inspector window. Alternatively, you can drag Scene assets to a group in the Addressables Groups window. (Don't make your new initialization Scene Addressable.)
6. Update the code you use to load Scenes to use the [Addressables] class Scene loading methods rather than the SceneManager methods.

At this point, you have included all the assets you have in your Scenes in an Addressable group and the Addressables system packages them in an AssetBundle when you build your Addressables content. If you only use one group for all your Scenes, the runtime loading and memory performance should be roughly equivalent to your project’s pre-Addressables state. 

You can now split your one, large Addressable Scene group into multiple groups. The best way to do that depends on the project goals. To proceed, you can move your Scenes into their own groups so that you can load and unload each of them independently of each other. As you do this, you can use the [Analyze tool] to check for duplicated assets that are shared between multiple Scenes. You can avoid duplicating an asset referenced from two different bundles by making the asset itself Addressable. It's often better to move shared assets to their own group as well to reduce interdependencies among your AssetBundles. 

<a name="addressables-in-regular-scenes"></a>
#### Using Addressable assets in non-Addressable Scenes

For Scenes that you don't want to make Addressable, you can still use Addressable assets as part of the Scene data through [AssetReferences].

When you add an AssetReference field to a custom MonoBehaviour or ScriptableObject class, you can assign an Addressable asset to the field in the Unity Editor in much the same way that you would assign an asset as a direct reference. The main difference is that you need to add code to your class to load and release the asset assigned to the AssetReference field (whereas Unity loads direct references automatically when it instantiates your object in the Scene).

> [!NOTE]
> You cannot use Addressable assets for the fields of any UnityEngine components in a non-Addressable Scene. For example, if you assign an Addressable mesh asset to a MeshFilter component in a non-Addressable Scene, Unity does not use the Addressable version of that mesh data for the Scene. Instead, Unity copies the mesh asset and includes two versions of the mesh in your application, one in the AssetBundle built for the Addressable group containing the mesh and one in the built-in Scene data of the non-Addressable Scene. (When used in an Addressable Scene, Unity does not copy the mesh data and always loads it from the AssetBundle.)

To replace direct references with AssetReferences in your custom classes, follow these steps:

1. Replace your direct references to objects with asset references (for example, `public GameObject directRefMember;` becomes `public AssetReference assetRefMember;`).
2. Drag assets onto the appropriate component’s Inspector, as you would for a direct reference.
3. Add runtime code to load the assigned asset using the [Addressables] API.
4. Add code to release the loaded asset when no longer needed.

See [Asset References] for more information about using AssetReference fields.

See [Loading Addressable assets] for more information about loading Addressable assets.

### Converting Prefabs

To convert a Prefab into an Addressable asset, check the __Addressables__ option in its Inspector window or drag it to a group in the Addressables [Groups] window.

You don't always need to make Prefabs Addressable when used in an Addressable Scene; Addressables automatically includes Prefabs that you add to the Scene hierarchy as part of the data contained in the Scene’s AssetBundle. If you use a Prefab in more than one Scene, however, you should make the Prefab into an Addressable asset so that the Prefab data isn't duplicated in each Scene that uses it. You must also make a Prefab Addressable if you want to load and instantiate it dynamically at runtime.

> [!NOTE]
> If you use a Prefab in a non-Addressable Scene, Unity copies the Prefab data into the built-in Scene data whether the Prefab is Addressable or not. You can identify assets duplicated between your Addressable asset groups and your non-Addressable Scene data using the __Check Scene to Addressable Duplicate Dependencies__ rule in the [Analyze tool]. 

### Converting Resources folders

If your project loads assets in Resources folders, you can migrate those assets to the Addressables system:

1. Make the assets Addressable. To do this, either enable the __Addressable__ option in each asset's Inspector window or drag the assets to groups in the Addressables [Groups] window.
2. Change any runtime code that loads assets using the [Resources] API to load them with the [Addressables] API.
3. Add code to release loaded assets when no longer needed.

As with Scenes, if you keep all the former Resources assets in one group, the loading and memory performance should be equivalent. Depending on your project, you can improve performance and flexibility by dividing your assets into separate groups. Use the [Analyze tool] to check for unwanted duplication across AssetBundles.

When you mark an asset in a Resources folder as Addressable, the system automatically moves the asset to a new folder in your project named Resources_moved. The default address for a moved asset is the old path, omitting the folder name. For example, your loading code might change from:

```csharp
Resources.LoadAsync\<GameObject\>("desert/tank.prefab"); 
```
to:

```csharp
Addressables.LoadAssetAsync\<GameObject\>("desert/tank.prefab");.
```

You might have to implement some functionality of the [Resources] class differently after modifying your project to use the Addressables system.

For example, consider the [Resources.LoadAll] function. Previously, if you had assets in a folder Resources/MyPrefabs/, and ran Resources.LoadAll\<SampleType\>("MyPrefabs");, it would have loaded all the assets in Resources/MyPrefabs/ matching type `SampleType`. The Addressables system doesn't support this exact functionality, but you can achieve similar results using Addressable [labels].

### Converting AssetBundles

When you first open the __Addressables Groups__ window, Unity offers to convert all AssetBundles into Addressables groups. This is the easiest way to migrate your AssetBundle setup to the Addressables system. You must still update your runtime code to load and release assets using the [Addressables] API.

If you want to convert your AssetBundle setup manually, click the __Ignore__ button. The process for manually migrating your AssetBundles to Addressables is similar to that described for Scenes and the Resources folder:

1. Make the assets Addressable by enabling the __Addressable__ option on each asset’s Inspector window or by dragging the asset to a group in the Addressables [Groups] window. The Addressables system ignores existing AssetBundle and Label settings for an asset.
2. Change any runtime code that loads assets using the [AssetBundle] or [UnityWebRequestAssetBundle] APIs to load them with the [Addressables] API. You don't need to explicitly load AssetBundle objects themselves or the dependencies of an asset; the Addressables system handles those aspects automatically.
3. Add code to release loaded assets when no longer needed.

> [!NOTE]
> The default path for the address of an asset is its file path. If you use the path as the asset's address, you'd load the asset in the same manner as you would load from a bundle. The Addressable Asset System handles the loading of the bundle and all its dependencies.

If you chose the automatic conversion option or manually added your assets to equivalent Addressables groups, then, depending on your group settings, you end up with the same set of bundles containing the same assets. (The bundle files themselves won't be identical.) You can check for unwanted duplication and other potential issues using the [Analyze tool]. You can make sure that asset loading and unloading behaves as you expect using the [Event viewer] window.

## Files in StreamingAssets

You can continue to load files from the StreamingAssets folder when you use the Addressables system. However, files in this folder cannot be Addressable nor can files reference other assets in your project. 

The Addressables system does place its runtime configuration files and local AssetBundles in the StreamingAssets folder during a build. (Addressables removes these files at the conclusion of the build process; you won’t see them in the Editor.) 


[Addressables.LoadAssetAsync\<GameObject\>("desert/tank.prefab");]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync*
[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[Analyze tool]: xref:addressables-analyze-tool
[Asset References]: xref:addressables-asset-references
[AssetBundle]: xref:UnityEngine.AssetBundle
[AssetBundles]: #converting-assetbundles
[AssetReferences]: xref:addressables-asset-references
[Build Settings]: xref:BuildSettings
[Converting AssetBundles]: #converting-assetbundles
[Converting Prefabs]: #converting-prefabs
[Converting Resources folders]: #converting-resources-folders
[Converting Scenes]: #converting-scenes
[Files in StreamingAssets]: #files-in-streamingassets
[Groups]: xref:addressables-groups
[Loading Addressable assets]: xref:addressables-api-load-asset-async
[Resources.LoadAll]: https://docs.unity3d.com/ScriptReference/Resources.LoadAll.html
[Resources]: xref:UnityEngine.Resources
[labels]: xref:addressables-labels
[Using Addressable assets in non-Addressable Scenes]: #addressables-in-regular-scenes
[UnityWebRequestAssetBundle]: xref:UnityEngine.Networking.UnityWebRequestAssetBundle
[Content Update]: xref:addressables-content-update-builds
