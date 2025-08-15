using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public static class GLoaderHelper
    {
        public static void Load(this GLoader gloader, string packageName, string name,
                IAssetReference reference, bool fromResources = false)
        {
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(name))
            {
                gloader.url = null;
            }
            else if (UIPackageHelper.IsPackageReady(packageName))
            {
                var packagePath = UIPackageHelper.GetPackagePath(packageName);
                reference.RefAsset(packagePath);
                gloader.url = UIPackage.GetItemURL(packageName, name);
            }
            else
            {
                LoadAsync(gloader, packageName, name, reference, fromResources).Forget();
            }
        }

        public static async UniTask LoadAsync(this GLoader gloader, string packageName, string name,
                IAssetReference reference, bool fromResources = false)
        {
            var version = gloader.ContentVersion;
            var assetHandleType = fromResources ? typeof(UIPackageResourcesHandle) : typeof(UIPackageAddressableHandle);
            var package = await UIPackageHelper.LoadPackageAsync(packageName, assetHandleType, reference);
            if (package == null || version != gloader.ContentVersion)
                return;
            gloader.url = UIPackage.GetItemURL(packageName, name);
        }

        public static void LoadRaw(this GLoader gloader, string path, IAssetReference reference)
        {
            if (string.IsNullOrEmpty(path))
            {
                gloader.url = null;
            }
            else if (AssetManager.Instance.TryGetAssetHandle(path, out var handle) && handle.IsDone)
            {
                reference.RefAsset(path);
                gloader.onExternalLoadSuccess(new NTexture((Texture) handle.Result));
            }
            else
            {
                LoadRawAsync(gloader, path, reference).Forget();
            }
        }

        public static async UniTask LoadRawAsync(this GLoader gloader, string path, IAssetReference reference)
        {
            var version = gloader.ContentVersion;
            var texture = await AssetManager.Instance.LoadAsync<Texture>(path, reference);
            if (texture == null || version != gloader.ContentVersion)
                return;
            gloader.onExternalLoadSuccess(new NTexture(texture));
        }

        public static void Unload(this GLoader gloader)
        {
            gloader.url = null;
        }
    }
}