namespace AddressableAssets.DocExampleCode
{
    #region doc_Load
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Events;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;

    internal class LoadWithLocation : MonoBehaviour
    {
        public Dictionary<string, AsyncOperationHandle<GameObject>> operationDictionary;
        public List<string> keys;
        public UnityEvent Ready;

        IEnumerator LoadAndAssociateResultWithKey(IList<string> keys) {
            if (operationDictionary == null)
                operationDictionary = new Dictionary<string, AsyncOperationHandle<GameObject>>();

            AsyncOperationHandle<IList<IResourceLocation>> locations
                = Addressables.LoadResourceLocationsAsync(keys,
                    Addressables.MergeMode.Union, typeof(GameObject));

            yield return locations;

            var loadOps = new List<AsyncOperationHandle>(locations.Result.Count);

            foreach (IResourceLocation location in locations.Result) {
                AsyncOperationHandle<GameObject> handle =
                    Addressables.LoadAssetAsync<GameObject>(location);
                handle.Completed += obj => operationDictionary.Add(location.PrimaryKey, obj);
                loadOps.Add(handle);
            }

            yield return Addressables.ResourceManager.CreateGenericGroupOperation(loadOps, true);

            Ready.Invoke();
        }

        void Start() {
            Ready.AddListener(OnAssetsReady);
            StartCoroutine(LoadAndAssociateResultWithKey(keys));
        }

        private void OnAssetsReady() {
            float x = 0, z = 0;
            foreach (var item in operationDictionary) {
                Debug.Log($"{item.Key} = {item.Value.Result.name}");
                Instantiate(item.Value.Result,
                            new Vector3(x++ * 2.0f, 0, z * 2.0f),
                            Quaternion.identity, transform);
                if (x > 9) {
                    x = 0;
                    z++;
                }
            }
        }

        private void OnDestroy() {
            foreach (var item in operationDictionary) {
                Addressables.Release(item.Value);
            }
        }
    }
    #endregion
    internal class LoadLocations
    {
        [System.Obsolete] //only because LoadResourceLocationsAsync now takes any IEnumerator rather than *just* a List
        IEnumerator example() {
            #region doc_LoadLocations
            AsyncOperationHandle<IList<IResourceLocation>> handle
                = Addressables.LoadResourceLocationsAsync(
                    new string[]{"knight", "villager"}, 
                    Addressables.MergeMode.Union);

            yield return handle;

            //...

            Addressables.Release(handle);
            #endregion
        }
    }
}