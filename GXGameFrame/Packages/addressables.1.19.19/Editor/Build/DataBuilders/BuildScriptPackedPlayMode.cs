using System;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    /// <summary>
    /// Uses data built by BuildScriptPacked class.  This script just sets up the correct variables and runs.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildScriptPackedPlayMode.asset", menuName = "Addressables/Content Builders/Use Existing Build (requires built groups)")]
    public class BuildScriptPackedPlayMode : BuildScriptBase
    {
        /// <inheritdoc />
        public override string Name
        {
            get
            {
                return "Use Existing Build (requires built groups)";
            }
        }

        private bool m_DataBuilt;

        /// <inheritdoc />
        public override void ClearCachedData()
        {
            m_DataBuilt = false;
        }

        /// <inheritdoc />
        public override bool IsDataBuilt()
        {
            return m_DataBuilt;
        }

        /// <inheritdoc />
        public override bool CanBuildData<T>()
        {
            return typeof(T).IsAssignableFrom(typeof(AddressablesPlayModeBuildResult));
        }

        /// <inheritdoc />
        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var settingsPath = Addressables.BuildPath + "/settings.json";
            var buildLogsPath = Addressables.BuildPath + "/buildLogs.json";
            if (!File.Exists(settingsPath))
            {
                IDataBuilderResult resE = new AddressablesPlayModeBuildResult() { Error = "Player content must be built before entering play mode with packed data.  This can be done from the Addressables window in the Build->Build Player Content menu command." };
                return (TResult)resE;
            }
            var rtd = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(settingsPath));
            if (rtd == null)
            {
                IDataBuilderResult resE = new AddressablesPlayModeBuildResult() { Error = string.Format("Unable to load initialization data from path {0}.  This can be done from the Addressables window in the Build->Build Player Content menu command.", settingsPath) };
                return (TResult)resE;
            }

            PackedPlayModeBuildLogs buildLogs = new PackedPlayModeBuildLogs();
            BuildTarget dataBuildTarget = BuildTarget.NoTarget;
            if (!Enum.TryParse(rtd.BuildTarget, out dataBuildTarget))
            {
                buildLogs.RuntimeBuildLogs.Add(new PackedPlayModeBuildLogs.RuntimeBuildLog(LogType.Warning,
                    $"Unable to parse build target from initialization data: '{rtd.BuildTarget}'."));
            }

            else if (BuildPipeline.GetBuildTargetGroup(dataBuildTarget) != BuildTargetGroup.Standalone)
            {
                buildLogs.RuntimeBuildLogs.Add(new PackedPlayModeBuildLogs.RuntimeBuildLog(LogType.Warning,
                    $"Asset bundles built with build target {dataBuildTarget} may not be compatible with running in the Editor."));
            }

            if(buildLogs.RuntimeBuildLogs.Count > 0)
                File.WriteAllText(buildLogsPath, JsonUtility.ToJson(buildLogs));

            //TODO: detect if the data that does exist is out of date..
            var runtimeSettingsPath = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/settings.json";
            PlayerPrefs.SetString(Addressables.kAddressablesRuntimeDataPath, runtimeSettingsPath);
            PlayerPrefs.SetString(Addressables.kAddressablesRuntimeBuildLogPath, buildLogsPath);
            IDataBuilderResult res = new AddressablesPlayModeBuildResult() { OutputPath = settingsPath, Duration = timer.Elapsed.TotalSeconds };
            m_DataBuilt = true;
            return (TResult)res;
        }
    }
}
