using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEditor.Presets;

public class BuildLayoutGenerationTaskTests
{
    AddressableAssetSettings m_Settings;

    const string kTempPath = "Assets/TempGen";
    static string TempPath;
    static int ExecCount;
    bool m_PrevGenerateBuildLayout;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        ExecCount = 0;
    }

    [SetUp]
    public void Setup()
    {
        TempPath = kTempPath + (ExecCount++).ToString();
        if (File.Exists(BuildLayoutGenerationTask.m_LayoutTextFile))
            File.Delete(BuildLayoutGenerationTask.m_LayoutTextFile);
        m_PrevGenerateBuildLayout = ProjectConfigData.GenerateBuildLayout;
        BuildScriptPackedMode.s_SkipCompilePlayerScripts = true;
        ProjectConfigData.GenerateBuildLayout = true;
        if (Directory.Exists(TempPath))
            Directory.Delete(TempPath, true);
        Directory.CreateDirectory(TempPath);

        m_Settings = AddressableAssetSettings.Create(Path.Combine(TempPath, "Settings"), "AddressableAssetSettings.Tests", false, true);
    }

    [TearDown]
    public void Teardown()
    {
        BuildScriptPackedMode.s_SkipCompilePlayerScripts = false;
        ProjectConfigData.GenerateBuildLayout = m_PrevGenerateBuildLayout;
        // Many of the tests keep recreating assets in the same path, so we need to unload them completely so they don't get reused by the next test
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_Settings));
        Resources.UnloadAsset(m_Settings);

        FileUtil.DeleteFileOrDirectory(TempPath);
        FileUtil.DeleteFileOrDirectory(TempPath + ".meta");

        AssetDatabase.Refresh();
    }

    static string CreateAsset(string name)
    {
        string assetPath = $"{TempPath}/{name}.prefab";
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        //this is to ensure that bundles are different for every run.
        go.transform.localPosition = UnityEngine.Random.onUnitSphere;
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        UnityEngine.Object.DestroyImmediate(go, false);
        return AssetDatabase.AssetPathToGUID(assetPath);
    }

    static string CreateTexture(string name, int size = 32)
    {
        string assetPath = $"{TempPath}/{name}.png";
        var texture = new Texture2D(size, size);
        var data = ImageConversion.EncodeToPNG(texture);
        UnityEngine.Object.DestroyImmediate(texture);
        File.WriteAllBytes(assetPath, data);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return AssetDatabase.AssetPathToGUID(assetPath);
    }

    static string CreateSpriteAtlas(string name, string guidTargetTexture)
    {
        var sa = new SpriteAtlas();
        var targetObjects = new UnityEngine.Object[] { AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guidTargetTexture)) };
        sa.Add(targetObjects);
        string saPath = $"{TempPath}/{name}.spriteAtlas";
        AssetDatabase.CreateAsset(sa, saPath);
        AssetDatabase.Refresh();
        return AssetDatabase.AssetPathToGUID(saPath);
    }

    static string CreateSpriteTexture(string name, int size, bool includesSource)
    {
        string guid = CreateTexture(name, size);
        string texturePath = AssetDatabase.GUIDToAssetPath(guid);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
        importer.textureType = TextureImporterType.Sprite; // creates a sprite subobject
        importer.SaveAndReimport();
        return guid;
    }

    string MakeAddressable(AddressableAssetGroup group, string guid, string address = null)
    {
        var entry = m_Settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = address == null ? Path.GetFileNameWithoutExtension(entry.AssetPath) : address;
        return guid;
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

    string CreateAddressablePrefab(string name, AddressableAssetGroup group)
    {
        string guid = CreateAsset($"{TempPath}/{name}.prefab", name);
        return MakeAddressable(group, guid);
    }

    string CreateAddressableTexture(string name, AddressableAssetGroup group, int size = 32)
    {
        string guid = CreateTexture(name, size);
        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
        ti.isReadable = false;
        ti.SaveAndReimport();
        return MakeAddressable(group, guid);
    }

    void MakePefabReference(string prefabGUID, string assetToReferenceGUID)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGUID));
        UnityEngine.Object target = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assetToReferenceGUID));
        prefab.AddComponent<TestBehaviourWithReference>().Reference = target;
    }

    AddressableAssetGroup CreateGroup(string name)
    {
        return m_Settings.CreateGroup(name, false, false, false, null, typeof(BundledAssetGroupSchema));
    }

    void PrintText(BuildLayout layout)
    {
        MemoryStream stream = new MemoryStream();
        BuildLayoutPrinter.WriteBundleLayout(stream, layout);
        string report = Encoding.ASCII.GetString(stream.ToArray());
        Debug.Log(report);
    }

    internal BuildLayout BuildAndExtractLayout()
    {
        try
        {
            BuildLayout layout = null;
            BuildLayoutGenerationTask.s_LayoutCompleteCallback = (x) => layout = x;
            m_Settings.BuildPlayerContentImpl();
            return layout;
        }
        finally
        {
            BuildLayoutGenerationTask.s_LayoutCompleteCallback = null;
        }
    }

    class WebExtractSession : IDisposable
    {
        public string DataDirectory;
        public string[] Files;
        public WebExtractSession(string filePath)
        {
            DataDirectory = filePath + "_data";
            if (Directory.Exists(DataDirectory))
                throw new Exception("Bundle data directory already exists");

            var baseDir = Path.GetDirectoryName(EditorApplication.applicationPath);
            var webExtractFiles = Directory.GetFiles(baseDir, "WebExtract*", SearchOption.AllDirectories);
            string webExtractPath = webExtractFiles[0];

            Assert.IsTrue(File.Exists(filePath), "Param filePath does not point to an existing file.");

            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = webExtractPath,
                    Arguments = string.Format(@"""{0}""", filePath),
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var exitCode = process.ExitCode;
            process.Close();

            Assert.AreEqual(0, exitCode);
            Files = Directory.GetFiles(DataDirectory);
        }

        public void Dispose()
        {
            Directory.Delete(DataDirectory, true);
        }
    }

    internal void AssertEditorBundleDetailsMatchPhysicalBundle(string bundlePath, BuildLayout.Bundle bundle)
    {
        Assert.AreEqual(new FileInfo(bundlePath).Length, bundle.FileSize);
        using (var wes = new WebExtractSession(bundlePath))
        {
            Assert.AreEqual(bundle.Files.Sum(x => x.SubFiles.Count), wes.Files.Length);
            foreach (BuildLayout.SubFile sf in bundle.Files.SelectMany(x => x.SubFiles))
            {
                string filename = Path.Combine(wes.DataDirectory, sf.Name);
                Assert.AreEqual(sf.Size, new FileInfo(filename).Length);
            }
        }
    }

    [Test]
    public void WhenBundleReferencesAnotherBundle_ExternalReferenceExists()
    {
        AddressableAssetGroup group = CreateGroup("Group1");
        string prefabGUID = CreateAddressablePrefab("p1", group);

        AddressableAssetGroup group2 = CreateGroup("Group2");
        string g2p1GUID = CreateAddressablePrefab("g2p1", group2);
        MakePefabReference(prefabGUID, g2p1GUID);

        BuildLayout layout = BuildAndExtractLayout();
        CollectionAssert.Contains(layout.Groups[0].Bundles[0].Dependencies, layout.Groups[1].Bundles[0]);
        Assert.AreEqual(layout.Groups[0].Bundles[0].Files[0].Assets[0].ExternallyReferencedAssets[0], layout.Groups[1].Bundles[0].Files[0].Assets[0]);
    }

    [Test]
    public void WhenAssetImplicitlyPulledIntoBundle_ImplicitEntryAndReferencesCreated()
    {
        AddressableAssetGroup group = CreateGroup("Group1");
        string prefabGUID = CreateAddressablePrefab("p1", group);
        string aGUID = CreateAsset("p2");
        MakePefabReference(prefabGUID, aGUID);

        BuildLayout layout = BuildAndExtractLayout();
        BuildLayout.DataFromOtherAsset oa = layout.Groups[0].Bundles[0].Files[0].OtherAssets.First(x => x.AssetPath.Contains("p2.prefab"));
        Assert.AreEqual(aGUID, oa.AssetGuid);
    }

    [Test]
    public void WhenBundleContainsMultipleFiles_FilesAndSizesMatchArchiveContent()
    {
        AddressableAssetGroup groupScenes = CreateGroup("SceneGroup");
        AddressableAssetGroup textureGroup = CreateGroup("TextureGroup");
        string scenePath = $"{TempPath}/scene.unity";
        Scene scene1 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        new GameObject().AddComponent<TestBehaviourWithReference>();
        EditorSceneManager.SaveScene(scene1, scenePath);
        AddressableAssetEntry e = m_Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(scenePath), groupScenes);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        CreateAddressableTexture("t1", textureGroup, 256);

        BuildLayout layout = BuildAndExtractLayout();

        BundledAssetGroupSchema schema = m_Settings.groups.First(x => x.HasSchema<BundledAssetGroupSchema>()).GetSchema<BundledAssetGroupSchema>();
        string path = schema.BuildPath.GetValue(m_Settings);
        foreach (BuildLayout.Bundle bundle in layout.Groups.SelectMany(x => x.Bundles))
            AssertEditorBundleDetailsMatchPhysicalBundle(Path.Combine(path, bundle.Name), bundle);
    }

    // Even though slim writes is true, the system will enable it if it needs to generate a build layout report
    [Test]
    public void WhenSlimWriteResultsIsTrue_LayoutStillGenerated()
    {
        bool prevSlim = ScriptableBuildPipeline.slimWriteResults;
        CreateAddressablePrefab("p1", CreateGroup("Group1"));
        try
        {
            ScriptableBuildPipeline.slimWriteResults = true;
            BuildAndExtractLayout();
        }
        finally { ScriptableBuildPipeline.slimWriteResults = prevSlim; }
        FileAssert.Exists(BuildLayoutGenerationTask.m_LayoutTextFile);
    }

    [Test]
    public void WhenBuildLayoutIsDisabled_BuildLayoutIsNotGenerated()
    {
        ProjectConfigData.GenerateBuildLayout = false;
        CreateAddressablePrefab("p1", CreateGroup("Group1"));
        BuildAndExtractLayout();
        FileAssert.DoesNotExist(BuildLayoutGenerationTask.m_LayoutTextFile);
    }

    [Test]
    public void WhenAssetHasStreamedData_IsReportedCorrectly()
    {
        AddressableAssetGroup group = CreateGroup("Group1");
        string prefabGUID = CreateAddressableTexture("t1", group, 256);
        BuildLayout layout = BuildAndExtractLayout();
        Assert.IsTrue(layout.Groups[0].Bundles[0].Files[0].Assets[0].StreamedSize != 0);
        BuildLayout.SubFile f = layout.Groups[0].Bundles[0].Files[0].SubFiles.First(x => x.Name.EndsWith(".resS"));
        Assert.IsFalse(f.IsSerializedFile);
    }

    [Test]
    public void WhenAllContentsOfAnAssetAreStripped_ExplicitAssetHasNoObjects()
    {
        AddressableAssetGroup group = CreateGroup("Group1");
        string assetPath = $"{TempPath}/testpreset.preset";
        Material obj = new Material(Shader.Find("Transparent/Diffuse"));
        Preset myPreset = new Preset(obj);
        AssetDatabase.CreateAsset(myPreset, assetPath);
        GameObject.DestroyImmediate(obj);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        MakeAddressable(group, guid);
        BuildLayout layout = BuildAndExtractLayout();
        Assert.AreEqual(0, layout.Groups[0].Bundles[0].Files[0].Assets[0].SerializedSize);
    }

    class SpritePackerScope : IDisposable
    {
        SpritePackerMode m_PrevMode;
        public SpritePackerScope(SpritePackerMode mode)
        {
            m_PrevMode = EditorSettings.spritePackerMode;
            EditorSettings.spritePackerMode = mode;
        }

        public void Dispose()
        {
            EditorSettings.spritePackerMode = m_PrevMode;
        }
    }

    [Test]
    public void WhenReferencedObjectIdentifiedWithFilename_ObjectRepresentedInDataFromOtherAssets()
    {
        using (new SpritePackerScope(SpritePackerMode.BuildTimeOnlyAtlas))
        {
            BuildCache.PurgeCache(false);
            AddressableAssetGroup group = CreateGroup("Group1");
            string textureGUID = CreateSpriteTexture("spritetexture", 256, false);
            MakeAddressable(group, CreateSpriteAtlas("atlas", textureGUID));
            BuildLayout layout = BuildAndExtractLayout();
            BuildLayout.DataFromOtherAsset otherAssets = layout.Groups[0].Bundles[0].Files[0].Assets[0].InternalReferencedOtherAssets[0];
            Assert.IsTrue(otherAssets.AssetPath.StartsWith("library/atlascache", StringComparison.OrdinalIgnoreCase));
            CollectionAssert.Contains(otherAssets.ReferencingAssets, layout.Groups[0].Bundles[0].Files[0].Assets[0]);
        }
    }
}
