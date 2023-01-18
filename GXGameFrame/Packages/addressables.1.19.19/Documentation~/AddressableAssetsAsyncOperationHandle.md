---
uid: addressables-async-operation-handling
---

# Operations

Many tasks in the Addressables need to load or download information before they can return a result. To avoid blocking program execution, Addressables implements such tasks as asynchronous operations.

In contrast to a synchronous operation, which doesn’t return control until the result is available, an asynchronous operation returns control to the calling function almost immediately. However, the results may not be available until some time in the future. When you call a function, such as [LoadAssetAsync], it doesn't return the loaded assets directly. Instead, it returns an [AsyncOperationHandle] object, which you can use to access the loaded assets when they become available.

You can use the following techniques to wait for the results of an asynchronous operation (while allowing other scripts to continue processing).

* [Coroutines and IEnumerator loops]
* [Events]
* [Tasks]

> [!NOTE]
> You can block the current thread to wait for the completion of an asynchronous operation. Doing so can introduce performance problems and frame rate hitches. See [Using operations synchronously].

## Releasing AsyncOperationHandle instances

Methods, like  [LoadAssetsAsync], return [AsyncOperationHandle] instances that both provide the results of the operation and a way to release both the results and the operation object itself. You must retain the handle object for as long as you want to use the results. Depending on the situation, that might be one frame, until the end of a level, or even the lifetime of the application. Use the [Addressables.Release] function to release operation handles and any associated addressable assets.

Releasing an operation handle decrements the reference count of any assets loaded by the operation and invalidates the operation handle object itself. See [Memory management] for more information about reference counting in the Addressables system.

In cases where you don’t need to use the results of an operation beyond a limited scope, you can release the handles right away. A few Addressables methods, such as [UnloadSceneAsync] allow you to automatically release the operation handle when it's complete.

If an operation is unsuccessful, you should still release the operation handle. Normally, Addressables releases any assets that it loaded during a failed operation, but releasing the handle still clears the handle’s instance data. Note that some functions, like [LoadAssetsAsync], which load multiple assets, give you the option to either retain any assets that it could load or to fail and release everything if any part of the load operation failed.

<a name="coroutine-operation-handling"></a>
## Coroutine- and IEnumerator-based operation handling

The [AsyncOperationHandle] implements the [IEnumerator] interface and will continue iteration until the operation is complete. In a coroutine, you can yield the operation handle to wait for the next iteration. When complete, the execution flow continues to the following statements. Recall that you can implement the [MonoBehaviour Start] function as a coroutine, which is a good way to have a GameObject load and instantiate the assets it needs.

The following script loads a Prefab as a child of its GameObject using a Start function coroutine. It yields the AsyncOperationHandle until the operation finishes and then uses the same handle to instantiate the Prefab.

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadWithIEnumerator.cs#doc_LoadWithIEnumerator)]

<!--
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadWithIEnumerator : MonoBehaviour
{
  public string address;
  AsyncOperationHandle<GameObject> opHandle;

  public IEnumerator Start()
  {
      opHandle = Addressables.LoadAssetAsync<GameObject>(address);

      // yielding when already done still waits until the next frame
      // so don't yield if done.
      if (!opHandle.IsDone)
        yield return opHandle;

      if (opHandle.Status == AsyncOperationStatus.Succeeded) {
        Instantiate(opHandle.Result, transform);
      } else {
          Addressables.Release(opHandle);
      }
  }

  void OnDestroy()
  {
    Addressables.Release(opHandle);
  }
}
```
-->
Note that [Addressables.LoadAssetsAsync] is not able to be canceled once started. However, releasing the handle before it has finished will decrement the handle reference count and it will automatically release when the load is complete.

See [Coroutines] for more information.

### Grouping operations in a coroutine

You will probably encounter situations in which you want to perform several operations before moving on to the next step in your game logic. For example, you want to load a number of Prefabs and other assets before you start a level.

If the operations all load assets, you can combine them with a single call to the [Addressables.LoadAssetsAsync] function. The AsyncOperationhandle for this method works the same as [LoadAssetAsync]; you can yield the handle in a coroutine to wait until all the assets in the operation load. In addition, you can pass a callback function to LoadAssetsAsync and the operation calls that function when it finishes loading a specific asset. See [Loading multiple assets] for an example.

Another option is to use the [ResourceManager.CreateGenericGroupOperation] to create a group operation that completes when all of its members finish.

## Event-based operation handling

You can add a delegate function to the [Completed] event of an [AsyncOperationHandle]. The operation calls the delegate function when it's finished.

The following script performs the same function as the example in [Coroutine- and IEnumerator-based operation handling], but uses an event delegate instead of a coroutine.

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadWithEvent.cs#doc_LoadWithEvent)]

<!--
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadWithEvent : MonoBehaviour
{
    public string address;
    AsyncOperationHandle<GameObject> opHandle;

    void Start() {
        opHandle = Addressables.LoadAssetAsync<GameObject>(address);
        opHandle.Completed += Operation_Completed;
    }

    private void Operation_Completed(AsyncOperationHandle<GameObject> obj) {

        if (obj.Status == AsyncOperationStatus.Succeeded) {
            Instantiate(obj.Result, transform);
            GameObject.Destroy(gameObject, 5);
        } else {
            Addressables.Release(obj);
        }
    }

    void OnDestroy() {
        Addressables.Release(opHandle);
    }
}
```
-->

Note that the handle instance passed to the event delegate is the same as that returned by the original function call. You can use either to access the results and status of the operation and, ultimately, to release the operation handle and loaded assets.

## Task-based operation handling

The [AsyncOperationHandle] provides a [Task] object that you can use with the C# [async] and [await] keywords to sequence code that calls asynchronous functions and handles the results.

The following example loads Addressable assets using a list of keys. The differences between this task-based approach and the coroutine or event-based approaches are in the signature of the calling function, which must include the [async] keyword and the use of the [await] keyword with the operation handle’s Task property. The calling function, Start() in this case, suspends operation while the task finishes. Execution then resumes and the example instantiates all the loaded Prefabs (in a grid pattern).

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadWithTask.cs#doc_LoadWithTask)]

<!--
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadWithTask : MonoBehaviour
{
    // Label or address strings to load
    public List<string> keys = new List<string>() { "characters", "animals" };
    public float lifespan = 12;

    // Operation handle used to load and release assets
    AsyncOperationHandle<IList<GameObject>> loadHandle;

    public async void Start(){
        loadHandle = Addressables.LoadAssetsAsync<GameObject>(
            keys, // Either a single key or a List of keys
            addressable => {
                // Called for every loaded asset
                Debug.Log(addressable.name);
            }, Addressables.MergeMode.Union, // How to combine multiple labels
            false); // Whether to fail if any asset fails to load

        // Wait for the operation to finish in the background
        await loadHandle.Task;

        // Instantiate the results
        float x = 0, z = 0;
        foreach (var addressable in loadHandle.Result){
            if (addressable != null) {
                Instantiate<GameObject>(addressable,
                        new Vector3(x++ * 2.0f, 0, z * 2.0f),
                        Quaternion.identity,
                        transform);

                if (x > 9) {
                    x = 0;
                    z++;
                }
            }
        }
    }

    private void OnDestroy() {
        Addressables.Release(loadHandle); // Release all the loaded assets associated with loadHandle
    }
}
```
-->

> [!IMPORTANT]
> The [AsyncOperationHandle.Task] property is not available on the Unity WebGL platform, which doesn't support multitasking.

When you use Task-based operation handling, you can use the C# [Task] class methods such as [WhenAll] to control which operations you run in parallel and which you want to run in sequence. The following example illustrates how to wait for more than one operation to finish before moving onto the next task:

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadWithTask.cs#doc_useWhenAll)]

<!--
```csharp
        // Load the Prefabs
        var prefabOpHandle = Addressables.LoadAssetsAsync<GameObject>(
            keys, null, Addressables.MergeMode.Union, false);

        // Load a Scene additively
        var sceneOpHandle = Addressables.LoadSceneAsync(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);

        await Task.WhenAll(prefabOpHandle.Task, sceneOpHandle.Task);
```
-->

## Using operations synchronously

You can wait for an operation to finish without yielding, waiting for an event, or using `async await` by calling an operation’s [WaitForCompletion] method. This method blocks the current program execution thread while it waits for the operation to finish before continuing in the current scope.

Avoid calling WaitForCompletion on operations that can take a significant amount of time, such as those that must download data. Calling WaitForCompletion can cause frame hitches and interrupt UI responsiveness.

In Unity 2020.1 or earlier, Unity also waits for all other pending asynchronous operations to finish, so the delay in execution can be much longer than that required for just the single operation for which you call this method. In Unity 2020.2 or later, the performance impact can be less pronounced, at least when loading assets that have already been downloaded.

The following example loads a Prefab asset by address, waits for the operation to complete, and then instantiates the Prefab:

[!code-cs[sample](../Tests/Editor/DocExampleCode/LoadSynchronously.cs#doc_LoadSynchronously)]

<!--
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadSynchronously : MonoBehaviour
{
    public string address;
    AsyncOperationHandle<GameObject> opHandle;

    void Start() {
        opHandle = Addressables.LoadAssetAsync<GameObject>(address);
        opHandle.WaitForCompletion();

        if (opHandle.Status == AsyncOperationStatus.Succeeded) {
            Instantiate(opHandle.Result, transform);
        } else {
            Addressables.Release(opHandle);
        }
    }

    void OnDestroy() {
        Addressables.Release(opHandle);
    }
}
```
-->

## Custom operations

To create a custom operation, extend the [AsyncOperationBase] class and override its virtual methods.

You can pass the derived operation to the [ResourceManager.StartOperation] method to start the operation and receive an [AsyncOperationHandle] struct. The [ResourceManager] registers operations started this way and shows them in the Addressables [Event Viewer].

### Executing the operation

The [ResourceManager] invokes the [AsyncOperationBase.Execute] method for your custom operation once the optional dependent operation completes.

### Completion handling

When your custom operation completes, call [AsyncOperationBase.Complete] on your custom operation object. You can call this within the [Execute] method or defer it to outside the call. AsyncOperationBase.Complete notifies the [ResourceManager] that the operation has finished. The ResourceManager invokes the associated [AsyncOperationHandle.Completed] events for the relevant instances of the custom operation.

### Terminating the operation

The [ResourceManager] invokes the [AsyncOperationBase.Destroy] method for your custom operation when you release the [AsyncOperationHandle] that references it. This is where you should release any memory or resources associated with your custom operation.

<a name="using-typed-versus-typeless-operation-handles"></a>
## Using typed versus typeless operation handles

Most [Addressables] methods that start an operation return a generic [AsyncOperationHandle\<T\>] struct, allowing type safety for the [AsyncOperationHandle.Completed] event and for the [AsyncOperationHandle.Result] object. You can also use a non-generic [AsyncOperationHandle] struct and convert between the two handle types as desired.

Note that a runtime exception occurs if you attempt to cast a non-generic handle to a generic handle of an incorrect type. For example:

[!code-cs[sample](../Tests/Editor/DocExampleCode/OperationHandleTypes.cs#doc_ConvertTypes)]

<!--
```csharp
AsyncOperationHandle<Texture2D> textureHandle = Addressables.LoadAssetAsync<Texture2D>("mytexture");

// Convert the AsyncOperationHandle<Texture2D> to an AsyncOperationHandle:
AsyncOperationHandle nonGenericHandle = textureHandle;

// Convert the AsyncOperationHandle to an AsyncOperationHandle<Texture2D>:
AsyncOperationHandle<Texture2D> textureHandle2 = nonGenericHandle.Convert<Texture2D>();

// This will throw and exception because Texture2D is required:
AsyncOperationHandle<Texture> textureHandle3 = nonGenericHandle.Convert<Texture>();
```
-->

## Reporting operation progress

[AsyncOperationHandle] has two methods that you can use to monitor and report the progress of the operation:

* [GetDownloadStatus] returns a [DownloadStatus] struct. This struct contains information about how many bytes have been downloaded and how many bytes still need to be downloaded. The [DownloadStatus.Percent] reports the percentage of bytes downloaded.
* [AsyncOperationHandle.PercentComplete] returns an equally-weighted aggregate percentage of all the sub-operations that are complete. For example, if an operation has five sub-operations, each of them represents 20% of the total. The value doesn't factor in the amount of data that must be downloaded by the individual sub-operations.

For example, if you called [Addressables.DownloadDependenciesAsync] and five AssetBundles needed to be downloaded, GetDownloadStatus would tell you what percentage of the total number of bytes for all sub-operations had been downloaded so far. PercentComplete would tell you what percentage of the number of operations had finished, regardless of their size.

On the other hand, if you called [LoadAssetAsync], and one bundle had to be downloaded before an asset could be loaded from it, the download percentage might be misleading. The values obtained from GetDownloadStatus would reach 100% before the operation finished, because the operation had additional sub-operations to conduct. The value of PercentComplete would be 50% when the download sub-operation finished and 100% when the actual load into memory was complete.

[Addressables.DownloadDependenciesAsync]: xref:UnityEngine.AddressableAssets.Addressables.DownloadDependenciesAsync*
[Addressables.LoadAssetsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync*
[Addressables.Release]: xref:UnityEngine.AddressableAssets.Addressables.Release*
[Addressables]: xref:UnityEngine.AddressableAssets.Addressables
[async]: xref:langword_csharp_async
[AsyncOperationHandles]:   xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle
[AsyncOperationBase.Complete]:    xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1.Complete*
[AsyncOperationBase.Destroy]:     xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1.Destroy*
[AsyncOperationBase.Execute]:     xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1.Execute*
[AsyncOperationBase]:             xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1
[AsyncOperationHandle.Completed]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Completed
[AsyncOperationHandle.PercentComplete]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.PercentComplete
[AsyncOperationHandle.Result]:    xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Result
[AsyncOperationHandle.Task]:      xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Task
[AsyncOperationHandle]:           xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle
[AsyncOperationHandle\<T\>]:        xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle`1
[await]: xref:langword_csharp_await
[Completed]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Completed
[Coroutine- and IEnumerator-based operation handling]: #coroutine-operation-handling
[Coroutines]: xref:Coroutines
[DownloadStatus.Percent]: xref:UnityEngine.ResourceManagement.AsyncOperations.DownloadStatus.Percent
[DownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.DownloadStatus
[Event Viewer]: xref:addressables-event-viewer
[Execute]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationBase`1.Execute*
[GetDownloadStatus]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.GetDownloadStatus*
[IEnumerator]: xref:System.Collections.IEnumerator
[LoadAssetAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetAsync*
[LoadAssetsAsync]: xref:UnityEngine.AddressableAssets.Addressables.LoadAssetsAsync*
[Loading multiple assets]: xref:addressables-api-load-asset-async#loading-multiple-assets
[Memory management]: xref:addressables-memory-management
[MonoBehaviour Start]: https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
[ResourceManager.CreateGenericGroupOperation]: xref:UnityEngine.ResourceManagement.ResourceManager.CreateGenericGroupOperation*
[ResourceManager.StartOperation]: xref:UnityEngine.ResourceManagement.ResourceManager.StartOperation*
[ResourceManager]: xref:UnityEngine.ResourceManagement.ResourceManager
[Task]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.Task
[UnloadSceneAsync]: xref:UnityEngine.AddressableAssets.Addressables.UnloadSceneAsync*
[WaitForCompletion]: xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.WaitForCompletion*
[WhenAll]: https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall
[Coroutines and IEnumerator loops]: #coroutine-operation-handling
[Events]: #event-based-operation-handling
[Tasks]: #task-based-operation-handling
[Using operations synchronously]: #using-operations-synchronously
