using System.IO;
using NUnit.Framework;
using UnityEngine.ResourceManagement;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
#endif

namespace AddressableTests.FastModeInitTests
{
#if UNITY_EDITOR
    public class FastModeInitializationTests : AddressablesTestFixture
    {
        protected override TestBuildScriptMode BuildScriptMode => TestBuildScriptMode.Fast;

        [TestCase(true)]
        [TestCase(false)]
        public void FastModeInit_EventViewerSetup_InitializesPostProfilerEventsValue(bool sendProfilerEvents)
        {
            //Setup
            bool originalValue = ProjectConfigData.PostProfilerEvents;
            ProjectConfigData.PostProfilerEvents = sendProfilerEvents;
            var settings = AddressableAssetSettings.Create(Path.Combine(GetGeneratedAssetsPath(), "Settings"), "AddressableAssetSettings.Tests", false, true);
           
            //Test
            FastModeInitializationOperation fmInit = new FastModeInitializationOperation(m_Addressables, settings);
            fmInit.InvokeExecute();

            //Assert
            Assert.AreEqual(sendProfilerEvents, m_Addressables.ResourceManager.postProfilerEvents);

            //Cleanup
            ProjectConfigData.PostProfilerEvents = originalValue;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FastModeInit_EventViewerSetup_InitializesDiagnosticEventCollectorCorrectly(bool sendProfilerEvents)
        {
            //Setup
            bool originalValue = ProjectConfigData.PostProfilerEvents;
            ProjectConfigData.PostProfilerEvents = sendProfilerEvents;
            var settings = AddressableAssetSettings.Create(Path.Combine(GetGeneratedAssetsPath(), "Settings"), "AddressableAssetSettings.Tests", false, true);

            //Test
            FastModeInitializationOperation fmInit = new FastModeInitializationOperation(m_Addressables, settings);
            fmInit.InvokeExecute();

            //Assert
            if(sendProfilerEvents)
                Assert.IsNotNull(fmInit.m_Diagnostics, "Diagnostic event collector was null when send profiler events was set to true.");
            else
                Assert.IsNull(fmInit.m_Diagnostics, "Diagnostic event collector was not null when send profiler events was false.");

            //Cleanup
            ProjectConfigData.PostProfilerEvents = originalValue;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FastModeInitialization_SetsExceptionHandlerToNull_WhenLogRuntimeExceptionsIsOff(bool logRuntimeExceptions)
        {
            //Setup
            var settings = AddressableAssetSettings.Create(Path.Combine(GetGeneratedAssetsPath(), "Settings"), "AddressableAssetSettings.Tests", false, true);
            settings.buildSettings.LogResourceManagerExceptions = logRuntimeExceptions;

            //Test
            FastModeInitializationOperation fmInit = new FastModeInitializationOperation(m_Addressables, settings);
            fmInit.InvokeExecute();

            //Assert
            Assert.AreEqual(logRuntimeExceptions, ResourceManager.ExceptionHandler != null);
        }
    }
#endif
}
