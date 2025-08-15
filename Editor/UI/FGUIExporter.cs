using System.Collections.Generic;
using System.IO;
using System.Xml;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class FGUIExporter : OdinEditorWindow
    {
        [MenuItem("GX框架工具/UI/组件导出")]
        public static void OpenWindow()
        {
            var window = GetWindow<FGUIExporter>();
            window.titleContent.text = "FGUI 导出";
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(16 * 50, 9 * 50);

            window.componentPath = UnityEngine.PlayerPrefs.GetString("FGUIExporter_componentPath", "");
            window.exportPath = UnityEngine.PlayerPrefs.GetString("FGUIExporter_exportPath", "");
        }

        [InfoBox("只能导出依赖的图片")] [LabelText("源组件路径"), Sirenix.OdinInspector.FilePath(Extensions = "xml", AbsolutePath = true)]
        public string componentPath;

        [LabelText("导出文件夹"), FolderPath(AbsolutePath = true),]
        public string exportPath;

        [HorizontalGroup("btn")]
        [Button("打开源文件夹")]
        void OpenSrc()
        {
            var file = new FileInfo(componentPath);
            System.Diagnostics.Process.Start("explorer.exe", file.Directory.FullName);
        }

        [HorizontalGroup("btn")]
        [Button("打开导出文件夹")]
        void OpenExoprt()
        {
            var dir = new FileInfo(exportPath);
            System.Diagnostics.Process.Start("explorer.exe", dir.FullName);
        }

        [HorizontalGroup("btn")]
        [Button("导出")]
        void OnClickExport()
        {
            if (string.IsNullOrEmpty(componentPath))
                return;
            if (string.IsNullOrEmpty(exportPath))
                return;

            PackageToDirectory();

            var file = new FileInfo(componentPath);
            var dirPath = file.Directory.FullName;

            XmlDocument xml = new XmlDocument();
            xml.Load(componentPath);
            var displayListNode = xml.SelectSingleNode("component")?.SelectSingleNode("displayList");
            if (displayListNode == null) return;

            var nodes = displayListNode.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Name == "image")
                {
                    var fileName = FGUIChecker.FindXmlAttr(node, "fileName");
                    var pkg = FGUIChecker.FindXmlAttr(node, "pkg");
                    var rootPath = dirPath;
                    if (!string.IsNullOrEmpty(pkg) && packages.ContainsKey(pkg))
                    {
                        rootPath = packages[pkg].rootPath;
                    }

                    if (fileName != null)
                    {
                        var fullPath = Path.Combine(rootPath, fileName);
                        if (File.Exists(fullPath))
                        {
                            var toDir = ToTargetDir(fullPath);
                            if (!Directory.Exists(toDir))
                                Directory.CreateDirectory(toDir);
                            var toPath = ToTargetFilePath(fullPath);
                            if (!File.Exists(toPath))
                                File.Copy(fullPath, toPath);
                        }
                    }
                }
            }

            var toComPath = ToTargetFilePath(file.FullName);
            File.WriteAllText(toComPath, File.ReadAllText(componentPath));

            UnityEngine.PlayerPrefs.SetString("FGUIExporter_componentPath", componentPath);
            UnityEngine.PlayerPrefs.SetString("FGUIExporter_exportPath", exportPath);
            Debug.Log("完成");
        }

        private string ToTargetDir(string srcFullPath)
        {
            var assetPath = srcFullPath.Replace('\\', '/');
            assetPath = assetPath.Substring(assetPath.IndexOf("/assets") + 1);
            assetPath = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            var toDir = Path.Combine(exportPath, assetPath);
            return toDir;
        }

        private string ToTargetFilePath(string srcFullPath)
        {
            var dir = ToTargetDir(srcFullPath);
            var file = new FileInfo(srcFullPath);
            return Path.Combine(dir, file.Name);
        }

        public class UIPackage
        {
            public string id;
            public string rootPath;
        }

        private Dictionary<string, UIPackage> packages = new();

        void PackageToDirectory()
        {
            if (packages.Count > 0)
                return;
            var assetsPath = componentPath.Substring(0, componentPath.IndexOf("/assets/") + "/assets".Length);
            Debug.Log("assetsPath" + assetsPath);
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
                pkg.rootPath = directory;

                XmlDocument xml = new XmlDocument();
                xml.Load(path);
                var node = xml.SelectSingleNode("packageDescription");
                var idVal = FGUIChecker.FindXmlAttr(node, "id");
                pkg.id = idVal;

                packages.Add(pkg.id, pkg);
            }
        }
    }
}