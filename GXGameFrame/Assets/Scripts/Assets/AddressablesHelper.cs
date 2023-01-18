using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class AddressablesHelper
{
    public const string Bundles = "Bundles";

    public const string CatalogName = "catalog_eden.zip.json";

    public const string CatalogHashEnd = ".hash";

    public const string LuaLabel = "Lua";

    public const string PreloadLabel = "Preload";

    public static string PlayerDataPath => $"{Application.streamingAssetsPath}/{Bundles}";

    public static string PlayerDataURL => GetPlayerDataProtocol();

    public static PlayerAssets PlayerAssets { get; private set; } = ScriptableObject.CreateInstance<PlayerAssets>();

    public static UpdateInfo UpdateInfo { get; private set; } = ScriptableObject.CreateInstance<UpdateInfo>();

    public static string UpdateURL { get; private set; } = "http://192.168.100.230/eden/aa";//资源更新地址,初始化时由PlayerVersion获得

    public static int AssetVersion { get; private set; }//资源版本号

    public static IEnumerator InitializeAsync()
    {
        if (!string.IsNullOrEmpty(PlayerVersion.version.updateURL))
        {
            UpdateURL = PlayerVersion.version.updateURL;
        }
        Debug.Log($"UpdateURL={UpdateURL}");
        Addressables.InternalIdTransformFunc = InternalIdTransformFunc;
        var fileUrl = GetPlayerDataUrl(PlayerAssets.Filename);
        var playerAssetRequest = UnityWebRequest.Get(fileUrl);
        yield return playerAssetRequest.SendWebRequest();
        if (playerAssetRequest.result == UnityWebRequest.Result.Success)
        {
            PlayerAssets = LoadFromJson<PlayerAssets>(playerAssetRequest.downloadHandler.text);
            AssetVersion = PlayerAssets.version;
        }
        playerAssetRequest.Dispose();

        fileUrl = GetDownloadUrl(UpdateInfo.Filename);
        var updateInfoRequest = UnityWebRequest.Get(fileUrl);
        yield return updateInfoRequest.SendWebRequest();
        if (updateInfoRequest.result == UnityWebRequest.Result.Success)
        {
            UpdateInfo = LoadFromJson<UpdateInfo>(updateInfoRequest.downloadHandler.text);
            AssetVersion = UpdateInfo.version;
            Debug.Log($"Bundle version:{UpdateInfo.version}, Build time:{GetDateTime(UpdateInfo.timestamp)}");
        }
        else
        {
            Debug.LogWarning($"UpdateInfo Failure:url={fileUrl},responseCode={updateInfoRequest.responseCode},error={updateInfoRequest.error}");
        }
        updateInfoRequest.Dispose();

#if UNITY_EDITOR
        //编辑器模式,加载资源远程资源清单
        var catalogPath = $"http://192.168.100.230/eden/aa/{GetPlatformName()}/{CatalogName}";
        var loadCatalogHandle = Addressables.LoadContentCatalogAsync(catalogPath, false);
        yield return loadCatalogHandle;
        if (loadCatalogHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"加载远程资源清单失败:{loadCatalogHandle.OperationException.Message}");
        }
#endif
    }

    private static string InternalIdTransformFunc(IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            if (PlayerAssets.Contains(location.PrimaryKey))
            {
                var path = GetPlayerDataPath(location.PrimaryKey);
                //Debug.Log("===streamingAssetsPath:" + path);
                return path;
            }
        }
        return location.InternalId;
    }

    public static string GetPlayerDataPath(string filename)
    {
        return $"{PlayerDataPath}/{filename}";
    }

    public static string GetPlayerDataUrl(string filename)
    {
        return $"{PlayerDataURL}{GetPlayerDataPath(filename)}";
    }

    public static string GetDownloadUrl(string filename)
    {
        return $"{UpdateURL}/{GetPlatformName()}/{filename}";
    }

    public static string GetPlayerDataProtocol()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.IPhonePlayer:
                return "file://";
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "file:///";
            default:
                return string.Empty;
        }
    }

    public static string GetPlatformName()
    {
        return PlatformMappingService.GetPlatformPathSubFolder();
    }

    public static T LoadFromJson<T>(string json) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        try
        {
            JsonUtility.FromJsonOverwrite(json, asset);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }

        return asset;
    }

    private static DateTime GetDateTime(long seconds)
    {
        var offset = DateTimeOffset.FromUnixTimeSeconds(seconds).ToLocalTime();
        return offset.DateTime;
    }
}
