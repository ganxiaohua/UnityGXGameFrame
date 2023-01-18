using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.U2D;
using UnityEngine.U2D;
namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetReferenceTests : AddressableAssetTestBase
    {
        private string m_ScriptableObjectPath;
        private string m_SpriteAtlasPath;
        private string m_TexturePath;
        TestObject mainSO;
        TestSubObject subSO;
        TestSubObject subSO2;

        protected override void OnInit()
        {
            mainSO = ScriptableObject.CreateInstance<TestObject>();
            subSO = ScriptableObject.CreateInstance<TestSubObject>();
            subSO2 = ScriptableObject.CreateInstance<TestSubObject>();
            subSO.name = "sub";

            m_ScriptableObjectPath = GetAssetPath("testScriptableObject.asset");
            AssetDatabase.CreateAsset(mainSO, m_ScriptableObjectPath);
            AssetDatabase.AddObjectToAsset(subSO, m_ScriptableObjectPath);
            AssetDatabase.AddObjectToAsset(subSO2, m_ScriptableObjectPath);
            AssetDatabase.ImportAsset(m_ScriptableObjectPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            // create a Sprite atlas, + sprite
            m_SpriteAtlasPath = GetAssetPath("testAtlas.spriteatlas");
            SpriteAtlas spriteAtlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(spriteAtlas, m_SpriteAtlasPath);

            Texture2D texture = Texture2D.whiteTexture;
            byte[] data = texture.EncodeToPNG();
            m_TexturePath = GetAssetPath("testTexture.png");
            File.WriteAllBytes(m_TexturePath, data);
            AssetDatabase.ImportAsset(m_TexturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            TextureImporter importer = TextureImporter.GetAtPath(m_TexturePath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            SpriteAtlasExtensions.Add(spriteAtlas, new[] { AssetDatabase.LoadAssetAtPath<Texture>(m_TexturePath) });
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget, false);
        }

        [Test]
        public void VerifySetEditorAsset_DoesNotMakeRefAssetDirty()
        {
            //Setup
            AssetReference reference = new AssetReference();
            string path = AssetDatabase.GUIDToAssetPath(m_AssetGUID.ToString());
            GameObject o = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;

            //Test
            Assert.IsFalse(EditorUtility.IsDirty(reference.editorAsset)); // IsDirty(Object o) only available in 2019.1 or newer
            reference.SetEditorAsset(o);
            Assert.IsFalse(EditorUtility.IsDirty(reference.editorAsset));
        }

        [Test]
        public void AssetReference_SetMainAsset_ResetsSubAsset()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_ScriptableObjectPath);
            AssetReference typeReference = new AssetReference(guid);
            typeReference.SubObjectName = "sub";
            typeReference.SetEditorAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(m_AssetGUID.ToString())));
            Assert.IsNull(typeReference.SubObjectName);
        }

        [Test]
        public void AssetReference_SetEditorAsset_NullsOnAttemptWithWrongTypeFromDerivedType()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_ScriptableObjectPath);
            AssetReference typeConflictReference = new AssetReferenceT<Animation>("badguid");
            typeConflictReference.SetEditorAsset(mainSO);
            Assert.IsNull(typeConflictReference.editorAsset, "Attempting to set editor asset on an AssetReferenceT should return null if the types do not match.");
        }

        [Test]
        public void AssetReference_SetEditorAsset_SucceedsOnMatchedTypeAssetReference()
        {
            AssetReference typeCorrectReference = new AssetReferenceT<TestSubObject>("badguid");
            typeCorrectReference.SetEditorAsset(subSO);
            Assert.NotNull(typeCorrectReference.editorAsset, "Attempting to set editor asset on an AssetReferenceT should return the first matching object at the guid");
            Assert.AreEqual(subSO, typeCorrectReference.editorAsset, "Attempting to set editor asset on an AssetReferenceT should return the first matching object at the guid");
        }

        [Test]
        public void AssetReference_SetEditorAsset_GetsRequestedAssetTypeOnUntypedAssetReference()
        {
            AssetReference untypedAssetReference = new AssetReference("badguid");
            untypedAssetReference.SetEditorAsset(mainSO);
            Assert.AreEqual(mainSO, untypedAssetReference.editorAsset, "Attempting to set editor asset on an untyped AssetReference should return the requested object");
        }
        
        [Test]
        public void AssetReference_SetEditorAsset_CorrectlyRetrievesSubAsset()
        {
            AssetReference untypedAssetReference = new AssetReference("badguid");
            untypedAssetReference.SetEditorAsset(subSO);
            Assert.AreEqual(mainSO, untypedAssetReference.editorAsset, "Attempting to use SetEditorAsset on untyped AssetReference should give main asset as editorAsset, subAsset as subAsset");
            Assert.AreEqual(subSO.name, untypedAssetReference.SubObjectName, "Attempting to use SetEditorAsset on a subObject in an untyped AssetReference should make the requested asset the subasset of the assetReference." );
        }

        [Test]
        public void AssetReference_SetEditorAsset_CorrectlySetsSubAssetWhenUsingTypedReference()
        {
            AssetReferenceT<TestSubObject> typedAssetReference = new AssetReferenceT<TestSubObject>("badguid");
            typedAssetReference.SetEditorAsset(subSO);
            Assert.AreEqual(subSO, typedAssetReference.editorAsset, "When using a typed asset reference, the editor asset should be set to the requested object even if its a subobject.");
            AssetReference typedAssetReference2 = new AssetReferenceT<TestSubObject>("badguid");
            typedAssetReference2.SetEditorAsset(subSO);
            Assert.AreEqual(subSO, typedAssetReference2.editorAsset, "When using a typed asset reference, the editor asset should be set to the requested object even if its a subobject.");
        }

        [Test]
        public void AssetReference_SetEditorAsset_ReturnsNullIfObjectTypeIsIncorrect()
        {
            AssetReferenceT<Sprite> incorrectlyTypedAssetReference = new AssetReferenceT<Sprite>("badguid");
            incorrectlyTypedAssetReference.SetEditorAsset(subSO);
            Assert.IsNull(incorrectlyTypedAssetReference.editorAsset, "Attempting to set an editor asset of an incorrect type should return null.");
        }

        [Test]
        public void AssetReference_SetEditorAsset_ReturnsCorrectObjectIfMultipleOfSameTypeExist()
        {
            AssetReferenceT<TestSubObject> typedAssetReference = new AssetReferenceT<TestSubObject>("badguid");
            typedAssetReference.SetEditorAsset(subSO2);
            Assert.AreEqual(subSO2, typedAssetReference.editorAsset, "When using a typed asset reference, the editor asset should be set to the requested object even if its a subobject.");
            Assert.AreNotEqual(subSO, typedAssetReference.editorAsset, "When using a typed asset reference, the editor asset should be set to specifically the requested object, not just an object with the same type and guid.");
        }
        

        [Test]
        public void AssetReferenceEditorAssetForSubObject_DifferentType()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_ScriptableObjectPath);
            AssetReferenceT<TestSubObject> typeReference = new AssetReferenceT<TestSubObject>(guid);
            typeReference.SubObjectName = "sub";

            //Test
            Assert.AreEqual(typeReference.editorAsset, AssetDatabase.LoadAssetAtPath<TestSubObject>(m_ScriptableObjectPath), "AssetReference with explicit type should get first instance of that type at that guid.");
            AssetReference asBase = typeReference;
            Assert.IsNotNull(asBase.editorAsset);
            Assert.AreEqual(asBase.editorAsset, AssetDatabase.LoadAssetAtPath<TestSubObject>(m_ScriptableObjectPath), "AssetReference with explicit type declared under generic AssetReference should still get the first instance of the specific type at the guid.");
            AssetReference baseReference = new AssetReference(guid);
            Assert.AreEqual(baseReference.editorAsset, AssetDatabase.LoadAssetAtPath<TestObject>(m_ScriptableObjectPath), "Generic AssetReference should get the asset of the main type at the guid.");
        }
        
        [Test]
        public void AssetReferenceEditorAssetForSubObject_NullIfIncorrectType()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_ScriptableObjectPath);
            AssetReferenceT<Animation> typeReference = new AssetReferenceT<Animation>(guid);
            typeReference.SubObjectName = "sub";

            //Test
            Assert.IsNull(typeReference.editorAsset, "Attempting to get an object of a type not located at a guid should return a null value.");
            AssetReference asBase = typeReference;
            Assert.IsNull(asBase.editorAsset, "Attempting to get an object type not located at a guid should return a null value even if the method of the generic AssetReference class is being called.");
            AssetReference baseReference = new AssetReference(guid);
            Assert.AreEqual(baseReference.editorAsset, AssetDatabase.LoadAssetAtPath<TestObject>(m_ScriptableObjectPath), "Generic AssetReference should get the asset of the main type at the guid.");
        }

        [Test]
        public void AssetReferenceEditorAssetForSubObject_Sprite()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_TexturePath);
            AssetReferenceSprite atlasReference = new AssetReferenceSprite(guid);
            atlasReference.SubObjectName = "testTexture";

            //Test
            Assert.IsNotNull(atlasReference.editorAsset);
        }

        [Test]
        public void AssetReferenceEditorAssetForSubObject_AtlasedSprite()
        {
            var guid = AssetDatabase.AssetPathToGUID(m_SpriteAtlasPath);
            AssetReferenceAtlasedSprite atlasReference = new AssetReferenceAtlasedSprite(guid);
            atlasReference.SubObjectName = "testTexture";

            //Test
            Assert.IsNotNull(atlasReference.editorAsset);
        }
        
        [Test]
        public void AssetReferenceNoAsset_CreatesCorrectLabelForType()
        {
            // Base Types
            string expected = "None (Addressable Asset)";
            string val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReference));
            Assert.AreEqual(expected, val, "General Asset string expected for an unrestricted AssetReference");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReference[]));
            Assert.AreEqual(expected, val, "General Asset string expected for an unrestricted AssetReference");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(List<AssetReference>));
            Assert.AreEqual(expected, val, "General Asset string expected for an unrestricted AssetReference");

            expected = "None (Addressable GameObject)";
            // generic types
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReferenceT<GameObject>));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReferenceT<GameObject>[]));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(List<AssetReferenceT<GameObject>>));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
            
            // inherited types
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReferenceGameObject));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(AssetReferenceGameObject[]));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
            val = AssetReferenceDrawerUtilities.ConstructNoAssetLabel(typeof(List<AssetReferenceGameObject>));
            Assert.AreEqual(expected, val, "Type restricted is expected in display string shown");
        }
    }
}
