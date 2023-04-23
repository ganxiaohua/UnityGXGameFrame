﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using FairyGUI;
using GameFrame;

namespace GameFrame
{
    public class UILoader
    {
        /// <summary>
        /// UIPack数据类
        /// </summary>
        public sealed class UIPackageData
        {
            /// <summary>
            /// 被加载的次数
            /// </summary>
            public int Count = 0;

            /// <summary>
            /// 加入UIPackage
            /// </summary>
            /// <param name="uipackage"></param>
            public void SetUIPackage(UIPackage uipackage)
            {
                if (uipackage == null)
                    return;
                m_UIpackage = uipackage;
            }

            /// <summary>
            /// 包被加载了
            /// </summary>
            /// <param name="childName"></param>
            public void AddReferenceCount()
            {
                ++Count;
            }

            /// <summary>
            /// 包被消除了
            /// </summary>
            /// <param name="childName"></param>
            public bool SubReferenceCount()
            {
                if (--Count == 0)
                    return true;
                return false;
            }

            /// <summary>
            /// 加入关联的资源
            /// </summary>
            /// <param name="name"></param>
            public void AddPackageItemsResPath(string name)
            {
                if (string.IsNullOrEmpty(name))
                    return;
                CancelOver = false;
                m_PackageItemsResPath.Add(name);
            }

            /// <summary>
            /// 便利资源,一般用于释放
            /// </summary>
            /// <param name="action"></param>
            public void ConveniencePackageItemsResPath(Action<string> action)
            {
                for (int i = 0; i < m_PackageItemsResPath.Count; i++)
                {
                    action(m_PackageItemsResPath[i]);
                }
            }

            /// <summary>
            /// 清除操作
            /// </summary>
            public void Dispose()
            {
                m_PackageItemsResPath.Clear();
                if (m_UIpackage != null)
                    UIPackage.RemovePackage(m_UIpackage.id);
                m_UIpackage = null;
                CancelOver = true;
                Count = 0;
                // ChildReference.Clear();
            }

            public Action Completed;
            public bool LoadOver;

            public bool CancelOver;

            private UIPackage m_UIpackage; //包本体
            private List<string> m_PackageItemsResPath = new List<string>(3); //引用资源的名字
        }

        Dictionary<string, UIPackageData> packages = new Dictionary<string, UIPackageData>();
        private StringBuilder m_TempSb = new StringBuilder(128);

        /// <summary>
        /// 加载包
        /// </summary>
        /// <param name="descFileName">包名</param>
        /// <param name="onCompleted">回调函数</param>
        public void AddPackage(string descFileName, System.Action onCompleted = null)
        {
            if (packages.TryGetValue(descFileName, out UIPackageData uiPackAgeData))
            {
                uiPackAgeData.AddReferenceCount();
                if (uiPackAgeData.LoadOver)
                {
                    onCompleted?.Invoke();
                }
                else
                {
                    uiPackAgeData.Completed += onCompleted;
                }

                return;
            }

            var desPath = GetFGUIDesPath(descFileName);
            uiPackAgeData = new UIPackageData();
            uiPackAgeData.LoadOver = false;
            uiPackAgeData.AddReferenceCount();

            packages.Add(descFileName, uiPackAgeData);
            uiPackAgeData.Completed = onCompleted;
            AssetManager.Instance.LoadAsync<TextAsset>(desPath, (asset) =>
            {
                if (uiPackAgeData.CancelOver)
                {
                    AssetManager.Instance.UnLoad(desPath);
                    return;
                }

                var desText = asset as TextAsset;
                var uipackage = UIPackage.AddPackage(desText.bytes, descFileName, LoadPackageInternalAsync);
                uiPackAgeData.SetUIPackage(uipackage);
                uipackage.LoadAllAssets();
                AssetManager.Instance.UnLoad(desPath);
                if (uipackage.LoadOver())
                {
                    uiPackAgeData.Completed?.Invoke();
                }
            });
        }

        /// <summary>
        /// 删除包体
        /// </summary>
        /// <param name="descFileName">名字</param>
        /// <param name="childname"></param>
        public void RemovePackages(string descFileName)
        {
            if (packages.TryGetValue(descFileName, out UIPackageData uiPackAgeData) && uiPackAgeData != null)
            {
                //只有计数为0才能删除包体
                if (uiPackAgeData.SubReferenceCount())
                {
                    uiPackAgeData.ConveniencePackageItemsResPath((x) => { AssetManager.Instance.UnLoad(x); });

                    uiPackAgeData.Dispose();
                    packages.Remove(descFileName);
                }
            }
        }

        string GetFGUIDesPath(string fileName)
        {
            return $"{StaticText.UIPath}{fileName}_fui.bytes";
        }

        string GetFGUIResPath(string fileName, string extension)
        {
            return $"{StaticText.UIPath}{fileName}{extension}";
        }

        private void LoadPackageInternalAsync(string name, string extension, System.Type type, PackageItem item)
        {
            var resPath = GetFGUIResPath(name, extension);
            string key = name[..name.IndexOf("_", StringComparison.Ordinal)];
            if (packages.TryGetValue(key, out UIPackageData uiPackAgeData) && uiPackAgeData != null)
            {
                uiPackAgeData.AddPackageItemsResPath(resPath);
            }
            else
            {
                return;
            }

            LoadResPath(resPath, name + extension, item, uiPackAgeData);
        }

        public async UniTask LoadResPath(string path, string readyLoadName, PackageItem package, UIPackageData uiPackAgeData)
        {
            object x = await AssetManager.Instance.LoadAsyncTask<object>(path);
            package.owner.SetItemAsset(package, x, DestroyMethod.None);
            try
            {
                if (package.owner.ReadyLoadFileLoadOver(readyLoadName))
                {
                    var callback = uiPackAgeData.Completed;
                    uiPackAgeData.LoadOver = true;
                    callback?.Invoke();
                }
            }
            catch (Exception e)
            {
                //当有个资源没在预加载列表的时候,不至于卡在那边
                Debugger.LogError($"LoadPackageInternalAsync path:{path}, error:{e.Message}");
                uiPackAgeData.LoadOver = true;
                uiPackAgeData.Completed?.Invoke();
            }
        }
    }
}