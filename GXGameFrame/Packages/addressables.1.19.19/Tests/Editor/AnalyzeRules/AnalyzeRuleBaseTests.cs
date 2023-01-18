using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Tests;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests.AnalyzeRules
{
    [TestFixture]
    public class AnalyzeRuleBaseTests : AddressableAssetTestBase
    {
        [Test]
        public void ConvertBundleNamesToGroupNames()
        {
            var bundleName = "2398471298347129034_bundlename_1";
            var fakeFileName = "archive://3912983hf9sdf902340jidf";
            var convertedBundleName = "group1_bundlename_1";

            var group = Settings.CreateGroup("group1", false, false, false, null, typeof(BundledAssetGroupSchema));

            AddressableAssetsBuildContext context = new AddressableAssetsBuildContext();
            context.Settings = Settings;
            context.assetGroupToBundles = new Dictionary<AddressableAssetGroup, List<string>>()
            {
                {group, new List<string>() {bundleName}}
            };

            BundleRuleBase baseRule = new BundleRuleBase();
            baseRule.m_ExtractData = new ExtractDataTask();

            var field = typeof(ExtractDataTask).GetField("m_WriteData", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(baseRule.m_ExtractData, new BundleWriteData());

            baseRule.m_AllBundleInputDefs.Add(new AssetBundleBuild()
            {
                assetBundleName = bundleName
            });

            baseRule.m_ExtractData.WriteData.FileToBundle.Add(fakeFileName, bundleName);
            baseRule.ConvertBundleNamesToGroupNames(context);

            Assert.AreEqual(convertedBundleName, baseRule.m_ExtractData.WriteData.FileToBundle[fakeFileName]);

            Settings.RemoveGroup(group);
        }

        [Test]
        public void BaseAnalyzeRule_DoesNotThrowOnFix()
        {
            BundleRuleBase baseRule = new BundleRuleBase();
            Assert.DoesNotThrow(() => baseRule.FixIssues(Settings));
        }
    }
}
