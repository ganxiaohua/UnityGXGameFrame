namespace AddressableAssets.DocExampleCode
{
    #region doc_DeclaringReferences
    using System;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    internal class DeclaringReferences : MonoBehaviour
    {
        // Any asset type
        public AssetReference reference;

        // Prefab assets
        public AssetReferenceGameObject gameObjectReference;

        // Sprite asset types
        public AssetReferenceSprite spriteReference;
        public AssetReferenceAtlasedSprite atlasSpriteReference;

        // Texture asset types
        public AssetReferenceTexture textureReference;
        public AssetReferenceTexture2D texture2DReference;
        public AssetReferenceTexture3D texture3DReference;

        // Any asset type with the specified labels
        [AssetReferenceUILabelRestriction("animals", "characters")]
        public AssetReference labelRestrictedReference;

        // Generic asset type (Unity 2020.3+)
        public AssetReferenceT<AudioClip> typedReference;

        // Custom asset reference class
        public AssetReferenceMaterial materialReference;

        [Serializable]
        public class AssetReferenceMaterial : AssetReferenceT<Material>
        {
            public AssetReferenceMaterial(string guid) : base(guid) { }
        }

        private void Start() {
            // Load assets...
        }

        private void OnDestroy() {
            // Release assets...
        }
    }
    #endregion
    #region doc_ConcreteSubClass
    [Serializable]
    internal class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }
    #endregion
    internal class UseExamples
    {
        #region doc_UseConcreteSubclass
        // Custom asset reference class
        public AssetReferenceMaterial materialReference;
        #endregion

        #region doc_RestrictionAttribute
        [AssetReferenceUILabelRestriction("animals", "characters")]
        public AssetReference labelRestrictedReference;
        #endregion
    }
}