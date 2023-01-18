using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.Utility;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UnityEditor.AddressableAssets.Tests
{
    using Debug = UnityEngine.Debug;
    using Random = UnityEngine.Random;

    public class ContentCatalogTests
    {
        List<object> m_Keys;
        List<Type> m_Providers;

        [Serializable]
        public class SerializableKey
        {
            public int index;
            public string path;
        }

        [OneTimeSetUp]
        public void Init()
        {
            m_Keys = new List<object>();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                var r = Random.Range(0, 100);
                if (r < 20)
                {
                    int len = Random.Range(1, 5);
                    for (int j = 0; j < len; j++)
                        sb.Append(GUID.Generate().ToString());
                    m_Keys.Add(sb.ToString());
                    sb.Length = 0;
                }
                else if (r < 40)
                {
                    m_Keys.Add((ushort)(i * 13));
                }
                else if (r < 50)
                {
                    m_Keys.Add(i * 13);
                }
                else if (r < 60)
                {
                    m_Keys.Add((uint)(i * 13));
                }
                else if (r < 80)
                {
                    m_Keys.Add(new SerializableKey { index = i, path = GUID.Generate().ToString() });
                }
                else
                {
                    m_Keys.Add(Hash128.Parse(GUID.Generate().ToString()));
                }
            }
            m_Providers = new List<Type>();
            m_Providers.Add(typeof(BundledAssetProvider));
            m_Providers.Add(typeof(AssetBundleProvider));
            m_Providers.Add(typeof(AssetDatabaseProvider));
            m_Providers.Add(typeof(LegacyResourcesProvider));
            m_Providers.Add(typeof(JsonAssetProvider));
            m_Providers.Add(typeof(TextDataProvider));
            m_Providers.Add(typeof(TextDataProvider));
        }

        List<T> GetRandomSubset<T>(List<T> keys, int count)
        {
            if (keys.Count == 0 || count == 0)
                return new List<T>();
            var entryKeys = new HashSet<T>();
            for (int k = 0; k < count; k++)
                entryKeys.Add(keys[Random.Range(0, keys.Count)]);
            return entryKeys.ToList();
        }

        [Serializable]
        public class EvenData
        {
            public int index;
            public string path;
        }

        [Serializable]
        public class OddData
        {
            public int index;
            public string path;
        }

        [Test]
        public void AssetBundleRequestOptionsTest()
        {
            var options = new AssetBundleRequestOptions
            {
                ChunkedTransfer = true,
                Crc = 123,
                Hash = new Hash128(1, 2, 3, 4).ToString(),
                RedirectLimit = 4,
                RetryCount = 7,
                Timeout = 12,
                AssetLoadMode = AssetLoadMode.AllPackedAssetsAndDependencies
            };
            var dataEntry = new ContentCatalogDataEntry(typeof(ContentCatalogData), "internalId", "provider", new object[] { 1 }, null, options);
            var entries = new List<ContentCatalogDataEntry>();
            entries.Add(dataEntry);
            var ccData = new ContentCatalogData("TestCatalog");
            ccData.SetData(entries, false);
            var locator = ccData.CreateLocator();
            IList<IResourceLocation> locations;
            if (!locator.Locate(1, typeof(object), out locations))
                Assert.Fail("Unable to locate resource location");
            var loc = locations[0];
            var locOptions = loc.Data as AssetBundleRequestOptions;
            Assert.IsNotNull(locOptions);
            Assert.AreEqual(locOptions.ChunkedTransfer, options.ChunkedTransfer);
            Assert.AreEqual(locOptions.Crc, options.Crc);
            Assert.AreEqual(locOptions.Hash, options.Hash);
            Assert.AreEqual(locOptions.RedirectLimit, options.RedirectLimit);
            Assert.AreEqual(locOptions.RetryCount, options.RetryCount);
            Assert.AreEqual(locOptions.Timeout, options.Timeout);
            Assert.AreEqual(locOptions.AssetLoadMode, options.AssetLoadMode);
        }

        [Test]
        public void VerifySerialization()
        {
            var sw = Stopwatch.StartNew();
            sw.Start();
            var catalog = new ContentCatalogData();
            var entries = new List<ContentCatalogDataEntry>();
            var availableKeys = new List<object>();

            for (int i = 0; i < 1000; i++)
            {
                var internalId = "Assets/TestPath/" + GUID.Generate() + ".asset";
                var eKeys = GetRandomSubset(m_Keys, Random.Range(1, 5));
                object data;
                if (i % 2 == 0)
                    data = new EvenData { index = i, path = internalId };
                else
                    data = new OddData { index = i, path = internalId };

                var e = new ContentCatalogDataEntry(typeof(ContentCatalogData), internalId, m_Providers[Random.Range(0, m_Providers.Count)].FullName, eKeys, GetRandomSubset(availableKeys, Random.Range(0, 1)), data);
                availableKeys.Add(eKeys[0]);
                entries.Add(e);
            }

            catalog.SetData(entries);
            sw.Stop();
            var t = sw.Elapsed.TotalMilliseconds;
            sw.Reset();
            sw.Start();
            var locMap = catalog.CreateLocator();
            sw.Stop();
            Debug.LogFormat("Create: {0}ms, Load: {1}ms", t, sw.Elapsed.TotalMilliseconds);

            foreach (var k in locMap.Locations)
            {
                foreach (var loc in k.Value)
                {
                    var entry = entries.Find(e => e.InternalId == loc.InternalId);
                    Assert.AreEqual(entry.Provider, loc.ProviderId);

                    var deps = loc.Dependencies;
                    if (deps != null)
                    {
                        foreach (var ed in entry.Dependencies)
                        {
                            IList<IResourceLocation> depList;
                            Assert.IsTrue(locMap.Locate(ed, typeof(object), out depList));
                            for (int i = 0; i < depList.Count; i++)
                                Assert.AreEqual(depList[i].InternalId, deps[i].InternalId);
                        }
                    }
                }
            }
        }

        [Test]
        public void VerifyDependencyHashCalculation()
        {
            var catalog = new ContentCatalogData();
            Dictionary<int, object> hashSources = new Dictionary<int, object>();

            var dummyValues = new List<object>()
            {
                "<WILL-BE-REPLACED>",
                "startup-shared_assets_assets/fx_data/textures.bundle",
                "shared_assets_assets/fx_data/materials.bundle",
                "shaders_assets_all.bundle",
                "music_assets_music/maptheme6final.bundle",
                "fx_tex_assets_all.bundle",
                "shared_assets_assets/textures/ui/campain_act02.bundle",
                "shared_assets_assets/fx_data/meshes.bundle",
                "startup-shared_assets_assets/textures/ui/campain_act02.bundle",
                "startup-shared_assets_assets/textures/ui/campainart.bundle",
                "startup-shared_assets_assets/fx_data/materials.bundle",
                "shared_assets_assets/textures/ui/valleyoftreasures.bundle",
                "startup-shared_assets_assets/fx_data/meshes.bundle",
                "startup_UnityBuiltInShaders.bundle"
            };

            dummyValues[0] = "maps_assets_ref/valley1.bundle";
            var hashPart1 = dummyValues[0].GetHashCode();
            var hashSum1 = catalog.CalculateCollectedHash(dummyValues, hashSources);

            var dummyValues2 = new List<object>()
            {
                "maps_assets_ref/valley1.bundle",
                "startup-shared_assets_assets/fx_data/textures.bundle",
                "shared_assets_assets/fx_data/materials.bundle",
                "shaders_assets_all.bundle",
                "music_assets_music/maptheme6final.bundle",
                "fx_tex_assets_all.bundle",
                "shared_assets_assets/textures/ui/campain_act02.bundle",
                "shared_assets_assets/fx_data/meshes.bundle",
                "startup-shared_assets_assets/textures/ui/campain_act02.bundle",
                "startup-shared_assets_assets/textures/ui/campainart.bundle",
                "startup-shared_assets_assets/fx_data/materials.bundle",
                "shared_assets_assets/textures/ui/valleyoftreasures.bundle",
                "startup-shared_assets_assets/fx_data/meshes.bundle",
                "startup_UnityBuiltInShaders.bundle"
            };

            var hashSum1DifferentList = catalog.CalculateCollectedHash(dummyValues2, hashSources);

            dummyValues[0] = "maps_assets_ref/valley3.bundle";
            var hashPart2 = dummyValues[0].GetHashCode();
            var hashSum2 = catalog.CalculateCollectedHash(dummyValues, hashSources);

            Assert.AreEqual(hashSum1, hashSum1DifferentList);
            Assert.AreNotEqual(hashPart1, hashPart2);
            Assert.AreNotEqual(hashSum1, hashSum2);
        }

        [Test]
        public void VerifyEnumerableHashCalculation()
        {
            var dummyValues = new List<object>()
            {
                "maps_assets_ref/valley1.bundle",
                "startup-shared_assets_assets/fx_data/textures.bundle",
                "shared_assets_assets/fx_data/materials.bundle",
                "shaders_assets_all.bundle",
                "music_assets_music/maptheme6final.bundle",
                "fx_tex_assets_all.bundle",
                "shared_assets_assets/textures/ui/campain_act02.bundle",
                "shared_assets_assets/fx_data/meshes.bundle",
                "startup-shared_assets_assets/textures/ui/campain_act02.bundle",
                "startup-shared_assets_assets/textures/ui/campainart.bundle",
                "startup-shared_assets_assets/fx_data/materials.bundle",
                "shared_assets_assets/textures/ui/valleyoftreasures.bundle",
                "startup-shared_assets_assets/fx_data/meshes.bundle",
                "startup_UnityBuiltInShaders.bundle"
            };

            var dummyValues2 = new List<object>()
            {
                "maps_assets_ref/valley1.bundle",
                "startup-shared_assets_assets/fx_data/textures.bundle",
                "shared_assets_assets/fx_data/materials.bundle",
                "shaders_assets_all.bundle",
                "music_assets_music/maptheme6final.bundle",
                "fx_tex_assets_all.bundle",
                "shared_assets_assets/textures/ui/campain_act02.bundle",
                "shared_assets_assets/fx_data/meshes.bundle",
                "startup-shared_assets_assets/textures/ui/campain_act02.bundle",
                "startup-shared_assets_assets/textures/ui/campainart.bundle",
                "startup-shared_assets_assets/fx_data/materials.bundle",
                "shared_assets_assets/textures/ui/valleyoftreasures.bundle",
                "startup-shared_assets_assets/fx_data/meshes.bundle",
                "startup_UnityBuiltInShaders.bundle"
            };

            var hash1 = ContentCatalogData.GetHashCodeForEnumerable(dummyValues);
            var hash2 = ContentCatalogData.GetHashCodeForEnumerable(dummyValues2);
            Assert.AreEqual(hash1, hash2);

            dummyValues[0] = "maps_assets_ref/valley3.bundle";
            var hash3 = ContentCatalogData.GetHashCodeForEnumerable(dummyValues);
            Assert.AreNotEqual(hash1, hash3);
        }


        [TestCase("0#b", "ab", new string[] { "a" })]
        [TestCase("1#b", "bb", new string[] { "a", "b" })]
        [TestCase("b", "b", new string[] {"a" })]
        [TestCase("b", "b", new string[] { })]
        [TestCase("b", "b", null)]
        [TestCase("x#b", "x#b", new string[] { "a" })]
        [Test]
        public void ContentCatalogData_ExpandInternalId_GeneratesExpectedResults(string input, string expected, string[] prefixes)
        {
            Assert.AreEqual(expected, ContentCatalogData.ExpandInternalId(prefixes, input));
        }

        [Test]
        public void SerializationUtility_ReadWrite_Int32()
        {
            var data = new byte[100];
            for (int i = 0; i < 1000; i++)
            {
                var val = Random.Range(int.MinValue, int.MaxValue);
                var off = Random.Range(0, data.Length - sizeof(int));
                Assert.AreEqual(off + sizeof(int), SerializationUtilities.WriteInt32ToByteArray(data, val, off));
                Assert.AreEqual(val, SerializationUtilities.ReadInt32FromByteArray(data, off));
            }
        }

        [Test]
        public void ExtractCommonPrefix_ReturnsExpectedString()
        {
            var prefixes = new List<string>();
            var prefixIndices = new Dictionary<string, int>();
            Assert.AreEqual("0#/z.ext", ContentCatalogData.ExtractCommonPrefix(prefixes, prefixIndices, "x/y/z.ext"));
            Assert.AreEqual("1#/z.ext", ContentCatalogData.ExtractCommonPrefix(prefixes, prefixIndices, "x/z.ext"));
            Assert.AreEqual("2#/z.ext", ContentCatalogData.ExtractCommonPrefix(prefixes, prefixIndices, "x/b/z.ext"));
            Assert.AreEqual("0#/z.ext", ContentCatalogData.ExtractCommonPrefix(prefixes, prefixIndices, "x/y/z.ext"));
            Assert.AreEqual("z.ext", ContentCatalogData.ExtractCommonPrefix(prefixes, prefixIndices, "z.ext"));
            Assert.AreEqual(3, prefixes.Count);
        }

        string testData = @"{""m_LocatorId"":""AddressablesMainContentCatalog"",""m_InstanceProviderData"":{""m_Id"":""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider"",""m_ObjectType"":{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.InstanceProvider""},""m_Data"":""""},""m_SceneProviderData"":{""m_Id"":""UnityEngine.ResourceManagement.ResourceProviders.SceneProvider"",""m_ObjectType"":{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.SceneProvider""},""m_Data"":""""},""m_ResourceProviderData"":[{""m_Id"":""UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider"",""m_ObjectType"":{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider""},""m_Data"":""""},{""m_Id"":""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider"",""m_ObjectType"":{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider""},""m_Data"":""""},{""m_Id"":""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider"",""m_ObjectType"":{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider""},""m_Data"":""""}],""m_ProviderIds"":[""UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider"",""UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider""],""m_InternalIds"":[""{UnityEngine.AddressableAssets.Addressables.RuntimePath}/StandaloneWindows64/defaultlocalgroup_assets_all_d4ed3973c342e6f06795a0f8daaebaad.bundle"",""{UnityEngine.AddressableAssets.Addressables.RuntimePath}/StandaloneWindows64/defaultlocalgroup_unitybuiltinshaders_8f144cd21867dc83f60ecd3c93095b52.bundle"",""{UnityEngine.AddressableAssets.Addressables.RuntimePath}/StandaloneWindows64/defaultlocalgroup_scenes_all_e91ebe7804da861b4deb67a340282541.bundle"",""Assets/New Material.mat"",""Assets/swef.unity""],""m_KeyDataString"":""CQAAAABEAAAAZGVmYXVsdGxvY2FsZ3JvdXBfYXNzZXRzX2FsbF9kNGVkMzk3M2MzNDJlNmYwNjc5NWEwZjhkYWFlYmFhZC5idW5kbGUATQAAAGRlZmF1bHRsb2NhbGdyb3VwX3VuaXR5YnVpbHRpbnNoYWRlcnNfOGYxNDRjZDIxODY3ZGM4M2Y2MGVjZDNjOTMwOTViNTIuYnVuZGxlAEQAAABkZWZhdWx0bG9jYWxncm91cF9zY2VuZXNfYWxsX2U5MWViZTc4MDRkYTg2MWI0ZGViNjdhMzQwMjgyNTQxLmJ1bmRsZQAXAAAAQXNzZXRzL05ldyBNYXRlcmlhbC5tYXQAIAAAADNlN2JmNTA3OTRhNzEyMjQ2YWU0ZGNiZTdhODQyOGM4ABEAAABBc3NldHMvc3dlZi51bml0eQAgAAAAYjY4MDdmODNlMWU0ODc2NGM4MjMyM2ZkNTExZTY0NjgEKToMuAQiVa/u"",""m_BucketDataString"":""CQAAAAQAAAABAAAAAAAAAE0AAAABAAAAAQAAAJ8AAAABAAAAAgAAAOgAAAABAAAAAwAAAAQBAAABAAAAAwAAACkBAAABAAAABAAAAD8BAAABAAAABAAAAGQBAAACAAAAAAAAAAEAAABpAQAAAgAAAAIAAAABAAAA"",""m_EntryDataString"":""BQAAAAAAAAAAAAAA/////wAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAP////8AAAAAhQIAAAEAAAAAAAAAAgAAAAAAAAD/////AAAAADQFAAACAAAAAAAAAAMAAAABAAAABwAAACk6DLj/////AwAAAAEAAAAEAAAAAQAAAAgAAAAiVa/u/////wUAAAACAAAA"",""m_ExtraDataString"":""B0xVbml0eS5SZXNvdXJjZU1hbmFnZXIsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsSlVuaXR5RW5naW5lLlJlc291cmNlTWFuYWdlbWVudC5SZXNvdXJjZVByb3ZpZGVycy5Bc3NldEJ1bmRsZVJlcXVlc3RPcHRpb25z6AEAAHsAIgBtAF8ASABhAHMAaAAiADoAIgBkADQAZQBkADMAOQA3ADMAYwAzADQAMgBlADYAZgAwADYANwA5ADUAYQAwAGYAOABkAGEAYQBlAGIAYQBhAGQAIgAsACIAbQBfAEMAcgBjACIAOgAyADAAMgAxADcANAA3ADAAOQA5ACwAIgBtAF8AVABpAG0AZQBvAHUAdAAiADoAMAAsACIAbQBfAEMAaAB1AG4AawBlAGQAVAByAGEAbgBzAGYAZQByACIAOgBmAGEAbABzAGUALAAiAG0AXwBSAGUAZABpAHIAZQBjAHQATABpAG0AaQB0ACIAOgAtADEALAAiAG0AXwBSAGUAdAByAHkAQwBvAHUAbgB0ACIAOgAwACwAIgBtAF8AQgB1AG4AZABsAGUATgBhAG0AZQAiADoAIgA5ADIAZAAwAGYAOABiAGMAOQBkAGYAZABjADAAMwBlADEAMABkAGYAMgBmADMAYgAzAGIANABjADgAMgA3AGUAIgAsACIAbQBfAEIAdQBuAGQAbABlAFMAaQB6AGUAIgA6ADIANQAyADgALAAiAG0AXwBVAHMAZQBDAHIAYwBGAG8AcgBDAGEAYwBoAGUAZABCAHUAbgBkAGwAZQBzACIAOgB0AHIAdQBlAH0AB0xVbml0eS5SZXNvdXJjZU1hbmFnZXIsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsSlVuaXR5RW5naW5lLlJlc291cmNlTWFuYWdlbWVudC5SZXNvdXJjZVByb3ZpZGVycy5Bc3NldEJ1bmRsZVJlcXVlc3RPcHRpb25zEgIAAHsAIgBtAF8ASABhAHMAaAAiADoAIgA4AGYAMQA0ADQAYwBkADIAMQA4ADYANwBkAGMAOAAzAGYANgAwAGUAYwBkADMAYwA5ADMAMAA5ADUAYgA1ADIAIgAsACIAbQBfAEMAcgBjACIAOgAzADgAMQAzADcAMgA0ADgANQA5ACwAIgBtAF8AVABpAG0AZQBvAHUAdAAiADoAMAAsACIAbQBfAEMAaAB1AG4AawBlAGQAVAByAGEAbgBzAGYAZQByACIAOgBmAGEAbABzAGUALAAiAG0AXwBSAGUAZABpAHIAZQBjAHQATABpAG0AaQB0ACIAOgAtADEALAAiAG0AXwBSAGUAdAByAHkAQwBvAHUAbgB0ACIAOgAwACwAIgBtAF8AQgB1AG4AZABsAGUATgBhAG0AZQAiADoAIgBmAGMAOAAyAGEAMAAxAGUAYgAwAGEAMgA0AGIAOQBiAGQAOQBjADAAZQBjADEAZAAzAGEAOQBiADIANgA1ADUAXwB1AG4AaQB0AHkAYgB1AGkAbAB0AGkAbgBzAGgAYQBkAGUAcgBzACIALAAiAG0AXwBCAHUAbgBkAGwAZQBTAGkAegBlACIAOgA0ADQANAA1ADQALAAiAG0AXwBVAHMAZQBDAHIAYwBGAG8AcgBDAGEAYwBoAGUAZABCAHUAbgBkAGwAZQBzACIAOgB0AHIAdQBlAH0AB0xVbml0eS5SZXNvdXJjZU1hbmFnZXIsIFZlcnNpb249MC4wLjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsSlVuaXR5RW5naW5lLlJlc291cmNlTWFuYWdlbWVudC5SZXNvdXJjZVByb3ZpZGVycy5Bc3NldEJ1bmRsZVJlcXVlc3RPcHRpb25z6AEAAHsAIgBtAF8ASABhAHMAaAAiADoAIgBlADkAMQBlAGIAZQA3ADgAMAA0AGQAYQA4ADYAMQBiADQAZABlAGIANgA3AGEAMwA0ADAAMgA4ADIANQA0ADEAIgAsACIAbQBfAEMAcgBjACIAOgAzADQAMAA1ADQAMwA2ADQANQAxACwAIgBtAF8AVABpAG0AZQBvAHUAdAAiADoAMAAsACIAbQBfAEMAaAB1AG4AawBlAGQAVAByAGEAbgBzAGYAZQByACIAOgBmAGEAbABzAGUALAAiAG0AXwBSAGUAZABpAHIAZQBjAHQATABpAG0AaQB0ACIAOgAtADEALAAiAG0AXwBSAGUAdAByAHkAQwBvAHUAbgB0ACIAOgAwACwAIgBtAF8AQgB1AG4AZABsAGUATgBhAG0AZQAiADoAIgA5ADEANwBlADUANQAzAGQAZQBiAGQAOAAyADMAOABkAGMAMgBjADIAZAA2ADIANQBkADAAZgA4ADUAOQA0AGMAIgAsACIAbQBfAEIAdQBuAGQAbABlAFMAaQB6AGUAIgA6ADgANwA4ADIALAAiAG0AXwBVAHMAZQBDAHIAYwBGAG8AcgBDAGEAYwBoAGUAZABCAHUAbgBkAGwAZQBzACIAOgB0AHIAdQBlAH0A"",""m_Keys"":[""defaultlocalgroup_assets_all_d4ed3973c342e6f06795a0f8daaebaad.bundle"",""defaultlocalgroup_unitybuiltinshaders_8f144cd21867dc83f60ecd3c93095b52.bundle"",""defaultlocalgroup_scenes_all_e91ebe7804da861b4deb67a340282541.bundle"",""Assets/New Material.mat"",""3e7bf50794a712246ae4dcbe7a8428c8"",""Assets/swef.unity"",""b6807f83e1e48764c82323fd511e6468"",""-1207158231"",""-290499294""],""m_resourceTypes"":[{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource""},{""m_AssemblyName"":""UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.Material""},{""m_AssemblyName"":""Unity.ResourceManager, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"",""m_ClassName"":""UnityEngine.ResourceManagement.ResourceProviders.SceneInstance""}]}";
        [Test]
        public void CanLoad_OldCatalogFormat()
        {
            var ccd = JsonUtility.FromJson<ContentCatalogData>(testData);
            Assert.IsNotNull(ccd);
            var loc = ccd.CreateLocator();
            Assert.IsNotNull(loc);
            Assert.AreEqual(9, loc.Keys.Count());
            foreach (var k in loc.Keys)
            {
                Assert.IsTrue(loc.Locate(k, null, out var res));
                Assert.IsNotEmpty(res[0].PrimaryKey);
                Assert.IsNotEmpty(res[0].InternalId);
                Assert.IsNotEmpty(res[0].ProviderId);
                Assert.IsNotNull(res[0].ResourceType);
            }

        }
    }
}
