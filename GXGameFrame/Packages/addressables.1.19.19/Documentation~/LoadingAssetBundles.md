---
uid: addressables-loading-bundles
---

# AssetBundle Loading

The Addressables system packs your assets in AssetBundles and loads these bundles "behind the scenes" as you load individual assets. You can control how AssetBundles load which are exposed on the `BundledAssetGroupSchema` class. You can set these options through the scripting API or under the Advanced options in the inspector of the `AddressablesAssetGroup` inspector.

## UnityWebRequestForLocalBundles

Addressables can load AssetBundles via two engine APIs: `UnityWebRequest.GetAssetBundle`, and `AssetBundle.LoadFromFileAsync`. The default behavior is to use `AssetBundle.LoadFromFileAsync` when the AssetBundle is in local storage and use `UnityWebRequest` when the AssetBundle path is a URL.

You can override this behavior to use `UnityWebRequest` for local Asset Bundles by setting `BundledAssetGroupSchema.UseUnityWebRequestForLocalBundles` to true. It can also be set through the BundledAssetGroupSchema GUI. 

A few of these situations would include:

1. You are shipping local AssetBundles that use LZMA compression because you want your shipped game package to be as small as possible. In this case, you would want to use UnityWebRequest to recompress those AssetBundles LZ4 into the local disk cache.
2. You are shipping an Android game and your APK contains AssetBundles that are compressed with the default APK compression.
3. You want the entire local AssetBundle to be loaded into memory to avoid disk seeks. If you use `UnityWebRequest` and have caching disabled, the entire AssetBundle file will be loaded into the memory cache. This increases your runtime memory usage, but may improve loading performance as it eliminates disk seeking after the initial AssetBundle load.
Both situations 1 and 2 above result in the AssetBundle existing on the player device twice (original and cached representations). This means the initial loads (decompressing and copying to cache) are slower than subsequent loads (loading from cache)

## Handling Download Errors
When a download fails, the RemoteProviderException contains errors that can be used to determine how to handle the failure.
The RemoteProviderException is either the `AsyncOperationHandle.OperationException` or an inner exception. As shown below:

[!code-cs[sample](../Tests/Editor/DocExampleCode/DownloadError.cs#doc_DownloadError)]

<!--
```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;

internal class HandleDownloadError : MonoBehaviour
{
    private AsyncOperationHandle m_Handle;
    void LoadAsset()
    {
        m_Handle = Addressables.LoadAssetAsync<GameObject>("addressKey");
        m_Handle.Completed += handle =>
        {
            string dlError = GetDownloadError(m_Handle);
            if (!string.IsNullOrEmpty(dlError))
            {
                // handle what error
            }
        };
    }
    
    string GetDownloadError(AsyncOperationHandle fromHandle)
    {
        if (fromHandle.Status != AsyncOperationStatus.Failed)
            return null;

        RemoteProviderException remoteException;
        System.Exception e = fromHandle.OperationException;
        while (e != null)
        {
            remoteException = e as RemoteProviderException;
            if (remoteException != null)
                return remoteException.WebRequestResult.Error;
            e = e.InnerException;
        }
        return null;
    }
}
```
-->

Possible error strings:
* "Request aborted"
* "Unable to write data"
* "Malformed URL"
* "Out of memory"
* "No Internet Connection"
* "Encountered invalid redirect (missing Location header?)"
* "Cannot modify request at this time"
* "Unsupported Protocol"
* "Destination host has an erroneous SSL certificate"
* "Unable to load SSL Cipher for verification"
* "SSL CA certificate error"
* "Unrecognized content-encoding"
* "Request already transmitted"
* "Invalid HTTP Method"
* "Header name contains invalid characters"
* "Header value contains invalid characters"
* "Cannot override system-specified headers"
* "Backend Initialization Error"
* "Cannot resolve proxy"
* "Cannot resolve destination host"
* "Cannot connect to destination host"
* "Access denied"
* "Generic/unknown HTTP error"
* "Unable to read data"
* "Request timeout"
* "Error during HTTP POST transmission"
* "Unable to complete SSL connection"
* "Redirect limit exceeded"
* "Received no data in response"
* "Destination host does not support SSL"
* "Failed to transmit data"
* "Failed to receive data"
* "Login failed"
* "SSL shutdown failed"
* "Redirect limit is invalid"
* "Not implemented"
* "Data Processing Error, see Download Handler error"
* "Unknown Error"