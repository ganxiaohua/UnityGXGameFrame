---
uid: addressables-api-download-dependencies-async
---

# Preloading dependencies

When you distribute content remotely, you can sometimes improve perceived performance by downloading dependencies in advance of when your application needs them. For example, you can download essential content on start up the first time a player launches your game to make sure that they don't have to wait for content in the middle of game play. 

<a name="download-dependencies"></a>
## Downloading dependencies

Use the [Addressables.DownloadDependenciesAsync] method to make sure that all the dependencies needed to load an Addressable key are available either in local content installed with the app or the download cache.

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_Download)]

> [!TIP]
> if you have a set of assets that you want to pre-download, you can assign the same label, such as "preload", to the assets and use that label as the key when calling  [Addressables.DownloadDependenciesAsync]. Addressables downloads all the AssetBundles containing an asset with that label if not already available (along with any bundles containing the assets' dependencies).

## Progress

An [AsyncOperationHandle] instance provides two ways to get progress:

* [AsyncOperationHandle.PercentComplete]\: reports the percentage of sub-operations that have finished. For example, if an operation uses six sub-operations to perform its task, the `PercentComplete` indicates the entire operation is 50% complete when three of those operations have finished (it doesn't matter how much data each operation loads).
* [AsyncOperationHandle.GetDownloadStatus]: returns a [DownloadStatus] struct that reports the percentage in terms of total download size. For example, if an operation has six sub-operations, but the first operation represented 50% of the total download size, then `GetDownloadStatus` indicates the operation is 50% complete when the first operation finishes.

The following example illustrates how you could use [GetDownloadStatus] to check the status and dispatch progress events during the download:

[!code-cs[sample](../Tests/Editor/DocExampleCode/PreloadWithProgress.cs#doc_Preload)]

<!--
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PreloadWithProgress : MonoBehaviour
{
    public string preloadLabel = "preload";
    public UnityEvent<float> ProgressEvent;
    public UnityEvent<bool> CompletionEvent;
    private AsyncOperationHandle downloadHandle;

    IEnumerator Start() {
        downloadHandle =  Addressables.DownloadDependenciesAsync(preloadLabel, false);
        float progress = 0;

        while (downloadHandle.Status == AsyncOperationStatus.None) {
            float percentageComplete = downloadHandle.GetDownloadStatus().Percent;
            if (percentageComplete > progress * 1.1) // Report at most every 10% or so
            {
                progress = percentageComplete; // More accurate %
                ProgressEvent.Invoke(progress);
            }
            yield return null;
        }

        CompletionEvent.Invoke(downloadHandle.Status == AsyncOperationStatus.Succeeded);
        Addressables.Release(downloadHandle); //Release the operation handle
    }
}
```
-->

To discover how much data you need to download in order to load one or more assets, you can call [Addressables.GetDownloadSizeAsync]: 

[!code-cs[sample](../Tests/Editor/DocExampleCode/PreloadWithProgress.cs#doc_DownloadSize)]

<!--
```csharp
AsyncOperationHandle<long> getDownloadSize =  
    Addressables.GetDownloadSizeAsync(key);
```
-->

The Result of the completed operation is the number of bytes that must be downloaded. If Addressables has already cached all the required AssetBundles, then Result is zero.

Always release the download operation handle after you have read the Result object. If you don't need to access the results of the download operation, you can automatically release the handle by setting the `autoReleaseHandle` parameter to true, as shown in the following example:

[!code-cs[sample](../Tests/Editor/DocExampleCode/Preload.cs#doc_Preload)]

<!--
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Preload : MonoBehaviour
{
    public IEnumerator Start()
    {
        yield return Addressables.DownloadDependenciesAsync("preload", true);
    }
}
```
-->

### Clearing the dependency cache

If you want to clear any AssetBundles cached by Addressables, call [Addressables.ClearDependencyCacheAsync]. This function clears the cached AssetBundles containing the assets identified by a key along with any bundles containing those assets' dependencies. 

Note that ClearDependencyCacheAsync only clears assets bundles related to the specified key. If you updated the content catalog such that the key no longer exists or it no longer depends on the same AssetBundles, then these no-longer-referenced bundles remain in the cache until they expire (based on [cache settings]).

To clear all AssetBundles, you can use functions in the [UnityEngine.Caching] class.

[Addressables.ClearDependencyCacheAsync]: xref:UnityEngine.AddressableAssets.Addressables.ClearDependencyCacheAsync*
[Addressables.DownloadDependenciesAsync]: xref:UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync*
[Addressables.GetDownloadSizeAsync]: xref:UnityEngine.AddressableAssets.Addressables.GetDownloadSizeAsync*
[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[AssetReference]: xref:UnityEngine.AddressableAssets.AssetReference
[AssetReferences]: xref:addressables-asset-references
[AsyncOperation.priority]: xref:UnityEngine.AsyncOperation.priority
[cache settings]: xref:UnityEngine.Cache
[Check Duplicate Bundle Dependencies]: AnalyzeTool.md#check-duplicate-bundle-dependencies
[GetDownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.GetDownloadStatus*
[IResourceLocation]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation
[LoadAssetAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetAsync*
[LoadAssetsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync*
[LoadResourceLocationsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync*
[LoadSceneMode.Single]: xref:UnityEngine.SceneManagement.LoadSceneMode.Single
[UnityEngine.Caching]: xref:UnityEngine.Caching
[ResourceManager.ExceptionHandler]: xref:UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler
[Log Runtime Exceptions]: xref:addressables-asset-settings#diagnostics
[Console]: xref:Console
[Object.Instantiate]: xref:UnityEngine.Instantiate
[Completed]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Completed
[AsyncOperation.allowSceneActivation]: xref:UnityEngine.AsyncOperation.allowSceneActivation
[SceneInstance]: xref:UnityEngine.ResourceManagement.ResourceProviders.SceneInstance
[LoadSceneAsync]: xref:UnityEngine.SceneManagement.SceneManager.LoadSceneAsync
[Resources.UnloadAllAssets]: xref:UnityEngine.Resources.UnloadAllAssets
[UnloadAsset]: xref:UnityEngine.Resources.UnloadAsset
[Addressables.InstantiateAsync]: xref:UnityEngine.AddressableAssets.Addressables.InstantiateAsync*
[AsyncOperationHandle]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle
[AsyncOperationHandle.GetDownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.GetDownloadStatus
[AsyncOperationHandle.PercentComplete]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.PercentComplete
[DownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.DownloadStatus
[GetDownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.GetDownloadStatus