using UnityEngine;

namespace GameFrame
{
    public sealed class DefaultAssetReferenceContainer : MonoBehaviour
    {
        public DefaultAssetReference AssetReference;

        private void Awake()
        {
            AssetReference?.RefInitialize();
        }

        private void OnDestroy()
        {
            AssetReference?.UnrefAssets();
        }
    }
}