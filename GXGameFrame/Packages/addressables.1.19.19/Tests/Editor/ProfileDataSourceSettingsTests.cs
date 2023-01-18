using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests
{
    public class ProfileDataSourceSettingsTest
    {
        protected const string k_TestConfigName = "ProfileDataSourceSettings.Tests";

        protected string TestFolderName => $"{GetType()}_Tests";
        protected string TestFolder => $"Assets/{TestFolderName}";
        protected string ConfigFolder => TestFolder + "/Config";

        protected ProfileDataSourceSettings m_Settings;
        protected ProfileDataSourceSettings Settings
        {
            get
            {
                if (m_Settings == null)
                    m_Settings = ProfileDataSourceSettings.Create(ConfigFolder, k_TestConfigName);
                return m_Settings;
            }
        }

        [OneTimeSetUp]
        public void Init()
        {
            if (Directory.Exists(TestFolder))
            {
                Debug.Log($"{GetType()} (init) - deleting {TestFolder}");
                if (!AssetDatabase.DeleteAsset(TestFolder))
                    Directory.Delete(TestFolder);
            }
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            if (Directory.Exists(TestFolder))
            {
                Debug.Log($"{GetType()} - (cleanup) deleting {TestFolder}");
                AssetDatabase.DeleteAsset(TestFolder);
            }
            EditorBuildSettings.RemoveConfigObject(k_TestConfigName);
        }

        [Test]
        public void CreateSettings_Returns_NotNull()
        {
            Assert.NotNull(Settings);
        }

        [Test]
        public void CreateDefaultGroups_Returns_DefaultGroups()
        {
            var result = ProfileDataSourceSettings.CreateDefaultGroupTypes();
            Assert.NotNull(result);
        }

        [Test]
        public void ValidFindGroupType_Returns_ValidGroup()
        {
            var result = Settings.FindGroupType(Settings.profileGroupTypes.First());
            Assert.NotNull(result);
        }

        [Test]
        public void InvalidFindGroupType_Returns_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Settings.FindGroupType(new ProfileGroupType("Test Group")));
        }

        [Test]
        public void NonExistentGroupFindGroupType_Returns_Null()
        {
            ProfileGroupType nonexistentGroup = new ProfileGroupType("Test");
            bool v1Added = nonexistentGroup.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kBuildPath, "Test Build Path"));
            bool v2Added = nonexistentGroup.AddVariable(new ProfileGroupType.GroupTypeVariable(AddressableAssetSettings.kLoadPath, "Test Load Path"));
            Assert.IsTrue(v1Added && v2Added, "Failed to add the variables for GroupTypes");
            var result = Settings.FindGroupType(nonexistentGroup);
            Assert.IsNull(result);
        }

        [Test]
        public void GetGroupTypesByPrefix_Returns_ValidList()
        {
            var results = Settings.GetGroupTypesByPrefix("Built-In");
            Assert.True(results.Count == 1);
        }
    }
}
