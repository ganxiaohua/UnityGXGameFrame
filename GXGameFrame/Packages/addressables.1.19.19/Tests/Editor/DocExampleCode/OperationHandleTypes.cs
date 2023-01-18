namespace AddressableAssets.DocExampleCode
{
    #region doc_LoadSynchronously
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;

    internal class OperationHandleTypes
    {
        void Snippet() {
            #region doc_ConvertTypes
            // Load asset using typed handle:
            AsyncOperationHandle<Texture2D> textureHandle = Addressables.LoadAssetAsync<Texture2D>("mytexture");

            // Convert the AsyncOperationHandle<Texture2D> to an AsyncOperationHandle:
            AsyncOperationHandle nonGenericHandle = textureHandle;

            // Convert the AsyncOperationHandle to an AsyncOperationHandle<Texture2D>:
            AsyncOperationHandle<Texture2D> textureHandle2 = nonGenericHandle.Convert<Texture2D>();

            // This will throw and exception because Texture2D is required:
            AsyncOperationHandle<Texture> textureHandle3 = nonGenericHandle.Convert<Texture>();
            #endregion
        }
    }
    #endregion
}