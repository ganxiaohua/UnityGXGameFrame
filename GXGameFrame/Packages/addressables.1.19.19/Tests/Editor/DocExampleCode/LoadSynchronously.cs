namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadSynchronously
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadSynchronously : MonoBehaviour
    {
        public string address;
        AsyncOperationHandle<GameObject> opHandle;

        void Start() {
            opHandle = Addressables.LoadAssetAsync<GameObject>(address);
            opHandle.WaitForCompletion(); // Returns when operation is complete

            if (opHandle.Status == AsyncOperationStatus.Succeeded) {
                Instantiate(opHandle.Result, transform);
            } else {
                Addressables.Release(opHandle);
            }
        }

        void OnDestroy() {
            Addressables.Release(opHandle);
        }
    }
    #endregion
}