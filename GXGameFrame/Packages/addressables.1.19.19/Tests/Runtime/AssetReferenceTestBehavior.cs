using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetReferenceTestBehavior : MonoBehaviour
{
    public AssetReference Reference;
    public AssetReference InValidAssetReference;
    public AssetReference ReferenceWithSubObject;
    public AssetReference ReferenceWithMultiTypedSubObject;
    public AssetReference ReferenceWithMultiTypedSubObjectSubReference;

    public string AssetReferenceAddress;

    public AssetLabelReference LabelReference;
    public AssetLabelReference InvalidLabelReference;
}
