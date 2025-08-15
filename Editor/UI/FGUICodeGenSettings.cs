using System;
using System.Collections.Generic;
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

        public static CodeGenInfo Find(string codePath)
        {
            foreach (var item in Instance.infos)
            {
                if (item.codePath == codePath)
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
            var info = Find(codePath);
            if (info != null)
                return;
            info = new CodeGenInfo()
            {
                    package = package,
                    name = name,
                    codePath = codePath,
            };
            Instance.infos.Add(info);
            var json = JsonUtility.ToJson(Instance, true);
            EditorPrefs.SetString(EditorPrefsKey.FGUICodeGenSettings, json);
        }
    }
}