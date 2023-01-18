using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;

namespace UnityEditor.AddressableAssets.Tests
{
    public class BuildScriptPackedPlayTests : AddressableAssetTestBase
    {
        protected override void OnInit()
        {
            //Player data must be built before PackPlaymode can be used.
            var input = new AddressablesDataBuilderInput(Settings);
            ScriptableObject.CreateInstance<BuildScriptPackedMode>().BuildData<AddressableAssetBuildResult>(input);
        }

        [Test]
        public void PackedPlayModeScript_CannotBuildPlayerContent()
        {
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedPlayMode>();

            Assert.IsFalse(buildScript.CanBuildData<AddressablesPlayerBuildResult>());

            Assert.IsTrue(buildScript.CanBuildData<AddressableAssetBuildResult>());
            Assert.IsTrue(buildScript.CanBuildData<AddressablesPlayModeBuildResult>());
        }

        [Test]
        public void PackedPlayModeScript_AppendsBuildLog_ForNonStandaloneBuildTarget()
        {
            //Setup
            BuildScriptPackedPlayMode buildScript = ScriptableObject.CreateInstance<BuildScriptPackedPlayMode>();
            AddressablesDataBuilderInput input = new AddressablesDataBuilderInput(Settings);
            input.SetAllValues(Settings, BuildTargetGroup.Android, BuildTarget.Android, "");
            ScriptableObject.CreateInstance<BuildScriptPackedMode>().BuildData<AddressableAssetBuildResult>(input);

            //Test
            buildScript.BuildData<AddressablesPlayModeBuildResult>(input);
            var buildLogPath = Addressables.BuildPath + "/buildLogs.json";
            var logs = JsonUtility.FromJson<PackedPlayModeBuildLogs>(File.ReadAllText(buildLogPath));

            //Cleanup (done early in case of test failure)
            File.Delete(buildLogPath);

            //Assert
            Assert.AreEqual(1, logs.RuntimeBuildLogs.Count);
            Assert.AreEqual(LogType.Warning, logs.RuntimeBuildLogs[0].Type);
            Assert.AreEqual($"Asset bundles built with build target {input.Target} may not be compatible with running in the Editor.", logs.RuntimeBuildLogs[0].Message);
        }

        [Test]
        public void PackedPlayModeScript_AppendsBuildLog_ForInvalidBuildTarget()
        {
            //Setup
            BuildScriptPackedPlayMode buildScript = ScriptableObject.CreateInstance<BuildScriptPackedPlayMode>();
            AddressablesDataBuilderInput input = new AddressablesDataBuilderInput(Settings);
            var settingsPath = Addressables.BuildPath + "/settings.json";
            var rtd = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(settingsPath));
            var buildLogPath = Addressables.BuildPath + "/buildLogs.json";
            
            string storedBuildTarget = rtd.BuildTarget;
            string invalidTarget = rtd.BuildTarget = "NotAValidBuildTarget";
            File.WriteAllText(settingsPath, JsonUtility.ToJson(rtd));

            //Test
            buildScript.BuildData<AddressablesPlayModeBuildResult>(input);
            var logs = JsonUtility.FromJson<PackedPlayModeBuildLogs>(File.ReadAllText(buildLogPath));

            //Cleanup (done early in case of test failure)
            File.Delete(buildLogPath);
            rtd.BuildTarget = storedBuildTarget;
            File.WriteAllText(settingsPath, JsonUtility.ToJson(rtd));

            //Assert
            Assert.AreEqual(1, logs.RuntimeBuildLogs.Count);
            Assert.AreEqual(LogType.Warning, logs.RuntimeBuildLogs[0].Type);
            Assert.AreEqual($"Unable to parse build target from initialization data: '{invalidTarget}'.", logs.RuntimeBuildLogs[0].Message);
        }
    }
}
