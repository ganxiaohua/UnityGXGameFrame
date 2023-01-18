using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressableAssets.DocExampleCode
{
#if UNITY_EDITOR
    #region doc_TransformerWebRequest
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.AddressableAssets;

    internal class WebRequestOverride : MonoBehaviour
    {
        //Register to override WebRequests Addressables creates to download
        private void Start()
        {
            Addressables.WebRequestOverride = EditWebRequestURL;
        }
        
        //Override the url of the WebRequest, the request passed to the method is what would be used as standard by Addressables.
        private void EditWebRequestURL(UnityWebRequest request)
        {
            if (request.url.EndsWith(".bundle"))
                request.url = request.url + "?customQueryTag=customQueryValue";
            else if (request.url.EndsWith(".json") || request.url.EndsWith(".hash"))
                request.url = request.url + "?customQueryTag=customQueryValue";
        }
    }
    #endregion

#endif
}
