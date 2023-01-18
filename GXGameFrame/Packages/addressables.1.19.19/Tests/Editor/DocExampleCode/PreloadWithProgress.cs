namespace AddressableAssets.DocExampleCode
{
    #region doc_Preload
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Events;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class PreloadWithProgress : MonoBehaviour
    {
        public string preloadLabel = "preload";
        public UnityEvent<float> ProgressEvent;
        public UnityEvent<bool> CompletionEvent;
        private AsyncOperationHandle downloadHandle;

        IEnumerator Start() {
            downloadHandle = Addressables.DownloadDependenciesAsync(preloadLabel, false);
            float progress = 0;

            while (downloadHandle.Status == AsyncOperationStatus.None) {
                float percentageComplete = downloadHandle.GetDownloadStatus().Percent;
                if (percentageComplete > progress * 1.1) // Report at most every 10% or so
                {
                    progress = percentageComplete; // More accurate %
                    ProgressEvent.Invoke(progress);
                }
                yield return null;
            }

            CompletionEvent.Invoke(downloadHandle.Status == AsyncOperationStatus.Succeeded);
            Addressables.Release(downloadHandle); //Release the operation handle
        }
    }
    #endregion

    internal class PreloadExamples
    {
        string key;
        void example() {
            #region doc_DownloadSize
            AsyncOperationHandle<long> getDownloadSize =
                Addressables.GetDownloadSizeAsync(key);
            #endregion
        }
    }
}