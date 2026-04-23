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


        [MenuItem("GX框架工具/脚本生成/生成组件绑定脚本", false, 2)]
        public static void AutoCreateAllScript()
        {
            AutoCreate.AutoBindingComponentsScript();
        }

        [MenuItem("GX框架工具/脚本生成/生成Capabilitys绑定脚本", false, 2)]
        public static void AutoCreateCapabilitysScript()
        {
            AutoCreate.AutoBindingCapabilityScript();
        }

        [MenuItem("GX框架工具/脚本生成/生成事件绑定脚本", false, 3)]
        public static void AutoEventBind()
        {
            MakeAutoEventBind.AutoBindingEventScript();
        }

        [MenuItem("GX框架工具/UI/UI创建", false, 3)]
        public static void CreateUI()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorString.GetPath("UIScenePath"));
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