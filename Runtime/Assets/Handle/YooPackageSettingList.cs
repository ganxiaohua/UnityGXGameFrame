using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public enum PackageContentType
    {
        AssetBundle = 0,
        RawFile = 1,
    }

    [Serializable]
    public class PackageSetting
    {
        public string Name;
        public EPlayMode PlayMode;
        public PackageContentType ContentType;
    }

    [CreateAssetMenu(menuName = "GX框架工具/生成资源配置文件", fileName = "YooAssetsData")]
    public class YooPackageSettingList : ScriptableObject
    {
        public string RemoteResUrl;
        public string DebugResUrl;
        public List<PackageSetting> Datas;
    }
}
