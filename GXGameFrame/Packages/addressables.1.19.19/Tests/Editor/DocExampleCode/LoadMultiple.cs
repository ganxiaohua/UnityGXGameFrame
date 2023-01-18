namespace AddressableAssets.DocExampleCode
{
    #region doc_Load
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadMultiple : MonoBehaviour
    {
        // Label strings to load
        public List<string> keys = new List<string>() { "characters", "animals" };

        // Operation handle used to load and release assets
        AsyncOperationHandle<IList<GameObject>> loadHandle;

        // Load Addressables by Label
        public IEnumerator Start() {
            float x = 0, z = 0;
            loadHandle = Addressables.LoadAssetsAsync<GameObject>(
                keys,
                addressable => {
            //Gets called for every loaded asset
            Instantiate<GameObject>(addressable,
                        new Vector3(x++ * 2.0f, 0, z * 2.0f),
                        Quaternion.identity,
                        transform);

                    if (x > 9) {
                        x = 0;
                        z++;
                    }
                }, Addressables.MergeMode.Union, // How to combine multiple labels 
                false); // Whether to fail and release if any asset fails to load

            yield return loadHandle;
        }

        private void OnDestroy() {
            Addressables.Release(loadHandle);
            // Release all the loaded assets associated with loadHandle
            // Note that if you do not make loaded addressables a child of this object,
            // then you will need to devise another way of releasing the handle when
            // all the individual addressables are destroyed.
        }
    }
    #endregion
}