using System.Collections.Generic;
using GameFrame.Runtime;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class GamePathData : SerializedScriptableObject
    {
        [Title("路径数据", TitleAlignment = TitleAlignments.Centered)]
        [BoxGroup]
        [DictionaryDrawerSettings(KeyLabel = "key", ValueLabel = "value", DisplayMode = DictionaryDisplayOptions.OneLine)]
        public Dictionary<string, string> PathData = new Dictionary<string, string>();

        [Title("路径数据", TitleAlignment = TitleAlignments.Centered)]
        [BoxGroup]
        [DictionaryDrawerSettings(KeyLabel = "key", ValueLabel = "value", DisplayMode = DictionaryDisplayOptions.OneLine)]
        public Dictionary<string, string[]> PathArrayData = new Dictionary<string, string[]>();

        [MenuItem("GX框架工具/脚本生成/编辑器路径配置文件")]
        public static void CreateGamePathData()
        {
            string[] guids = AssetDatabase.FindAssets("t:GamePathData");
            if (guids != null && guids.Length != 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Debugger.LogError($"GamePathData already exists {assetPath}");
                return;
            }

            var gamePathData = SerializedScriptableObject.CreateInstance<GamePathData>();
            string savePath = "Assets/GamePathData.asset";
            AssetDatabase.CreateAsset(gamePathData, savePath);
            Debugger.Log($"已经放入{savePath},请将其转移到你自己的Editor文件夹下");
        }
    }
}