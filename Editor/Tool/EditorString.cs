using UnityEngine;

namespace GameFrame.Editor
{
    public static class EditorString
    {
        /// <summary>
        /// 主场景路径
        /// </summary>
        public static string FirstScenePath = "Assets/GXGame/Scenes/Main.unity";

        /// <summary>
        /// fgui文件目录
        /// </summary>
        public static string FguiConfigAssetPath = "../../Fgui/asset";

        /// <summary>
        /// UI创建的脚本路径
        /// </summary>
        public static string UIScriptsPath = "Assets/GXGame/Scripts/Runtime/UI";

        /// <summary>
        /// 外部配表数据
        /// </summary>
        public static string ConfigFullPath = $"{Application.dataPath}/GXGame/Config/ConfigData";

        /// <summary>
        /// 配表数据
        /// </summary>
        public static string ConfigAssetPath = "Assets/GXGame/Config/ConfigData";


        /// <summary>
        /// ecs绑定路径
        /// </summary>
        public static string ECSOutPutPath = "Assets/GXGame/Scripts/Runtime/Auto/EcsBind/";

        public static string EventBindOutPutPath = "Assets/GXGame/Scripts/Runtime/Event/EventBind.cs";

        public static string EventSendOutPutPath = "Assets/GXGame/Scripts/Runtime/Event/EventSend.cs";

        // ---------------------------------------------------------以下路径最好不要改

        public static string GameFramePath = "Assets/3rd/UnityGXGameFrame/";

        // public static string SystemBindOutPutPath = "Assets/GXGame/Scripts/Runtime/Auto/SystemBindAuto.cs";

        public static string UIScenePath = "Assets/3rd/UnityGXGameFrame/Editor/Tool/Fgui/CreateUIScene.unity";

        public static string RESSEQ = "RESSEQ";

        public static string SHOWASSERT = "SHOWASSERT";

        public static string[] AssemblyNames = new string[] {"GamePlay.Runtime", "GameFrame.Runtime"};
    }
}