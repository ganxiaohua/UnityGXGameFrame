using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Editor
{
    public partial class FGUIChecker
    {
        private class BadReferenceSetting
        {
            public string NodeName;
            public string AttrName;
            public bool CheckClear;

            public BadReferenceSetting(string nodeName, string attrName, bool checkClear)
            {
                this.NodeName = nodeName;
                this.AttrName = attrName;
                this.CheckClear = checkClear;
            }
        }

        private static List<BadReferenceSetting> badReferenceSettings = new()
        {
            new BadReferenceSetting("loader", "url", true),
            new BadReferenceSetting("text", "font", false),
            new BadReferenceSetting("list", "defaultItem", false),
            new BadReferenceSetting("Label", "icon", false),
        };


        // A-B-C-A 的依赖关系。找出 每一个依赖 的组件列表即可。剩下的自己查了。
        // 想要切断，把某一环里面的所有组件切断即可。
        [HorizontalGroup("line1", 0.2f)]
        [Button("依赖问题", buttonSize: 50)]
        void CheckBadReference()
        {
            // 先找直接依赖(A-B-A)
            foreach (var k1 in packages.Keys)
            {
                foreach (var k2 in packages.Keys)
                {
                    var (ret1, component1, name1) = IsDirectDependOn(k1, k2);
                    var (ret2, component2, name2) = IsDirectDependOn(k2, k1);
                    if (ret1 && ret2)
                    {
                        var str = $"{k1}-{k2}-{k1} 循环依赖。 \n";
                        str += $"检查包: {k1}, 组件: {component1.componentName}, 原件: {name1} \n";
                        str += $"检查包: {k2}, 组件: {component2.componentName}, 原件: {name2} \n";
                        str += "断掉某个包与包之间所有联系即可";
                        Debug.LogError(str);
                        return;
                    }
                }
            }

            // (A-B-C-A)
            foreach (var k1 in packages.Keys)
            {
                foreach (var k2 in packages.Keys)
                {
                    var (ret1, component1, name1) = IsDirectDependOn(k1, k2);
                    if (ret1)
                    {
                        // Debug.Log($"{k1} 和 {k2} 循环依赖。 检查{component1.path}, {name1}");
                        foreach (var k3 in packages.Keys)
                        {
                            if (k3 == k1 || k3 == k2) continue;

                            var (ret2, component2, name2) = IsDirectDependOn(k2, k3);
                            var (ret3, component3, name3) = IsDirectDependOn(k3, k1);
                            if (ret3 && ret2)
                            {
                                var str = $"{k1}-{k2}-{k3}-{k1} 循环依赖。 \n";
                                str += $"检查包: {k1}, 组件: {component1.componentName}, 原件: {name1} \n";
                                str += $"检查包: {k2}, 组件: {component2.componentName}, 原件: {name2} \n";
                                str += $"检查包: {k3}, 组件: {component3.componentName}, 原件: {name3} \n";
                                str += "断掉某个包与包之间所有联系即可";
                                Debug.LogError(str);
                                return;
                            }
                        }
                    }
                }
            }

            Debug.Log("检查完成.无循环依赖");
        }

        /// <summary>
        /// A直接依赖B， 返回因为哪个组件以及原件导致的依赖。
        /// </summary>
        (bool, UIComponent, string) IsDirectDependOn(string pkgA, string pkgB)
        {
            if (pkgA == pkgB) return (false, null, null);
            var fGuiPackageA = packages[pkgA];
            foreach (var component in fGuiPackageA.Components)
            {
                foreach (var (k2, v2) in component.yuanJianPackageDict)
                {
                    if (v2 == pkgB)
                    {
                        return (true, component, k2);
                    }
                }
            }

            return (false, null, null);
        }

        /// <summary>
        /// 处理组件， 补充组件信息（例如原件的包列表）
        /// </summary>
        /// <param name="component"></param>
        void ProcessComponent(UIComponent component)
        {
            component.yuanJianPackageDict = new Dictionary<string, string>();

            if (!File.Exists(component.path)) return;

            XmlDocument xml = new XmlDocument();
            xml.Load(component.path);
            var displayListNode = xml.SelectSingleNode("component")?.SelectSingleNode("displayList");
            if (displayListNode == null) return;

            var nodes = displayListNode.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var pkgID = FindXmlAttr(node, "pkg");
                var yuanJianName = FindXmlAttr(node, "name");
                // if (node.Name == "loader")  // image 等 如果隐藏的，还是会load 包的。
                // {
                //     var visible = FindXmlAttr(node, "visible");
                //     if (visible == "false") continue;
                // }

                if (component.yuanJianPackageDict.ContainsKey(yuanJianName)) continue;

                if (!string.IsNullOrEmpty(pkgID))
                {
                    component.yuanJianPackageDict.Add(yuanJianName, GetPackageName(pkgID));
                }
                else
                {
                    foreach (var setting in badReferenceSettings)
                    {
                        if (node.Name == setting.NodeName)
                        {
                            var value = FindXmlAttr(node, setting.AttrName);
                            var clearOnPublishValue = FindXmlAttr(node, "clearOnPublish");
                            if (setting.CheckClear && !string.IsNullOrEmpty(clearOnPublishValue))
                            {
                                break;
                            }

                            if (!string.IsNullOrEmpty(value))
                            {
                                var linkPkg = value.Substring(5, 8);
                                component.yuanJianPackageDict.Add(yuanJianName, GetPackageName(linkPkg));
                            }
                        }
                    }
                }
            }
        }

        public static string FindXmlAttr(XmlNode node, string attrName)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == attrName)
                {
                    return node.Attributes[i].Value;
                }
            }

            return null;
        }

        string GetPackageID(string name)
        {
            packageNameToID.TryGetValue(name, out var ret);
            return ret;
        }

        string GetPackageName(string id)
        {
            foreach (var (k, v) in packageNameToID)
            {
                if (v == id) return k;
            }

            return null;
        }
    }
}