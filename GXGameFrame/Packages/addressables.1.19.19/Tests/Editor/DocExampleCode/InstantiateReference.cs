namespace AddressableAssets.DocExampleCode
{
    #region doc_InstantiateReference
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    internal class InstantiateReference : MonoBehaviour
    {
        [SerializeField]
        private AssetReferenceGameObject reference;

        void Start() {
            if (reference != null)
                reference.InstantiateAsync(this.transform);
        }

        private void OnDestroy() {
            if (reference != null && reference.IsValid())
                reference.ReleaseAsset();
        }
    }
    #endregion
}