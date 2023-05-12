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
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class CreateEnitiyWind : OdinEditorWindow
    {
        public static void OpenCreateEnitiyWind()
        {
            CreateEnitiyWind.OpenWindow();
        }


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
        public CreateEnitiyAuto.InheritedObject InheritedObjectEnum;


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
                    CreateEnitiyAuto.WriteEnitit(InheritedObjectEnum, ComponentName, false, CreatePath);
                }
            }
            else
            {
                CreateEnitiyAuto.WriteEnitit(InheritedObjectEnum, ComponentName, false, CreatePath);
            }

            AssetDatabase.Refresh();
            Debug.Log("创建成功");
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void SystemBind()
        {
            // AutoCreateSystem.AutoCreateScript();
            // EditorBuildDatabase.SetBuildbaseData(false);
        }
    }
}