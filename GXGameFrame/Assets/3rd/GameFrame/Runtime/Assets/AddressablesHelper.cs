using System;
using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
#if UNITY_EDITOR
#endif

public static class AddressablesHelper
{
    public const string Bundles = "Bundles";

    public const string CatalogName = "catalog_GXGame.zip.json";

    public static string PlayerDataPath => $"{Application.streamingAssetsPath}/{Bundles}";

    public static string PlayerDataURL => GetPlayerDataProtocol();

    public static PlayerAssets PlayerAssets { get; private set; } = ScriptableObject.CreateInstance<PlayerAssets>();

    public static UpdateInfo UpdateInfo { get; private set; } = ScriptableObject.CreateInstance<UpdateInfo>();

    public static string UpdateURL { get; private set; } //资源更新地址,初始化时由PlayerVersion获得

    public static int AssetVersion { get; private set; } //资源版本号

    public static async UniTask InitializeAsync()
    {
        if (!string.IsNullOrEmpty(PlayerVersion.Version.UpdateURL))
        {
            UpdateURL = PlayerVersion.Version.UpdateURL;
        }

        Debugger.Log($"UpdateURL={UpdateURL}");
#if !UNITY_EDITOR
        await PlayerAssetLoad();
#endif
        await UpdateInfoLoad();
#if UNITY_EDITOR && RESSEQ
        //编辑器模式,加载资源远程资源清单 启动这个优先本地资源,其次远程资源
        string url = ""; //填写远程URL
        var catalogPath = $"{url}/{CatalogName}";
        var loadCatalogHandle = Addressables.LoadContentCatalogAsync(catalogPath, false);
        await loadCatalogHandle;
        if (loadCatalogHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"加载远程资源清单失败:{loadCatalogHandle.OperationException.Message}");
        }
#endif
    }

    private static async UniTask PlayerAssetLoad()
    {
        var fileUrl = GetPlayerDataUrl(PlayerAssets.Filename);
        var playerAssetRequest = UnityWebRequest.Get(fileUrl);
        try
        {
            await playerAssetRequest.SendWebRequest();
            if (playerAssetRequest.result == UnityWebRequest.Result.Success)
            {
                Addressables.InternalIdTransformFunc = InternalIdTransformFunc;
                PlayerAssets = LoadFromJson<PlayerAssets>(playerAssetRequest.downloadHandler.text);
                AssetVersion = PlayerAssets.Version;
            }
        }
        catch (Exception e)
        {
            Debugger.LogError(e.Message);
        }
        finally
        {
            playerAssetRequest.Dispose();
        }
    }

    private static async UniTask UpdateInfoLoad()
    {
        bool needUpdateInfo = true;
#if UNITY_EDITOR
        needUpdateInfo  = EditorCSharp.GetProjectConfigData_ActivePlayModeIndex() == 2;
#endif
        if (!needUpdateInfo)
            return;
        var fileUrl = GetDownloadUrl(UpdateInfo.Filename);
        var updateInfoRequest = UnityWebRequest.Get(fileUrl);
        try
        {
            await updateInfoRequest.SendWebRequest();
            if (updateInfoRequest.result == UnityWebRequest.Result.Success)
            {
                UpdateInfo = LoadFromJson<UpdateInfo>(updateInfoRequest.downloadHandler.text);
                AssetVersion = UpdateInfo.Version;
                Debugger.Log($"Bundle version:{UpdateInfo.Version}, Build time:{GetDateTime(UpdateInfo.Timestamp)}");
            }
            else
            {
                Debugger.LogWarning($"UpdateInfo Failure:url={fileUrl},responseCode={updateInfoRequest.responseCode},error={updateInfoRequest.error}");
            }
        }
        catch (Exception e)
        {
            Debugger.LogError(e.Message);
        }
        finally
        {
            updateInfoRequest.Dispose();
        }
    }

    private static string InternalIdTransformFunc(IResourceLocation location)
    {
        if (location.Data is AssetBundleRequestOptions)
        {
            if (PlayerAssets.Contains(location.PrimaryKey))
            {
                var path = GetPlayerDataPath(location.PrimaryKey);
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