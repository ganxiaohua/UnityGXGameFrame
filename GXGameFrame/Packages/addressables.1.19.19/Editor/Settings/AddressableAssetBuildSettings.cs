using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Settings
{
    using BuildCompression = UnityEngine.BuildCompression;

    /// <summary>
    /// Build settings for addressables.
    /// </summary>
    [Serializable]
    public class AddressableAssetBuildSettings
    {
        /// <summary>
        /// Controls whether to compile scripts when running in virtual mode.  When disabled, build times are faster but the simulated bundle contents may not be accurate due to including editor code.
        /// </summary>
        public bool compileScriptsInVirtualMode
        {
            get { return m_CompileScriptsInVirtualMode; }
            set
            {
                m_CompileScriptsInVirtualMode = value;
                SetDirty();
            }
        }
        [FormerlySerializedAs("m_compileScriptsInVirtualMode")]
        [SerializeField]
        bool m_CompileScriptsInVirtualMode;

        /// <summary>
        /// Controls whether to remove temporary files after each build.  When disabled, build times in packed mode are faster, but may not reflect all changes in assets.
        /// </summary>
        public bool cleanupStreamingAssetsAfterBuilds
        {
            get { return m_CleanupStreamingAssetsAfterBuilds; }
            set
            {
                m_CleanupStreamingAssetsAfterBuilds = value;
                SetDirty();
            }
        }
        [FormerlySerializedAs("m_cleanupStreamingAssetsAfterBuilds")]
        [SerializeField]
        bool m_CleanupStreamingAssetsAfterBuilds = true;

        [FormerlySerializedAs("m_logResourceManagerExceptions")]
        [SerializeField]
        bool m_LogResourceManagerExceptions = true;
        /// <summary>
        /// When enabled, the Addressables.ResourceManager.ExceptionHandler is set to (op, ex) => Debug.LogException(ex);
        /// </summary>
        public bool LogResourceManagerExceptions
        {
            get { return m_LogResourceManagerExceptions; }
            set
            {
                if (m_LogResourceManagerExceptions != value)
                {
                    m_LogResourceManagerExceptions = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// //Specifies where to build asset bundles, this is usually a temporary folder (or a folder in the project).  Bundles are copied out of this location to their final destination.
        /// </summary>
        public string bundleBuildPath
        {
            get { return m_BundleBuildPath; }
            set
            {
                m_BundleBuildPath = value;
                SetDirty();
            }
        }

        [FormerlySerializedAs("m_bundleBuildPath")]
        [SerializeField]
        string m_BundleBuildPath = "Temp/com.unity.addressables/AssetBundles";

        internal void SerializeForHash(BinaryFormatter formatter, Stream stream)
        {
        }

        [NonSerialized]
        AddressableAssetSettings m_Settings;
        void SetDirty()
        {
            if (m_Settings != null)
                m_Settings.SetDirty(AddressableAssetSettings.ModificationEvent.BuildSettingsChanged, this, true, false);
        }

        internal void OnAfterDeserialize(AddressableAssetSettings settings)
        {
            m_Settings = settings;
        }

        internal void Validate(AddressableAssetSettings addressableAssetSettings)
        {
        }
    }
}
