namespace AddressableAssets.DocExampleCode
{
    #region doc_AddExceptionHandler
    using System;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class AddExceptionHandler : MonoBehaviour
    {
        void Start() {
            ResourceManager.ExceptionHandler = CustomExceptionHandler;
        }

        // Gets called for every error scenario encountered during an operation.
        // A common use case for this is having InvalidKeyExceptions fail silently when 
        // a location is missing for a given key.
        void CustomExceptionHandler(AsyncOperationHandle handle, Exception exception) {
            if (exception.GetType() != typeof(InvalidKeyException))
                Addressables.LogException(handle, exception);
        }
    }
    #endregion
}