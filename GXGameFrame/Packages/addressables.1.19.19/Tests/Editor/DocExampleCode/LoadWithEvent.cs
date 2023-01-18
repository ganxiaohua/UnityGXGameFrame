namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithEvent
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithEvent : MonoBehaviour
    {
        public string address;
        AsyncOperationHandle<GameObject> opHandle;

        void Start() {
            // Create operation
            opHandle = Addressables.LoadAssetAsync<GameObject>(address);
            // Add event handler
            opHandle.Completed += Operation_Completed;
        }

        private void Operation_Completed(AsyncOperationHandle<GameObject> obj) {

            if (obj.Status == AsyncOperationStatus.Succeeded) {
                Instantiate(obj.Result, transform);
            } else {
                Addressables.Release(obj);
            }
        }

        void OnDestroy() {
            Addressables.Release(opHandle);
        }
    }
    #endregion
}