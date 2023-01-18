using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Settings
{
    /// <summary>
    /// The project configuration settings for addressables.
    /// </summary>
    public class ProjectConfigData
    {
        [Serializable]
        class ConfigSaveData
        {
            [FormerlySerializedAs("m_postProfilerEvents")]
            [SerializeField]
            internal bool postProfilerEventsInternal;
            [FormerlySerializedAs("m_localLoadSpeed")]
            [SerializeField]
            internal long localLoadSpeedInternal = 1024 * 1024 * 10;
            [FormerlySerializedAs("m_remoteLoadSpeed")]
            [SerializeField]
            internal long remoteLoadSpeedInternal = 1024 * 1024 * 1;
            [FormerlySerializedAs("m_hierarchicalSearch")]
            [SerializeField]
            internal bool hierarchicalSearchInternal = true;
            [SerializeField]
            internal int activePlayModeIndex = 0;
            [SerializeField]
            internal bool hideSubObjectsInGroupView = false;
            [SerializeField]
            internal bool showGroupsAsHierarchy = false;
            [SerializeField]
            internal bool generateBuildLayout = false;
        }

        static ConfigSaveData s_Data;

        /// <summary>
        /// Whether to display sub objects in the Addressables Groups window. 
        /// </summary>
        public static bool ShowSubObjectsInGroupView
        {
            get
            {
                ValidateData();
                return !s_Data.hideSubObjectsInGroupView;
            }
            set
            {
                ValidateData();
                s_Data.hideSubObjectsInGroupView = !value;
                SaveData();
            }
        }

        /// <summary>
        /// Whether to generate the bundle build layout report.
        /// </summary>
        public static bool GenerateBuildLayout
        {
            get
            {
                ValidateData();
                return s_Data.generateBuildLayout;
            }
            set
            {
                ValidateData();
                if (s_Data.generateBuildLayout != value)
                {
                    s_Data.generateBuildLayout = value;
                    SaveData();
                }
            }
        }

        /// <summary>
        /// The active play mode data builder index.
        /// </summary>
        public static int ActivePlayModeIndex
        {
            get
            {
                ValidateData();
                return s_Data.activePlayModeIndex;
            }
            set
            {
                ValidateData();
                s_Data.activePlayModeIndex = value;
                SaveData();
            }
        }

        /// <summary>
        /// Whether to post profiler events in the ResourceManager profiler window.
        /// </summary>
        public static bool PostProfilerEvents
        {
            get
            {
                ValidateData();
                return s_Data.postProfilerEventsInternal;
            }
            set
            {
                ValidateData();
                s_Data.postProfilerEventsInternal = value;
                SaveData();
            }
        }

        /// <summary>
        /// The local bundle loading speed used in the Simulate Groups (advanced) playmode.
        /// </summary>
        public static long LocalLoadSpeed
        {
            get
            {
                ValidateData();
                return s_Data.localLoadSpeedInternal;
            }
            set
            {
                ValidateData();
                s_Data.localLoadSpeedInternal = value;
                SaveData();
            }
        }
        
        /// <summary>
        /// The remote bundle loading speed used in the Simulate Groups (advanced) playmode.
        /// </summary>
        public static long RemoteLoadSpeed
        {
            get
            {
                ValidateData();
                return s_Data.remoteLoadSpeedInternal;
            }
            set
            {
                ValidateData();
                s_Data.remoteLoadSpeedInternal = value;
                SaveData();
            }
        }

        /// <summary>
        /// Whether to allow searching for assets parsed hierarchally in the Addressables Groups window.  
        /// </summary>
        public static bool HierarchicalSearch
        {
            get
            {
                ValidateData();
                return s_Data.hierarchicalSearchInternal;
            }
            set
            {
                ValidateData();
                s_Data.hierarchicalSearchInternal = value;
                SaveData();
            }
        }

        /// <summary>
        /// Whether to display groups names parsed hierarchally in the Addressables Groups window. 
        /// </summary>
        public static bool ShowGroupsAsHierarchy
        {
            get
            {
                ValidateData();
                return s_Data.showGroupsAsHierarchy;
            }
            set
            {
                ValidateData();
                s_Data.showGroupsAsHierarchy = value;
                SaveData();
            }
        }

        internal static void SerializeForHash(Stream stream)
        {
            ValidateData();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, s_Data);
        }

        static void ValidateData()
        {
            if (s_Data == null)
            {
                var dataPath = Path.GetFullPath(".");
                dataPath = dataPath.Replace("\\", "/");
                dataPath += "/Library/AddressablesConfig.dat";

                if (File.Exists(dataPath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    try
                    {
                        using (FileStream file = new FileStream(dataPath, FileMode.Open, FileAccess.Read))
                        {
                            var data = bf.Deserialize(file) as ConfigSaveData;
                            if (data != null)
                            {
                                s_Data = data;
                            }
                        }
                    }
                    catch
                    {
                        //if the current class doesn't match what's in the file, Deserialize will throw. since this data is non-critical, we just wipe it
                        Addressables.LogWarning("Error reading Addressable Asset project config (play mode, etc.). Resetting to default.");
                        File.Delete(dataPath);
                    }
                }

                //check if some step failed.
                if (s_Data == null)
                {
                    s_Data = new ConfigSaveData();
                }
            }
        }

        static void SaveData()
        {
            if (s_Data == null)
                return;

            var dataPath = Path.GetFullPath(".");
            dataPath = dataPath.Replace("\\", "/");
            dataPath += "/Library/AddressablesConfig.dat";

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataPath);

            bf.Serialize(file, s_Data);
            file.Close();
        }
    }
}
