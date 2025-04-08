﻿using GameFrame.Editor.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameFrame.Editor
{
    public class ToolMenu
    {
        [MenuItem("GX框架工具/打开实体审查")]
        public static void OpenDialogueGraphWindow()
        {
            DialogueGraphWindow.OpenDialogueGraphWindow();
        }

        [MenuItem("GX框架工具/打开图片处理工具")]
        public static void OpenPictureDispose()
        {
            TextureEditor.TextureEditorWindow.OpenWindow();
        }


        [MenuItem("GX框架工具/生成ECS绑定脚本", false, 2)]
        public static void AutoCreateScript()
        {
            AutoCreate.AutoCreateScript();
        }

        [MenuItem("GX框架工具/事件绑定", false, 3)]
        public static void AutoEventBind()
        {
            MakeAutoEventBind.AutoCreateScript();
        }


        [MenuItem("GX框架工具/UI/UI创建", false, 3)]
        public static void CreateUI()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorString.UIScenePath);
        }


        [MenuItem("GX框架工具/UI/FGUI检查")]
        public static void CheckUI()
        {
            FGUIChecker.OpenWindow();
        }
        
        
        [MenuItem("GX框架工具/远程资源/开启", true)]
        private static bool OpenRemoteResourceBindValidate()
        {
            string macro =PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            Menu.SetChecked("GX框架工具/远程资源/开启", macro.Contains(EditorString.RESSEQ));
            return true;
        }
        
        

        [MenuItem("GX框架工具/远程资源/开启", false, 4)]
        public static void OpenRemoteResource()
        {
            Debugger.Log("开启这个,并且AA设置成Use Asset Database的话,优先读取本地,没有本地则读取服务器资源.用于项目资源过大,拆分资源");
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (macro.Contains(EditorString.RESSEQ))
            {
                macro = macro.Replace($"{EditorString.RESSEQ};", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
                return;
            }

            macro = $"{EditorString.RESSEQ};{macro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }
        

        [MenuItem("GX框架工具/路径/Open PersistentDataPath")]
        public static void OpenPersistentDataPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        [MenuItem("GX框架工具/路径/Open Cache Path")]
        public static void OpenCachePath()
        {
            Application.OpenURL(Caching.currentCacheForWriting.path);
        }

        [MenuItem("GX框架工具/路径/打开包体路径")]
        public static void OpenPackagePath()
        {
            // Application.OpenURL(Application.dataPath + "/../Bin/" + BuildScript.BuildTarget);
        }

        [MenuItem("GX框架工具/路径/打开AB包路径")]
        public static void OpenABPackagePath()
        {
            // Application.OpenURL($"{Application.dataPath + "/../"}ServerData/{BuildScript.BuildTarget}");
        }

        [MenuItem("GX框架工具/断言/开启" , true)]
        private static bool OpenAssertBindValidate()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            Menu.SetChecked("GX框架工具/断言/开启", macro.Contains(EditorString.SHOWASSERT));
            return true;
        }
        

        [MenuItem("GX框架工具/断言/开启", false, 7)]
        public static void OpenAssertBind()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (macro.Contains(EditorString.SHOWASSERT))
            {
                macro = macro.Replace(EditorString.SHOWASSERT, "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
                return;
            }

            macro = $"{EditorString.SHOWASSERT};{macro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }
    }
}