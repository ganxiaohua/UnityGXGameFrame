using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;
using System;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.TestTools;

namespace UnityEngine.AddressableAssets.DynamicResourceLocators
{
    public abstract class DynamicLocatorTests : AddressablesTestFixture
    {
        const string kGoKey = "go";
        const string kSpriteKey = "sprite";

        class TestLocator : IResourceLocator
        {
            struct KeyType : IEquatable<KeyType>
            {
                public object key;
                public Type type;
                public override int GetHashCode()
                {
                    return type.GetHashCode() * 31 + key.GetHashCode();
                }

                public bool Equals(KeyType other)
                {
                    return key.Equals(other.key) && type.Equals(other.type);
                }
            }

            Dictionary<KeyType, IList<IResourceLocation>> m_Locations = new Dictionary<KeyType, IList<IResourceLocation>>();
            public TestLocator()
            {
                m_Locations.Add(new KeyType() { key = kGoKey, type = typeof(GameObject) }, new List<IResourceLocation>(new IResourceLocation[]
                {
                    new ResourceLocationBase("go1", "internalId1", "provider", typeof(GameObject)),
                    new ResourceLocationBase("go2", "internalId2", "provider", typeof(GameObject)),
                }));

                m_Locations.Add(new KeyType() { key = kSpriteKey, type = typeof(SpriteAtlas) }, new List<IResourceLocation>(new IResourceLocation[]
                {
                    new ResourceLocationBase("spriteAtlas1", "internalId1", "provider", typeof(SpriteAtlas)),
                    new ResourceLocationBase("spriteAtlas2", "internalId2", "provider", typeof(SpriteAtlas)),
                }));
            }

            public string LocatorId => "";
            public IEnumerable<object> Keys => null;
            public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
            {
                return m_Locations.TryGetValue(new KeyType() { key = key, type = type }, out locations);
            }
        }

        protected override void OnRuntimeSetup()
        {
            m_Addressables.ClearResourceLocators();
            m_Addressables.AddResourceLocator(new TestLocator());
            m_Addressables.AddResourceLocator(new DynamicResourceLocator(m_Addressables));
        }

        [Test]
        public void GetResourceLocations_WhenLocationExistsAndTypeIsCorrect_LocationIsReturned()
        {
            var res = m_Addressables.GetResourceLocations($"{kGoKey}[blah]", typeof(GameObject), out IList<IResourceLocation> locs);
            Assert.IsTrue(res);
            Assert.AreEqual(2, locs.Count);
            foreach (var l in locs)
                Assert.AreEqual(typeof(GameObject), l.ResourceType);
        }

        [Test]
        public void CreateDynamicLocations_CreatesLocationsWithResourceTypes()
        {
            //Setup
            DynamicResourceLocator locator = new DynamicResourceLocator(m_Addressables);
            List<IResourceLocation> locations = new List<IResourceLocation>();
            IResourceLocation location = new ResourceLocationBase("test", "test", typeof(BundledAssetProvider).FullName, typeof(GameObject));

            //Test
            locator.CreateDynamicLocations(null, locations, "test", "test", location);

            //Assert
            Assert.AreEqual(typeof(GameObject), locations[0].ResourceType);
        }

        [Test]
        public void CreateDynamicLocations_WithDepdencies_CreatesLocationsWithResourceTypes()
        {
            //Setup
            DynamicResourceLocator locator = new DynamicResourceLocator(m_Addressables);
            List<IResourceLocation> locations = new List<IResourceLocation>();
            IResourceLocation depLocation = new ResourceLocationBase("dep1", "dep1", typeof(BundledAssetProvider).FullName, typeof(Texture2D));
            IResourceLocation location = new ResourceLocationBase("test", "test", typeof(BundledAssetProvider).FullName, typeof(GameObject), depLocation);

            //Test
            locator.CreateDynamicLocations(null, locations, "test", "test", location);

            //Assert
            Assert.AreEqual(typeof(GameObject), locations[0].ResourceType);
        }

        [Test]
        public void CreateDynamicLocations_WithSpriteAtlas_CreatesLocationsSpriteResourceTypes()
        {
            //Setup
            DynamicResourceLocator locator = new DynamicResourceLocator(m_Addressables);
            List<IResourceLocation> locations = new List<IResourceLocation>();
            IResourceLocation location = new ResourceLocationBase("test", "test", typeof(BundledAssetProvider).FullName, typeof(U2D.SpriteAtlas));

            //Test
            locator.CreateDynamicLocations(typeof(Sprite), locations, "test", "test", location);

            //Assert
            Assert.AreEqual(typeof(Sprite), locations[0].ResourceType);
        }

        [Test]
        public void GetResourceLocations_WithInvalidMainKey_DoesNotReturnALocation()
        {
            var res = m_Addressables.GetResourceLocations("none[blah]", typeof(GameObject), out IList<IResourceLocation> locs);
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

        [Test]
        public void GetResourceLocations_WithInvalidType_DoesNotReturnALocation()
        {
            var res = m_Addressables.GetResourceLocations($"{kGoKey}[blah]", typeof(Sprite), out IList<IResourceLocation> locs);
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

        [Test]
        public void GetResourceLocations_WhenSpriteWithoutSubkey_DoesNotReturnALocation()
        {
            var res = m_Addressables.GetResourceLocations(kSpriteKey, typeof(Sprite), out IList<IResourceLocation> locs);
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

        [Test]
        public void GetResourceLocations_WithInvalidMainKeyAndSpriteType_DoesNotReturnALocation()
        {
            var res = m_Addressables.GetResourceLocations("none[blah]", typeof(Sprite), out IList<IResourceLocation> locs);
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

        [Test]
        public void GetResourceLocations_WhenSpecifiedTypeDoesNotMatch_NoLocationReturned()
        {
            var res = m_Addressables.GetResourceLocations($"{kSpriteKey}[blah]", typeof(GameObject), out IList<IResourceLocation> locs);
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

        [Test]
        public void GetResourceLocations_WithCorrectBaseTypeForSprite_ReturnsTrue()
        {
            var res = m_Addressables.GetResourceLocations($"{kSpriteKey}[blah]", typeof(Sprite), out IList<IResourceLocation> locs);
            Assert.IsTrue(res);
            Assert.AreEqual(2, locs.Count);
            foreach (var l in locs)
            {
                Assert.AreEqual(1, l.Dependencies.Count);
                Assert.AreEqual(typeof(SpriteAtlas), l.Dependencies[0].ResourceType);
            }
        }
    }

#if UNITY_EDITOR
    class DynamicLocatorTests_FastMode : DynamicLocatorTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class DynamicLocatorTests_VirtualMode : DynamicLocatorTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class DynamicLocatorTests_PackedPlaymodeMode : DynamicLocatorTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class DynamicLocatorTests_PackedMode : DynamicLocatorTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
