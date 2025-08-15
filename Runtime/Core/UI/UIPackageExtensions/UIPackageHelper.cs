using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public static class UIPackageHelper
    {
        public static readonly string PackagePathPattern = "{0}_fui";    //"Assets/Art/UI/{0}_fui.bytes";
        public static readonly string PackageResPathPattern = "{0}_{1}"; //"Assets/Art/UI/{0}_{1}";

        public static readonly UIPackage.LoadResourceAsync OnLoadResFromBundleAsync =
                (string name, string extension, Type type, PackageItem item) => { LoadResFromBundleAsync(name, item).Forget(); };

        public static readonly UIPackage.LoadResource OnLoadResFromResources =
                (string name, string extension, Type type, out DestroyMethod destroyMethod) =>
                {
                    destroyMethod = DestroyMethod.Unload;
                    return Resources.Load(name, type);
                };

        private static readonly Dictionary<string, string> packagePaths = new Dictionary<string, string>();

        public static string GetPackagePath(string packageName)
        {
            if (!packagePaths.TryGetValue(packageName, out var path))
            {
                path = string.Format(PackagePathPattern, packageName);
                packagePaths.Add(packageName, path);
            }

            return path;
        }

        public static string GetPackageResPath(string packageName, string name)
        {
            return string.Format(PackageResPathPattern, packageName, name);
        }

        public static bool IsPackageReady(string packageName,
#if UNITY_EDITOR
                List<string> debugChain = null)
#else
            int depth = 0)
#endif
        {
            var package = UIPackage.GetByName(packageName);
            if (package == null)
                return false;
#if UNITY_EDITOR
            if (debugChain == null) debugChain = new List<string>();
            if (debugChain.Count > 8) throw new Exception($"Bad UI Package dependence chain: {string.Concat(debugChain)}");
            debugChain.Add($"{packageName} - ");
#else
            if (depth > 8) throw new Exception($"UI Package({packageName}) bad dependence chain");
            depth++;
#endif
            foreach (var dep in package.dependencies)
            {
                var depName = dep["name"];
                if (!IsPackageReady(depName,
#if UNITY_EDITOR
                            debugChain))
#else
                    depth))
#endif
                {
#if UNITY_EDITOR
                    debugChain.Pop();
#endif
                    return false;
                }
            }
#if UNITY_EDITOR
            debugChain.Pop();
#endif
            return true;
        }

        public static bool RefPackage(string packageName, IAssetReference reference)
        {
            var package = UIPackage.GetByName(packageName);
            Assert.AreNotEqual(null, package, $"UI Package({packageName}) not loaded");
            var path = GetPackagePath(packageName);
            if (!reference.RefAsset(path))
                return false; // already ref
            foreach (var dep in package.dependencies)
            {
                foreach (var dp in dep)
                {
                    RefPackage(dp.Value, reference);
                }
            }

            return true;
        }

        public static GObject CreateGObjectImmediate(string packageName, string name, IAssetReference reference)
        {
            Assert.IsTrue(IsPackageReady(packageName), $"UI Package({packageName}) not ready");
            var package = UIPackage.GetByName(packageName);
            var go = package.CreateObject(name);
            go.name = name;
            RefPackage(packageName, reference);
            return go;
        }

        /// <summary>
        /// assetHandleType: <see cref="IAssetHandle"/>
        /// </summary>
        public static async UniTask<UIPackage> LoadPackageAsync(string packageName, Type assetHandleType,
                IAssetReference reference, CancellationToken cancelToken = default,
#if UNITY_EDITOR
                List<string> debugChain = null)
#else
            int depth = 0)
#endif
        {
            var path = GetPackagePath(packageName);
            var handle = await AssetManager.Instance.LoadAsync(path, null, assetHandleType, reference, cancelToken);
            var package = (UIPackage) handle.Result;
            if (package == null)
                return null;
            if (package.dependencies.Length == 0)
                return package;
#if UNITY_EDITOR
            if (debugChain == null) debugChain = new List<string>();
            if (debugChain.Count > 8) throw new Exception($"Bad UI Package dependence chain: {string.Concat(debugChain)}");
            debugChain.Add($"{packageName} - ");
#else
            if (depth > 8) throw new Exception($"UI Package({packageName}) bad dependence chain");
            depth++;
#endif
            foreach (var dep in package.dependencies)
            {
                var depName = dep["name"];
                var depPackage = await LoadPackageAsync(depName, assetHandleType, reference, cancelToken,
#if UNITY_EDITOR
                        debugChain);
#else
                    depth);
#endif
                if (depPackage == null)
                {
#if UNITY_EDITOR
                    debugChain.Pop();
#endif
                    return null;
                }
            }
#if UNITY_EDITOR
            debugChain.Pop();
#endif
            return package;
        }

        private static async UniTaskVoid LoadResFromBundleAsync(string name, PackageItem item)
        {
            var packageName = item.owner.name;
            var path = GetPackagePath(packageName);
            if (!AssetManager.Instance.TryGetAssetHandle(path, out var handle))
                throw new Exception($"UI Package({path}) not loaded");
            var resPath = GetPackageResPath(packageName, name);
            var reference = handle.AssetReference;
            var asset = await AssetManager.Instance.LoadAsync<UnityEngine.Object>(resPath, reference);
            if (asset != null && handle.IsValid)
            {
                try
                {
                    item.owner.SetItemAsset(item, asset, DestroyMethod.None);
                }
                catch (Exception e)
                {
                    Debug.LogException(new Exception($"Bad res load: {resPath}", e));
                }
            }
        }
    }
}