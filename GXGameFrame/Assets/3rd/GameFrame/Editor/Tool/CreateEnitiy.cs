using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using DG.Tweening.Plugins.Core.PathCore;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace UnityEditor
{
    public class CreateEnitiyWind : OdinEditorWindow
    {
        [MenuItem("Tool/创建一个组件脚本")]
        public static void OpenCreateEnitiyWind()
        {
            CreateEnitiyWind.OpenWindow();
        }

        public static string Path = "Assets/3rd/GameFrame/Editor/Text/EnitiyCreate/EnitiyCreate.txt";

        public static void OpenWindow()
        {
            var window = GetWindow<CreateEnitiyWind>();

            window.maxSize = new Vector2(500, 150);
            window.minSize = new Vector2(500, 150);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(600, 600);
        }

        [Title("自定义路径选择")] [HideLabel] [FolderPath(ParentFolder = "Assets/", AbsolutePath = true)]
        public string CreatePath;

        [LabelText("组件名字")] [ShowInInspector] private string ComponentName;

        [LabelText("想要继承的接口")] [EnumToggleButtons]
        public InheritedObject InheritedObjectEnum;

        [System.Flags]
        public enum InheritedObject
        {
            IStart = 1 << 1,
            IShow = 1 << 2,
            IHide = 1 << 3,
            IUpdate = 1 << 4,
            IClear = 1 << 5,
        }

        [Button]
        public void Create()
        {
            if (string.IsNullOrEmpty(CreatePath))
            {
                return;
            }

            string path = $"{CreatePath}/{ComponentName}.cs";
            if (File.Exists(path))
            {
                if (EditorUtility.DisplayDialog("警告！", ComponentName + "已经存在是否覆盖", "覆盖", "取消"))
                {
                    WriteEnitit(InheritedObjectEnum, ComponentName, false, CreatePath);
                }
            }
            else
            {
                WriteEnitit(InheritedObjectEnum, ComponentName, false, CreatePath);
            }
        }

        public static void WriteEnitit(InheritedObject inheritedobject, string componentName, bool isUI, string createPath)
        {
            var txt = File.ReadAllText(Path);
            string[] text = txt.Split('#', StringSplitOptions.None);
            string scriptComponentInherit = "";
            string startSystem = "";
            string startSystemContent = "";
            string showSystem = "";
            string showSystemContent = "";
            string hideSystem = "";
            string hideSystemContent = "";
            string updateSystem = "";
            string updateSystemContent = "";
            string clearSystem = "";
            string clearSystemContent = "";


            if ((inheritedobject & InheritedObject.IStart) == InheritedObject.IStart)
            {
                startSystem = string.Format(text[2], componentName, startSystemContent);
                scriptComponentInherit += "IStart,";
            }

            if ((inheritedobject & InheritedObject.IShow) == InheritedObject.IShow)
            {
                showSystem = string.Format(text[3], componentName, showSystemContent);
                scriptComponentInherit += "IShow,";
            }

            if ((inheritedobject & InheritedObject.IHide) == InheritedObject.IHide)
            {
                hideSystem = string.Format(text[4], componentName, hideSystemContent);
                scriptComponentInherit += "IHide,";
            }

            if ((inheritedobject & InheritedObject.IUpdate) == InheritedObject.IUpdate)
            {
                updateSystem = string.Format(text[5], componentName, updateSystemContent);
                scriptComponentInherit += "IUpdate,";
            }

            if ((inheritedobject & InheritedObject.IClear) == InheritedObject.IClear)
            {
                clearSystem = string.Format(text[6], componentName, clearSystemContent);
                scriptComponentInherit += "IClear,";
            }

            string scriptComponent = string.Format(text[0], componentName, scriptComponentInherit.Substring(0, scriptComponentInherit.Length - 1));
            var scriptComponentSystem = string.Format(text[1], componentName, startSystem + showSystem + hideSystem + updateSystem + clearSystem);
            File.WriteAllText($"{createPath}/{componentName}.cs", scriptComponent);
            File.WriteAllText($"{createPath}/{componentName}System.cs", scriptComponentSystem);
            AssetDatabase.Refresh();
            Debug.Log("创建成功");
        }
    }
}