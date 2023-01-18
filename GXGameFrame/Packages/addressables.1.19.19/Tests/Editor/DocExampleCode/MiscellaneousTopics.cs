namespace AddressableAssets.DocExampleCode
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.AddressableAssets.ResourceLocators;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;

    internal class MiscellaneousTopics : MonoBehaviour
    {
        #region doc_LoadAdditionalCatalog
        public IEnumerator Start() {
            //Load a catalog and automatically release the operation handle.
            AsyncOperationHandle<IResourceLocator> handle 
                = Addressables.LoadContentCatalogAsync("path_to_secondary_catalog", true);
            yield return handle;

            //...
        }
        #endregion
        #region doc_UpdateCatalog
        IEnumerator UpdateCatalogs() {
            AsyncOperationHandle<List<IResourceLocator>> updateHandle
                = Addressables.UpdateCatalogs();

            yield return updateHandle;
            Addressables.Release(updateHandle);
        }
        #endregion
        #region doc_CheckCatalog
        IEnumerator CheckCatalogs() {
            List<string> catalogsToUpdate = new List<string>();
            AsyncOperationHandle<List<string>> checkForUpdateHandle 
                = Addressables.CheckForCatalogUpdates();
            checkForUpdateHandle.Completed += op =>
            {
                catalogsToUpdate.AddRange(op.Result);
            };

            yield return checkForUpdateHandle;

            if (catalogsToUpdate.Count > 0) {
                AsyncOperationHandle<List<IResourceLocator>> updateHandle 
                    = Addressables.UpdateCatalogs(catalogsToUpdate);
                yield return updateHandle;
                Addressables.Release(updateHandle);
            }

            Addressables.Release(checkForUpdateHandle);
        }
        #endregion

        #region doc_TransformID
        void SetIDTransformFunction() {
            Addressables.ResourceManager.InternalIdTransformFunc = TransformFunc;
        }

        string TransformFunc(IResourceLocation location) {
            //Implement a method that gets the base url for a given location
            string baseUrl = GetBaseURL(location);

            //Get the url you want to use to point to your current server
            string currentUrlToUse = GetCurrentURL();

            return location.InternalId.Replace(baseUrl, currentUrlToUse);
        }

        string GetBaseURL(IResourceLocation location) {
            return "baseURL"; // The part of the id that represents the base URL
        }

        string GetCurrentURL() {
            return "https://example.com"; // The replacement URL
        }
        #endregion

        IEnumerator example_GetAddress() {
            AssetReference MyRef1 = new AssetReference();

            #region doc_AddressFromReference
            var opHandle = Addressables.LoadResourceLocationsAsync(MyRef1);
            yield return opHandle;

            if (opHandle.Status == AsyncOperationStatus.Succeeded &&
                opHandle.Result != null &&
                opHandle.Result.Count > 0) 
            {
                Debug.Log("address is: " + opHandle.Result[0].PrimaryKey);
            }
            #endregion
        }
        #region doc_PreloadHazards
        Dictionary<string, GameObject> _preloadedObjects 
            = new Dictionary<string, GameObject>();

        private IEnumerator PreloadHazards() {
            //find all the locations with label "SpaceHazards"
            var loadResourceLocationsHandle 
                = Addressables.LoadResourceLocationsAsync("SpaceHazards", typeof(GameObject));

            if (!loadResourceLocationsHandle.IsDone)
                yield return loadResourceLocationsHandle;

            //start each location loading
            List<AsyncOperationHandle> opList = new List<AsyncOperationHandle>();

            foreach (IResourceLocation location in loadResourceLocationsHandle.Result) {
                AsyncOperationHandle<GameObject> loadAssetHandle 
                    = Addressables.LoadAssetAsync<GameObject>(location);
                loadAssetHandle.Completed += 
                    obj => { _preloadedObjects.Add(location.PrimaryKey, obj.Result); };
                opList.Add(loadAssetHandle);
            }

            //create a GroupOperation to wait on all the above loads at once. 
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(opList);

            if (!groupOp.IsDone)
                yield return groupOp;

            Addressables.Release(loadResourceLocationsHandle);

            //take a gander at our results.
            foreach (var item in _preloadedObjects) {
                Debug.Log(item.Key + " - " + item.Value.name);
            }
        }
        #endregion

        private IEnumerator PreloadAssets() {
            #region doc_Download
            string key = "assetKey";

            // Check the download size
            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(key);
            yield return getDownloadSize;

            //If the download size is greater than 0, download all the dependencies.
            if (getDownloadSize.Result > 0) {
                AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(key);
                yield return downloadDependencies;
            }
            #endregion
        }
    }
}