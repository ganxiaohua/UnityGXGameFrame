namespace YooAsset.Editor
{
    [DisplayName("定位地址: 文件夹_文件夹_文件名")]
    public class AddressByFolderFile : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string path = data.AssetPath.Replace(data.CollectPath, "");
            string name = path.Substring(1, path.LastIndexOf('.')-1);
            return name;
        }
    }
}