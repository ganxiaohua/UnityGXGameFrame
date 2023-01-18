namespace AddressableAssets.DocExampleCode
{
    #region doc_DownloadError
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.Exceptions;

    internal class HandleDownloadError : MonoBehaviour
    {
        private AsyncOperationHandle m_Handle;
        void LoadAsset()
        {
            m_Handle = Addressables.LoadAssetAsync<GameObject>("addressKey");
            m_Handle.Completed += handle =>
            {
                string dlError = GetDownloadError(m_Handle);
                if (!string.IsNullOrEmpty(dlError))
                {
                    // handle what error
                }
            };
        }
        
        string GetDownloadError(AsyncOperationHandle fromHandle)
        {
            if (fromHandle.Status != AsyncOperationStatus.Failed)
                return null;

            RemoteProviderException remoteException;
            System.Exception e = fromHandle.OperationException;
            while (e != null)
            {
                remoteException = e as RemoteProviderException;
                if (remoteException != null)
                    return remoteException.WebRequestResult.Error;
                e = e.InnerException;
            }
            return null;
        }
    }
    #endregion
}