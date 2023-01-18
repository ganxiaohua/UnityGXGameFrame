using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.U2D;
#endif

static class AddressablesTestUtility
{
    static public void Reset(AddressablesImpl aa)
    {
        aa.ClearResourceLocators();
        aa.ResourceManager.ResourceProviders.Clear();
        aa.InstanceProvider = null;
    }

    static public void TearDown(string testType, string pathFormat, string suffix)
    {
#if UNITY_EDITOR
        Reset(Addressables.Instance);
        var RootFolder = string.Format(pathFormat, testType, suffix);
        AssetDatabase.DeleteAsset(RootFolder);
#endif
    }

    static public string GetPrefabLabel(string suffix) { return "prefabs" + suffix; }
    static public string GetPrefabAlternatingLabel(string suffix, int index) { return string.Format("prefabs_{0}{1}", ((index % 2) == 0) ? "even" : "odd", suffix); }
    static public string GetPrefabUniqueLabel(string suffix, int index) { return string.Format("prefab_{0}{1}", index, suffix); }
    public const int kPrefabCount = 10;
    public const int kMaxWebRequestCount = 5;

    static public void Setup(string testType, string pathFormat, string suffix)
    {
#if UNITY_EDITOR
        bool currentIgnoreState = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;

        var RootFolder = string.Format(pathFormat, testType, suffix);

        Directory.CreateDirectory(RootFolder);

        var settings = AddressableAssetSettings.Create(RootFolder + "/Settings", "AddressableAssetSettings.Tests", false, true);
        settings.MaxConcurrentWebRequests = kMaxWebRequestCount;
        var group = settings.FindGroup("TestStuff" + suffix);

        if (group == null)
            group = settings.CreateGroup("TestStuff" + suffix, true, false, false, null, typeof(BundledAssetGroupSchema));
        group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;
        settings.DefaultGroup = group;
        for (int i = 0; i < kPrefabCount; i++)
        {
            var guid = CreateAsset(RootFolder + "/test" + i + suffix + ".prefab", "testPrefab" + i);
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = Path.GetFileNameWithoutExtension(entry.AssetPath);

            entry.SetLabel(GetPrefabLabel(suffix), true, true);
            entry.SetLabel(GetPrefabAlternatingLabel(suffix, i), true, true);
            entry.SetLabel(GetPrefabUniqueLabel(suffix, i), true, true);
            entry.SetLabel("mixed", true, true, false);
        }

        var texture = new Texture2D(32, 32);
        var data = ImageConversion.EncodeToPNG(texture);
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.GenerateUniqueAssetPath(RootFolder);
        var spritePath = RootFolder + "/sprite.png";
        File.WriteAllBytes(spritePath, data);

        AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

        var spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritesheet = new SpriteMetaData[] { new SpriteMetaData() { name = "topleft", pivot = Vector2.zero, rect = new Rect(0, 0, 16, 16) },
                                                      new SpriteMetaData() { name = "botright", pivot = Vector2.zero, rect = new Rect(16, 16, 16, 16) }};
        importer.SaveAndReimport();

        var spriteEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(spritePath), group, false, false);
        spriteEntry.address = "sprite";

        var so = ScriptableObject.CreateInstance<UnityEngine.AddressableAssets.Tests.TestObject>();
        var sub = ScriptableObject.CreateInstance<UnityEngine.AddressableAssets.Tests.TestObject>();
        sub.name = "sub-shown";
        var sub2 = ScriptableObject.CreateInstance<UnityEngine.AddressableAssets.Tests.TestObject>();
        sub2.hideFlags |= HideFlags.HideInHierarchy;
        sub2.name = "sub2-hidden";
        so.name = "main";
        AssetDatabase.CreateAsset(so, RootFolder + "/assetWithSubObjects.asset");
        AssetDatabase.AddObjectToAsset(sub, RootFolder + "/assetWithSubObjects.asset");
        AssetDatabase.AddObjectToAsset(sub2, RootFolder + "/assetWithSubObjects.asset");
        AssetDatabase.ImportAsset(RootFolder + "/assetWithSubObjects.asset", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        var assetWithSubObjectsGUID = AssetDatabase.AssetPathToGUID(RootFolder + "/assetWithSubObjects.asset");
        string assetRefGuid = CreateAsset(RootFolder + "/testIsReference.prefab", "IsReference");
        GameObject go = new GameObject("AssetReferenceBehavior");
        AssetReferenceTestBehavior aRefTestBehavior = go.AddComponent<AssetReferenceTestBehavior>();
        var scriptableObject = settings.CreateOrMoveEntry(assetWithSubObjectsGUID, settings.DefaultGroup);
        scriptableObject.address = "assetWithSubObjects";
        scriptableObject.SetLabel("mixed", true, true, false);
        aRefTestBehavior.Reference = settings.CreateAssetReference(assetRefGuid);
        aRefTestBehavior.ReferenceWithSubObject = settings.CreateAssetReference(assetWithSubObjectsGUID);
        aRefTestBehavior.ReferenceWithSubObject.SubObjectName = "sub-shown";
        aRefTestBehavior.LabelReference = new AssetLabelReference()
        {
            labelString = settings.labelTable.labelNames[0]
        };

        string hasBehaviorPath = RootFolder + "/AssetReferenceBehavior.prefab";
        
        //AssetDatabase.StopAssetEditing();

        ScriptableObject assetWithDifferentTypedSubAssets = ScriptableObject.CreateInstance<UnityEngine.AddressableAssets.Tests.TestObject>();
        AssetDatabase.CreateAsset(assetWithDifferentTypedSubAssets, $"{RootFolder}/assetWithDifferentTypedSubAssets.asset");

        Material mat = new Material(Shader.Find("Transparent/Diffuse"));
        Mesh mesh = new Mesh();
        AssetDatabase.AddObjectToAsset(mat, assetWithDifferentTypedSubAssets);
        AssetDatabase.AddObjectToAsset(mesh, assetWithDifferentTypedSubAssets);

        AssetDatabase.ImportAsset($"{RootFolder}/assetWithDifferentTypedSubAssets.asset", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        var assetWithDifferentTypedSubObjectsGUID = AssetDatabase.AssetPathToGUID($"{RootFolder}/assetWithDifferentTypedSubAssets.asset");
        var multiTypedSubAssetsEntry = settings.CreateOrMoveEntry(assetWithDifferentTypedSubObjectsGUID, settings.DefaultGroup);
        multiTypedSubAssetsEntry.address = "assetWithDifferentTypedSubAssets";
        aRefTestBehavior.ReferenceWithMultiTypedSubObject = settings.CreateAssetReference(multiTypedSubAssetsEntry.guid);
        aRefTestBehavior.ReferenceWithMultiTypedSubObjectSubReference = settings.CreateAssetReference(multiTypedSubAssetsEntry.guid);
        aRefTestBehavior.ReferenceWithMultiTypedSubObjectSubReference.SetEditorSubObject(mat);

        PrefabUtility.SaveAsPrefabAsset(go, hasBehaviorPath);
        settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(hasBehaviorPath), group, false, false);
        
        CreateFolderEntryAssets(RootFolder, settings, group);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        RunBuilder(settings, testType, suffix);
        LogAssert.ignoreFailingMessages = currentIgnoreState;

#endif
    }

#if UNITY_EDITOR
	static void CreateFolderEntryAssets(string RootFolder, AddressableAssetSettings settings, AddressableAssetGroup group)
	{
		AssetDatabase.CreateFolder(RootFolder, "folderEntry");
		string folderPath = RootFolder + "/folderEntry";

		{
			var texture = new Texture2D(32, 32);
			var data = ImageConversion.EncodeToPNG(texture);
			UnityEngine.Object.DestroyImmediate(texture);
			AssetDatabase.GenerateUniqueAssetPath(RootFolder);
			var spritePath = folderPath + "/spritesheet.png";
			File.WriteAllBytes(spritePath, data);

			AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

			var spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
			var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
			importer.textureType = TextureImporterType.Sprite;
			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritesheet = new SpriteMetaData[] { new SpriteMetaData() { name = "topleft", pivot = Vector2.zero, rect = new Rect(0, 0, 16, 16) },
				new SpriteMetaData() { name = "botright", pivot = Vector2.zero, rect = new Rect(16, 16, 16, 16) }};
			importer.SaveAndReimport();
		}

		{
			var texture = new Texture2D(32, 32);
			var data = ImageConversion.EncodeToPNG(texture);
			UnityEngine.Object.DestroyImmediate(texture);

			var spritePath = folderPath + "/sprite.png";
			File.WriteAllBytes(spritePath, data);
			AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

			var spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
			var importer = (TextureImporter) AssetImporter.GetAtPath(spritePath);
			importer.textureType = TextureImporterType.Sprite;
			importer.SaveAndReimport();

			string atlasPath = folderPath + "/atlas.spriteatlas";
			var sa = new SpriteAtlas();
			AssetDatabase.CreateAsset(sa, atlasPath);
			sa.Add(new UnityEngine.Object[]
			{
				AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(spriteGuid))
			});
			SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] {sa}, EditorUserBuildSettings.activeBuildTarget, false);
		}

		var folderEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(folderPath), group, false, false);
		folderEntry.address = "folderEntry";
	}
	
    static string CreateAsset(string assetPath, string objectName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objectName;
        //this is to ensure that bundles are different for every run.
        go.transform.localPosition = UnityEngine.Random.onUnitSphere;
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        UnityEngine.Object.DestroyImmediate(go, false);
        return AssetDatabase.AssetPathToGUID(assetPath);
    }

    static void RunBuilder(AddressableAssetSettings settings, string testType, string suffix)
    {
        var buildContext = new AddressablesDataBuilderInput(settings);
        buildContext.RuntimeSettingsFilename = "settings" + suffix + ".json";
        buildContext.RuntimeCatalogFilename = "catalog" + suffix + ".json";

        foreach (var db in settings.DataBuilders)
        {
            var b = db as IDataBuilder;
            if (b.GetType().Name != testType)
                continue;
            buildContext.PathFormat = "{0}" + Addressables.LibraryPath + "{1}_" + testType + "_TEST_" + suffix + ".json";
            b.BuildData<AddressableAssetBuildResult>(buildContext);
            PlayerPrefs.SetString(Addressables.kAddressablesRuntimeDataPath + testType, PlayerPrefs.GetString(Addressables.kAddressablesRuntimeDataPath, ""));
        }
    }

#endif
}
