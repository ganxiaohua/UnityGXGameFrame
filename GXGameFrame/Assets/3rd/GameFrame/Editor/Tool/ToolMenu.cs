using UnityEditor;
using UnityEditor.SceneManagement;

namespace GameFrame.Editor
{
    public class ToolMenu
    {
        [MenuItem("Tool/打开实体审查")]
        public static void OpenDialogueGraphWindow()
        {
            DialogueGraphWindow.OpenDialogueGraphWindow();
        }

        [MenuItem("Tool/生成ECS绑定脚本", false, 2)]
        public static void AutoCreateScript()
        {
            AutoCreate.AutoCreateScript();
        }

        [MenuItem("Tool/生成System绑定脚本", false, 1)]
        public static void AutoCreateSystemScript()
        {
            AutoCreateSystem.AutoCreateScript();
        }

        [MenuItem("Tool/创建一组实体脚本", false, 0)]
        public static void OpenCreateEnitiyWind()
        {
            CreateEnitiyWind.OpenCreateEnitiyWind();
        }
        
        [MenuItem("Tool/UI创建", false, 3)]
        public static void CreateUI()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.OpenScene(EditorString.UIScenePath);
        }
    }
}