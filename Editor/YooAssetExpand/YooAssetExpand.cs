using System.IO;

namespace YooAsset.Editor
{
    [DisplayName("定位地址: 文件夹_文件夹_文件名")]
    public class AddressByFolderFile : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string path = data.AssetPath.Replace(data.CollectPath, "");
            string name = path.Substring(1, path.LastIndexOf('.') - 1);
            return name;
        }
    }


    [DisplayName("定位地址: 文件名加后缀")]
    public class AddressByName : IAddressRule
    {
        string IAddressRule.GetAssetAddress(AddressRuleData data)
        {
            string path = data.AssetPath.Replace(data.CollectPath, "");
            string name = Path.GetFileName(path);
            return name;
        }
    }
    
    [DisplayName("打包路径OneSelf顶层路径")]
    public class PackOneSelfDirectory : IPackRule
    {
        PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
        {
            var assetPath = data.AssetPath;
            var path = assetPath.Replace("Assets/Res/_OneSelf/", "");
            var foldName = path.Substring(0, path.IndexOf('/'));
            PackRuleResult result = new PackRuleResult("Assets/Res/_OneSelf/"+foldName, DefaultPackRule.AssetBundleFileExtension);
            return result;   
        }
    }
}