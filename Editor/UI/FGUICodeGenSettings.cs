using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    [Serializable]
    public class FGUICodeGenSettings
    {
        [Serializable]
        public class CodeGenInfo
        {
            public string package;
            public string name;
            public string codePath;
        }


        private static FGUICodeGenSettings instance;

        public static FGUICodeGenSettings Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                string cont = EditorPrefs.GetString(EditorPrefsKey.FGUICodeGenSettings);
                instance = !string.IsNullOrEmpty(cont) ? JsonUtility.FromJson<FGUICodeGenSettings>(cont) : new FGUICodeGenSettings();
                return instance;
            }
        }

        public List<CodeGenInfo> infos = new();

        internal static StringComparison PathComparison => Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        internal static string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("路径不能为空", nameof(path));

            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", path.Replace('\\', '/'))).Replace('\\', '/');
        }

        internal static string NormalizeCodePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                path = GetFullPath(path);
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    path += ".cs";
                if (path.EndsWith(".gen.cs", StringComparison.OrdinalIgnoreCase))
                    path = path.Substring(0, path.Length - ".gen.cs".Length) + ".cs";

                var projectDir = GetFullPath(".").TrimEnd('/') + "/";
                return path.StartsWith(projectDir, PathComparison) ? path.Substring(projectDir.Length) : path;
            }
            catch (ArgumentException) { return null; }
            catch (NotSupportedException) { return null; }
            catch (PathTooLongException) { return null; }
        }

        public static CodeGenInfo Find(string codePath)
        {
            codePath = NormalizeCodePath(codePath);
            if (codePath == null)
                return null;

            foreach (var item in Instance.infos)
            {
                if (string.Equals(NormalizeCodePath(item.codePath), codePath, PathComparison))
                    return item;
            }

            return null;
        }

        public static CodeGenInfo Find(string package, string name)
        {
            foreach (var item in Instance.infos)
            {
                if (item.package == package && item.name == name)
                    return item;
            }

            return null;
        }

        public static void Save(string package, string name, string codePath)
        {
            codePath = NormalizeCodePath(codePath);
            if (codePath == null)
                throw new ArgumentException("组件代码路径无效", nameof(codePath));

            var info = Find(package, name);
            if (info == null)
            {
                info = new CodeGenInfo
                {
                    package = package,
                    name = name,
                };
                Instance.infos.Add(info);
            }

            info.codePath = codePath;
            Instance.infos.RemoveAll(item => item != info && item.package == package && item.name == name);
            var json = JsonUtility.ToJson(Instance, true);
            EditorPrefs.SetString(EditorPrefsKey.FGUICodeGenSettings, json);
        }
    }
}
