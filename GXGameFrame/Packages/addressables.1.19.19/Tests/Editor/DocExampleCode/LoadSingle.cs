namespace AddressableAssets.DocExampleCode
{
    #region doc_Load
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadAddress : MonoBehaviour
    {
        public string key;
        AsyncOperationHandle<GameObject> opHandle;

        public IEnumerator Start() {
            opHandle = Addressables.LoadAssetAsync<GameObject>(key);
            yield return opHandle;

            if (opHandle.Status == AsyncOperationStatus.Succeeded) {
                GameObject obj = opHandle.Result;
                Instantiate(obj, transform);
            }
        }

        void OnDestroy() {
            Addressables.Release(opHandle);
        }
    }
    #endregion
}