namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithReference
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithReference : MonoBehaviour
    {
        // Assign in Editor
        public AssetReference reference;

        // Start the load operation on start
        void Start() {
            AsyncOperationHandle handle = reference.LoadAssetAsync<GameObject>();
            handle.Completed += Handle_Completed;
        }

        // Instantiate the loaded prefab on complete
        private void Handle_Completed(AsyncOperationHandle obj) {
            if (obj.Status == AsyncOperationStatus.Succeeded) {
                Instantiate(reference.Asset, transform);
            } else {
                Debug.LogError($"AssetReference {reference.RuntimeKey} failed to load.");
            }
        }

        // Release asset when parent object is destroyed
        private void OnDestroy() {
            reference.ReleaseAsset();
        }
    }
    #endregion
}