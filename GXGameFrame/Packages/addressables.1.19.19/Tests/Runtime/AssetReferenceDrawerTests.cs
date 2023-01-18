using NUnit.Framework;
using System;
using System.Collections;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine;
using UnityEngine.TestTools;
namespace AssetReferenceDrawerTests
{
    public abstract class AssetReferenceDrawerTests : AddressablesTestFixture
    {
        const string textureName = "testTexture";
        readonly string[] allowedLabels = new string[] { "label1", "label2" };

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            var group = settings.CreateGroup("TestStuff", true, false, false, null, typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
            Directory.CreateDirectory(tempAssetFolder);
            var texturePath = Path.Combine(tempAssetFolder, string.Concat(GetBuildScriptTypeFromMode(BuildScriptMode), textureName, ".png"));
            CreateTextureOnPath(texturePath);
            var texEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(texturePath), group, false, false);
            texEntry.address = textureName;
            texEntry.SetLabel(allowedLabels[0], true, true);
        }

        void CreateTextureOnPath(string spritePath)
        {
            var data = ImageConversion.EncodeToPNG(new Texture2D(32, 32));
            File.WriteAllBytes(spritePath, data);
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

#endif
        string LabelsToString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var t in allowedLabels)
            {
                if (!first)
                    sb.Append(',');
                first = false;
                sb.Append(t);
            }
            return sb.ToString();
        }

        [Test]
        public void AssetReferenceUIRestriction_WithoutUNITYEDITOR_Initializes()
        {
            AssetReferenceUILabelRestriction uiLabelRest = new AssetReferenceUILabelRestriction(allowedLabels);
            string restAllowedLabels = uiLabelRest.ToString();
            Assert.IsNotEmpty(restAllowedLabels);
            Assert.AreEqual(restAllowedLabels, LabelsToString());
        }

        [UnityTest]
        public IEnumerator AssetReferenceUILabelRestriction_WithoutUNITYEDITOR_Validates()
        {
            var op = m_Addressables.LoadAssetAsync<Texture>(textureName);
            yield return op;
            AssetReferenceUILabelRestriction uiLabelRest = new AssetReferenceUILabelRestriction(allowedLabels);
            Texture tex = op.Result;
            bool isValid = uiLabelRest.ValidateAsset(tex);
            Assert.IsTrue(isValid);
            m_Addressables.Release(op);
        }

#if UNITY_EDITOR
        [Test]
        public void AssetReferenceUIRestriction_Surrogate_Initializes()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(typeof(AssetReferenceUILabelRestriction));
            Assert.IsNotNull(surrogate);
            var surrogateInstance = Activator.CreateInstance(surrogate) as AssetReferenceUIRestrictionSurrogate;
            Assert.IsNotNull(surrogateInstance);
            surrogateInstance.Init(new AssetReferenceUILabelRestriction());
        }

        [Test]
        public void AssetReferenceUtilityGetSurrogate_PassNullArgument_ReturnsNull()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(null);
            Assert.IsNull(surrogate);
            LogAssert.Expect(LogType.Error, "targetType cannot be null");
        }

        [Test]
        public void AssetReferenceUtilityGetSurrogate_PassInvalidType_ReturnsNull()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(typeof(NUnit.Env));
            Assert.IsNull(surrogate);
        }

        [Test]
        public void AssetReferenceUtilityGetSurrogate_PassValidType_ReturnsSurrogate()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(typeof(AssetReferenceUILabelRestriction));
            Assert.IsNotNull(surrogate);
        }

        [Test]
        public void AssetReferenceUtilityGetSurrogate_IfSurrogateExistsInDifferentAssembly_ReturnsSurrogate()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(typeof(AssetReferenceUILabelRestriction));
            Assert.AreEqual(typeof(AssetReferenceUILabelRestrictionSurrogate), surrogate);
        }

        [Test]
        public void AssetReferenceUtilityGetSurrogate_IfSurrogateNotStronglyTyped_ReturnsSurrogateLowestInHierarchy()
        {
            var surrogate = AssetReferenceUtility.GetSurrogate(typeof(TestAssetReferenceUICustomRestriction));
            Assert.AreEqual(typeof(TestAssetReferenceUIRestrictionSurrogate3), surrogate);
        }

#endif
    }

#if UNITY_EDITOR

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    class TestAssetReferenceUICustomRestriction : AssetReferenceUIRestriction
    {}

    class TestAssetReferenceUICustomRestrictionSurrogate : AssetReferenceUIRestrictionSurrogate
    {}

    class TestAssetReferenceUIRestrictionSurrogate1 : TestAssetReferenceUICustomRestrictionSurrogate
    {}

    class TestAssetReferenceUIRestrictionSurrogate2 : TestAssetReferenceUIRestrictionSurrogate1
    {}

    class TestAssetReferenceUIRestrictionSurrogate3 : TestAssetReferenceUIRestrictionSurrogate2
    {}

    class AssetReferenceDrawerTests_FastMode : AssetReferenceDrawerTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class AssetReferenceDrawerTests_VirtualMode : AssetReferenceDrawerTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class AssetReferenceDrawerTests_PackedPlaymodeMode : AssetReferenceDrawerTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class AssetReferenceDrawerTests_PackedMode : AssetReferenceDrawerTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
