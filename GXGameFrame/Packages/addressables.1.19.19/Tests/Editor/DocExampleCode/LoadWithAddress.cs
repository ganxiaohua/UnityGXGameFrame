namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithAddress
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithAddress : MonoBehaviour
    {
        // Assign in Editor or in code
        public string address;

        // Retain handle to release asset and operation
        private AsyncOperationHandle<GameObject> handle;

        // Start the load operation on start
        void Start() {
            handle = Addressables.LoadAssetAsync<GameObject>(address);
            handle.Completed += Handle_Completed;
        }

        // Instantiate the loaded prefab on complete
        private void Handle_Completed(AsyncOperationHandle<GameObject> operation) {
            if (operation.Status == AsyncOperationStatus.Succeeded) {
                Instantiate(operation.Result, transform);
            } else {
                Debug.LogError($"Asset for {address} failed to load.");
            }
        }

        // Release asset when parent object is destroyed
        private void OnDestroy() {

            Addressables.Release(handle);
        }
    }
    #endregion
}