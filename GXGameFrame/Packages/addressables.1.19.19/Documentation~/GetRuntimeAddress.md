---
uid: addressables-get-address
---

# Getting addresses at runtime

By default, Addressables uses the address you assign to an asset as the [PrimaryKey] value of its [IResourceLocation] instance. (If you disable the __[Include Addresses in Catalog]__ option of the Addressables group to which the asset belongs, the PrimaryKey could be a GUID, label, or an empty string.) If you want to get the address of an asset that you load with an AssetReference or label, you can first load the asset's locations, as described in [Loading Assets by Location]. You can then use the IResourceLocation instance to both access the PrimaryKey value and to load the asset.

The following example gets the address of the asset assigned to an [AssetReference] object named `MyRef1`:

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_AddressFromReference)]

<!--
```csharp
var opHandle = Addressables.LoadResourceLocationsAsync(MyRef1);
yield return opHandle;

if (op.Status == AsyncOperationStatus.Succeeded &&
    opHandle.Result != null &&
    opHandle.Result.Count > 0)
{
    Debug.Log("address is: " + opHandle.Result[0].PrimaryKey);
}
```
 -->

Labels often refer to multiple assets. The following example illustrates how to load multiple Prefab assets and use their primary key value to add them to a dictionary:

[!code-cs[sample](../Tests/Editor/DocExampleCode/MiscellaneousTopics.cs#doc_PreloadHazards)]

<!--
```csharp
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
 -->
 
[Include Addresses in Catalog]: xref:addressables-group-settings#advanced-options
[IResourceLocation]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation
[Loading Assets by Location]: xref:addressables-api-load-asset-async#loading-assets-by-location
[PrimaryKey]: xref:UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation.PrimaryKey
[AssetReference]: xref:UnityEngine.AddressableAssets.AssetReference
