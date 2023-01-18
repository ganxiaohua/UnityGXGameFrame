---
uid: addressables-api-load-asset-async
---

# Loading Addressable assets

The [Addressables] class provides several methods for loading Addressable assets. You can load assets one at a time or in batches. To identify the assets to load, you pass either a single key or a list of keys to the loading function. A key can be one of the following objects:

* Address: a string containing the address you assigned to the asset
* Label: a string containing a label assigned to one or more assets
* AssetReference object: an instance of [AssetReference]
* [IResourceLocation] instance: an intermediate object that contains information to load an asset and its dependencies. 

When you call one of the asset loading functions, the Addressables system begins an asynchronous operation that carries out the following tasks:

1. Looks up the resource locations for the specified keys (except IResourceLocation keys)
2. Gathers the list of dependencies
3. Downloads any remote AssetBundles that are required
4. Loads the AssetBundles into memory
5. Sets the [Result] object of the operation to the loaded objects
6. Updates the [Status] of the operation and calls any  [Completed] event listeners

If the load operation succeeds, the Status is set to Succeeded and the loaded object or objects can be accessed from the [Result] object.

If an error occurs, the exception is copied to the [OperationException] member of the operation object and the Status is set to Failed. By default, the exception is not thrown as part of the operation. However, you can assign a handler function to the [ResourceManager.ExceptionHandler] property to handle any exceptions. Additionally, you can enable the [Log Runtime Exceptions] option in your Addressable system settings to record errors to the Unity [Console]. 

When you call loading functions that can load multiple Addressable assets, you can specify whether the entire operation should abort if any single load operation fails or whether the operation should load any assets it can. In both cases, the operation status is set to failed. (Set the `releaseDependenciesOnFailure` parameter to true in the call to the loading function to abort the entire operation on any failure.)

See [Operations] for more information about asynchronous operations and writing asynchronous code in Unity scripts.

## Loading a single asset

Use the [LoadAssetAsync] method to load a single Addressable asset, typically with an address as the key:

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadSingle.cs#doc_Load)]

<!--
``` csharp
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadAddress : MonoBehaviour
{
  public string key;
  AsyncOperationHandle<GameObject> opHandle;

  public IEnumerator Start()
  {
      opHandle = Addressables.LoadAssetAsync<GameObject>(key);
      yield return opHandle;

      if (opHandle.Status == AsyncOperationStatus.Succeeded) {
          GameObject obj = opHandle.Result;
          Instantiate(obj, transform);
          GameObject.Destroy(gameObject, 12);
      }
  }

  void OnDestroy()
  {
      Addressables.Release(opHandle);
  }
}
```
-->

> [!NOTE]
> You can use a label or other type of key when you call [LoadAssetAsync], not just an address. However, if the key resolves to more than one asset, only the first asset found is loaded. For example, if you call this method with a label applied to several assets, Addressables returns whichever one of those assets that happens to be located first.

## Loading multiple assets

Use the [LoadAssetsAsync] method to load more than one Addressable asset in a single operation. When using this function, you can specify a single key, such as a label, or a list of keys. 

When you specify multiple keys, you can specify a [merge mode] to determine how the sets of assets matching each key are combined:

* __Union __: include assets that match any key
* __Intersection __: include assets that match every key
* __UseFirst__: include assets only from the first key that resolves to a valid location

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadMultiple.cs#doc_Load)]

<!--
```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadWithLabels : MonoBehaviour
{
  // Label strings to load
  public List<string> keys = new List<string>(){"characters", "animals"};

  // Operation handle used to load and release assets
  AsyncOperationHandle<IList<GameObject>> loadHandle;

  // Load Addressables by Label
  public IEnumerator Start() {
    float x = 0, z = 0;
    loadHandle = Addressables.LoadAssetsAsync<GameObject>(
        keys,
        addressable => {
          //Gets called for every loaded asset
          Instantiate<GameObject>(addressable, 
                      new Vector3(x++ * 2.0f, 0, z * 2.0f), 
                      Quaternion.identity, 
                      transform);

          if (x > 9) 
          {
            x = 0;
            z++;
          }
        }, Addressables.MergeMode.Union, // How to combine multiple labels 
        false); // Whether to fail and release if any asset fails to load

    yield return loadHandle;
  }

  private void OnDestroy() {
    Addressables.Release(loadHandle); // Release all the loaded assets associated with loadHandle
  }
}
```
-->

You can specify how to handle loading errors with the `releaseDependenciesOnFailure` parameter. If true, then the operation fails if it encounters an error loading any single asset. The operation and any assets that did successfully load are released.

If false, then the operation loads any objects that it can and does not release the operation. In the case of failures, the operation still completes with a status of Failed. In addition, the list of assets returned has null values where the failed assets would otherwise appear.

Set  `releaseDependenciesOnFailure` to true when loading a group of assets that must be loaded as a set in order to be used. For example, if you are loading the assets for a game level, it might make sense to fail the operation as a whole rather than load only some of the required assets.

### Correlating loaded assets to their keys

When you load multiple assets in one operation, the order in which individual assets are loaded is not necessarily the same as the order of the keys in the list you pass to the loading function.

If you need to associate an asset in a combined operation with the key used to load it, you can perform the operation in two steps:

1. Load the [IResourceLocation] instances with the list of asset keys.
2. Load the individual assets using their IResourceLocation instances as keys.

The IResourceLocation object contains the key information so you can, for example, keep a dictionary to correlate the key to an asset. Note that when you call a loading function, such as [LoadAssetsAsync], the operation first looks up the [IResourceLocation] instances that correspond to a key and then uses that to load the asset. When you load an asset using an IResourceLocation, the operation skips the first step. Thus, performing the operation in two steps does not add significant additional work.

The following example loads the assets for a list of keys and inserts them into a dictionary by their address ([PrimaryKey]). The example first loads the resource locations for the specified keys. When that operation is complete, it loads the asset for each location, using the Completed event to insert the individual operation handles into the dictionary. The operation handles can be used to instantiate the assets, and, when the assets are no longer needed, to release them.

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadLocation.cs#doc_Load)]

<!--
``` csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class LoadWithLocation : MonoBehaviour
{
    public Dictionary<string, AsyncOperationHandle<GameObject>> operationDictionary;
    public List<string> keys;
    public UnityEvent Ready;

    IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
        if(operationDictionary == null)
            operationDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

        AsyncOperationHandle<IList<IResourceLocation>> locations 
            = Addressables.LoadResourceLocationsAsync(keys, 
                Addressables.MergeMode.Union, typeof(GameObject));

        yield return locations;

        var loadOps = new List<AsyncOperationHandle>(locations.Result.Count); 

        foreach (IResourceLocation location in locations.Result) {
            AsyncOperationHandle<GameObject> handle =
                Addressables.LoadAssetAsync<GameObject>(location);
            handle.Completed += obj => operationDictionary.Add(location.PrimaryKey, obj);
            loadOps.Add(handle);
        }

        yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

        Ready.Invoke();
    }

    void Start() {
        Ready.AddListener(OnAssetsReady);
        StartCoroutine(LoadAndAssociateResultWithKey(keys));
    }

    private void OnAssetsReady() {
        float x = 0, z = 0;
        foreach (var item in operationDictionary) {
            Debug.Log($"{item.Key} = {item.Value.Result.name}");
            Instantiate(item.Value.Result,
                        new Vector3(x++ * 2.0f, 0, z * 2.0f),
                        Quaternion.identity, transform);
            if (x > 9) {
                x = 0;
                z++;
            }
        }
    }

    private void OnDestroy() {
        foreach(var item in operationDictionary) {
            Addressables.Release(item.Value);
        }
    }
}
```
-->

Note that the loading function creates a group operation with [ResourceManager.CreateGenericGroupOperation]. This allows the function to continue after all the loading operations have finished. In this case, the function dispatches a "Ready" event to notify other scripts that the loaded data can be used.

## Loading an AssetReference

The [AssetReference] class has its own load method, [LoadAssetAsync].

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadReference.cs#doc_Load)]

<!--
``` csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadFromReference : MonoBehaviour
{
  // Assign in Editor
  public AssetReference reference;

  // Start the load operation on start
  void Start() {
    AsyncOperationHandle handle = reference.LoadAssetAsync<GameObject>();
    handle.Completed += Handle_Completed;
  }

  // Instantiate the loaded prefab on complete
  private void Handle_Completed(AsyncOperationHandle obj) {
    if (obj.Status == AsyncOperationStatus.Succeeded) {
      Instantiate(reference.Asset, transform);
    } else {
      Debug.LogError("AssetReference failed to load.");
    }
    Destroy(gameobject, 12); // Destroy object to demonstrate release
  }

  // Release asset when parent object is destroyed
  private void OnDestroy() {
    reference.ReleaseAsset();
  }
}
```
-->

You can also use the AssetReference object as a key to the [Addressables.LoadAssetAsync] methods. If you need to spawn multiple instances of the asset assigned to an AssetReference, use [Addressables.LoadAssetAsync], which gives you an operation handle that you can use to release each instance.  

See [AssetReference] for more information about using AssetReferences.

## Loading Scenes

Use the [Addressables.LoadSceneAsync] method to load an Addressable Scene asset by address or other Addressable key object. 

The remaining parameters of the method correspond to those used with the Unity Engine [SceneManager.LoadSceneAsync] method:

* __loadMode__: whether to add the loaded Scene into the current Scene or to unload and replace the current Scene. 
* __activateOnLoad__: whether to activate the scene as soon as it finishes loading or to wait until you call the SceneInstance object's [ActivateAsync] method. Corresponds to the [AsyncOperation.allowSceneActivation] option. Defaults to true.
* __priority__: the priority of the AsyncOperation used to load the Scene. Corresponds to the [AsyncOperation.priority] option. Defaults to 100.

> [!WARNING]
> Setting the `activateOnLoad` parameter to false blocks the AsyncOperation queue, including the loading of any other Addressable assets, until you activate the scene. To activate the scene, call the [ActivateAsync] method of the [SceneInstance] returned by [LoadSceneAsync]. See [AsyncOperation.allowSceneActivation] for additional information.

The following example loads a scene additively. The Component that loads the Scene, stores the operation handle and uses it to unload and release the Scene when the parent GameObject is destroyed.

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadScene.cs#doc_Load)]

<!--
``` csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LoadSceneByAddress : MonoBehaviour
{
    public string key;
    private AsyncOperationHandle<SceneInstance> loadHandle;

    void Start()
    {
      loadHandle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive);
      Destroy(this, 12); // Trigger unload to release Scene
    }

  void OnDestroy() {
    Addressables.UnloadSceneAsync(loadHandle);
  }
}
```
-->

See the [Scene loading project] in the [Addressables-Sample] repository for additional examples.

If you load a Scene with [LoadSceneMode.Single], the Unity runtime unloads the current Scene and calls [Resources.UnloadUnusedAssets]. The unloaded Scene is released, which allows its AssetBundle to be unloaded. Individual Addressables and their operation handles that you loaded separately are not released; you must release them yourself. (The exception to this is that any Addressable assets that you instantiated using [Addressables.InstantiateAsync] with `trackHandle` set to true, the default, are automatically released.) 

> [!NOTE]
> In the Editor, you can always load scenes in the current project, even when they are packaged in a remote bundle that is not available and you set the Play Mode Script to __Use Existing Build__. The Editor loads the Scene using the asset database.

## Loading assets by location

When you load an Addressable asset by address, label, or AssetReference, the Addressables system first looks up the resource locations for the assets and uses these [IResourceLocation] instances to download the required AssetBundles and any dependencies. You can perform the asset load operation in two steps by first getting the IResourceLocation objects with [LoadResourceLocationsAsync] and then using those objects as keys to load or instantiate the assets.

[IResourceLocation] objects contain the information needed to load one or more assets. 

The [LoadResourceLocationsAsync] method never fails. If it cannot resolve the specified keys to the locations of any assets, it returns an empty list. You can restrict the types of asset locations returned by the function by specifying a specific type in the `type` parameter.

The following example loads locations for all assets labeled with "knight" or "villager":

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadLocation.cs#doc_LoadLocations)]

<!--
```csharp
AsyncOperationHandle<IList<IResourceLocation>> handle 
    = Addressables.LoadResourceLocationsAsync(new string[]
    {
        "knight",
        "villager"
    }, Addressables.MergeMode.Union);

    yield return handle;

    //...

    Addressables.Release(handle);
```
-->

## Loading locations of subobjects 

Locations for SubObjects are generated at runtime to reduce the size of the content catalogs and improve runtime performance. When you call [LoadResourceLocationsAsync] with the key of an asset with subobjects and don't specify a type, then the function generates IResourceLocation instances for all of the subobjects as well as the main object (if applicable). Likewise, if you do not specify which subobject to use for an AssetReference that points to an asset with subobjects, then the system generates IResourceLocations for every subobject.

For example, if you load the locations for an FBX asset, with the address, "myFBXObject", you might get locations for three assets: a GameObject, a Mesh, and a Material. If, instead, you specified the type in the address, "myFBXObject[Mesh]", you would only get the Mesh object. You can also specify the type using the `type` parameter of the LoadResourceLocationsAsync function.

 
<a name="instantiate"></a>
## Instantiating objects from Addressables

You can load an asset, such as a Prefab, and then create an instance of it with [Instantiate]. You can also load and create an instance of an asset with [Addressables.InstantiateAsync]. The primary difference between these two ways of instantiating objects is how the asset reference counts are affected.

When you use InstantiateAsync, the reference counts of the loaded assets are incremented each time you call the method. Thus if you instantiate a Prefab five times, the reference count for the Prefab asset and any of its dependencies are incremented by five. You can then release each instance separately as they are destroyed in the game.

When you use LoadAssetAsync and Object.Instantiate, then the asset reference counts are only incremented once, for the initial load. If you release the loaded asset (or its operation handle) and the reference count drops to zero, then the asset is unloaded and all the additional instantiated copies lose their subassets as well -- they still exist as GameObjects in the scene, but without Meshes, Materials, or other assets that they depend on.

Which scenario is better, depends on how you organize your object code. For example, if you have a single manager object that supplies a pool of Prefab enemies to spawn into a game level, it might be most convenient to release them all at the completion of the level with a single operation handle stored in the manager class. In other situations, you might want to instantiate and release assets individually.  

The following example calls [InstantiateAsync] to instantiate a Prefab. The example adds a component to the instantiated GameObject that releases the asset when the GameObject is destroyed.

[!code-cs[sample](../Tests/Editor/DocExampleCode/InstantiateAsset.cs#doc_Instantiate)]

<!--
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InstantiateFromKey : MonoBehaviour
{
  public string key; // Identify the asset

  void Start() {
    // Load and instantiate
    Addressables.InstantiateAsync(key).Completed += instantiate_Completed;
  }

  private void instantiate_Completed(AsyncOperationHandle<GameObject> obj) {
    // Add component to release asset in GameObject OnDestroy event
    obj.Result.AddComponent(typeof(SelfCleanup));
    Destroy(obj.Result, 12); // Destroy to trigger release
  }
}

// Releases asset (trackHandle must be true in InstantiateAsync)
public class SelfCleanup : MonoBehaviour
{
  void OnDestroy() {
    Addressables.ReleaseInstance(gameObject);
  }
}
```
-->

When you call [InstantiateAsync] you have the same options as the [Object.Instantiate] method, and also the following additional parameters:

* __instantiationParameters__: this parameter takes a [InstantiationParameters] struct that you can use to specify the instantiation options instead of specifying them in every call to the InstantiateAsync call. This can be convenient if you use the same values for multiple instantiations.
* __trackHandle__:  If true, which is the default, then the Addressables system keeps track of the operation handle for the instantiated instance. This allows you to release the asset with the [Addressables.ReleaseInstance] method. If false, then the operation handle is not tracked for you and you must store a reference to the handle returned by InstantiateAsync in order to release the instance when you destroy it.

## Asynchronous Loading

The Addressables system API is asynchronous and returns an [AsyncOperationHandle] for use with managing operation progress and completion.
Addressables is designed to content location agnostic. The content may need to be downloaded first or use other methods that can take a long time. To force synchronous execution, See [Synchronous Addressables] for more information.

When loading an asset for the first time, the handle is done after a minimum of one frame. You can wait until the load has completed using different methods as shown below.
If the content has already been loaded, execution times may differ between the various asynchronous loading options shown below.
* [Coroutine]: Always be delayed at minimum of one frame before execution continues.
* [Completed callback]: Is a minimum of one frame if the content has not already been loaded, otherwise the callback is invoked in the same frame.
* Awaiting [AsyncOperationHandle.Task]: Is a minimum of one frame if the content has not already been loaded, otherwise the execution continues in the same frame.

[!code-cs[sample](../Tests/Editor/DocExampleCode/AsynchronousLoading.cs#doc_asyncload)]

## Releasing Addressable assets

Because the Addressables system uses reference counting to determine whether an asset is in use, you must release every asset that you load or instantiate when you are done with it. See [Memory Management] for more information.

When you unload a Scene, implicit assets in the Scene are not automatically unloaded. You must call [Resources.UnloadUnusedAssets] or [UnloadAsset] to free these assets. Note that the Unity runtime automatically calls `UnloadUnusedAssets` when you load a Scene using the [LoadSceneMode.Single] mode.

## Using Addressables in a Scene

If a Scene is itself Addressable, you can use Addressable assets in the scene just as you would any assets. You can place Prefabs and other assets in the Scene, assign assets to component properties, and so on. If you use an asset that is not Addressable, that asset becomes an implicit dependency of the Scene and the build system packs it in the same AssetBundle as the Scene when you make a content build. (Addressable assets are packed into their own AssetBundles according to the group they are in.)  

> [!NOTE]
> Implicit dependencies used in more than one place can be duplicated in multiple AssetBundles and in the built-in scene data. Use the [Check Duplicate Bundle Dependencies] rule in the Analyze tool to find unwanted duplication of assets.

If a Scene is NOT Addressable, then any Addressable assets you add directly to the scene hierarchy become implicit dependencies and Unity includes copies of those assets in the built-in scene data even if they also exist in an Addressable group. The same is true for any assets, such as Materials, assigned to a component on a GameObject in the scene. 

In custom component classes, you can use [AssetReference] fields to allow the assignment of Addressable assets in non-Addressable scenes. Otherwise, you can use [addresses] and [labels] to load assets at runtime from a script. Note that you must load an AssetReference in code whether or not the Scene is Addressable. 



[ActivateAsync]: xref:UnityEngine.ResourceManagement.ResourceProviders.SceneInstance.ActivateAsync*
[Addressables.ClearDependencyCacheAsync]: xref:UnityEngine.AddressableAssets.Addressables.ClearDependencyCacheAsync*
[Addressables.DownloadDependenciesAsync]: xref:UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync*
[Addressables.GetDownloadSizeAsync]: xref:UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync*
[Addressables.InstantiateAsync]: xref:UnityEngine.AddressableAssets.Addressables.InstantiateAsync*
[Addressables.LoadAssetAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetAsync*
[Addressables.LoadSceneAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadSceneAsync*
[Addressables.ReleaseInstance]: xref:UnityEngine.AddressableAssets.Addressables.ReleaseInstance*
[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[AssetReference]: xref:UnityEngine.AddressableAssets.AssetReference
[AssetReferences]: xref:addressables-asset-references
[AsyncOperation.priority]: xref:UnityEngine.AsyncOperation.priority
[cache settings]: xref:UnityEngine.Cache
[Check Duplicate Bundle Dependencies]: AnalyzeTool.md#check-duplicate-bundle-dependencies
[GetDownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.GetDownloadStatus*
[Instantiate]: xref:UnityEngine.Object.Instantiate*
[InstantiateAsync]: xref:UnityEngine.AddressableAssets.Addressables.InstantiateAsync*
[InstantiationParameters]: xref:UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters
[IResourceLocation]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation
[AsyncOperationHandle]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle-1
[AsyncOperationHandle.Task]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle-1.Task.html
[Completed callback]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle-1.Completed.html
[Coroutine]: xref:UnityEngine.Coroutine*
[LoadAssetAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetAsync*
[LoadAssetsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync*
[LoadResourceLocationsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync*
[LoadSceneMode.Single]: xref:UnityEngine.SceneManagement.LoadSceneMode.Single
[Memory Management]: xref:addressables-memory-management
[merge mode]: xref:UnityEngine.AddressableAssets.Addressables.MergeMode
[OperationException]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.OperationException
[Operations]: xref:addressables-async-operation-handling
[PrimaryKey]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation.PrimaryKey
[ResourceManager.CreateGenericGroupOperation]: xref:UnityEngine.ResourceManagement.ResourceManager.CreateGenericGroupOperation*
[Resources.UnloadUnusedAssets]: xref:UnityEngine.Resources.UnloadUnusedAssets
[Result]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Result
[SceneManager.LoadSceneAsync]: xref:UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(System.String,UnityEngine.SceneManagement.LoadSceneMode)
[Status]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Status
[UnityEngine.Caching]: xref:UnityEngine.Caching
[ResourceManager.ExceptionHandler]: xref:UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler
[Log Runtime Exceptions]: xref:addressables-asset-settings#diagnostics
[Console]: xref:Console
[Object.Instantiate]: xref:UnityEngine.Object.Instantiate*
[addresses]: xref:addressables-overview#asset-addresses
[labels]: xref:addressables-labels
[Completed]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Completed
[AsyncOperation.allowSceneActivation]: xref:UnityEngine.AsyncOperation.allowSceneActivation
[SceneInstance]: xref:UnityEngine.ResourceManagement.ResourceProviders.SceneInstance
[LoadSceneAsync]: xref:UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(System.String,UnityEngine.SceneManagement.LoadSceneMode)
[UnloadAsset]: xref:UnityEngine.Resources.UnloadAsset(UnityEngine.Object)
[Addressables.InstantiateAsync]: xref:UnityEngine.AddressableAssets.Addressables.InstantiateAsync*
[Scene loading project]: https://github.com/Unity-Technologies/Addressables-Sample/tree/master/Basic/Scene%20Loading
[Addressables-Sample]: https://github.com/Unity-Technologies/Addressables-Sample
[Synchronous Addressables]: xref:synchronous-addressables
