using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FairyGUI;
using FairyGUIEditor;
using Microsoft.CSharp;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public sealed class FGUIPanelCodeGen : AssetPostprocessor
    {
        private static string SourceDir = "";
        private static string OutputDir = "";

        private const string LogicCodeTemplatePath = "Assets/3rd/UnityGXGameFrame/Editor/Text/UI/Panel.txt";
        private const string GenCodeTemplatePath = "Assets/3rd/UnityGXGameFrame/Editor/Text/UI/Panel.Gen.txt";
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
            SourceDir = EditorString.GetPath("FguiConfigAssetPath");
            OutputDir = EditorString.GetPath("UIScriptsPath");
            foreach (var path in paths)
            {
                if (path.StartsWith(OutputDir) && path.EndsWith(".cs") && !path.EndsWith(".gen.cs"))
                {
                    var dir = Path.GetDirectoryName(path);
                    var name1 = Path.GetFileNameWithoutExtension(path);
                    var name2 = Path.GetFileNameWithoutExtension(name1);
                    var gen = $"{dir}/{name2}.gen.cs";
                    if (File.Exists(gen) && name2.EndsWith("Panel"))
                    {
                        var packageName = Path.GetFileName(dir);
                        var name = name2.Substring(0, name2.Length - "Panel".Length);
                        Build(packageName, name);
                    }

                    var info = FGUICodeGenSettings.Find(path);
                    if (info != null)
                    {
                        ExportToPath(info.codePath, info.package, info.name);
                    }
                }
                else if (path.StartsWith(SourceDir) && path.EndsWith("_fui.bytes"))
                {
                    EditorToolSet.LoadPackages();
                    var name1 = Path.GetFileNameWithoutExtension(path);
                    var packageName = name1.Substring(0, name1.Length - "_fui".Length);
                    var package = UIPackage.GetByName(packageName);
                    if (package != null)
                    {
                        foreach (var item in package.GetItems())
                        {
                            if (ExistsGenFile(packageName, item.name))
                            {
                                Build(packageName, item.name);
                            }
                            else
                            {
                                var info = FGUICodeGenSettings.Find(packageName, item.name);
                                if (info != null)
                                {
                                    ExportToPath(info.codePath, packageName, item.name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool Exists(string package, string name)
        {
            OutputDir = EditorString.GetPath("UIScriptsPath");
            var path = $"{OutputDir}/{package}/{name}Panel.cs";
            return File.Exists(path);
        }

        public static bool ExistsGenFile(string package, string name)
        {
            OutputDir = EditorString.GetPath("UIScriptsPath");
            var path = $"{OutputDir}/{package}/{name}Panel.gen.cs";
            return File.Exists(path);
        }

        public static void UpdateAll()
        {
            OutputDir = EditorString.GetPath("UIScriptsPath");
            foreach (var file in Directory.GetFiles(OutputDir, "*.gen.cs", SearchOption.AllDirectories))
            {
                var package = Path.GetDirectoryName(file)?.Replace('\\', '/').Replace(OutputDir + "/", "");
                var name = Path.GetFileName(file).Replace("Panel.gen.cs", "");
                Build(package, name);
            }
        }

        public static void Build(string package, string name)
        {
            OutputDir = EditorString.GetPath("UIScriptsPath");
            GComponent root = null;
            try
            {
                EditorToolSet.LoadPackages();
                root = UIPackage.CreateObject(package, name) as GComponent;
                if (root == null)
                    return;

                var genCodePath = $"{OutputDir}/{package}/{name}Panel.gen.cs";
                var logicCodePath = $"{OutputDir}/{package}/{name}Panel.cs";

                OpFile.CreateDirectory($"{OutputDir}/{package}");

                var item = GenCode(root, logicCodePath);
                string genTemplate = File.ReadAllText(GenCodeTemplatePath);
                File.WriteAllText(genCodePath, string.Format(genTemplate, package, name, item.Item1, item.Item2));

                if (File.Exists(logicCodePath)) return;
                string loginTemplate = File.ReadAllText(LogicCodeTemplatePath);
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

        private static void ExportToPath(string path, string package, string name)
        {
            if (string.IsNullOrEmpty(path))
                throw new InvalidPathException("路径不能为空");
            var fileName = Path.GetFileNameWithoutExtension(path).Replace(".gen", string.Empty);
            GComponent root = null;
            try
            {
                EditorToolSet.LoadPackages();
                root = UIPackage.CreateObject(package, name) as GComponent;
                if (root == null)
                    return;

                var dir = Path.GetDirectoryName(path);
                var genCodePath = $"{dir}/{fileName}.gen.cs";
                var logicCodePath = genCodePath.Replace(".gen", string.Empty);
                OpFile.CreateDirectory(dir);
                var gen = GenCode(root, logicCodePath);
                var genTemplate = File.ReadAllText(ComponentTemplatePath);
                File.WriteAllText(genCodePath, string.Format(genTemplate, package, name, fileName, gen.Item1, gen.Item2));
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

        public static void Export(string package, string name)
        {
            var info = FGUICodeGenSettings.Find(package, name);
            if (info != null)
            {
                ExportToPath(info.codePath, package, name);
                return;
            }

            var path = EditorUtility.SaveFilePanel("Export", "", name, "");
            ExportToPath(path, package, name);
            path = path.Replace(Environment.CurrentDirectory.Replace("\\", "/") + "/", string.Empty);
            path = path.Replace(".gen", string.Empty);
            FGUICodeGenSettings.Save(package, name, path);
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