namespace AddressableAssets.DocExampleCode
{
    #region doc_Instantiate
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class InstantiateFromKey : MonoBehaviour
    {
        public string key; // Identify the asset

        void Start() {
            // Load and instantiate
            Addressables.InstantiateAsync(key).Completed += instantiate_Completed;
        }

        private void instantiate_Completed(AsyncOperationHandle<GameObject> obj) {
            // Add component to release asset in GameObject OnDestroy event
            obj.Result.AddComponent(typeof(SelfCleanup));
        }
    }

    // Releases asset (trackHandle must be true in InstantiateAsync,
    // which is the default)
    internal class SelfCleanup : MonoBehaviour
    {
        void OnDestroy() {
            Addressables.ReleaseInstance(gameObject);
        }
    }
    #endregion
}