using System;
using System.Collections.Generic;
using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public static class EditorString
    {
        private static GamePathData gamePathData;

        public static string GetPath(string key)
        {
            var pathDatas = Get();
            if (pathDatas == null)
                return null;
            if (pathDatas.PathData.TryGetValue(key, out var data))
            {
                return data;
            }

            throw new Exception($"gamePathData PathData not have {key}");
        }

        public static string[] GetPaths(string key)
        {
            var pathDatas = Get();
            if (pathDatas.PathArrayData.TryGetValue(key, out var data))
            {
                return data;
            }

            throw new Exception($"gamePathData PathArrayData not have {key}");
        }

        private static GamePathData Get()
        {
            if (gamePathData != null)
                return gamePathData;
            string[] guids = AssetDatabase.FindAssets("t:GamePathData");
            if (guids == null || guids.Length == 0)
            {
                throw new Exception("Please create it first GX框架工具/脚本生成/编辑器路径配置文件");
            }

            gamePathData = AssetDatabase.LoadAssetAtPath<GamePathData>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return gamePathData;
        }
    }
}