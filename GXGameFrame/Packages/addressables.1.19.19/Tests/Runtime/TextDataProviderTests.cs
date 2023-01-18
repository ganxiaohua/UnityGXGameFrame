using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.TestTools;

public abstract class TextDataProviderTests : AddressablesTestFixture
{
    [SetUp]
    public void Setup()
    {
        if (m_Addressables != null)
            m_Addressables.WebRequestOverride = null;
    }

    [UnityTest]
    public IEnumerator WhenWebRequestOverrideIsSet_CallbackIsCalled_TextDataProvider()
    {
        bool webRequestOverrideCalled = false;
        m_Addressables.WebRequestOverride = request => webRequestOverrideCalled = true;

        var prev = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;

        var nonExistingPath = "http://127.0.0.1/non-existing-catalog";
        var loc = new ResourceLocationBase(nonExistingPath, nonExistingPath, typeof(TextDataProvider).FullName, typeof(string));
        var h = m_Addressables.ResourceManager.ProvideResource<string>(loc);
        yield return h;

        if (h.IsValid()) h.Release();
        LogAssert.ignoreFailingMessages = prev;
        Assert.IsTrue(webRequestOverrideCalled);
    }

    [UnityTest]
    public IEnumerator WhenWebRequestOverrideIsSet_CallbackIsCalled_JsonAssetProvider()
    {
        bool webRequestOverrideCalled = false;
        m_Addressables.WebRequestOverride = request => webRequestOverrideCalled = true;

        var prev = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;

        var nonExistingPath = "http://127.0.0.1/non-existing-catalog";
        var loc = new ResourceLocationBase(nonExistingPath, nonExistingPath, typeof(JsonAssetProvider).FullName, typeof(string));
        var h = m_Addressables.ResourceManager.ProvideResource<string>(loc);
        yield return h;

        if (h.IsValid()) h.Release();
        LogAssert.ignoreFailingMessages = prev;
        Assert.IsTrue(webRequestOverrideCalled);
    }
}

#if UNITY_EDITOR
class TextDataProviderTests_PackedPlaymodeMode : TextDataProviderTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

[UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
class TextDataProviderTests_PackedMode : TextDataProviderTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
