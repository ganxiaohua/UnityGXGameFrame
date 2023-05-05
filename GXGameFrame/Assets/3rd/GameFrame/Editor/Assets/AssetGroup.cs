using BundlePackingMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundlePackingMode;
using BundleCompressionMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundleCompressionMode;
using Sirenix.OdinInspector;

namespace GameFrame.Editor
{
    [System.Serializable]
    public class AssetGroup
    {
        [LabelText("组名称")]
        [GUIColor(0.3f, 0.8f, 0.8f, 1f)]
        public string groupName;
        [LabelText("路径")]
        public string[] searchPaths;
        [LabelText("类型")]
        public string filter;
        [LabelText("使用短路径")]
        public bool simplifyAddressName = true;
        [LabelText("按目录打包")]
        public bool packByDir;
        [LabelText("打包方式")]
        public BundlePackingMode bundleMode = BundlePackingMode.PackSeparately;
        [LabelText("压缩方式")]
        public BundleCompressionMode compression = BundleCompressionMode.LZ4;
    }
}
