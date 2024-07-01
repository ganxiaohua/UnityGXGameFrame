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


        [MenuItem("GX框架工具/UI创建", false, 3)]
        public static void CreateUI()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorString.UIScenePath);
        }


        [MenuItem("GX框架工具/远程资源/开启", false, 4)]
        public static void OpenRemoteResource()
        {
            Menu.SetChecked("GX框架工具/远程资源/开启", true);
            Menu.SetChecked("GX框架工具/远程资源/关闭", false);
            Debugger.Log("开启这个,并且AA设置成Use Asset Database的话,优先读取本地,没有本地则读取服务器资源.用于项目资源过大,拆分资源");
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (macro.Contains(EditorString.RESSEQ))
            {
                return;
            }

            macro = $"{EditorString.RESSEQ};{macro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }

        [MenuItem("GX框架工具/远程资源/关闭", false, 5)]
        public static void CloseRemoteResource()
        {
            Menu.SetChecked("GX框架工具/远程资源/开启", false);
            Menu.SetChecked("GX框架工具/远程资源/关闭", true);
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!macro.Contains(EditorString.RESSEQ))
            {
                return;
            }

            macro = macro.Replace($"{EditorString.RESSEQ};", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }
        
        // [MenuItem("GX框架工具/SystemBind/开启", false, 5)]
        // public static void OpenSystemBind()
        // {
        //     string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        //     if (macro.Contains("SYSTEMBIND"))
        //     {
        //         return;
        //     }
        //
        //     macro = $"SYSTEMBIND;{macro}";
        //     PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        // }
        //
        // [MenuItem("GX框架工具/SystemBind/关闭", false, 6)]
        // public static void CloseSystemBind()
        // {
        //     
        //     string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        //     if (!macro.Contains("SYSTEMBIND"))
        //     {
        //         return;
        //     }
        //
        //     macro = macro.Replace($"SYSTEMBIND", "");
        //     PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        // }
        
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
            Application.OpenURL(Application.dataPath + "/../Bin/" + BuildScript.BuildTarget);
        }

        [MenuItem("GX框架工具/路径/打开AB包路径")]
        public static void OpenABPackagePath()
        {
            Application.OpenURL($"{Application.dataPath + "/../"}ServerData/{BuildScript.BuildTarget}");
        }
        
        [MenuItem("GX框架工具/断言/开启", false, 7)]
        public static void OpenAssertBind()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            Menu.SetChecked("GX框架工具/断言/开启", true);
            Menu.SetChecked("GX框架工具/断言/关闭", false);
            if (macro.Contains(EditorString.SHOWASSERT))
            {
                return;
            }
            macro = $"{EditorString.SHOWASSERT};{macro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }
        
        [MenuItem("GX框架工具/断言/关闭", false, 8)]
        public static void CloseAssertBind()
        {
            Menu.SetChecked("GX框架工具/断言/开启", false);
            Menu.SetChecked("GX框架工具/断言/关闭", true);
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!macro.Contains(EditorString.SHOWASSERT))
            {
                return;
            }
            
            macro = macro.Replace(EditorString.SHOWASSERT, "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }

    }
}