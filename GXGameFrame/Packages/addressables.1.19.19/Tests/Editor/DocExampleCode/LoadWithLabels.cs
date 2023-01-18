namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithLabels
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithLabels : MonoBehaviour
    {
        // Label strings to load
        public List<string> keys = new List<string>() { "characters", "animals" };

        // Operation handle used to load and release assets
        AsyncOperationHandle<IList<GameObject>> loadHandle;

        // Load Addressables by Label
        void Start() {
            float x = 0, z = 0;
            loadHandle = Addressables.LoadAssetsAsync<GameObject>(
                keys, // Either a single key or a List of keys 
                addressable => {
                //Gets called for every loaded asset
                if (addressable != null) {
                        Instantiate<GameObject>(addressable,
                            new Vector3(x++ * 2.0f, 0, z * 2.0f),
                            Quaternion.identity,
                            transform);
                        if (x > 9) {
                            x = 0;
                            z++;
                        }
                    }
                }, Addressables.MergeMode.Union, // How to combine multiple labels 
                false); // Whether to fail if any asset fails to load
            loadHandle.Completed += LoadHandle_Completed;
        }

        private void LoadHandle_Completed(AsyncOperationHandle<IList<GameObject>> operation) {
            if (operation.Status != AsyncOperationStatus.Succeeded)
                Debug.LogWarning("Some assets did not load.");
        }

        private void OnDestroy() {
            // Release all the loaded assets associated with loadHandle
            Addressables.Release(loadHandle);
        }
    }
    #endregion
}