using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.TestTools;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.Build.Content;
using BuildCompression = UnityEngine.BuildCompression;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetsBundleBuildParametersTests : AddressableAssetTestBase
    {
        protected override bool PersistSettings { get { return false; } }

        static IEnumerable<Enum> GetValues(Type t)
        {
            List<Enum> enumerations = new List<Enum>();
            foreach (FieldInfo fieldInfo in t.GetFields(BindingFlags.Static | BindingFlags.Public))
                enumerations.Add((Enum)fieldInfo.GetValue(null));
            return enumerations;
        }

        [Test]
        public void WhenNonRecursiveBuildingSet_BuildParametersHaveCorrectValue()
        {
#if !NONRECURSIVE_DEPENDENCY_DATA
            Assert.Ignore($"Skipping test {nameof(WhenNonRecursiveBuildingSet_BuildParametersHaveCorrectValue)}.");
#else
            var bundleToAssetGroup = new Dictionary<string, string>();

            Settings.NonRecursiveBuilding = true;
            var testParams = new AddressableAssetsBundleBuildParameters(Settings, bundleToAssetGroup, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Unused");
            Assert.AreEqual(testParams.NonRecursiveDependencies, Settings.NonRecursiveBuilding);

            Settings.NonRecursiveBuilding = false;
            testParams = new AddressableAssetsBundleBuildParameters(Settings, bundleToAssetGroup, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Unused");
            Assert.AreEqual(testParams.NonRecursiveDependencies, Settings.NonRecursiveBuilding);
#endif
        }

        [Test]
        public void WhenCompressionSetForGroups_GetCompressionForIdentifier_ReturnsExpectedCompression()
        {
            var bundleToAssetGroup = new Dictionary<string, string>();
            var expectedValues = new BuildCompression[] { BuildCompression.Uncompressed, BuildCompression.LZ4, BuildCompression.LZMA, BuildCompression.UncompressedRuntime, BuildCompression.LZ4Runtime };
            var bundleNames = new List<string>();

            foreach (var en in GetValues(typeof(BundledAssetGroupSchema.BundleCompressionMode)))
            {
                var g = Settings.CreateGroup(en.ToString(), true, false, false, null, typeof(BundledAssetGroupSchema));
                g.GetSchema<BundledAssetGroupSchema>().Compression = (BundledAssetGroupSchema.BundleCompressionMode)en;
                var bName = "bundle_" + en;
                bundleToAssetGroup.Add(bName, g.Guid);
                bundleNames.Add(bName);
            }
            var testParams = new AddressableAssetsBundleBuildParameters(Settings, bundleToAssetGroup, BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Unused");

            for (int i = 0; i < bundleNames.Count; i++)
            {
                var comp = testParams.GetCompressionForIdentifier(bundleNames[i]);
                Assert.AreEqual(expectedValues[i].blockSize, comp.blockSize);
                Assert.AreEqual(expectedValues[i].compression, comp.compression);
                Assert.AreEqual(expectedValues[i].level, comp.level);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void StripUnityVersion_SetsBuildFlagCorrectly(bool stripUnityVersion)
        {
            bool oldValue = Settings.StripUnityVersionFromBundleBuild;
            Settings.StripUnityVersionFromBundleBuild = stripUnityVersion;

            var testParams = new AddressableAssetsBundleBuildParameters(Settings, new Dictionary<string, string>(),
                BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Unused");

            var buildSettings = testParams.GetContentBuildSettings();

            Assert.AreEqual(stripUnityVersion, (buildSettings.buildFlags & ContentBuildFlags.StripUnityVersion) != 0);

            Settings.StripUnityVersionFromBundleBuild = oldValue;
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WhenDisableVisibleSubAssetRepresentationsSet_BuildParametersHaveCorrectValue(bool disableVisibleSubAssetRepresentations)
        {
            bool oldValue = Settings.DisableVisibleSubAssetRepresentations;
            Settings.DisableVisibleSubAssetRepresentations = disableVisibleSubAssetRepresentations;

            var testParams = new AddressableAssetsBundleBuildParameters(Settings, new Dictionary<string, string>(),
                BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone, "Unused");

            Assert.AreEqual(disableVisibleSubAssetRepresentations, testParams.DisableVisibleSubAssetRepresentations);

            Settings.DisableVisibleSubAssetRepresentations = oldValue;
        }
    }
}
