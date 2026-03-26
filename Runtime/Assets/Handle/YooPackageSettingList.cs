using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    [Serializable]
    public class PackageSetting
    {
        public string Name;
        public EPlayMode PlayMode;
    }

    [CreateAssetMenu(menuName = "GX框架工具/生成资源配置文件", fileName = "YooAssetsData")]
    public class YooPackageSettingList : ScriptableObject
    {
        public string RemoteResUrl;
        public string DebugResUrl;
        public List<PackageSetting> Datas;
    }
}