using System.Collections.Generic;
using YooAsset;

namespace GameFrame
{
    public class PackageSetting
    {
        public string name;
        public EPlayMode playMode;
        public string copyFromName; // 是哪个包裹的复制
        public bool needCopy;
    }

    public static class YooConst
    {
#if REMOTE_SERVICE
        public const string ResUrl = "https://127.0.0.1";
#else
        public const string ResUrl = "http://127.0.0.1";
#endif

        public static readonly List<PackageSetting> PackageSettings = new List<PackageSetting>()
        {
                new PackageSetting()
                {
                        name = "DefaultPackage",
#if UNITY_EDITOR
                        playMode = EPlayMode.EditorSimulateMode,
#elif UNITY_WEBGL
                playMode = EPlayMode.WebPlayMode,
#else
                playMode = EPlayMode.HostPlayMode,
#endif
                },
        };

        public enum PlatformName
        {
            Android,
            iOS,
            WebGL,
            Windows,
            WxWebGL
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