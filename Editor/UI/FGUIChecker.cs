using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public partial class FGUIChecker : OdinEditorWindow
    {
        [MenuItem("GX框架工具/UI/FGUI检查")]
        public static void OpenWindow()
        {
            var window = GetWindow<FGUIChecker>();
            window.titleContent.text = "FGUI检查";
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(16 * 50, 9 * 50);
        }

        // 在fgui的资源管理器中，不是资源就是组件
        // 在组件视图中， 有组件，也有原件（fgui提供的一些功能组件，不能再分割)
        public class UIPackage
        {
            public string packageName;
            public string path;
            public string id;

            public List<UIComponent> components;           // 相当于FGui资源管理器里面组件列表
            public Dictionary<string, UIAssetInfo> assets; // 相当于FGui资源管理器资源列表
        }

        [Serializable]
        public class UIComponent
        {
            public string componentName;
            [HideInInspector] public string path;
            public string packageName;

            /// <summary>
            /// fgui 原件。 key：原件的display name， value 依赖的包。
            /// 可以认为是资源/组件 所在包。
            /// </summary>
            public Dictionary<string, string> yuanJianPackageDict;
        }

        public class UIAssetInfo
        {
            public string id;
            public string packageName;
            public bool exported;
            public string name;
        }

        private string assetsPath;
        private Dictionary<string, string> packageNameToID = new Dictionary<string, string>();

        /// <summary>
        /// key 为 包的文件名称，非包id，需要注意。
        /// </summary>
        private Dictionary<string, UIPackage> packages = new Dictionary<string, UIPackage>();

        protected override void OnEnable()
        {
            base.OnEnable();

            assetsPath = $"{Application.dataPath}/../../eden_fgui/assets";
            assetsPath = new DirectoryInfo(assetsPath).FullName;

            packageNameToID.Clear();
            packages.Clear();

            foreach (var directory in Directory.GetDirectories(assetsPath, "*.*", SearchOption.TopDirectoryOnly))
            {
                var dir = new DirectoryInfo(directory);
                var path = $"{directory}/package.xml";
                if (!File.Exists(path))
                {
                    Debug.Log($"Could not find file:{path}");
                    continue;
                }

                var pkg = new UIPackage();
                pkg.packageName = dir.Name;
                pkg.path = path;
                pkg.components = new List<UIComponent>();
                pkg.assets = new Dictionary<string, UIAssetInfo>();

                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                var node = xml.SelectSingleNode("packageDescription");
                var idVal = FindXmlAttr(node, "id");
                pkg.id = idVal;

                packages.Add(pkg.packageName, pkg);
                packageNameToID.Add(pkg.packageName, idVal);
            }

            foreach (var (k, v) in packages)
            {
                var root = v.path.Substring(0, v.path.LastIndexOf("/"));
                XmlDocument xml = new XmlDocument();
                xml.Load(v.path);

                var nodes = xml.SelectSingleNode("packageDescription").SelectSingleNode("resources").ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    var idValue = FindXmlAttr(node, "id");
                    var nameValue = FindXmlAttr(node, "name");
                    if (idValue == null || nameValue == null) continue;
                    var pathValue = FindXmlAttr(node, "path");
                    var exportedValue = FindXmlAttr(node, "exported");

                    // 不是组件，那就是资源
                    if (node.Name != "component")
                    {
                        if (!v.assets.ContainsKey(idValue))
                        {
                            var yj = new UIAssetInfo();
                            yj.packageName = k;
                            yj.id = idValue;
                            yj.name = nameValue;
                            yj.exported = exportedValue == "true";
                            v.assets.TryAdd(idValue, yj);
                        }

                        continue;
                    }

                    var childPath = $"{root}{pathValue}{nameValue}";
                    var component = new UIComponent();
                    component.path = childPath;
                    component.componentName = nameValue;
                    component.packageName = k;
                    v.components.Add(component);
                }
            }
        }
    }
}