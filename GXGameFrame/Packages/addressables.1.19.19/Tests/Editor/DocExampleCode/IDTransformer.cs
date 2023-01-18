namespace AddressableAssets.DocExampleCode
{
    //Prevent Unity from actually registering the rule in this example
    using RuntimeInitializeOnLoadMethod = Dummy;

#if UNITY_EDITOR
    #region doc_Transformer
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.AddressableAssets;

    static class IDTransformer
    {
        //Implement a method to transform the internal ids of locations
        static string MyCustomTransform(IResourceLocation location) {
            if (location.ResourceType == typeof(IAssetBundleResource) 
                                         && location.InternalId.StartsWith("http"))
                return location.InternalId + "?customQueryTag=customQueryValue";

            return location.InternalId;
        }

        //Override the Addressables transform method with your custom method.
        //This can be set to null to revert to default behavior.
        [RuntimeInitializeOnLoadMethod]
        static void SetInternalIdTransform() {
            Addressables.InternalIdTransformFunc = MyCustomTransform;
        }
    }
    #endregion

#endif
}
