using NUnit.Framework;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

public abstract class AddressablesTestFixture : IPrebuildSetup, IPostBuildCleanup
{
    internal AddressablesImpl m_Addressables;
    internal string m_RuntimeSettingsPath;
    internal readonly string m_UniqueTestName;

    protected AddressablesTestFixture()
    {
        m_UniqueTestName = this.GetType().Name;
    }

    protected enum TestBuildScriptMode
    {
        Fast,
        Virtual,
        PackedPlaymode,
        Packed
    }
    protected virtual TestBuildScriptMode BuildScriptMode { get; }

    protected string GetGeneratedAssetsPath()
    {
        return Path.Combine("Assets", "gen", m_UniqueTestName);
    }

    [UnitySetUp]
    public virtual IEnumerator RuntimeSetup()
    {
#if ENABLE_CACHING
        Caching.ClearCache();
#endif
        Assert.IsNull(m_Addressables);
        m_Addressables = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
        m_RuntimeSettingsPath = m_Addressables.ResolveInternalId(GetRuntimeAddressablesSettingsPath(m_UniqueTestName));
        var op = m_Addressables.InitializeAsync(m_RuntimeSettingsPath, null, false);
        yield return op;
        Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
        OnRuntimeSetup();
        if (op.IsValid())
            op.Release();
    }

    protected virtual void OnRuntimeSetup()
    {
    }

    [TearDown]
    public virtual void RuntimeTeardown()
    {
        m_Addressables.ResourceManager.Dispose();
        m_Addressables = null;
    }

    void IPrebuildSetup.Setup()
    {
#if UNITY_EDITOR
        bool currentIgnoreState = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;

        var activeScenePath = EditorSceneManager.GetActiveScene().path;

        string rootFolder = GetGeneratedAssetsPath();
        AddressableAssetSettings settings = CreateSettings("Settings", rootFolder);

        Setup(settings, rootFolder);
        RunBuilder(settings);

        if (activeScenePath != EditorSceneManager.GetActiveScene().path)
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
        LogAssert.ignoreFailingMessages = currentIgnoreState;
#endif
    }

    void IPostBuildCleanup.Cleanup()
    {
#if UNITY_EDITOR
        string path = Path.Combine("Assets", "gen");
        if (Directory.Exists(path))
            AssetDatabase.DeleteAsset(path);
#endif
    }

#if UNITY_EDITOR

    internal virtual void Setup(AddressableAssetSettings settings, string tempAssetFolder) {}

    protected AddressableAssetSettings CreateSettings(string name, string rootFolder)
    {
        if (Directory.Exists(rootFolder))
            Directory.Delete(rootFolder, true);
        Directory.CreateDirectory(rootFolder);
        return AddressableAssetSettings.Create(Path.Combine(rootFolder, name), "AddressableAssetSettings.Tests", false, true);
    }

    protected virtual void RunBuilder(AddressableAssetSettings settings)
    {
        RunBuilder(settings, m_UniqueTestName);
    }

    protected void RunBuilder(AddressableAssetSettings settings, string id)
    {
        var buildContext = new AddressablesDataBuilderInput(settings);
        buildContext.RuntimeSettingsFilename = "settings" + id + ".json";
        buildContext.RuntimeCatalogFilename = "catalog" + id + ".json";
        buildContext.PathFormat = "{0}" + Addressables.LibraryPath + "{1}_" + id + ".json";
        if (BuildScriptMode == TestBuildScriptMode.PackedPlaymode)
        {
            IDataBuilder packedModeBuilder = GetBuilderOfType(settings, typeof(BuildScriptPackedMode));
            packedModeBuilder.BuildData<AddressableAssetBuildResult>(buildContext);
        }
        IDataBuilder b = GetBuilderOfType(settings, GetBuildScriptTypeFromMode(BuildScriptMode));
        b.BuildData<AddressableAssetBuildResult>(buildContext);
        PlayerPrefs.SetString(Addressables.kAddressablesRuntimeDataPath + id, PlayerPrefs.GetString(Addressables.kAddressablesRuntimeDataPath, ""));
    }

    static IDataBuilder GetBuilderOfType(AddressableAssetSettings settings, Type modeType)
    {
        foreach (var db in settings.DataBuilders)
        {
            var b = db;
            if (b.GetType() == modeType)
                return b as IDataBuilder;
        }
        throw new Exception("DataBuilder not found");
    }

    protected Type GetBuildScriptTypeFromMode(TestBuildScriptMode mode)
    {
        switch (mode)
        {
            case TestBuildScriptMode.Fast: return typeof(BuildScriptFastMode);
            case TestBuildScriptMode.Virtual: return typeof(BuildScriptVirtualMode);
            case TestBuildScriptMode.Packed: return typeof(BuildScriptPackedMode);
            case TestBuildScriptMode.PackedPlaymode: return typeof(BuildScriptPackedPlayMode);
        }
        throw new Exception("Unknown script mode");
    }

#endif

    protected string GetRuntimeAddressablesSettingsPath(string id)
    {
        if (BuildScriptMode == TestBuildScriptMode.Packed || BuildScriptMode == TestBuildScriptMode.PackedPlaymode)
            return "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/settings" + id + ".json";
        else if (BuildScriptMode == TestBuildScriptMode.Fast)
        {
            return PlayerPrefs.GetString(Addressables.kAddressablesRuntimeDataPath + id, "");
        }
        else
        {
            return string.Format("{0}" + Addressables.LibraryPath + "settings_{1}.json", "file://{UnityEngine.Application.dataPath}/../", id);
        }
    }

    internal static string CreateAssetPath(string rootFolder, string key, string extension)
    {
        return Path.Combine(rootFolder, String.Concat(key, extension));
    }

#if UNITY_EDITOR
    internal static string CreateScene(string assetPath)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
        EditorSceneManager.SaveScene(scene, assetPath);
        return AssetDatabase.AssetPathToGUID(scene.path);
    }

    internal static string CreatePrefab(string assetPath)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        Object.DestroyImmediate(go, false);
        return AssetDatabase.AssetPathToGUID(assetPath);
    }

#endif

    internal static IEnumerator UnloadSceneFromHandler(AsyncOperationHandle<SceneInstance> op, AddressablesImpl addressables)
    {
        string sceneName = op.Result.Scene.name;
        Assert.IsNotNull(sceneName);
        var unloadOp = addressables.UnloadSceneAsync(op, UnloadSceneOptions.None, false);
        yield return unloadOp;
        Assert.AreEqual(AsyncOperationStatus.Succeeded, unloadOp.Status);
        Assert.IsFalse(unloadOp.Result.Scene.isLoaded);
        Assert.IsTrue(unloadOp.IsDone);
        addressables.Release(unloadOp);
        Assert.IsNull(SceneManager.GetSceneByName(sceneName).name);
    }

    internal static IEnumerator UnloadSceneFromHandlerRefCountCheck(AsyncOperationHandle<SceneInstance> op, AddressablesImpl addressables)
    {
        string sceneName = op.Result.Scene.name;
        Assert.IsNotNull(sceneName);
        var prevRefCount = op.ReferenceCount;
        var unloadOp = addressables.UnloadSceneAsync(op, UnloadSceneOptions.None, false);
        yield return unloadOp;
        Assert.AreEqual(AsyncOperationStatus.Succeeded, unloadOp.Status);
        Assert.IsFalse(unloadOp.Result.Scene.isLoaded);
        if (op.IsValid())
            Assert.AreEqual(prevRefCount - 1, op.ReferenceCount);
    }
}
