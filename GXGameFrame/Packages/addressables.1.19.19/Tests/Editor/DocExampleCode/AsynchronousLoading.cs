namespace AddressableAssets.DocExampleCode
{
    #region doc_asyncload
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class AsynchronousLoading : MonoBehaviour
    {
        private string address = "tree";
        private AsyncOperationHandle loadHandle;
        
        // always minimum of 1 frame
        IEnumerator LoadAssetCoroutine() {
            loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
            yield return loadHandle;
        }

        // minimum of 1 frame for new asset loads
        // callback called in current frame for already loaded assets
        void LoadAssetCallback() {
            loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
            loadHandle.Completed += h =>
            {
                // Loaded here
            };
        }

        // minimum of 1 frame for new asset loads
        // await completes in current frame for already loaded assets
        async void LoadAssetWait() {
            loadHandle = Addressables.LoadAssetAsync<GameObject>(address);
            await loadHandle.Task;
        }
        
        private void OnDestroy() {
            Addressables.Release(loadHandle);
        }
    }
    #endregion
}