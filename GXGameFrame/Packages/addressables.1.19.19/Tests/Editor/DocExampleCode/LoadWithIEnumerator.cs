namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithIEnumerator
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithIEnumerator : MonoBehaviour
    {
        public string address;
        AsyncOperationHandle<GameObject> opHandle;

        public IEnumerator Start() {
            opHandle = Addressables.LoadAssetAsync<GameObject>(address);

            // yielding when already done still waits until the next frame
            // so don't yield if done.
            if (!opHandle.IsDone)
                yield return opHandle;

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