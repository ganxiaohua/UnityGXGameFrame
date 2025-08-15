using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public partial class FGUIChecker
    {
        //原件依赖关系
        public class YuanJian
        {
            public string name;
            [LabelText("依赖包")] public string dependPackage;

            public string reason;
        }

        //组件依赖关系
        public class ZuJian
        {
            public string name;
            public string package;
            public List<YuanJian> yuanJians; //有依赖别的包的原件
        }

        public class Package
        {
            public string name;
            [HideInInspector] public List<ZuJian> zuJians;

            [OnValueChanged("OnPackageChange")] [ValueDropdown("dependPackages")]
            public string searchPackage;

            [HideInInspector] public List<string> dependPackages = new();
            public List<string> dependYuanJian = new();

            void OnPackageChange()
            {
                EditorApplication.delayCall += () =>
                {
                    dependYuanJian.Clear();
                    Debug.LogError(zuJians.Count);
                    foreach (var v in zuJians)
                    {
                        Debug.LogError(v.yuanJians.Count);
                    }

                    foreach (var v in zuJians)
                    {
                        foreach (var v2 in v.yuanJians)
                        {
                            if (v2.dependPackage == searchPackage)
                            {
                                dependYuanJian.Add($"{v.name} - {v2.name} - {v2.reason}");
                            }
                        }
                    }
                };
            }
        }

        [HorizontalGroup("Depend3", order: 204, width: 100)]
        [Button("刷新")]
        void RefreshDependency()
        {
            dependPackages.Clear();
            foreach (var (k, v) in packages)
            {
                Package package = new();
                package.name = v.packageName;
                package.zuJians = new();
                dependPackages.Add(package);
                foreach (var v2 in v.components)
                {
                    FindDepend(v2, package);
                }
            }

            allPackages.Clear();
            foreach (var v in dependPackages)
            {
                allPackages.Add(v.name);
                v.dependPackages.Clear();
                foreach (var zuJian in v.zuJians)
                {
                    foreach (var yuanJian in zuJian.yuanJians)
                    {
                        v.dependPackages.Add(yuanJian.dependPackage);
                    }
                }
            }
        }

        private List<Package> dependPackages = new();

        [HorizontalGroup("Depend3", order: 202)] [OnValueChanged("OnPackageChange2")] [ValueDropdown("allPackages")] [LabelText("源包")]
        public string srcPackage;

        [HorizontalGroup("Depend3", order: 203)] [OnValueChanged("OnPackageChange2")] [ValueDropdown("allPackages")] [LabelText("依赖包")]
        public string dependPackage;

        [HorizontalGroup("Depend4", order: 204)] [LabelText("提示"), LabelWidth(120), ReadOnly]
        public string tip = "点击刷新，然后选择源包和依赖包";

        private List<string> allPackages = new();

        [HorizontalGroup("Depend5", order: 205)] [LabelText("元件依赖关系")]
        public List<string> dependYuanJian = new();

        void OnPackageChange2()
        {
            EditorApplication.delayCall += () =>
            {
                tip = "";
                dependYuanJian.Clear();
                Package package = null;
                foreach (var v in dependPackages)
                {
                    if (v.name == srcPackage)
                    {
                        package = v;
                        break;
                    }
                }

                if (package == null)
                    return;
                foreach (var v in package.zuJians)
                {
                    foreach (var v2 in v.yuanJians)
                    {
                        if (v2.dependPackage == dependPackage)
                        {
                            dependYuanJian.Add($"{v.name} - {v2.name} - {v2.reason}");
                        }
                    }
                }

                if (dependYuanJian.Count == 0)
                {
                    tip = $"{srcPackage} 没有依赖 {dependPackage}";
                }
            };
        }


        void FindDepend(UIComponent component, Package package)
        {
            component.yuanJianPackageDict = new Dictionary<string, string>();

            if (!File.Exists(component.path)) return;

            // 先查找本节点，所有字段
            // 再查找子节点
            ZuJian zuJian = new ZuJian();
            zuJian.name = component.componentName;
            zuJian.package = component.packageName;
            zuJian.yuanJians = new();
            package.zuJians.Add(zuJian);

            XmlDocument xml = new XmlDocument();
            xml.Load(component.path);

            ProcessNode(xml, zuJian);
        }

        void ProcessNode(XmlNode node, ZuJian zuJian)
        {
            if (node == null)
                return;

            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                var noCheck = false;
                noCheck = !noCheck && FindXmlAttr(node, "clearOnPublish") == "true";
                noCheck = !noCheck && !string.IsNullOrEmpty(FindXmlAttr(node, "designImage"));
                if (!noCheck)
                {
                    var Attributes = node.Attributes;
                    foreach (XmlAttribute attribute in Attributes)
                    {
                        if (attribute.Name == "pkg") //直接引用了别的包（例如组件/图片等)
                        {
                            var yuanJian = new YuanJian();
                            yuanJian.name = FindXmlAttr(node, "name");
                            yuanJian.dependPackage = GetPackageName(attribute.Value);
                            yuanJian.reason = $"{attribute.Name} - {attribute.Value}";
                            zuJian.yuanJians.Add(yuanJian);
                        }
                        else if (attribute.Value.StartsWith("ui://"))
                        {
                            var linkPkg = attribute.Value.Substring(5, 8); //去掉ui://
                            var yuanJian = new YuanJian();
                            yuanJian.name = FindXmlAttr(node, "name");
                            if (string.IsNullOrEmpty(yuanJian.name))
                                yuanJian.name = "no_name_" + node.Name;
                            yuanJian.dependPackage = GetPackageName(linkPkg);
                            yuanJian.reason = $"{attribute.Name} - {attribute.Value}";
                            zuJian.yuanJians.Add(yuanJian);
                        }
                    }
                }
            }

            if (node.ChildNodes != null && node.ChildNodes.Count > 0)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    ProcessNode(child, zuJian);
                }
            }
        }
    }
}