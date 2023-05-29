﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace GameFrame.Editor
{
    [CreateAssetMenu(fileName = "AssetGroupSettings", menuName = "Build/AssetGroupSettings", order = 1)]

    public class AssetGroupSettings : ScriptableObject
    {
        [ListDrawerSettings(NumberOfItemsPerPage = 4)]
        public List<AssetGroup> data = new List<AssetGroup>();

        static AssetGroupSettings s_Settings;
        public static AssetGroupSettings settings
        {
            get
            {
                if (s_Settings == null)
                {
                    s_Settings = AssetDatabase.LoadAssetAtPath<AssetGroupSettings>("Assets/GXGame/AssetGroupSettings.asset");
                }
                return s_Settings;
            }
        }

        [Button("应用", ButtonSizes.Large)]
        private void Apply()
        {
            BuildScript.ProcessAllAssetGroup();
        }
    }
}
