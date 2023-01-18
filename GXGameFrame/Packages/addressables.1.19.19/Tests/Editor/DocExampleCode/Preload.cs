namespace AddressableAssets.DocExampleCode
{
    #region doc_Preload
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    internal class Preload : MonoBehaviour
    {
        public IEnumerator Start() {
            yield return Addressables.DownloadDependenciesAsync("preload", true);
        }
    }
    #endregion
}