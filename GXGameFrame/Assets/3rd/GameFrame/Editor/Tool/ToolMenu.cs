using UnityEditor;
using UnityEditor.SceneManagement;

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
    }
}