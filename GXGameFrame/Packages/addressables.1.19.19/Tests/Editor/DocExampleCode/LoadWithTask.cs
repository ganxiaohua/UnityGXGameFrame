namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadWithTask
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class LoadWithTask : MonoBehaviour
    {
        // Label or address strings to load
        public List<string> keys = new List<string>() { "characters", "animals" };

        // Operation handle used to load and release assets
        AsyncOperationHandle<IList<GameObject>> loadHandle;

        public async void Start() {
            loadHandle = Addressables.LoadAssetsAsync<GameObject>(
                keys, // Either a single key or a List of keys 
                addressable => {
                // Called for every loaded asset
                Debug.Log(addressable.name);
                }, Addressables.MergeMode.Union, // How to combine multiple labels 
                false); // Whether to fail if any asset fails to load

            // Wait for the operation to finish in the background
            await loadHandle.Task;

            // Instantiate the results
            float x = 0, z = 0;
            foreach (var addressable in loadHandle.Result) {
                if (addressable != null) {
                    Instantiate<GameObject>(addressable,
                            new Vector3(x++ * 2.0f, 0, z * 2.0f),
                            Quaternion.identity,
                            transform); // make child of this object

                    if (x > 9) {
                        x = 0;
                        z++;
                    }
                }
            }
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

    internal class LoadSequence
    {
        List<string> keys = new List<string>() { "characters", "animals" };
        UnityEngine.SceneManagement.Scene nextScene;
        async void load() {
            #region doc_UseWhenAll
            // Load the Prefabs
            var prefabOpHandle = Addressables.LoadAssetsAsync<GameObject>(
                keys, null, Addressables.MergeMode.Union, false);

            // Load a Scene additively
            var sceneOpHandle 
                = Addressables.LoadSceneAsync(nextScene, 
                    UnityEngine.SceneManagement.LoadSceneMode.Additive);

            await System.Threading.Tasks.Task.WhenAll(prefabOpHandle.Task, sceneOpHandle.Task);
            #endregion
        }
    }
}