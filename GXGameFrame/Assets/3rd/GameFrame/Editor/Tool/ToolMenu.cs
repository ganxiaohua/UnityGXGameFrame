using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
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

        [MenuItem("GX框架工具/生成ECS绑定脚本", false, 2)]
        public static void AutoCreateScript()
        {
            AutoCreate.AutoCreateScript();
        }

        [MenuItem("GX框架工具/生成System绑定脚本", false, 1)]
        public static void AutoCreateSystemScript()
        {
            AutoCreateSystem.AutoCreateScript();
        }

        [MenuItem("GX框架工具/创建一组实体脚本", false, 0)]
        public static void OpenCreateEnitiyWind()
        {
            CreateEnitiyWind.OpenCreateEnitiyWind();
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
            Debugger.Log("开启这个,并且AA设置成Use Asset Database的话,优先读取本地,没有本地则读取服务器资源.用于项目资源过大,拆分资源");
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (macro.Contains(EditorString.RESSEQ))
            {
                return;
            }

            macro = $"{EditorString.RESSEQ};{macro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macro);
        }

        [MenuItem("GX框架工具/远程资源/关闭", false, 4)]
        public static void CloseRemoteResource()
        {
            string macro = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!macro.Contains(EditorString.RESSEQ))
            {
                return;
            }

            macro = macro.Replace($"{EditorString.RESSEQ};", "");
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
            Application.OpenURL(Application.dataPath + "/../Bin/" + BuildScript.BuildTarget);
        }

        [MenuItem("GX框架工具/路径/打开AB包路径")]
        public static void OpenABPackagePath()
        {
            Application.OpenURL($"{Application.dataPath + "/../"}ServerData/{BuildScript.BuildTarget}");
        }

        // [MenuItem("GX框架工具/测试")]
        // public static void Text()
        // {
        //     // BuildScript.AssetSettings.ContentStateBuildPath
        //     // var x = ProjectConfigData.ActivePlayModeIndex;
        //     // Debugger.Log(BuildScript.AssetSettings.RemoteCatalogLoadPath.GetValue(BuildScript.AssetSettings));
        // }
    }
}