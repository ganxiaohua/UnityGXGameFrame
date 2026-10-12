using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FairyGUI;
using FairyGUIEditor;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public sealed class FGUIPanelCodeGen : AssetPostprocessor
    {
        private static string SourceDir = "";
        private static string OutputDir = "";

        private const string LogicCodeTemplatePath = "Assets/3rd/UnityGXGameFrame/Editor/Text/UI/Panel.txt";
        private const string GenCodeTemplatePath = "Assets/3rd/UnityGXGameFrame/Editor/Text/UI/Panel.gen.txt";
        private const string ComponentTemplatePath = "Assets/3rd/UnityGXGameFrame/Editor/Text/UI/Component.gen.txt";

        private const string VarTab = "\t\t";
        private const string BodyTab = "\t\t\t";

        private static readonly CSharpCodeProvider CodeProvider = new CSharpCodeProvider();

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!EditorPrefs.HasKey(EditorPrefsKey.KeyAuto))
                return;

            ProcessAssets(importedAssets);
            ProcessAssets(movedAssets);
        }

        private static void ProcessAssets(string[] paths)
        {
            SourceDir = NormalizePath(FGUICodeGenSettings.GetFullPath(EditorString.GetPath("FguiConfigAssetPath")));
            OutputDir = NormalizePath(FGUICodeGenSettings.GetFullPath(EditorString.GetPath("UIScriptsPath")));
            foreach (var assetPath in paths)
            {
                var path = FGUICodeGenSettings.GetFullPath(assetPath);
                if (path.EndsWith(".cs") && !path.EndsWith(".gen.cs"))
                {
                    var info = FGUICodeGenSettings.Find(path);
                    if (info != null)
                    {
                        Export(info.package, info.name);
                        continue;
                    }
                }

                if (IsUnderDirectory(path, OutputDir) && path.EndsWith(".cs") && !path.EndsWith(".gen.cs"))
                {
                    var dir = Path.GetDirectoryName(path);
                    var name1 = Path.GetFileNameWithoutExtension(path);
                    var name2 = Path.GetFileNameWithoutExtension(name1);
                    var gen = $"{dir}/{name2}.gen.cs";
                    if (File.Exists(gen) && TryGetPanelInfo($"{dir}/{name2}.cs", "Panel.cs", out var packageName, out var name))
                    {
                        Build(packageName, name);
                    }
                }
                else if (IsUnderDirectory(path, SourceDir) && path.EndsWith("_fui.bytes"))
                {
                    EditorToolSet.LoadPackages();
                    var name1 = Path.GetFileNameWithoutExtension(path);
                    var packageName = name1.Substring(0, name1.Length - "_fui".Length);
                    var package = UIPackage.GetByName(packageName);
                    if (package != null)
                    {
                        foreach (var item in package.GetItems())
                        {
                            var info = FGUICodeGenSettings.Find(packageName, item.name);
                            if (info != null)
                            {
                                Export(packageName, item.name);
                            }
                            else if (ExistsGenFile(packageName, item.name))
                            {
                                Build(packageName, item.name);
                            }
                        }
                    }
                }
            }
        }

        public static bool Exists(string package, string name)
        {
            return TryGetPanelPaths(package, name, out var logicCodePath, out _) && File.Exists(logicCodePath);
        }

        public static bool ExistsGenFile(string package, string name)
        {
            return TryGetPanelPaths(package, name, out _, out var genCodePath) && File.Exists(genCodePath);
        }

        public static void UpdateAll()
        {
            OutputDir = NormalizePath(FGUICodeGenSettings.GetFullPath(EditorString.GetPath("UIScriptsPath")));
            if (!Directory.Exists(OutputDir))
                return;

            foreach (var file in Directory.GetFiles(OutputDir, "*Panel.gen.cs", SearchOption.AllDirectories))
            {
                if (!TryGetPanelInfo(file, "Panel.gen.cs", out var package, out var name))
                    continue;

                if (FGUICodeGenSettings.Find(package, name) != null)
                    Export(package, name);
                else
                    Build(package, name);
            }
        }

        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsUnderDirectory(string path, string directory)
        {
            return !string.IsNullOrEmpty(directory) && path.StartsWith(directory.TrimEnd('/') + "/", FGUICodeGenSettings.PathComparison);
        }

        private static bool TryGetPanelInfo(string path, string suffix, out string package, out string name)
        {
            package = null;
            name = null;
            path = FGUICodeGenSettings.GetFullPath(path);
            if (!IsUnderDirectory(path, OutputDir))
                return false;

            var relativePath = path.Substring(OutputDir.Length + 1);
            var separator = relativePath.IndexOf('/');
            var fileName = Path.GetFileName(path);
            if (separator <= 0 || !fileName.EndsWith(suffix, StringComparison.Ordinal) || fileName.Length <= suffix.Length)
                return false;

            package = relativePath.Substring(0, separator);
            name = fileName.Substring(0, fileName.Length - suffix.Length);
            return true;
        }

        private static bool TryGetPanelPaths(string package, string name, out string logicCodePath, out string genCodePath)
        {
            logicCodePath = null;
            genCodePath = null;
            OutputDir = NormalizePath(FGUICodeGenSettings.GetFullPath(EditorString.GetPath("UIScriptsPath")));
            var packageDir = Path.Combine(OutputDir, package);
            var logicFileName = $"{name}Panel.cs";
            var genFileName = $"{name}Panel.gen.cs";
            string panelDir = null;

            // 已有面板沿用实际目录；新面板才创建组件子目录。
            if (Directory.Exists(packageDir))
            {
                foreach (var file in Directory.GetFiles(packageDir, $"{name}Panel*.cs", SearchOption.AllDirectories))
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName != logicFileName && fileName != genFileName)
                        continue;

                    var directory = NormalizePath(Path.GetDirectoryName(file));
                    if (panelDir != null && !string.Equals(panelDir, directory, FGUICodeGenSettings.PathComparison))
                    {
                        Debug.LogError($"FGUI 面板 {package}/{name} 存在多个代码目录，已跳过生成：{panelDir}、{directory}");
                        return false;
                    }

                    panelDir = directory;
                }
            }

            panelDir = panelDir ?? Path.Combine(packageDir, name);
            logicCodePath = NormalizePath(Path.Combine(panelDir, logicFileName));
            genCodePath = NormalizePath(Path.Combine(panelDir, genFileName));
            return true;
        }

        public static void Build(string package, string name)
        {
            GComponent root = null;
            try
            {
                if (!TryGetPanelPaths(package, name, out var logicCodePath, out var genCodePath))
                    return;

                EditorToolSet.LoadPackages();
                root = UIPackage.CreateObject(package, name) as GComponent;
                if (root == null)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(logicCodePath));

                var item = GenCode(root, logicCodePath);
                string genTemplate = File.ReadAllText(FGUICodeGenSettings.GetFullPath(GenCodeTemplatePath));
                File.WriteAllText(genCodePath, string.Format(genTemplate, package, name, item.Item1, item.Item2));

                if (File.Exists(logicCodePath)) return;
                string loginTemplate = File.ReadAllText(FGUICodeGenSettings.GetFullPath(LogicCodeTemplatePath));
                File.WriteAllText(logicCodePath, string.Format(loginTemplate, package, name));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                root?.Dispose();
            }
        }

        private static (StringBuilder, StringBuilder) GenCode(GComponent root, string logicCodePath)
        {
            var identifiers = new HashSet<string>();
            foreach (var file in SearchPartialFiles(logicCodePath))
            {
                if (file.EndsWith(".gen.cs"))
                    continue;
                LoadIdentifiers(file, identifiers);
            }

            List<string> vars = new List<string>();
            List<string> exps = new List<string>();
            Search(root, "root", "", vars, exps, identifiers);
            StringBuilder sbVar = new StringBuilder();
            vars.Reverse();
            foreach (var v in vars) sbVar.AppendLine(v);
            StringBuilder sbBody = new StringBuilder();
            exps.Reverse();
            foreach (var v in exps) sbBody.AppendLine(v);
            return (sbVar, sbBody);
        }

        private static bool ExportToPath(string path, string package, string name)
        {
            GComponent root = null;
            try
            {
                var logicCodePath = FGUICodeGenSettings.GetFullPath(path);
                var genCodePath = Path.ChangeExtension(logicCodePath, ".gen.cs");
                var fileName = Path.GetFileNameWithoutExtension(logicCodePath);
                if (!CodeProvider.IsValidIdentifier(fileName))
                    throw new ArgumentException($"组件类名无效：{fileName}");

                EditorToolSet.LoadPackages();
                root = UIPackage.CreateObject(package, name) as GComponent;
                if (root == null)
                    throw new InvalidOperationException($"无法加载 FGUI 组件：{package}/{name}");

                Directory.CreateDirectory(Path.GetDirectoryName(logicCodePath));
                var gen = GenCode(root, logicCodePath);
                var genTemplate = File.ReadAllText(FGUICodeGenSettings.GetFullPath(ComponentTemplatePath));
                File.WriteAllText(genCodePath, string.Format(genTemplate, package, name, fileName, gen.Item1, gen.Item2));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"FGUI 组件 {package}/{name} 导出失败，目标路径：{path}\n{e}");
                return false;
            }
            finally
            {
                root?.Dispose();
            }
        }

        public static void Export(string package, string name)
        {
            string path = null;
            try
            {
                // 与生成面板代码共用 name + Panel 命名及已有目录，旧缓存仅记录结果。
                if (!TryGetPanelPaths(package, name, out path, out _))
                    return;

                if (ExportToPath(path, package, name))
                    FGUICodeGenSettings.Save(package, name, path);
            }
            catch (Exception e)
            {
                Debug.LogError($"FGUI 组件 {package}/{name} 导出失败，目标路径：{path}\n{e}");
            }
        }

        private static int Search(GComponent parent, string parentVar, string prefix,
                List<string> vars, List<string> exps, HashSet<string> identifiers)
        {
            if (parent == null) return 0;
            int k = 0;
            int n = parent.numChildren;
            for (int i = 0; i < n; i++)
            {
                var child = parent.GetChildAt(i);
                if (Skip(child, parent)) continue;
                var childClass = child.GetType().Name;
                var childVar = $"{prefix}{NameToVar(child.name)}";
                var used = identifiers.Contains(childVar);
                var t = Search(child as GComponent, childVar, childVar + "_", vars, exps, identifiers);
                if (used || t > 0)
                {
                    k += t + 1;
                    vars.Add($"{VarTab}private {childClass} {childVar};");
                    exps.Add($"{BodyTab}{childVar} = ({childClass}){parentVar}.GetChildAt({i});");
                }
                else
                {
                    vars.Add($"{VarTab}//private {childClass} {childVar};");
                    exps.Add($"{BodyTab}//{childVar} = ({childClass}){parentVar}.GetChildAt({i});");
                }
            }

            return k;
        }

        private static string NameToVar(string name)
        {
            return name.Replace('.', '_');
        }

        private static bool Skip(GObject child, GComponent parent)
        {
            if (!CodeProvider.IsValidIdentifier(child.name))
                return true;
            // skip duplicate-name components
            int n = parent.numChildren;
            for (int i = 0; i < n; i++)
            {
                var child2 = parent.GetChildAt(i);
                if (child == child2) continue;
                if (child.name == child2.name)
                    return true;
            }

            return false;
        }

        private static List<string> SearchPartialFiles(string originFile)
        {
            var list = new List<string>() {originFile};
            var dir = Path.GetDirectoryName(originFile);
            var name = Path.GetFileNameWithoutExtension(originFile);
            foreach (var file in Directory.GetFiles(dir, "*.cs", SearchOption.TopDirectoryOnly))
            {
                var name1 = Path.GetFileNameWithoutExtension(file);
                var name2 = Path.GetFileNameWithoutExtension(name1);
                if (name2 == name)
                    list.Add(file);
            }

            return list;
        }

        private static HashSet<string> LoadIdentifiers(string logicCodePath, HashSet<string> hs = null)
        {
            hs = hs ?? new HashSet<string>();
            if (File.Exists(logicCodePath))
            {
                var word = "";
                foreach (var c in File.ReadAllText(logicCodePath))
                {
                    if (c == '_' || c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' ||
                        word.Length > 0 && c >= '0' && c <= '9')
                    {
                        word += c;
                    }
                    else if (word.Length > 0)
                    {
                        hs.Add(word);
                        word = "";
                    }
                }
            }

            return hs;
        }
    }
}
