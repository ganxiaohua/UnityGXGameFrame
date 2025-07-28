using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class UILoader : Singleton<UILoader>
    {

        private UIPackage.LoadResourceAsync LoadResource;
        private Action<object> UnloadHandle;
        private const string fui = "_fui";
        private Dictionary<string, DefaultAssetReference> AuxResDic;

        public UILoader()
        {
            LoadResource = LoadPackageInternalAsync;
            UnloadHandle = RemovePackage;
            AuxResDic = new Dictionary<string, DefaultAssetReference>();
        }

        public async UniTask AddPackage(string packageName, DefaultAssetReference reference, CancellationToken token = default)
        {
            string desPath = $"{packageName}{fui}";
            if (AuxResDic.TryGetValue(packageName, out DefaultAssetReference curAsset))
            {
                reference.RefAsset(desPath);
                return;
            }
            
            var desText = await AssetManager.Instance.LoadRawAsync(desPath,token);
            if (desText == null)
            {
                return;
            }
            var uipackage = UIPackage.AddPackage(desText, packageName, LoadResource);
            var dependencies = uipackage.dependencies;
            foreach (var dependenciesDic in dependencies)
            {
                if (dependenciesDic.TryGetValue("name", out string packname))
                {
                    await AddPackage(packname, reference, token);
                }
            }
        }

        /// <summary>
        /// 包体被卸载了附属资源也要卸载
        /// </summary>
        /// <param name="packName"></param>
        public void RemovePackage(object packName)
        {
            string packNameStr = (string) packName;
            if (AuxResDic.TryGetValue(packNameStr, out DefaultAssetReference curAsset))
            {
                curAsset.UnrefAssets();
            }

            AuxResDic.Remove(packNameStr);
            UIPackage.RemovePackage(packNameStr);
        }

        /// <summary>
        /// 加载附属资源
        /// </summary>
        /// <param name="name"></param>
        /// <param name="extension"></param>
        /// <param name="type"></param>
        /// <param name="item"></param>
        private void LoadPackageInternalAsync(string name, string extension, System.Type type, PackageItem item)
        {
            if (!AuxResDic.TryGetValue(item.owner.name, out DefaultAssetReference curAsset))
            {
                curAsset = new DefaultAssetReference();
                AuxResDic.Add(item.owner.name, curAsset);
            }

            Load(name, curAsset, item).Forget();
        }

        private async UniTaskVoid Load(string path, DefaultAssetReference defaultAssetReference, PackageItem item)
        {
            var obj = await AssetManager.Instance.LoadAsync<UnityEngine.Object>(path, defaultAssetReference);
            item.owner.SetItemAsset(item, obj, DestroyMethod.None);
        }

        public async UniTask<bool> LoadOver(string packageName)
        {
            while (AuxResDic.TryGetValue(packageName, out DefaultAssetReference curAsset) && curAsset.PercentComplete>0.9f)
            {
                await UniTask.Yield();
            }

            return AuxResDic.ContainsKey(packageName);
        }
    }
}