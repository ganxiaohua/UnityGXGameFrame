using System.Collections;
using System.IO;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets.Tests;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using UnityEngine.U2D;

namespace LegacyResourcesTests
{
    abstract class LegacyResourceTests : AddressablesTestFixture
    {
        private const string kSpriteResourceName = "testSprite";
        private const string kSpriteAtlasResourceName = "testAtlas";
        private const string kObjectResourceName = "subfolder/testObject";
        
#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string rootFolder)
        {
	        var group = settings.CreateGroup("Legacy", true, false, false, null, typeof(PlayerDataGroupSchema));
            var schema = group.GetSchema<PlayerDataGroupSchema>();
            schema.IncludeResourcesFolders = true;
            schema.IncludeBuildSettingsScenes = false;
            var resourceEntry = settings.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, group);
            resourceEntry.IsInResources = true;
            
            string resourceDirectory = Path.Combine(rootFolder, "Resources");
            Directory.CreateDirectory(resourceDirectory+"/subfolder");
            
            var spritePath = Path.Combine(resourceDirectory, kSpriteResourceName+".png");
            CreateSpriteOnPath(spritePath);
            string spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
            SessionState.SetString("spriteGuid", spriteGuid);
            
            CreateScriptableObjectOnPath(Path.Combine(resourceDirectory, kObjectResourceName+".asset"));
            var atlasPath = Path.Combine(resourceDirectory, kSpriteAtlasResourceName+".spriteatlas");
            CreateSpriteAtlas(atlasPath, new string[] {spritePath});
        }

        void CreateSpriteOnPath(string spritePath)
        {
            CreateTextureOnPath(spritePath);
            var importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        void CreateTextureOnPath(string spritePath)
        {
	        var texture = new Texture2D(32, 32);
            var data = ImageConversion.EncodeToPNG(texture);
            Object.DestroyImmediate(texture);
            File.WriteAllBytes(spritePath, data);
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        void CreateScriptableObjectOnPath(string path)
        {
	        AssetDatabase.CreateAsset(TestObject.Create("test"), path);
        }

        void CreateSpriteAtlas(string path, string[] spriteAssetPaths)
        {
	        var sa = new SpriteAtlas();
	        AssetDatabase.CreateAsset(sa, path);
	        foreach (string spritePath in spriteAssetPaths)
	        {
		        sa.Add(new UnityEngine.Object[]
		        {
			        AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(spritePath)
		        });
	        }
	        SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] {sa}, EditorUserBuildSettings.activeBuildTarget, false);
        }
#endif

        [UnityTest]
        public IEnumerator CanLoadFromResources_TextureSprite()
        {
            var op = m_Addressables.LoadAssetAsync<Sprite>(kSpriteResourceName);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.IsNotNull(op.Result);
            m_Addressables.Release(op);
        }
        
        [UnityTest]
        public IEnumerator CanLoadFromResources_ByGuid()
        {
#if UNITY_EDITOR
	        string resourceDirectory = Path.Combine(GetGeneratedAssetsPath(), "Resources");
	        Directory.CreateDirectory(resourceDirectory+"/subfolder");
	        string spriteGuid = AssetDatabase.AssetPathToGUID(Path.Combine(resourceDirectory, kSpriteResourceName + ".png"));
	        Assert.IsFalse(string.IsNullOrEmpty(spriteGuid));
	        var op = m_Addressables.LoadAssetAsync<Sprite>(spriteGuid);
	        yield return op;
	        Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
	        Assert.IsNotNull(op.Result);
	        m_Addressables.Release(op);
#else
	        UnityEngine.Debug.Log($"Skipping test {nameof(CanLoadFromResources_ByGuid)} due to running outside of Editor.");
			yield break;
#endif
        }

        [UnityTest]
        public IEnumerator WhenLoadingSpecificTypes_ObjectOfSpecifiedTypeIsReturned()
        {
            AsyncOperationHandle spriteOp = m_Addressables.LoadAssetAsync<Sprite>(kSpriteResourceName);
            AsyncOperationHandle texOp = m_Addressables.LoadAssetAsync<Texture>(kSpriteResourceName);
            yield return spriteOp;
            yield return texOp;
            Assert.AreEqual(typeof(Sprite), spriteOp.Result.GetType());
            Assert.AreEqual(typeof(Texture2D), texOp.Result.GetType());
            m_Addressables.Release(spriteOp);
            m_Addressables.Release(texOp);
        }
        
        [UnityTest]
        public IEnumerator CanLoadFromResources_ObjectWithPath()
        {
	        var op = m_Addressables.LoadAssetAsync<TestObject>(kObjectResourceName);
	        yield return op;
	        Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
	        Assert.IsNotNull(op.Result);
	        m_Addressables.Release(op);
        }
        
        [UnityTest]
        public IEnumerator CanLoadFromResources_AtlasedSprite()
        {
	        var op = m_Addressables.LoadAssetAsync<Sprite>(kSpriteAtlasResourceName+"["+kSpriteResourceName+"]");
	        yield return op;
	        Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
	        Assert.IsNotNull(op.Result.GetType());
	        m_Addressables.Release(op);
        }
    }

#if UNITY_EDITOR
    class LegacyResourceTests_FastMode : LegacyResourceTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class LegacyResourceTests_VirtualMode : LegacyResourceTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class LegacyResourceTests_PackedPlaymodeMode : LegacyResourceTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class LegacyResourceTests_PackedMode : LegacyResourceTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
