using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace UnityEditor.AddressableAssets.Tests
{
    class EditorAddressableAssetsTestFixture
    {
        EditorBuildSettingsScene[] m_PreviousScenes;
        protected AddressableAssetSettings m_Settings;

        protected const string TempPath = "Assets/TempGen";

        [SetUp]
        public void Setup()
        {
            DirectoryUtility.DeleteDirectory(TempPath, onlyIfEmpty: false, recursiveDelete: true);
            Directory.CreateDirectory(TempPath);
            m_Settings = AddressableAssetSettings.Create(Path.Combine(TempPath, "Settings"), "AddressableAssetSettings.Tests", true, true);
            m_PreviousScenes = EditorBuildSettings.scenes;
        }

        [TearDown]
        public void Teardown()
        {
            // Many of the tests keep recreating assets in the same path, so we need to unload them completely so they don't get reused by the next test
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_Settings));
            Resources.UnloadAsset(m_Settings);
            DirectoryUtility.DeleteDirectory(TempPath, onlyIfEmpty: false, recursiveDelete: true);
            EditorBuildSettings.scenes = m_PreviousScenes;
            AssetDatabase.Refresh();
        }

        protected static string CreateAsset(string assetPath, string objectName = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName ?? Path.GetFileNameWithoutExtension(assetPath);
            //this is to ensure that bundles are different for every run.
            go.transform.localPosition = UnityEngine.Random.onUnitSphere;
            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            UnityEngine.Object.DestroyImmediate(go, false);
            return AssetDatabase.AssetPathToGUID(assetPath);
        }

        protected static string CreateScene(string scenePath, string sceneName = null, bool addToBuild = true)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            scene.name = sceneName ?? Path.GetFileNameWithoutExtension(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);

            //Clear out the active scene so it doesn't affect tests
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            if (addToBuild)
            {
                var list = new List<EditorBuildSettingsScene>() { new EditorBuildSettingsScene(scenePath, true)};
                SceneManagerState.AddScenesForPlayMode(list);
            }
            return AssetDatabase.AssetPathToGUID(scenePath);
        }

        protected string CreateTexture(string texturePath, int size = 32)
        {
            var texture = new Texture2D(size, size);
            var data = ImageConversion.EncodeToPNG(texture);
            UnityEngine.Object.DestroyImmediate(texture);
            File.WriteAllBytes(texturePath, data);
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            return AssetDatabase.AssetPathToGUID(texturePath);
        }

        protected string CreateSpriteTexture(string texturePath, int size = 32)
        {
            string guid = CreateTexture(texturePath, size);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            importer.textureType = TextureImporterType.Sprite; // creates a sprite subobject
            importer.SaveAndReimport();
            return guid;
        }

        protected string CreateSpriteAtlas(string atlasPath, string spriteTextureGuid)
        {
            return CreateSpriteAtlas(atlasPath, new[] { spriteTextureGuid });
        }

        protected string CreateSpriteAtlas(string atlasPath, string[] spriteTextureGuids)
        {
            var sa = new SpriteAtlas();
            AssetDatabase.CreateAsset(sa, atlasPath);
            Object[] targetObjects = spriteTextureGuids.Select(g => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            sa.Add(targetObjects);
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { sa }, EditorUserBuildSettings.activeBuildTarget, false);

            AssetDatabase.Refresh();
            return AssetDatabase.AssetPathToGUID(atlasPath);
        }
    }
}
