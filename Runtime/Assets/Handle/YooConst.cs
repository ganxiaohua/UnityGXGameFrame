using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public static class YooConst
    {
        private static YooPackageSettingList sDataList;

        public static string ResUrl
        {
            get
            {
                GetDataList();
#if REMOTE_SERVICE
                return sDataList.RemoteResUrl;
#else
                return sDataList.DebugResUrl;
#endif
            }
        }

        public static List<PackageSetting> PackageSettings
        {
            get
            {
                GetDataList();
                return sDataList.Datas;
            }
        }

        public enum PlatformName
        {
            Android,
            iOS,
            WebGL,
            Windows,
            WxWebGL
        }

        public static void GetDataList()
        {
            if (sDataList != null)
                return;
            sDataList = Resources.Load<YooPackageSettingList>("YooAssetsData");
            if (sDataList == null)
            {
                Debug.LogError("Please go to Resources to generate assets GX框架工具/YooAssetsData");
            }
        }

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            var activeBuildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            return activeBuildTarget switch
            {
                UnityEditor.BuildTarget.Android => PlatformName.Android.ToString(),
                UnityEditor.BuildTarget.iOS => PlatformName.iOS.ToString(),
                UnityEditor.BuildTarget.WebGL => PlatformName.WebGL.ToString(),
                _ => PlatformName.Windows.ToString(),
            };
#else
            var platform = UnityEngine.Application.platform;
            return platform switch
            {
                UnityEngine.RuntimePlatform.Android => PlatformName.Android.ToString(),
                UnityEngine.RuntimePlatform.IPhonePlayer => PlatformName.iOS.ToString(),
                UnityEngine.RuntimePlatform.WebGLPlayer => PlatformName.WebGL.ToString(),
                _ => PlatformName.Windows.ToString(),
            };
#endif
        }
    }
}