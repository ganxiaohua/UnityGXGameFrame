using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class GroupSchemaTests : AddressableAssetTestBase
    {
        CustomTestSchema m_TestSchemaObject;
        CustomTestSchemaSubClass m_TestSchemaObjectSubClass;
        protected override bool PersistSettings { get { return true; } }
        protected override void OnInit()
        {
            m_TestSchemaObject = ScriptableObject.CreateInstance<CustomTestSchema>();
            AssetDatabase.CreateAsset(m_TestSchemaObject, GetAssetPath("testSchemaObject.asset"));
            m_TestSchemaObjectSubClass = ScriptableObject.CreateInstance<CustomTestSchemaSubClass>();
            AssetDatabase.CreateAsset(m_TestSchemaObjectSubClass, GetAssetPath("testSchemaObjectSubClass.asset"));
        }

        private static string ObjectToFilename(UnityEngine.Object obj)
        {
            string guid;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out long lfid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == null)
                return null;

            return Path.GetFileName(path);
        }

        [Test]
        public void CanAddSchemaWithSavedAsset()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var newSchema = group.AddSchema(m_TestSchemaObject);
            Assert.IsNotNull(newSchema);
            Assert.AreNotEqual(newSchema, m_TestSchemaObject);
            Assert.IsTrue(group.HasSchema(m_TestSchemaObject.GetType()));
            Assert.IsTrue(group.RemoveSchema(m_TestSchemaObject.GetType()));
        }

        [Test]
        public void CanAddSchemaWithSavedAssetGeneric()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var newSchema = group.AddSchema(m_TestSchemaObject);
            Assert.IsNotNull(newSchema);
            Assert.AreNotEqual(newSchema, m_TestSchemaObject);
            Assert.IsTrue(group.HasSchema<CustomTestSchema>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
        }

        [Test]
        public void CanAddSchemaWithNonSavedAsset()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var templateSchema = ScriptableObject.CreateInstance<CustomTestSchema>();
            var newSchema = group.AddSchema(templateSchema);
            Assert.IsNotNull(newSchema);
            Assert.AreNotEqual(newSchema, templateSchema);
            Assert.IsTrue(group.HasSchema(templateSchema.GetType()));
            Assert.IsTrue(group.RemoveSchema(templateSchema.GetType()));
        }

        [Test]
        public void CanAddAndRemoveSchemaObjectByType()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var s = group.AddSchema(typeof(CustomTestSchema));
            Assert.IsNotNull(s);
            string guid;
            long lfid;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out guid, out lfid));
            var path = AssetDatabase.GUIDToAssetPath(guid);
            FileAssert.Exists(path);
            Assert.IsTrue(group.RemoveSchema(typeof(CustomTestSchema)));
            FileAssert.DoesNotExist(path);
        }

        [Test]
        public void CanAddAndRemoveSchemaObjectByGenericType()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var s = group.AddSchema<CustomTestSchema>();
            Assert.IsNotNull(s);
            string guid;
            long lfid;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out guid, out lfid));
            var path = AssetDatabase.GUIDToAssetPath(guid);
            FileAssert.Exists(path);
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
            FileAssert.DoesNotExist(path);
        }

        [Test]
        public void CanCheckSchemaObjectByGenericType()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            Assert.IsNotNull(group.AddSchema<CustomTestSchema>());
            Assert.IsTrue(group.HasSchema<CustomTestSchema>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
        }

        [Test]
        public void CanCheckSchemaObjectAsSubclass()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            Assert.IsNotNull(group.AddSchema<CustomTestSchemaSubClass>());
            Assert.IsFalse(group.HasSchema<CustomTestSchema>());
            Assert.IsTrue(group.HasSchema<CustomTestSchemaSubClass>());
            Assert.IsFalse(group.RemoveSchema<CustomTestSchema>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchemaSubClass>());
        }

        [Test]
        public void CanCheckSchemaObjectAsBaseclass()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            Assert.IsNotNull(group.AddSchema<CustomTestSchema>());
            Assert.IsTrue(group.HasSchema<CustomTestSchema>());
            Assert.IsFalse(group.HasSchema<CustomTestSchemaSubClass>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
            Assert.IsFalse(group.RemoveSchema<CustomTestSchemaSubClass>());
        }

        [Test]
        public void CanNotAddDuplicateSchemaObjects()
        {
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var added = group.AddSchema<CustomTestSchemaSubClass>();
            Assert.IsNotNull(added);
            Assert.AreEqual(added, group.AddSchema<CustomTestSchemaSubClass>());
            Assert.IsNotNull(group.AddSchema<CustomTestSchema>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchemaSubClass>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
        }

        [Test]
        public void WhenCreatingNewGroup_SchemaAndSchemaSubclassUseGroupName()
        {
            // Set up
            var group = Settings.CreateGroup("TestGroup", false, false, false, null);
            var testSchema = group.AddSchema<CustomTestSchema>();
            var testSchemaSubClass = group.AddSchema<CustomTestSchemaSubClass>();

            string testSchemaFilename = ObjectToFilename(testSchema);
            string testSchemaSubClassFilename = ObjectToFilename(testSchemaSubClass);

            // Test
            Assert.IsTrue(testSchemaFilename.Contains("TestGroup"));
            Assert.IsTrue(testSchemaSubClassFilename.Contains("TestGroup"));

            // Cleanup
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
            Assert.IsTrue(group.RemoveSchema<CustomTestSchemaSubClass>());
        }

        [Test]
        public void ModifyingGroupName_ChangesSchemaAssetPath()
        {
            // Set up
            var group = Settings.CreateGroup("OldTestGroup", false, false, false, null);
            var testSchema = group.AddSchema<CustomTestSchema>();
            AssetDatabase.SaveAssets();

            string testSchemaFilename = ObjectToFilename(testSchema);
            Assert.IsTrue(testSchemaFilename.Contains("OldTestGroup"));

            // Test
            group.Name = "NewTestGroup";
            Assert.AreEqual("NewTestGroup", group.name);

            testSchemaFilename = ObjectToFilename(testSchema);
            Assert.IsTrue(testSchemaFilename.Contains("NewTestGroup"));

            // Cleanup
            Assert.IsTrue(group.RemoveSchema<CustomTestSchema>());
        }
    }

    class BundledAssetGroupSchemaTests : EditorAddressableAssetsTestFixture
    {
        [Test]
        public void BundledAssetGroupSchema_OnSetGroup_SendsWarningsForNullBuildAndLoadPath()
        {
            AddressableAssetGroup group = null;
            try
            {
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalLoadPath));
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalBuildPath));
                group = m_Settings.CreateGroup("Group1", false, false, false, null, typeof(BundledAssetGroupSchema));
                LogAssert.Expect(LogType.Warning, "Default path variable " + AddressableAssetSettings.kLocalBuildPath + " not found when initializing BundledAssetGroupSchema. Please manually set the path via the groups window.");
                LogAssert.Expect(LogType.Warning, "Default path variable " + AddressableAssetSettings.kLocalLoadPath + " not found when initializing BundledAssetGroupSchema. Please manually set the path via the groups window.");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalBuildPathValue);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalLoadPath, AddressableAssetSettings.kLocalLoadPathValue);
            }
        }

        [Test]
        public void BundledAssetGroupSchema_BuildLoadPathsCanBeSetByReference()
        {
            AddressableAssetGroup group = null;
            try
            {
                //Set up new profile state
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalLoadPath));
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalBuildPath));
                m_Settings.profileSettings.CreateValue("LocalLoadPath", "loadDefault1");
                m_Settings.profileSettings.CreateValue("LocalBuildPath", "buildDefault1");

                //Create group with BundledAssetGroupSchema
                group = m_Settings.CreateGroup("Group1", false, false, false, null, typeof(BundledAssetGroupSchema));
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                List<string> variableNames = m_Settings.profileSettings.GetVariableNames();
                SetBundledAssetGroupSchemaPaths(m_Settings, schema.BuildPath,  AddressableAssetSettings.kLocalBuildPath, "LocalBuildPath", variableNames);
                SetBundledAssetGroupSchemaPaths(m_Settings, schema.LoadPath, AddressableAssetSettings.kLocalLoadPath, "LocalLoadPath",  variableNames);

                var expectedLoadPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "LocalLoadPath");
                var expectedBuildPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "LocalBuildPath");

                Assert.AreEqual(expectedLoadPath, schema.LoadPath.GetValue(m_Settings), "Load path was not properly set within BundledAssetGroupSchema.OnSetGroup");
                Assert.AreEqual(expectedBuildPath, schema.BuildPath.GetValue(m_Settings), "Build path was not properly set within BundledAssetGroupSchema.OnSetGroup");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalBuildPathValue);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalLoadPath, AddressableAssetSettings.kLocalLoadPathValue);
                m_Settings.profileSettings.RemoveValue("LocalLoadPath");
                m_Settings.profileSettings.RemoveValue("LocalBuildPath");
            }
        }

        internal void SetBundledAssetGroupSchemaPaths(AddressableAssetSettings settings, ProfileValueReference pvr, string newVariableName, string oldVariableName, List<string> variableNames)
        {
            if (variableNames.Contains(newVariableName))
                pvr.SetVariableByName(settings, newVariableName);
            else if (variableNames.Contains(oldVariableName))
                pvr.SetVariableByName(settings, oldVariableName);
            else
                Debug.LogWarning("Default path variable not found when initializing BundledAssetGroupSchema. Please manually set the path via the groups window.");
        }

        [Test]
        public void BundledAssetGroupSchema_OnSetGroup_DefaultsToOldVariablesIfNewNotPresent()
        {
            AddressableAssetGroup group = null;
            try
            {
                //Set up new profile state
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalLoadPath));
                m_Settings.profileSettings.RemoveValue(m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalBuildPath));
                m_Settings.profileSettings.CreateValue("LocalLoadPath", "loadDefault1");
                m_Settings.profileSettings.CreateValue("LocalBuildPath", "buildDefault1");
                var defaultId = m_Settings.profileSettings.GetProfileId("Default");
                var profile1Id = m_Settings.profileSettings.AddProfile("BundledAssetGroupSchemaTestsProfile1", defaultId);


                //Create group with BundledAssetGroupSchema
                group = m_Settings.CreateGroup("Group1", false, false, false, null, typeof(BundledAssetGroupSchema));

                Assert.AreEqual("loadDefault1", m_Settings.profileSettings.GetValueByName(profile1Id, "LocalLoadPath"), "Old variables value was not properly transferred over to new variable.");
                Assert.AreEqual("buildDefault1", m_Settings.profileSettings.GetValueByName(profile1Id, "LocalBuildPath"), "Old variables value was not properly transferred over to new variable.");
                var schema = group.GetSchema<BundledAssetGroupSchema>();

                var expectedLoadPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "LocalLoadPath");
                var expectedBuildPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "LocalBuildPath");

                Assert.AreEqual(expectedLoadPath, schema.LoadPath.GetValue(m_Settings), "Load path was not properly set within BundledAssetGroupSchema.OnSetGroup");
                Assert.AreEqual(expectedBuildPath, schema.BuildPath.GetValue(m_Settings), "Build path was not properly set within BundledAssetGroupSchema.OnSetGroup");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalBuildPathValue);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalLoadPath, AddressableAssetSettings.kLocalLoadPathValue);
                m_Settings.profileSettings.RemoveValue("LocalLoadPath");
                m_Settings.profileSettings.RemoveValue("LocalBuildPath");
            }
        }

        [Test]
        public void BundledAssetGroupSchema_OnSetGroup_GivenChoiceOfNewAndOldVariablesChoosesNew()
        {
            AddressableAssetGroup group = null;
            try
            {
                //Set up new profile state
                m_Settings.profileSettings.CreateValue("LocalLoadPath", "WrongLoadValue");
                m_Settings.profileSettings.CreateValue("LocalBuildPath", "WrongBuildValue");
                m_Settings.profileSettings.SetValue(m_Settings.activeProfileId, "Local.LoadPath", "CorrectLoadValue");
                m_Settings.profileSettings.SetValue(m_Settings.activeProfileId, "Local.BuildPath", "CorrectBuildValue");

                //Create group with BundledAssetGroupSchema
                group = m_Settings.CreateGroup("BundledAssetGroupSchemaTestGroup1", false, false, false, null, typeof(BundledAssetGroupSchema));

                var schema = group.GetSchema<BundledAssetGroupSchema>();
                var expectedLoadPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "Local.LoadPath");
                var expectedBuildPath = m_Settings.profileSettings.GetValueByName(m_Settings.activeProfileId, "Local.BuildPath");

                var loadValue = schema.LoadPath.GetValue(m_Settings);
                var buildValue = schema.BuildPath.GetValue(m_Settings);

                Assert.AreEqual(expectedLoadPath, loadValue, "Load path was not properly set within BundledAssetGroupSchema.OnSetGroup");
                Assert.AreEqual(expectedBuildPath, buildValue, "Build path was not properly set within BundledAssetGroupSchema.OnSetGroup");
                Assert.AreEqual("CorrectLoadValue", loadValue, "Path value is correctly set to the value of Local.LoadPath");
                Assert.AreEqual("CorrectBuildValue", buildValue, "Build value is correctly set to the value of Local.BuildPath");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalBuildPathValue);
                m_Settings.profileSettings.CreateValue(AddressableAssetSettings.kLocalLoadPath, AddressableAssetSettings.kLocalLoadPathValue);
                m_Settings.profileSettings.RemoveValue("LocalLoadPath");
                m_Settings.profileSettings.RemoveValue("LocalBuildPath");
            }
        }

        [Test]
        public void BundledAssetGroupSchema_OnSetGroup_SetsVariableAsExpectedWhenUserHasCorrectBuildPath()
        {
            AddressableAssetGroup group = null;
            try
            {
                //Group should default to having the correct default values
                group = m_Settings.CreateGroup("Group1", false, false, false, null, typeof(BundledAssetGroupSchema));
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                var activeProfileId = m_Settings.activeProfileId;
                var expectedLoadValuePreEvaluate = m_Settings.profileSettings.GetValueByName(activeProfileId, "Local.LoadPath");
                var expectedBuildValuePreEvaluate = m_Settings.profileSettings.GetValueByName(activeProfileId, "Local.BuildPath");
                Assert.AreEqual(m_Settings.profileSettings.EvaluateString(activeProfileId, expectedLoadValuePreEvaluate), schema.LoadPath.GetValue(m_Settings), "Value is not correctly set in the basic case where user has the default load path already created.");
                Assert.AreEqual(m_Settings.profileSettings.EvaluateString(activeProfileId, expectedBuildValuePreEvaluate), schema.BuildPath.GetValue(m_Settings), "Value is not correctly set in the basic case where user has the default build path already created.");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
            }
        }

        [Test]
        public void BundledAssetGroupSchema_SetPathVariable_ProperlySetsReferenceOnPath()
        {
            AddressableAssetGroup group = null;
            ProfileValueReference pathValue = null;
            BundledAssetGroupSchema schema = null;
            try
            {
                Type[] types = new Type[] {};
                group = m_Settings.CreateGroup("Group1", false, false, false, null, types);
                List<string> variableNames = new List<string>();
                variableNames.Add("LocalBuildPath");
                variableNames.Add(AddressableAssetSettings.kLocalBuildPath);
                schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
                schema.SetPathVariable(group.Settings, ref pathValue, AddressableAssetSettings.kLocalBuildPath, "LocalBuildPath", variableNames);
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                if (schema != null)
                    ScriptableObject.DestroyImmediate(schema);
            }
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("", null)]
        public void BundledAssetGroupSchema_Validate_PreservesOldPathCustomValues(string loadId, string buildId)
        {
            AddressableAssetGroup group = null;
            AddressableAssetProfileSettings.BuildProfile profile = m_Settings.profileSettings.GetProfile(m_Settings.activeProfileId);
            AddressableAssetProfileSettings profileParent = profile.m_ProfileParent;

            profileParent.CreateValue("LocalLoadPath", "loadpath");
            profileParent.CreateValue("LocalBuildPath", "buildpath");

            profileParent.RemoveValue(profileParent.GetVariableId(AddressableAssetSettings.kLocalLoadPath));
            profileParent.RemoveValue(profileParent.GetVariableId(AddressableAssetSettings.kLocalBuildPath));

            try
            {
                group = m_Settings.CreateGroup("BundledAssetGroupSchemaTestGroup1", false, false, false, null, typeof(BundledAssetGroupSchema));

                string expectedLoadPath = m_Settings.profileSettings.GetVariableId("LocalLoadPath");
                string expectedBuildPath = m_Settings.profileSettings.GetVariableId("LocalBuildPath");
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.LoadPath.Id = loadId;
                schema.BuildPath.Id = buildId;

                profile.m_ProfileParent = null;
                schema.Validate();

                var loadValue = schema.LoadPath.Id;
                var buildValue = schema.BuildPath.Id;
                Assert.AreEqual(expectedLoadPath, loadValue, $"Load path not properly set in BundledAssetGroupSchema.Validate");
                Assert.AreEqual(expectedBuildPath, buildValue, $"Build path not properly set in BundledAssetGroupSchema.Validate");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                profile.m_ProfileParent = profileParent;
                profile.m_ProfileParent.RemoveValue(profileParent.GetVariableId("LocalLoadPath"));
                profile.m_ProfileParent.RemoveValue(profileParent.GetVariableId("LocalBuildPath"));
                profile.m_ProfileParent.CreateValue(AddressableAssetSettings.kLocalLoadPath, AddressableAssetSettings.kLocalLoadPathValue);
                profile.m_ProfileParent.CreateValue(AddressableAssetSettings.kLocalBuildPath, AddressableAssetSettings.kLocalBuildPathValue);
            }
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase(null, "")]
        [TestCase("", null)]
        public void BundledAssetGroupSchema_Validate_PreservesNewPathCustomValues(string loadId, string buildId)
        {
            AddressableAssetGroup group = null;
            AddressableAssetProfileSettings.BuildProfile profile = m_Settings.profileSettings.GetProfile(m_Settings.activeProfileId);
            AddressableAssetProfileSettings profileParent = profile.m_ProfileParent;
            try
            {
                group = m_Settings.CreateGroup("BundledAssetGroupSchemaTestGroup1", false, false, false, null, typeof(BundledAssetGroupSchema));

                string expectedLoadPath = m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalLoadPath);
                string expectedBuildPath = m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kLocalBuildPath);
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.LoadPath.Id = loadId;
                schema.BuildPath.Id = buildId;

                profile.m_ProfileParent = null;
                schema.Validate();

                var loadValue = schema.LoadPath.Id;
                var buildValue = schema.BuildPath.Id;
                Assert.AreEqual(expectedLoadPath, loadValue, $"Load path not properly set in BundledAssetGroupSchema.Validate");
                Assert.AreEqual(expectedBuildPath, buildValue, $"Build path not properly set in BundledAssetGroupSchema.Validate");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                profile.m_ProfileParent = profileParent;
            }
        }

        [Test]
        public void BundledAssetGroupSchema_Validate_PreservesCustomPaths()
        {
            AddressableAssetGroup group = null;
            AddressableAssetProfileSettings.BuildProfile profile = m_Settings.profileSettings.GetProfile(m_Settings.activeProfileId);
            AddressableAssetProfileSettings profileParent = profile.m_ProfileParent;
            try
            {
                group = m_Settings.CreateGroup("BundledAssetGroupSchemaTestGroup1", false, false, false, null, typeof(BundledAssetGroupSchema));

                string expectedLoadPath = "<undefined>";
                string expectedBuildPath = "C://special/path";
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.LoadPath.Id = expectedLoadPath;
                schema.BuildPath.Id = expectedBuildPath;

                profile.m_ProfileParent = null;
                schema.Validate();

                var loadValue = schema.LoadPath.Id;
                var buildValue = schema.BuildPath.Id;
                Assert.AreEqual(expectedLoadPath, loadValue, $"Load path reset to {AddressableAssetSettings.kLocalLoadPath} in BundledAssetGroupSchema.Validate");
                Assert.AreEqual(expectedBuildPath, buildValue, $"Build path reset to {AddressableAssetSettings.kLocalBuildPath} in BundledAssetGroupSchema.Validate");
            }
            finally
            {
                if (group != null)
                    m_Settings.RemoveGroupInternal(group, true, false);
                profile.m_ProfileParent = profileParent;
            }
        }
    }
}
