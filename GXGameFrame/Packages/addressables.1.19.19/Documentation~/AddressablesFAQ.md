---
uid:  addressables-faq
---

# Addressables FAQ

<a name="faq-bundle-size"></a>
### Is it better to have many small bundles or a few bigger ones?
There are a few key factors that go into deciding how many bundles to generate.
First, it's important to note that you control how many bundles you have both by how large your groups are, and by the groups' build settings.  "Pack Together" for example, creates one bundle per group, while "Pack Separately" creates many.  See [schema build settings for more information](xref:UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundleMode).

Once you know how to control bundle layout, the decision of how to set these up will be game specific.  Here are key pieces of data to help make that decision:

Dangers of too many bundles:
* Each bundle has memory overhead.  Details are [on the memory management page](MemoryManagement.md#assetbundle-memory-overhead). This is tied to a number of factors, outlined on that page, but the short version is that this overhead can be significant.  If you anticipate 100's or even 1000's of bundles loaded in memory at once, this could mean a noticeable amount of memory eaten up.
* There are concurrency limits for downloading bundles.  If you have 1000's of bundles you need all at once, they cannot not all be downloaded at the same time.  Some number will be downloaded, and as they finish, more will trigger. In practice this is a fairly minor concern, so minor that you'll often be gated by the total size of your download, rather than how many bundles it's broken into.
* Bundle information can bloat the catalog.  To be able to download or load catalogs, we store string-based information about your bundles.  1000's of bundles worth of data can greatly increase the size of the catalog.
* Greater likelihood of duplicated assets. Say two materials are marked as Addressable and each depend on the same texture. If they are in the same bundle, then the texture is pulled in once, and referenced by both. If they are in separate bundles, and the texture is not itself Addressable, then it will be duplicated. You then either need to mark the texture as Addressable, accept the duplication, or put the materials in the same bundle. 

Dangers of too few bundles:
* The UnityWebRequest (which we use to download) does not resume failed downloads.  So if a large bundle is downloading and your user loses connection, the download is started over once they regain connection. 
* Items can be loaded individually from bundles, but cannot be unloaded individually.  For example, if you have 10 materials in a bundle, load all 10, then tell Addressables to release 9 of them, all 10 will likely be in memory.  This is also covered [on the memory management page](MemoryManagement.md#when-is-memory-cleared).

<a name="faq-best-compression"></a>
### What compression settings are best?
Addressables provides three different options for bundle compression: Uncompressed, LZ4, and LZMA.  Generally speaking, LZ4 should be used for local content, and LZMA for remote, but more details are outlined below as there can be exceptions to this.  
You can set the compression option using the Advanced settings on each group. Compression does not affect in-memory size of your loaded content. 
* Uncompressed - This option is largest on disk, and generally fasted to load.  If your game happens to have space to spare, this option should at least be considered for local content.  A key advantage of uncompressed bundles is how they handle being patched.  If you are developing for a platform where the platform itself provides patching (such as Steam or Switch), uncompressed bundles provide the most accurate (smallest) patching.  Either of the other compression options will cause at least some bloat of patches.
* LZ4 - If Uncompressed is not a viable option, then LZ4 should be used for all other local content.  This is a chunk-based compression which provides the ability to load parts of the file without needing to load it in its entirety. 
* LZMA - LZMA should be used for all remote content, but not for any local content.  It provides the smallest bundle size, but is slow to load. If you were to store local bundles in LZMA you could create a smaller player, but load times would be significantly worse than uncompressed or LZ4. For downloaded bundles, we avoid the slow load time by recompressing the downloaded bundle when storing it in the AssetBundle cache.  By default, bundles will be stored in the cache Uncompressed.  If you wish to compress the cache with LZ4, you can do so by creating a [`CacheInitializationSettings`](xref:UnityEditor.AddressableAssets.Settings.CacheInitializationSettings).  See [Initialization Objects](xref:addressables-api-initialize-async#initialization-objects) for more information about setting this up. 

> [!NOTE] 
> LZMA AssetBundle compression is not available for AssetBundles on WebGL. LZ4 compression can be used instead. For more WebGL AssetBundle information, see [Building and running a WebGL project](xref:webgl-building#AssetBundles).

Note that the hardware characteristics of a platform can mean that uncompressed bundles are not always the fastest to load.  The maximum speed of loading uncompressed bundles is gated by IO speed, while the speed of loading LZ4-compressed bundles can be gated by either IO speed or CPU, depending on hardware.  On most platforms, loading LZ4-compressed bundles is CPU bound, and loading uncompressed bundles will be faster. On platforms that have low IO speeds and high CPU speeds, LZ4 loading can be faster. It is always a good practice to run performance analysis to validate whether your game fits the common patterns, or needs some unique tweaking.

More information on Unity's compression selection is available in the [Asset Bundle documentation](https://docs.unity3d.com/Manual/AssetBundles-Cache.html).  

<a name="faq-minimize-catalog-size"></a>
### Are there ways to minimize the catalog size?
Currently there are two optimizations available.
1. Compress the local catalog.  If your primary concern is how big the catalog is in your build, there is an option in the inspector for the top level settings of **Compress Local Catalog**. This option builds catalog that ships with your game into an AssetBundle. Compressing the catalog makes the file itself smaller, but note that this does increase catalog load time.  
2. Disable built-in scenes and Resources.  Addressables provides the ability to load content from Resources and from the built-in scenes list. By default this feature is on, which can bloat the catalog if you do not need this feature.  To disable it, select the "Built In Data" group within the Groups window (**Window** > **Asset Management** > **Addressables** > **Groups**). From the settings for that group, you can uncheck "Include Resources Folders" and "Include Build Settings Scenes". Unchecking these options only removes the references to those asset types from the Addressables catalog.  The content itself is still built into the player you create, and you can still load it via legacy API. 

<a name="faq-content-state-file"></a>
### What is addressables_content_state?
After every new Addressable content build, we produce an addressables_content_state.bin file, which is saved to the folder path defined in the Addressable Assets Settings value "Content State build Path" appended with /<Platform>.  A new content build here is defined as a content build that is not part of the [content update workflow](ContentUpdateWorkflow.md). If this value is empty, the default location will be the `Assets/AddressableAssetsData/<Platform>/` folder of your Unity project.
This file is critical to our [content update workflow](ContentUpdateWorkflow.md). If you are not doing any content updates, you can completely ignore this file.
If you are planning to do content updates, you will need the version of this file produced for the previous release. We recommend checking it into version control and creating a branch each time you release a player build.  More information is available on our [content update workflow page](ContentUpdateWorkflow.md).

<a name="faq-scale-implications"></a>
### What are possible scale implications?
As your project grows larger, keep an eye on the following aspects of your assets and bundles:
* Total bundle size - Historically Unity has not supported files larger than 4GB.  This has been fixed in some recent editor versions, but there can still be issues. It is recommended to keep the content of a given bundle under this limit for best compatibility across all platforms.  
* Sub assets affecting UI performance - There is no hard limit here, but if you have many assets, and those assets have many subassets, it may be best to turn off sub-asset display. This option only affects how the data is displayed in the Groups window, and does not affect what you can and cannot load at runtime.  The option is available in the groups window under **Tools** > **Groups View** > **Show Sprite and Subobject Addresses**.  Disabling this will make the UI more responsive.
* Group hierarchy display - Another UI-only option to help with scale is **Group Hierarchy with Dashes**.  The option is available in the groups window under **Tools** > **Groups View** > **Group Hierarchy with Dashes**. With this enabled, groups that contain dashes '-' in their names will display as if the dashes represented folder hierarchy. This does not affect the actual group name, or the way things are built.  For example, two groups called "x-y-z" and "x-y-w" would display as if inside a folder called "x", there was a folder called "y".  Inside that folder were two groups, called "x-y-z" and "x-y-w". This will not really affect UI responsiveness, but simply makes it easier to browse a large collection of groups. 
* Bundle layout at scale - For more information about how best to set up your layout, see the earlier question: [_Is it better to have many small bundles or a few bigger ones_](AddressablesFAQ.md#faq-bundle-size)

<a name="faq-load-modes"></a>
### What Asset Load Mode to use?
For most platforms and collection of content, it is recommended to use `Requested Asset and Dependencies`. This mode will only load what is required for the Assets requested with `LoadAssetAsync` or `LoadAssetsAsync`.
This prevents situations where Assets are loaded into memory that are not used.

Performance in situations where you will load all Assets that are packed together, such as a loading screen. Most types of content will have either have similar or improved performance when loading each individually using `Requested Asset and Dependencies` mode.
Loading performance can vary between content type. As an example, large counts of serialized data such as Prefabs or ScriptableObjects with direct references to other serialized data will load faster using `All Packed Assets and Dependencies`. With some other Assets like Textures, you can often achieve better performance when you load each Asset individually..
If using [Synchronous Addressables](SynchronousAddressables.md), there is little performance between between Asset load modes. Because of greater flexibility it is recommended to use `Requested Asset and Dependencies` where you know the content will be loaded synchronously.

**Note**: The above examples are taken for Desktop and Mobile. Performance may differ between platforms. `All Packed Assets and Dependencies` mode typically performs better than loading assets individually on the Nintendo Switch.
It is recommended to profile loading performance for your specific content and platform to see what works for your Application.

On loading the first Asset with `All Packed Assets and Dependencies`, all Assets are loaded into memory. Later LoadAssetAsync calls for Assets from that pack will return the preloaded Asset without needing to load it. 
Even though all the Assets in a group and any dependencies are loaded in memory when you use the All Packed Assets and Dependencies option, the reference count of an individual asset is not incremented unless you explicitly load it (or it is a dependency of an asset that you load explicitly). If you later call [`Resources.UnloadUnusedAssets`](https://docs.unity3d.com/ScriptReference/Resources.UnloadUnusedAssets.html), or you load a new Scene using [`LoadSceneMode.Single`](https://docs.unity3d.com/ScriptReference/SceneManagement.LoadSceneMode.html), then any unused assets (those with a reference count of zero) are unloaded.

<a name="faq-internal-naming"></a>
### What are the Internal naming mode implications?
In the [Group Settings](GroupSettings.md) "Internal Asset Naming Mode" and "Internal Bundle ID Mode", can be used to determine how assets and bundles are identified. This affects the bundle data in different ways.

`Internal Asset Naming Mode` determines the identification of assets in AssetBundles and is used to load the asset from the bundle. This value is used as the internalId of the asset Location.
Changing this setting affects a bundles CRC and Hash value.

* `Full Path` is recommended to use during development. This option is the path of the asset in your project. This allows you to identify Assets being loaded by their ID if needed.
* `Filename` can also be used to identify an asset as with the full path. **Note**: You cannot have multiple assets with the same name.
* `GUID` is a deterministic value for the asset.
* `Dynamic` is recommended to use for release. This option generates the smallest unique length of the asset GUID. Resulting in a reduction of data in the AssetBundle and catalog, and lower runtime memory overhead.

`Internal Bundle Id Mode` determines how an AssetBundle is identified internally. This affects how an AssetBundle locates dependencies that are contained in other bundles. Changing this value affects the CRC and Hash of this bundle and all other bundles that reference it.

* `Group Guid` is recommended, this is unique per Group and does not change.
* `Group Guid Project Id Hash` uses a combination of the Group GUID and the Cloud Project Id (if Cloud Services are enabled). This changes if the Project is bound to a different Cloud Project Id.
* `Group Guid Project Id Entries Hash` includes all the asset entries in the Group, and can change where adding or removing entries to the Group occurs. This setting should be used with caution.

<a name="faq-edit-loaded-assets"></a>
### Is it safe to edit loaded Assets?
When editing Assets loaded from Bundles, in a Player or when using "Use Existing Build (requires built groups)" playmode setting. The Assets are loaded from the Bundle and only exist in memory. Changes cannot be written back to the Bundle on disk, and any modifications to the Object in memory do not persist between sessions.

This is different when using "Use Asset Database (fastest)" or "Simulate Groups (advanced)" playmode settings, in these modes the Assets are loaded from the Project files. Any modifications that are made to loaded Asset modifies the Project Asset, and are saved to file.

In order to prevent this, when making runtime changes, create a new instance of the Object to modify and use as the Object to create an instance of with the Instantiate method. As shown in the example code below. 

```
var op = Addressables.LoadAssetAsync<GameObject>("myKey");
yield return op;
if (op.Result != null)
{
    GameObject inst = UnityEngine.Object.Instantiate(op.Result);
    // can now use and safely make edits to inst, without the source Project Asset being changed.
}
```

**Please Note**, When instancing an Object:
* The AsyncOperationHandle or original Asset must be used when releasing the Asset, not the instance.
* Instantiating an Asset that has references to other Assets does not create new instances other those referenced Assets. The references remain targeting the Project Asset.
* Unity Methods are invoked on the new instance, such as Start, OnEnable, and OnDisable.

<a name="faq-get-address"></a>
### Is it possible to retrieve the address of an asset or reference at runtime?
In the most general case, loaded assets no longer have a tie to their address or `IResourceLocation`. There are ways, however, to get the properly associated `IResourceLocation` and use that to read the field PrimaryKey. The PrimaryKey field will be set to the assets' address unless "Include Address In Catalog" is disabled for the group this object came from. In that case, the PrimaryKey will be the next item in the list of keys (probably a GUID, but possibly a Label or empty string). 

#### Examples

Retrieving an address of an AssetReference. This can be done by looking up the Location associated with that reference, and getting the PrimaryKey:

```
var op = Addressables.LoadResourceLocationsAsync(MyRef1);
yield return op;
if (op.Status == AsyncOperationStatus.Succeeded &&
	op.Result != null &&
	op.Result.Count > 0)
{
	Debug.Log("address is: " + op.Result[0].PrimaryKey);
}
```

Loading multiple assets by label, but associating each with their address. Here, again LoadResourceLocationsAsync is needed:

```
Dictionary<string, GameObject> _preloadedObjects = new Dictionary<string, GameObject>();
private IEnumerator PreloadHazards()
{
	//find all the locations with label "SpaceHazards"
	var loadResourceLocationsHandle = Addressables.LoadResourceLocationsAsync("SpaceHazards", typeof(GameObject));
	if( !loadResourceLocationsHandle.IsDone )
		yield return loadResourceLocationsHandle;
	
	//start each location loading
	List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();
	foreach (IResourceLocation location in loadResourceLocationsHandle.Result)
	{
		AsyncOperationHandle<GameObject> loadAssetHandle = Addressables.LoadAssetAsync<GameObject>(location);
		loadAssetHandle.Completed += obj => { _preloadedObjects.Add(location.PrimaryKey, obj.Result); };
		opList.Add(loadAssetHandle);
	}
	
	//create a GroupOperation to wait on all the above loads at once. 
	var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);
	if( !groupOp.IsDone )
		yield return groupOp;
	
	Addressables.Release(loadResourceLocationsHandle);

	//take a gander at our results.
	foreach (var item in _preloadedObjects)
	{
		Debug.Log(item.Key + " - " + item.Value.name);
	}
}
```

<a name="faq-build-while-compiling"></a>
### Can I build Addressables when recompiling scripts?
If you have a pre-build step that triggers a domain reload, then you must take special care that the Addressables build itself does not start until after the domain reload is finished.

Using methods such as setting scripting define symbols ([PlayerSettings.SetScriptingDefineSymbolsForGroup](https://docs.unity3d.com/ScriptReference/PlayerSettings.SetScriptingDefineSymbolsForGroup.html)) or switching active build target ([EditorUserBuildSettings.SwitchActiveBuildTarget](https://docs.unity3d.com/ScriptReference/EditorUserBuildSettings.SwitchActiveBuildTarget.html)), triggers scripts to recompile and reload. The execution of the Editor code will continue with the currently loaded domain until the domain reloads and execution stops. Any [platform dependent compilation](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html) or custom defines will not be set until after the domain reloads. This can lead to unexpected issues where code relies on these defines to build correctly, and can be easily missed.

#### Best Practice
When building via commandline arguments or CI, Unity recommends restarting the Editor for each desired platform using [command line arguments](https://docs.unity3d.com/Manual/CommandLineArguments.html). This ensures that scripts are compiled for a platform before -executeMethod is invoked.

#### Is there a safe way to change scripts before building?
To switch Platform, or modify Editor scripts in code and then continue with the defines set, a domain reload must be performed. Note in this case, -quit argument should not be used or the Editor will exit immediately after execution of the invoked method.

When the domain reloads, InitialiseOnLoad is invoked. The code below demonstrates how to set scripting define symbols and react to those in the Editor code, building Addressables after the domain reload completes. The same process can be done for switching platforms and [platform dependent compilation](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html).

```
[InitializeOnLoad]
public class BuildWithScriptingDefinesExample
{
    static BuildWithScriptingDefinesExample()
    {
        bool toBuild = SessionState.GetBool("BuildAddressables", false);
        SessionState.EraseBool("BuildAddressables");
        if (toBuild)
        {
            Debug.Log("Domain reload complete, building Addressables as requested");
            BuildAddressablesAndRevertDefines();
        }
    }

    [MenuItem("Build/Addressables with script define")]
    public static void BuildTest()
    {
#if !MYDEFINEHERE
        Debug.Log("Setting up SessionState to inform an Addressables build is requested on next Domain Reload");
        SessionState.SetBool("BuildAddressables", true);
        string originalDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string newDefines = string.IsNullOrEmpty(originalDefines) ? "MYDEFINEHERE" : originalDefines + ";MYDEFINEHERE";
        Debug.Log("Setting Scripting Defines, this will then start compiling and begin a domain reload of the Editor Scripts.");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
#endif
    }

    static void BuildAddressablesAndRevertDefines()
    {
#if MYDEFINEHERE
        Debug.Log("Correct scripting defines set for desired build");
        AddressableAssetSettings.BuildPlayerContent();
        string originalDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if (originalDefines.Contains(";MYDEFINEHERE"))
            originalDefines = originalDefines.Replace(";MYDEFINEHERE", "");
        else
            originalDefines = originalDefines.Replace("MYDEFINEHERE", "");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, originalDefines);
        AssetDatabase.SaveAssets();
#endif
        EditorApplication.Exit(0);
    }
}
``` 

<a name="faq-monoscript-changes"></a>
### What changes to scripts require rebuilding Addressables?
Classes in Addressables content are referenced using a MonoScript object. Which defines a class using the Assembly name, [Namespace], and class name or the referenced class.

When loading content at runtime the MonoScript is used to load and create and instance of the runtime class from the player assemblies.
Changes to MonoScripts need to be consistent between the Player and the built Addressables content. Both the player and Addressables content must be rebuilt in order to load the classes correctly.

The following can result in changes to the MonoScript data:
- Moving the script file to a location that comes under another [Assembly Definition File]
- Changing the name of the [Assembly Definition File] containing the class
- Adding or Changing the class [Namespace]
- Changing the class name

#### How to minimize changes to bundles
Content bundles can be large, and having to update the whole bundle for small changes can result in a large amount of data being updated for a small change to the MonoScript.
Enabling the "MonoScript Bundle Naming Prefix" option in the [Addressables settings] will build an asset bundle that contains the MonoScript objects, separate to your serialized data.
If there are no changes to the serialized class data then only the MonoScript bundle will have changed and other bundles will not need to be updated.

#### Referencing Subobjects
What gets included in a content build relies heavily on how your assets, and scripts, reference each other.  This can be tricky when subobjects get involved.  

If an `AssetReference` points to a subobject of an Asset that is Addressable, the entire object is built into the `AssetBundle` at build time.  If, instead, the `AssetReference` points to an Addressable object, such as a `GameObject`, `ScriptableObject`, or `Scene`, that in turn directly refrences a subobject, only the subobject is pulled into the `AssetBundle` as an implicit dependency.

[Addressables settings]: xref:addressables-asset-settings#build
[Assembly Definition File]: https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html
[Namespace]: https://docs.unity3d.com/Manual/Namespaces.html