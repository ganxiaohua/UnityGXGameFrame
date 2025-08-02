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

        // [MenuItem("GX框架工具/打开图片处理工具")]
        // public static void OpenPictureDispose()
        // {
        //     TextureEditor.TextureEditorWindow.OpenWindow();
        // }


        [MenuItem("GX框架工具/脚本生成/生成所有绑定脚本", false, 2)]
        public static void AutoCreateAllScript()
        {
            AutoCreate.AutoAllScript();
        }
        
        [MenuItem("GX框架工具/脚本生成/生成Capabilitys", false, 2)]
        public static void AutoCreateCapabilitysScript()
        {
            AutoCreate.AutoCapabilityScript();
        }

        [MenuItem("GX框架工具/脚本生成/事件绑定", false, 3)]
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
    }
}