using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.Diagnostics;
using GameFrame.Editor;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using Debug = UnityEngine.Debug;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace GameFrame.Editor
{
    public static class BuildScript
    {
        public static bool isFullRes = false;
        public static bool isBuild = false;
        static readonly string DefaultProfileId = "7d72f42cde32f304595d34a6eab45502";
        static readonly string RemoteProfileId = "c0313e369c48c164b9b196ec6f7a7bc4";

        static readonly Dictionary<string, string> UpdateURL = new Dictionary<string, string>()
        {
            [DefaultProfileId] = "http://192.168.62.31/GxGame/aa",
        };

        static readonly string[] patchFilterFiles = new string[] {"catalog.bundle", "settings.json", "addressables_content_state.bin"};

        private static readonly double[] byteUnits =
        {
            1073741824.0, 1048576.0, 1024.0, 1
        };

        private static readonly string[] byteUnitsNames =
        {
            "GB", "MB", "KB", "B"
        };

        public static AddressableAssetSettings AssetSettings
        {
            get { return AddressableAssetSettingsDefaultObject.Settings; }
        }

        public static string BuildTarget
        {
            get { return PlatformMappingService.GetPlatformPathSubFolder(); }
        }

        private static AddressableAssetGroup FindGroup(string name, bool create = true)
        {
            foreach (var item in AssetSettings.groups)
            {
                if (item && item.name == name)
                {
                    return item;
                }
            }

            AddressableAssetGroup group = null;
            if (create)
            {
                AddressableAssetGroupTemplate addressableAssetGroupTemplate = AssetSettings.GroupTemplateObjects[0] as AddressableAssetGroupTemplate;
                group = AssetSettings.CreateGroup(name, false, false, false, AssetSettings.DefaultGroup.Schemas, addressableAssetGroupTemplate.GetTypes());
                addressableAssetGroupTemplate.ApplyToAddressableAssetGroup(group);
            }

            return group;
        }

        public static void CreateDirectory(string path)
        {
            string file = Path.GetFileName(path);
            var dir = path;
            if (file.Contains('.'))
            {
                dir = Path.GetDirectoryName(path);
            }

            if (string.IsNullOrEmpty(dir) || Directory.Exists(dir))
            {
                return;
            }

            Directory.CreateDirectory(dir);
        }

        public static void ProcessConfigGroup(AssetGroup assetGroup)
        {
            var targetPath =EditorString.ConfigFullPath;
            CreateDirectory(targetPath);
            string[] files = Directory.GetFileSystemEntries("GenerateDatas/bytes");

            foreach (string file in files)
            {
                string dstPath = targetPath + "/" + Path.GetFileName(file);
                if (!File.Exists(dstPath))
                {
                    File.Copy(file, dstPath);
                }
            }

            AssetDatabase.Refresh();
            var group = FindGroup(assetGroup.groupName);
            var guid = AssetDatabase.AssetPathToGUID(EditorString.ConfigAssetPath);
            var entry = AssetSettings.CreateOrMoveEntry(guid, group, false, false);
            entry.SetLabel(assetGroup.groupName, true, true);
            entry.SetLabel(AddressablesHelper.PreloadLabel, true, true);
        }

        public static void BuildBundles(bool withUpdate = false)
        {
            var log = new StringBuilder();
            var stopWatch = Stopwatch.StartNew();
            isBuild = true;

            ProcessAllAssetGroup();
            log.AppendLine($"process duration:{stopWatch.Elapsed}");

            var contentStateDataPath = ContentUpdateScript.GetContentStateDataPath(false);
            AddressablesPlayerBuildResult buildResult;
            if (!File.Exists(contentStateDataPath))
            {
                AddressableAssetSettings.BuildPlayerContent(out buildResult);
            }
            else
            {
                Debug.Log(contentStateDataPath);
                buildResult = ContentUpdateScript.BuildContentUpdate(AssetSettings, contentStateDataPath);
            }

            var patchLog = CopyBuildResultToPatchFolder(buildResult);
            stopWatch.Stop();
            log.AppendLine($"build duration:{buildResult.Duration}s");
            log.AppendLine($"total duration:{stopWatch.Elapsed}");
            Debug.Log($"build bundle success:{log}");
            log.AppendLine(patchLog);
            var logPath = $"ServerData/{BuildTarget}/{DateTime.Now:yyyyMMddHHmmss}.log";
            File.WriteAllText(logPath, log.ToString());
            string path = EditorString.ConfigFullPath;
            FileUtil.DeleteFileOrDirectory(path);
            FileUtil.DeleteFileOrDirectory(path + ".meta");
            AssetDatabase.Refresh();
        }

        public static bool IsExcludePath(string assetPath)
        {
            return assetPath.Contains("Resources");
        }

        public static void ProcessAssetGroupByName(string groupName)
        {
            var groupInfo = AssetGroupSettings.settings.data.Find((group) => { return group.groupName == groupName; });
            ProcessAssetGroup(groupInfo);
        }

        public static void ProcessAssetGroup(AssetGroup assetGroup)
        {
            if (assetGroup == null || string.IsNullOrEmpty(assetGroup.groupName))
                return;
            var group = FindGroup(assetGroup.groupName);
            var bags = group.GetSchema<BundledAssetGroupSchema>();
            bags.BundleMode = assetGroup.bundleMode;
            bags.Compression = assetGroup.compression;
            if (assetGroup.groupName == "Config" && isBuild)
            {
                ProcessConfigGroup(assetGroup);
                isBuild = false;
                return;
            }

            if (assetGroup.packByDir)
            {
                foreach (var path in assetGroup.searchPaths)
                {
                    Debug.Log(System.Environment.CurrentDirectory);
                    var dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                    {
                        if (IsExcludePath(dir))
                            continue;
                        //Debug.Log($"packByDir {dir}:{AssetDatabase.AssetPathToGUID(dir)}");
                        var guid = AssetDatabase.AssetPathToGUID(dir);
                        if (group != null)
                        {
                            var entry = AssetSettings.CreateOrMoveEntry(guid, group, false, false);
                            entry.SetLabel(AddressablesHelper.PreloadLabel, true, true);
                            entry.SetLabel(assetGroup.groupName, true, true);
                        }
                    }
                }
            }
            else
            {
                var guids = AssetDatabase.FindAssets(assetGroup.filter, assetGroup.searchPaths);
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (IsExcludePath(assetPath))
                        continue;
                    if (group != null)
                    {
                        var entry = AssetSettings.CreateOrMoveEntry(guid, group, false, false);
                        if (assetGroup.simplifyAddressName)
                        {
                            entry.SetAddress(Path.GetFileNameWithoutExtension(assetPath));
                        }

                        entry.SetLabel(AddressablesHelper.PreloadLabel, true, true);
                        entry.SetLabel(assetGroup.groupName, true, true);
                    }
                }
            }
        }

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            if (EditorApplication.timeSinceStartup < 30)
            {
                ProcessAllAssetGroup();
            }
        }

        public static void ProcessAllAssetGroup()
        {
            Debug.Log("===Start ProcessAllAssetGroup");
            foreach (var item in AssetGroupSettings.settings.data)
            {
                ProcessAssetGroup(item);
            }

            var duplicateGroup = FindGroup("Duplicate Asset Isolation", false);
            if (duplicateGroup != null)
            {
                foreach (var item in duplicateGroup.entries)
                {
                    item.SetLabel(AddressablesHelper.PreloadLabel, true, true);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static string FormatBytes(long bytes)
        {
            var size = "0 B";
            if (bytes == 0)
            {
                return size;
            }

            for (var index = 0; index < byteUnits.Length; index++)
            {
                var unit = byteUnits[index];
                if (!(bytes >= unit))
                {
                    continue;
                }

                size = $"{bytes / unit:##.##} {byteUnitsNames[index]}";
                break;
            }

            return size;
        }

        private static string CopyBuildResultToPatchFolder(AddressablesPlayerBuildResult buildResult)
        {
            var log = new StringBuilder();
            long totalSize = 0;
            if (buildResult == null)
                return "[buildResult is null]";
            var files = new List<FileInfo>();
            var nameLengthMax = 0;
            var lastVersions = ScriptableObject.CreateInstance<PlayerAssets>();
            var newVersions = ScriptableObject.CreateInstance<PlayerAssets>();
            var versionPath = GetAssetVersionPath();
            if (File.Exists(versionPath))
            {
                var versionJson = File.ReadAllText(versionPath);
                lastVersions = AddressablesHelper.LoadFromJson<PlayerAssets>(versionJson);
            }

            var patchFolder = $"Patch_{lastVersions.version + 1}";
            var toPath = $"ServerData/{BuildTarget}/{patchFolder}";
            CreateDirectory(toPath);
            foreach (var item in buildResult.FileRegistry.GetFilePaths())
            {
                if (Array.Exists(patchFilterFiles, item.Contains) || item.EndsWith(".log"))
                    continue;
                var filePath = item.Replace("\\", "/").Replace($"ServerData/{BuildTarget}/", string.Empty);
                if (filePath.EndsWith(".bundle"))
                {
                    //保留当前资源版本信息,不包含旧的版本信息
                    newVersions.data.Add(filePath);
                }

                if (lastVersions.data.Contains(filePath))
                {
                    //未修改资源不用复制到补丁目录
                    continue;
                }

                var path = Path.Combine(toPath, filePath);
                CreateDirectory(path);
                //进行压缩或者加密
                if (item.Contains(".json"))
                {
                    byte[] sourceByte = File.ReadAllBytes(item);
                    byte[] compressByte = TextDataProvider.CompressBytes(sourceByte);
                    File.WriteAllBytes(path, compressByte);
                }
                else
                {
                    File.Copy(item, path, true);
                }

                // CopyToLocalServer(path, filePath);
                var fileInfo = new FileInfo(item);
                if (fileInfo.Name.Length > nameLengthMax)
                {
                    nameLengthMax = fileInfo.Name.Length;
                }

                files.Add(fileInfo);
                totalSize += fileInfo.Length;
            }

            newVersions.version = lastVersions.version + 1;
            File.WriteAllText(versionPath, JsonUtility.ToJson(newVersions));
            var updateInfo = ScriptableObject.CreateInstance<UpdateInfo>();
            updateInfo.version = newVersions.version;
            updateInfo.timestamp = GetTimestamp();
            var updateFilePath = $"{toPath}/{UpdateInfo.Filename}";
            File.WriteAllText(updateFilePath, JsonUtility.ToJson(updateInfo));
            // CopyToLocalServer(updateFilePath, UpdateInfo.Filename);
            log.AppendLine($"totalSize:{FormatBytes(totalSize)}");
            files.Sort((a, b) => (int) (b.Length - a.Length));
            foreach (var file in files)
            {
                log.AppendLine($"{file.Name.PadRight(nameLengthMax)}\t{FormatBytes(file.Length)}");
            }

            // ModifyCDNConfig(toPath);
            return log.ToString();
        }

        private static string GetAssetVersionPath()
        {
            return $"ServerData/{BuildTarget}/versions.json";
        }

        public static void CopyToStreamingAssets(bool copyFile = true)
        {
            var versionPath = GetAssetVersionPath();
            if (!File.Exists(versionPath))
            {
                throw new FileNotFoundException();
            }

            var json = File.ReadAllText(versionPath);
            var versions = AddressablesHelper.LoadFromJson<PlayerAssets>(json);
            var playerAssets = ScriptableObject.CreateInstance<PlayerAssets>();
            playerAssets.version = versions.version;
            if (copyFile)
            {
                foreach (var fileName in versions.data)
                {
                    if (fileName.EndsWith(".bundle"))
                    {
                        var from = Path.Combine($"ServerData/{BuildTarget}", fileName);
                        if (File.Exists(from))
                        {
                            playerAssets.data.Add(fileName);
                            var to = AddressablesHelper.GetPlayerDataPath(fileName);
                            CreateDirectory(to);
                            File.Copy(from, to, true);
                        }
                    }
                }
            }

            json = JsonUtility.ToJson(playerAssets);
            var savePath = AddressablesHelper.GetPlayerDataPath(PlayerAssets.Filename);
            CreateDirectory(savePath);
            File.WriteAllText(savePath, json);
        }

        public static void CopyToLocalServer(string filePath, string relativePath)
        {
            if (AssetSettings.activeProfileId != DefaultProfileId)
                return;
            var to = AddressablesHelper.GetDownloadUrl(relativePath).Replace("http://", "\\\\").Replace("/", "\\");
            CreateDirectory(to);
            File.Copy(filePath, to, true);
        }

        public static long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        private static string GetTimeForNow()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private static string GetBuildTargetName(BuildTarget target)
        {
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var scriptBackend = PlayerSettings.GetScriptingBackend(buildTargetGroup);
            var fullRes = isFullRes ? "整包" : "微端";
            var targetName =
                $"{PlayerSettings.productName}-{fullRes}-v{PlayerSettings.bundleVersion}b{PlayerVersion.version.bundleVersionCode}-{PlayerSettings.applicationIdentifier}-{scriptBackend}-{GetTimeForNow()}";
            switch (target)
            {
                case UnityEditor.BuildTarget.Android:
                    return targetName + (EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk");
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    return targetName + ".exe";
                case UnityEditor.BuildTarget.StandaloneOSX:
                    return targetName + ".app";
                default:
                    return targetName;
            }
        }

        /// <summary>
        /// 修改构建版本号
        /// </summary>
        /// <param name="delta">默认加1,构建失败时减1</param>
        private static void ModifyVersionCode(int delta = 1)
        {
            var playerVersion = PlayerVersion.version;
            playerVersion.updateURL = UpdateURL[AssetSettings.activeProfileId];
            playerVersion.bundleVersionCode += delta;
            playerVersion.Save();
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            PlayerSettings.bundleVersion = playerVersion.bundleVersion;
            if (buildTarget == UnityEditor.BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode = playerVersion.bundleVersionCode;
            }
            else if (buildTarget == UnityEditor.BuildTarget.iOS)
            {
                PlayerSettings.iOS.buildNumber = playerVersion.bundleVersionCode.ToString();
            }
        }

        public static void BuildPlayer(bool isFullRes = true)
        {
            BuildScript.isFullRes = isFullRes;

            var levels = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    levels.Add(scene.path);
            }

            if (levels.Count == 0)
            {
                Debug.Log("Nothing to build.");
                return;
            }

            ModifyVersionCode();

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetName = GetBuildTargetName(buildTarget);
            if (targetName == null) return;

            var path = $"Bin/{BuildTarget}/{GetTimeForNow()}/{targetName}";

            var options = new BuildPlayerOptions
            {
                scenes = levels.ToArray(),
                locationPathName = path,
                target = buildTarget,
                options = EditorUserBuildSettings.development
                    ? BuildOptions.Development
                    : BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                ModifyVersionCode(-1);
                throw new Exception("build player failed!");
            }

            var serverUrl = UpdateURL[DefaultProfileId];
            serverUrl = serverUrl.Substring(0, serverUrl.LastIndexOf("/"));
            var to = $"{serverUrl}/Bin/{BuildTarget}/{DateTime.Now:yyyyMMdd}/{targetName}".Replace("http://", "\\\\").Replace("/", "\\");
            CreateDirectory(to);
            File.Copy(path, to, true);
        }

        public static void BuildSettings(string productName, string packageName, string version, bool IL2CPP)
        {
            if (string.IsNullOrEmpty(productName))
            {
                throw new ArgumentNullException(nameof(productName));
            }

            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (string.IsNullOrEmpty(packageName))
            {
                throw new ArgumentNullException(nameof(packageName));
            }

            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = version;
            PlayerSettings.applicationIdentifier = packageName;
            PlayerVersion.version.bundleVersion = version;
            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            PlayerSettings.SetScriptingBackend(buildTargetGroup, IL2CPP ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
        }

        public static void ModifyCDNConfig(string folder)
        {
            var patchPath = Path.GetFullPath(folder).Replace("\\", "\\\\");
            var filePath = Path.GetFullPath("Tools/cdn/config.toml");
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("LocalRoot="))
                {
                    lines[i] = $"LocalRoot=\"{patchPath}\"";
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);
        }

        public static void ActiveProfile(bool isLocal)
        {
            AssetSettings.activeProfileId = isLocal ? DefaultProfileId : RemoteProfileId;
        }
    }
}