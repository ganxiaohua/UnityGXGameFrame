using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Common.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using FairyGUI;

namespace GameFrame
{
    public class UILoader : Singleton<UILoader>
    {
        // /// <summary>
        // /// UIPack数据类
        // /// </summary>
        // public sealed class UIPackageData
        // {
        //     public bool LoadOver;
        //
        //     public CancellationToken CancellationToken;
        //
        //     public UIPackage mUIpackage; //包本体
        //     
        //     public DefaultAssetReference assetReference;
        //     /// <summary>
        //     /// 被加载的次数
        //     /// </summary>
        //     private int count = 0;
        //
        //     /// <summary>
        //     /// 加入UIPackage
        //     /// </summary>
        //     /// <param name="uipackage"></param>
        //     public void SetUIPackage(UIPackage uipackage)
        //     {
        //         if (uipackage == null)
        //             return;
        //         assetReference = new DefaultAssetReference();
        //         mUIpackage = uipackage;
        //     }
        //
        //     /// <summary>
        //     /// 包被加载了
        //     /// </summary>
        //     /// <param name="childName"></param>
        //     public void AddReferenceCount()
        //     {
        //         ++count;
        //     }
        //
        //     /// <summary>
        //     /// 包被消除了
        //     /// </summary>
        //     /// <param name="childName"></param>
        //     public bool SubReferenceCount()
        //     {
        //         if (--count == 0)
        //             return true;
        //         return false;
        //     }
        //
        //     /// <summary>
        //     /// 加入关联的资源
        //     /// </summary>
        //     /// <param name="name"></param>
        //     public void AddPackageItemsResPath(string name)
        //     {
        //         if (string.IsNullOrEmpty(name))
        //             return;
        //     }
        //     
        //
        //     /// <summary>
        //     /// 清除操作
        //     /// </summary>
        //     public void Dispose()
        //     {
        //         if (mUIpackage != null)
        //             UIPackage.RemovePackage(mUIpackage.id);
        //     }
        // }
        //
        // Dictionary<string, UIPackageData> packages = new Dictionary<string, UIPackageData>();
        //
        // /// <summary>
        // /// 加载包
        // /// </summary>
        // /// <param name="descFileName">包名</param>
        // /// <param name="onCompleted">回调函数</param>
        // public async UniTask AddPackage(string descFileName, CancellationToken token = default(CancellationToken))
        // {
        //     if (!PackNameForPath.PackNameForPathDic.TryGetValue(descFileName,out var desPath))
        //     {
        //         return;
        //     }
        //
        //     if (packages.TryGetValue(descFileName, out UIPackageData uiPackAgeData))
        //     {
        //         uiPackAgeData.AddReferenceCount();
        //         return;
        //     }
        //     
        //     uiPackAgeData = new UIPackageData();
        //     uiPackAgeData.LoadOver = false;
        //     uiPackAgeData.AddReferenceCount();
        //     packages.Add(descFileName, uiPackAgeData);
        //     var desText = await AssetSystem.Instance.LoadAsync<TextAsset>(desPath, uiPackAgeData.assetReference,token);
        //     if (desText == null)
        //     {
        //         return;
        //     }
        //     uiPackAgeData.CancellationToken = token;
        //     var uipackage = UIPackage.AddPackage(desText.bytes, descFileName, LoadPackageInternalAsync);
        //     uiPackAgeData.SetUIPackage(uipackage);
        //     uiPackAgeData.assetReference.UnrefAsset(desPath);
        //     //加载依赖包
        //     var dependencies = uipackage.dependencies;
        //     foreach (var dependenciesDic in dependencies)
        //     {
        //         if (dependenciesDic.TryGetValue("name", out string packname))
        //         {
        //            await AddPackage(packname, token);
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// 删除包体
        // /// </summary>
        // /// <param name="descFileName">名字</param>
        // /// <param name="childname"></param>
        // public void RemovePackages(string descFileName)
        // {
        //     if (packages.TryGetValue(descFileName, out UIPackageData uiPackAgeData) && uiPackAgeData != null)
        //     {
        //         uiPackAgeData.assetReference.UnrefAssets();
        //         //只有计数为0才能删除包体
        //         if (uiPackAgeData.SubReferenceCount())
        //         {
        //             uiPackAgeData.Dispose();
        //             packages.Remove(descFileName);
        //         }
        //         
        //         var dependencies = uiPackAgeData.mUIpackage.dependencies;
        //         foreach (var dependenciesDic in dependencies)
        //         {
        //             if (dependenciesDic.TryGetValue("name", out string packname))
        //             {
        //                 RemovePackages(packname);
        //             }
        //         }
        //     }
        // }
        //
        //
        // string GetFGUIResPath(string packname,string fileName, string extension)
        // {
        //     string str = PackNameForPath.PackNameForPathDic[packname];
        //      str = str.Substring(0, str.LastIndexOf("/")+1);
        //     return $"{str}{fileName}{extension}";
        // }
        //
        // private void LoadPackageInternalAsync(string name, string extension, System.Type type, PackageItem item)
        // {
        //     var resPath = "";
        //     string key = name[..name.IndexOf("_", StringComparison.Ordinal)];
        //     if (packages.TryGetValue(key, out UIPackageData uiPackAgeData) && uiPackAgeData != null)
        //     {
        //         resPath = GetFGUIResPath(uiPackAgeData.mUIpackage.name,name, extension);
        //         LoadResPath(resPath, name + extension, item, uiPackAgeData).Forget();
        //     }
        // }

        // public async UniTaskVoid LoadResPath(string path, string readyLoadName, PackageItem package, UIPackageData uiPackAgeData)
        // {
        //     object obj = await AssetSystem.Instance.LoadAsync<object>(path,uiPackAgeData.assetReference,uiPackAgeData.CancellationToken);
        //     if (obj == null)
        //     {
        //         return;
        //     }
        //     package.owner.SetItemAsset(package, obj, DestroyMethod.None);
        //     try
        //     {
        //         if (package.owner.ReadyLoadFileLoadOver(readyLoadName))
        //         {
        //             var callback = uiPackAgeData.Completed;
        //             uiPackAgeData.LoadOver = true;
        //             callback?.Invoke();
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         //当有个资源没在预加载列表的时候,不至于卡在那边
        //         Debugger.LogError($"LoadPackageInternalAsync path:{path}, error:{e.Message}");
        //         uiPackAgeData.LoadOver = true;
        //         uiPackAgeData.Completed?.Invoke();
        //     }
        // }
    }
}