//检查版本更新

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class CheckUpdate
{
    private float m_HasDownLoadSize;
    private float m_TotalDownLoadSize;
    private AsyncOperationHandle m_DownloadHandle;
    private DownloadStatus m_DownloadStatus;
    private StringBuilder m_CachedStringBuilder = null;
    private List<object> m_UpdateResKeysList = new List<object>();

    private string m_AddressablesTempPath;
    private string m_Comunityaddressables;

    /// <summary>
    /// 切换平台
    /// </summary>
    public async UniTask CheckVersions()
    {
        m_CachedStringBuilder = new StringBuilder(1024);
        m_AddressablesTempPath = Path.Combine(Application.persistentDataPath, "AddressablesTemp");
        m_Comunityaddressables = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
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
                            if (m_UpdateResKeysList.Contains(location.PrimaryKey) ||
                                AddressablesHelper.PlayerAssets.Contains(location.PrimaryKey))
                                continue;
                            var sizeData = location.Data as ILocationSizeData;
                            if (sizeData != null)
                            {
                                var size = sizeData.ComputeSize(location, Addressables.ResourceManager);
                                if (size > 0)
                                {
                                    m_TotalDownLoadSize += size;
                                    m_UpdateResKeysList.Add(location.PrimaryKey);
                                }
                            }
                        }
                    }

                    Addressables.Release(locationsHandle);
                }

                Debugger.Log($"更新大小 Size: {(m_TotalDownLoadSize / 1024f / 1024f).ToString("0.00")}M");
                m_HasDownLoadSize = 0;
                //下载更新
                await DownloadRes();
                MovePath(true);
            }
            else
            {
                Debugger.Log("不需要更新!");
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
            if (!Directory.Exists(m_Comunityaddressables))
                return;
            if (Directory.Exists(m_AddressablesTempPath))
                Directory.Delete(m_AddressablesTempPath, true);
            Directory.Move(m_Comunityaddressables, m_AddressablesTempPath);
        }
        else
        {
            if (!Directory.Exists(m_AddressablesTempPath))
                return;
            if (Directory.Exists(m_Comunityaddressables))
                Directory.Delete(m_Comunityaddressables, true);
            Directory.Move(m_AddressablesTempPath, m_Comunityaddressables);
        }
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    private async UniTask DownloadRes()
    {
        MovePath(false);
        var startTime = Time.realtimeSinceStartup;
        if (m_TotalDownLoadSize > 0)
            
            for (int i = m_UpdateResKeysList.Count - 1; i >= 0; i--)
            {
                m_DownloadHandle = Addressables.DownloadDependenciesAsync(m_UpdateResKeysList[i]);
                m_DownloadStatus = m_DownloadHandle.GetDownloadStatus();
                if (m_DownloadStatus.TotalBytes == 0)
                {
                    Addressables.Release(m_DownloadHandle);
                    m_UpdateResKeysList.RemoveAt(i);
                    continue;
                }

                await m_DownloadHandle.Task;
                if (m_DownloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    //下载失败相关处理
                    Debugger.LogError("下载失败");
                }

                m_HasDownLoadSize += m_DownloadStatus.TotalBytes;
                m_UpdateResKeysList.RemoveAt(i);
                Addressables.Release(m_DownloadHandle);
            }
        Debugger.Log($"下载完毕!下载时间:{Time.realtimeSinceStartup - startTime}s");
    }

    public void Update()
    {
        if (m_TotalDownLoadSize != 0)
        {
            if (!m_DownloadHandle.IsDone && m_DownloadHandle.Status != AsyncOperationStatus.Failed)
            {
                m_DownloadStatus = m_DownloadHandle.GetDownloadStatus();
                m_CachedStringBuilder.Length = 0;
                m_CachedStringBuilder.AppendFormat("{0},{1}", ((m_DownloadStatus.DownloadedBytes + m_HasDownLoadSize) / 1024f / 1024f).ToString("0.00"),
                    (m_TotalDownLoadSize / 1024f / 1024f).ToString("0.00"));
                Debugger.Log(m_CachedStringBuilder.ToString());
            }
        }
    }

    /// <summary>
    /// 销毁预下载
    /// </summary>
    public void Disable()
    {
        if (m_UpdateResKeysList.Count != 0 && !m_DownloadHandle.IsDone)
        {
            Addressables.Release(m_DownloadHandle);
        }

        m_UpdateResKeysList.Clear();
    }
}