﻿using UnityEngine;

namespace GameFrame.Editor
{
    public static class EditorString
    {
        /// <summary>
        /// 主场景路径
        /// </summary>
        public static string FirstScenePath = "Assets/GXGame/Scenes/Main.unity";

        
        /// <summary>
        /// UI资源路径
        /// </summary>
        public static string UIAssetPath = "Assets/GXGame/UI";
        
        /// <summary>
        /// UI创建的脚本路径
        /// </summary>
        public static string UIScriptsPath = "Assets/GXGame/Scripts/UI";
        
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
        public static string ECSOutPutPath = "Assets/GXGame/Scripts/Auto/EcsBind/";
        
        // ---------------------------------------------------------以下路径最好不要改
        
        public static string GameFramePath = "Assets/3rd/GameFrame/";
        
        public static string SystemBindOutPutPath = "Assets/3rd/GameFrame/Runtime/Core/Auto/SystemBindAuto.cs";

        public static string EditorDatabasePath = "Assets/3rd/GameFrame/Editor/Rec/bansheegz_database.bytes";
        
        public static string UIScenePath = "Assets/3rd/GameFrame/CreateUIEditor/CreateUIScene.unity";
        
        public static string RESSEQ = "RESSEQ";
    }
}