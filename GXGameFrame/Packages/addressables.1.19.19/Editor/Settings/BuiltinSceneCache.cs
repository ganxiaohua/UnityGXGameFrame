using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Settings
{
    internal static class BuiltinSceneCache
    {
        internal static EditorBuildSettingsScene[] m_Scenes;
        static Dictionary<GUID, int> s_GUIDSceneIndexLookup;
        static Dictionary<string, int> s_PathSceneIndexLookup;
        static bool s_IsListening;
        public static event Action sceneListChanged;

        internal static void ClearState(bool clearCallbacks = false)
        {
            InvalidateCache();
            if (s_IsListening)
            {
                EditorBuildSettings.sceneListChanged -= EditorBuildSettings_sceneListChanged;
                s_IsListening = false;
            }
            if (clearCallbacks)
                sceneListChanged = null;
        }

        public static EditorBuildSettingsScene[] scenes
        {
            get
            {
                if (m_Scenes == null)
                {
                    if (!s_IsListening)
                    {
                        s_IsListening = true;
                        EditorBuildSettings.sceneListChanged += EditorBuildSettings_sceneListChanged;
                    }
                    InvalidateCache();
                    m_Scenes = EditorBuildSettings.scenes;
                }
                return m_Scenes;
            }
            set
            {
                EditorBuildSettings.scenes = value;
            }
        }

        public static Dictionary<GUID, int> GUIDSceneIndexLookup
        {
            get
            {
                if (s_GUIDSceneIndexLookup == null)
                {
                    EditorBuildSettingsScene[] localScenes = scenes;
                    s_GUIDSceneIndexLookup = new Dictionary<GUID, int>();
                    int enabledIndex = 0;
                    for (int i = 0; i < scenes.Length; i++)
                    {
                        if (localScenes[i] != null && localScenes[i].enabled)
                            s_GUIDSceneIndexLookup[localScenes[i].guid] = enabledIndex++;
                    }
                }
                return s_GUIDSceneIndexLookup;
            }
        }

        public static Dictionary<string, int> PathSceneIndexLookup
        {
            get
            {
                if (s_PathSceneIndexLookup == null)
                {
                    EditorBuildSettingsScene[] localScenes = scenes;
                    s_PathSceneIndexLookup = new Dictionary<string, int>();
                    int enabledIndex = 0;
                    for (int i = 0; i < scenes.Length; i++)
                    {
                        if (localScenes[i] != null && localScenes[i].enabled)
                            s_PathSceneIndexLookup[localScenes[i].path] = enabledIndex++;
                    }
                }
                return s_PathSceneIndexLookup;
            }
        }

        private static void InvalidateCache()
        {
            m_Scenes = null;
            s_GUIDSceneIndexLookup = null;
            s_PathSceneIndexLookup = null;
        }

        public static int GetSceneIndex(GUID guid)
        {
            int index = -1;
            return GUIDSceneIndexLookup.TryGetValue(guid, out index) ? index : -1;
        }

        public static bool Contains(GUID guid)
        {
            return GUIDSceneIndexLookup.ContainsKey(guid);
        }

        public static bool Contains(string path)
        {
            return PathSceneIndexLookup.ContainsKey(path);
        }

        public static bool GetSceneFromGUID(GUID guid, out EditorBuildSettingsScene outScene)
        {
            int index = GetSceneIndex(guid);
            if (index == -1)
            {
                outScene = null;
                return false;
            }
            outScene = scenes[index];
            return true;
        }

        private static void EditorBuildSettings_sceneListChanged()
        {
            InvalidateCache();
            if (sceneListChanged != null)
                sceneListChanged();
        }
    }
}
