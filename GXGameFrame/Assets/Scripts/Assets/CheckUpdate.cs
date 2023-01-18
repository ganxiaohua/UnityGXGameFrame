//检查版本更新

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using LuaInterface;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class CheckUpdate
{
    private float HasDownLoadSize;
    private float TotalDownLoadSize;
    private AsyncOperationHandle DownloadHandle;
    private DownloadStatus DownloadStatus;
    private StringBuilder CachedStringBuilder = null;
    private List<object> UpdateResKeysList = new List<object>();
    private Action LoadOverCallBack;

    private string AddressablesTempPath;
    private string Comunityaddressables;

    /// <summary>
    /// 切换平台
    /// </summary>
    public async UniTask CheckVersions(Action loadoverCallBack)
    {
        CachedStringBuilder = new StringBuilder(1024);
        AddressablesTempPath = Path.Combine(Application.persistentDataPath, "AddressablesTemp");
        Comunityaddressables = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
        LoadOverCallBack = loadoverCallBack;
        var InitializeAsync = Addressables.InitializeAsync();
        await InitializeAsync.Task;
        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = checkHandle.Result;
            if (catalogs != null && catalogs.Count > 0)
            {
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                await updateHandle.Task;
                for (int i = 0; i < updateHandle.Result.Count; i++)
                {
                    var locationsHandle = Addressables.LoadResourceLocationsAsync(updateHandle.Result[i].Keys, Addressables.MergeMode.Union);
                    await locationsHandle.Task;
                    if (locationsHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach (var location in locationsHandle.Result)
                        {
                            if (UpdateResKeysList.Contains(location.PrimaryKey) ||
                                AddressablesHelper.PlayerAssets.Contains(location.PrimaryKey))
                                continue;
                            var sizeData = location.Data as ILocationSizeData;
                            if (sizeData != null)
                            {
                                var size = sizeData.ComputeSize(location, Addressables.ResourceManager);
                                if (size > 0)
                                {
                                    TotalDownLoadSize += size;
                                    UpdateResKeysList.Add(location.PrimaryKey);
                                }
                            }
                        }
                    }

                    Addressables.Release(locationsHandle);
                }

                Debugger.Log($"更新大小 Size: {(TotalDownLoadSize / 1024f / 1024f).ToString("0.00")}M");
                HasDownLoadSize = 0;
                //下载更新
                await DownloadRes();
                MovePath(true);
            }
            else
            {
                Debugger.Log("不需要更新!");
                LoadOverCallBack?.Invoke();
            }
        }
        else
        {
            Debugger.Log("获取更新失败!");
        }

        Addressables.Release(checkHandle);
    }


    /// <summary>
    /// 转移目录 断线重下问题
    /// </summary>
    /// <param name="p2t"></param>
    private void MovePath(bool p2t)
    {
        if (p2t)
        {
            if (!Directory.Exists(Comunityaddressables))
                return;
            if (Directory.Exists(AddressablesTempPath))
                Directory.Delete(AddressablesTempPath, true);
            Directory.Move(Comunityaddressables, AddressablesTempPath);
        }
        else
        {
            if (!Directory.Exists(AddressablesTempPath))
                return;
            if (Directory.Exists(Comunityaddressables))
                Directory.Delete(Comunityaddressables, true);
            Directory.Move(AddressablesTempPath, Comunityaddressables);
        }
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    private async UniTask DownloadRes()
    {
        var startTime = Time.realtimeSinceStartup;
        if (TotalDownLoadSize > 0)
            
            for (int i = UpdateResKeysList.Count - 1; i >= 0; i--)
            {
                DownloadHandle = Addressables.DownloadDependenciesAsync(UpdateResKeysList[i]);
                DownloadStatus = DownloadHandle.GetDownloadStatus();
                if (DownloadStatus.TotalBytes == 0)
                {
                    Addressables.Release(DownloadHandle);
                    UpdateResKeysList.RemoveAt(i);
                    continue;
                }

                await DownloadHandle.Task;
                if (DownloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    //下载失败相关处理
                    Debugger.LogError("下载失败");
                }

                HasDownLoadSize += DownloadStatus.TotalBytes;
                UpdateResKeysList.RemoveAt(i);
                Addressables.Release(DownloadHandle);
            }
        MovePath(false);
        Debugger.Log($"下载完毕!下载时间:{Time.realtimeSinceStartup - startTime}s");
        LoadOverCallBack?.Invoke();
    }

    public void Update()
    {
        if (TotalDownLoadSize != 0)
        {
            if (!DownloadHandle.IsDone && DownloadHandle.Status != AsyncOperationStatus.Failed)
            {
                DownloadStatus = DownloadHandle.GetDownloadStatus();
                CachedStringBuilder.Length = 0;
                CachedStringBuilder.AppendFormat("{0},{1}", ((DownloadStatus.DownloadedBytes + HasDownLoadSize) / 1024f / 1024f).ToString("0.00"),
                    (TotalDownLoadSize / 1024f / 1024f).ToString("0.00"));
                Debugger.Log(CachedStringBuilder.ToString());
            }
        }
    }

    /// <summary>
    /// 销毁预下载
    /// </summary>
    public void Disable()
    {
        if (UpdateResKeysList.Count != 0 && !DownloadHandle.IsDone)
        {
            Addressables.Release(DownloadHandle);
        }

        UpdateResKeysList.Clear();
    }
}