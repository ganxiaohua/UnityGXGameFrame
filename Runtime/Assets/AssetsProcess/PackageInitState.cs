using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace GameFrame.Runtime
{
    public class PackageInitState : FsmState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            InitializationOperation().Forget();
        }

        private async UniTask InitializationOperation()
        {
            var playMode = (EPlayMode) GetData("playMode");
            var packageName = (string) GetData("packageName");
            var contentType = (PackageContentType) GetData("contentType");

            try
            {
                if (!YooAssets.TryGetPackage(packageName, out var package))
                    package = YooAssets.CreatePackage(packageName);

                if (package.InitializeStatus != EOperationStatus.Succeeded)
                {
                    InitializePackageOptions options;
                    switch (playMode)
                    {
                        case EPlayMode.EditorSimulateMode:
                            var bundleType = contentType == PackageContentType.RawFile
                                    ? EBundleType.VirtualRawBundle
                                    : EBundleType.VirtualAssetBundle;
                            var buildResult = EditorSimulateBuildInvoker.Build(packageName, (int) bundleType);
                            options = new EditorSimulateModeOptions
                            {
                                EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(buildResult.PackageRootDirectory),
                            };
                            break;
                        case EPlayMode.OfflinePlayMode:
                            options = new OfflinePlayModeOptions
                            {
                                BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(),
                            };
                            break;
                        case EPlayMode.HostPlayMode:
                            var hostServer = GetHostServerURL();
                            var remoteServices = new RemoteServices(hostServer, hostServer);
                            options = new HostPlayModeOptions
                            {
                                BuiltinFileSystemParameters = FileSystemParameters.CreateDefaultBuiltinFileSystemParameters(),
                                CacheFileSystemParameters = FileSystemParameters.CreateDefaultSandboxFileSystemParameters(remoteServices),
                            };
                            break;
                        case EPlayMode.WebPlayMode:
                            options = new WebPlayModeOptions
                            {
                                WebServerFileSystemParameters = FileSystemParameters.CreateDefaultWebServerFileSystemParameters(),
                            };
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported play mode for {packageName}: {playMode}");
                    }

                    var initializationOperation = package.InitializePackageAsync(options);
                    await initializationOperation.ToUniTask();
                    if (initializationOperation.Status != EOperationStatus.Succeeded)
                    {
                        Debug.LogWarning(initializationOperation.Error);
                        EventData.Instance.FireAssetEvent(AssetEventType.InitFail, packageName);
                        return;
                    }
                }

                ChangeState<PackageVersionUpdateState>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EventData.Instance.FireAssetEvent(AssetEventType.InitFail, packageName);
            }
        }


        private string GetHostServerURL()
        {
            //不同的流程组合不同.
            return $"{YooConst.ResUrl}/{YooConst.GetPlatformName()}";
        }

        /// <summary>
        /// 远端资源地址查询服务类
        /// </summary>
        private class RemoteServices : IRemoteService
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;

            public RemoteServices(string defaultHostServer, string fallbackHostServer)
            {
                _defaultHostServer = defaultHostServer;
                _fallbackHostServer = fallbackHostServer;
            }

            public IReadOnlyList<string> GetRemoteUrls(string fileName)
            {
                return new[]
                {
                    $"{_defaultHostServer}/{fileName}",
                    $"{_fallbackHostServer}/{fileName}",
                };
            }
        }
    }
}
