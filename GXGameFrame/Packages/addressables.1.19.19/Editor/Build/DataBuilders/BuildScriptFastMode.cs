using System;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    /// <summary>
    /// Only saves the guid of the settings asset to PlayerPrefs.  All catalog data is generated directly from the settings as needed.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(BuildScriptFastMode) + ".asset", menuName = "Addressables/Content Builders/Use Asset Database (fastest)")]
    public class BuildScriptFastMode : BuildScriptBase
    {
        /// <inheritdoc />
        public override string Name
        {
            get
            {
                return "Use Asset Database (fastest)";
            }
        }

        private bool m_DataBuilt;

        /// <inheritdoc />
        public override void ClearCachedData()
        {
            m_DataBuilt = false;
        }

        /// <inheritdoc />
        public override bool IsDataBuilt()
        {
            return m_DataBuilt;
        }

        /// <inheritdoc />
        protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public override bool CanBuildData<T>()
        {
            return typeof(T).IsAssignableFrom(typeof(AddressablesPlayModeBuildResult));
        }

        /// <inheritdoc />
        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(builderInput.AddressableSettings, out var guid, out long _))
            {
                IDataBuilderResult res = new AddressablesPlayModeBuildResult() { Error = "Invalid Settings asset." };
                return (TResult)res;
            }
            else
            {
                PlayerPrefs.SetString(Addressables.kAddressablesRuntimeDataPath, $"GUID:{guid}");
                IDataBuilderResult res = new AddressablesPlayModeBuildResult() { OutputPath = "", Duration = 0 };
                m_DataBuilt = true;
                return (TResult)res;
            }
        }
    }
}
