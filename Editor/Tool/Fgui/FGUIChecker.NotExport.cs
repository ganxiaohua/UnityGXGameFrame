using System.IO;
using System.Xml;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Editor.UI
{
    public partial class FGUIChecker
    {
        // 检查引用了未设置为导出原件 的 组件。
        [HorizontalGroup("line1", 0.2f)]
        [Button("未导出问题", buttonSize: 50)]
        void CheckNotExport()
        {
            Debug.Log("开始检查未导出问题");
            var find = false;
            foreach (var (k, v) in packages)
            {
                foreach (var component in v.Components)
                {
                    if (CheckComponentLinkAsset(component))
                    {
                        find = true;
                    }
                }
            }

            Debug.Log("结束检查未导出问题:" + (find ? "存在未导出问题" : "不存在未导出问题"));
        }

        /// <summary>
        /// 检查组件引用的资源 是否有问题。
        /// 例如图片未导出， 或者图片导出了但包未发布。
        /// </summary>
        /// <param name="component"></param>
        /// <returns>true 表示有问题</returns>
        bool CheckComponentLinkAsset(UIComponent component)
        {
            if (!File.Exists(component.path)) return false;

            XmlDocument xml = new XmlDocument();
            xml.Load(component.path);
            var displayListNode = xml.SelectSingleNode("component")?.SelectSingleNode("displayList");
            if (displayListNode == null) return false;

            var nodes = displayListNode.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var displayName = FindXmlAttr(node, "name");
                if (node.Name == "loader")
                {
                    var urlValue = FindXmlAttr(node, "url");
                    var clearOnPublishValue = FindXmlAttr(node, "clearOnPublish");
                    if (!string.IsNullOrEmpty(urlValue) && string.IsNullOrEmpty(clearOnPublishValue))
                    {
                        var pkgID = urlValue.Substring(5, 8);
                        var pkgName = GetPackageName(pkgID);
                        if (string.IsNullOrEmpty(pkgName))
                        {
                            return false;
                        }

                        if (pkgName == component.packageName)
                            continue; // 组件和资源同一个包，没有清除的话，fgui会强制导出的（不管资源图片是否导出）

                        if (!IsPackagePublish(pkgName))
                        {
                            Debug.LogWarning($"检查组件{component.path}, 节点名{displayName}, 其依赖{pkgName}包的未发布");
                            return true;
                        }

                        var assetID = urlValue.Substring(13);
                        var (exported, assetName) = IsAssetExported(pkgName, assetID);
                        if (!exported)
                        {
                            Debug.LogWarning($"检查组件{component.path}, 节点名{displayName}, 其依赖{pkgName}包的原件{assetName}未导出");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 包是否发布
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetID"></param>
        /// <returns></returns>
        bool IsPackagePublish(string packageName)
        {
            return File.Exists($"{Application.dataPath}/Art/UI/{packageName}_fui.bytes");
        }

        /// <summary>
        /// 资源是否导出
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="assetID"></param>
        /// <returns></returns>
        (bool, string) IsAssetExported(string packageName, string assetID)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                // 包不存在，运行时不会报错的。
                return (true, assetID);
            }

            if (packages.TryGetValue(packageName, out var package))
            {
                if (package.Assets.TryGetValue(assetID, out var assetInfo))
                {
                    return (assetInfo.exported, assetInfo.name);
                }
            }

            return (true, assetID);
        }
    }
}