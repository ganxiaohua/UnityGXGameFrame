using System;
using UnityEngine;
using YooAsset;

namespace GameFrame
{
    public static class PackageSearcher
    {
        public static ResourcePackage SearchByAssetLocation(string location, out AssetInfo info,Type type, bool raiseError = true)
        {
            info = default;
            foreach (var packageSetting in YooConst.PackageSettings)
            {
                var package = YooAssets.GetPackage(packageSetting.name);
                if (package.CheckLocationValid(location))
                {
                    info = package.GetAssetInfo(location,type);
                    return package;
                }
            }

            if (raiseError)
                Debug.LogError($"[YooAssets] Search package by location fail: {location}");
            return null;
        }
        
        
        public static ResourcePackage SearchByAssetLocation(string location)
        {
            foreach (var packageSetting in YooConst.PackageSettings)
            {
                var package = YooAssets.GetPackage(packageSetting.name);
                if (package.CheckLocationValid(location))
                {
                    return package;
                }
            }
            return null;
        }

        public static ResourcePackage SearchByAssetTag(string tag, out AssetInfo[] infos, bool raiseError = true)
        {
            infos = default;
            foreach (var packageSetting in YooConst.PackageSettings)
            {
                var package = YooAssets.GetPackage(packageSetting.name);
                infos = package.GetAssetInfos(tag);
                if (infos != null && infos.Length > 0)
                {
                    return package;
                }
            }

            if (raiseError)
                Debug.LogError($"[YooAssets] Search package by tag fail: {tag}");
            return null;
        }
    }
}