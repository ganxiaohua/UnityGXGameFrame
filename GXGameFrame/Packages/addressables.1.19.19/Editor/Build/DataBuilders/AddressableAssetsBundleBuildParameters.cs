using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEditor.Build.Content;
using BuildCompression = UnityEngine.BuildCompression;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    /// <summary>
    /// Custom bundle parameter container that provides custom compression settings per bundle.
    /// </summary>
    public class AddressableAssetsBundleBuildParameters : BundleBuildParameters
    {
        Dictionary<string, string> m_bundleToAssetGroup;
        AddressableAssetSettings m_settings;
        /// <summary>
        /// Create a AddressableAssetsBundleBuildParameters with data needed to determine the correct compression per bundle.
        /// </summary>
        /// <param name="aaSettings">The AddressableAssetSettings object to use for retrieving groups.</param>
        /// <param name="bundleToAssetGroup">Mapping of bundle identifier to guid of asset groups.</param>
        /// <param name="target">The build target.  This is used by the BundleBuildParameters base class.</param>
        /// <param name="group">The build target group. This is used by the BundleBuildParameters base class.</param>
        /// <param name="outputFolder">The path for the output folder. This is used by the BundleBuildParameters base class.</param>
        public AddressableAssetsBundleBuildParameters(AddressableAssetSettings aaSettings, Dictionary<string, string> bundleToAssetGroup, BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
        {
            UseCache = true;
            ContiguousBundles = aaSettings.ContiguousBundles;
#if NONRECURSIVE_DEPENDENCY_DATA
            NonRecursiveDependencies = aaSettings.NonRecursiveBuilding;
#endif
            DisableVisibleSubAssetRepresentations = aaSettings.DisableVisibleSubAssetRepresentations;

            m_settings = aaSettings;
            m_bundleToAssetGroup = bundleToAssetGroup;

            //If default group has BundledAssetGroupSchema use the compression there otherwise check if the target is webgl or not and try set the compression accordingly
            if (m_settings.DefaultGroup.HasSchema<BundledAssetGroupSchema>())
                BundleCompression = ConverBundleCompressiontToBuildCompression(m_settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>().Compression);
            else
                BundleCompression = target == BuildTarget.WebGL ? BuildCompression.LZ4Runtime : BuildCompression.LZMA;

            if (aaSettings.StripUnityVersionFromBundleBuild)
                ContentBuildFlags |= ContentBuildFlags.StripUnityVersion;
        }

        private BuildCompression ConverBundleCompressiontToBuildCompression(
            BundledAssetGroupSchema.BundleCompressionMode compressionMode)
        {
            BuildCompression compresion = BuildCompression.LZMA;
            switch (compressionMode)
            {
                case BundledAssetGroupSchema.BundleCompressionMode.LZMA:
                    break;
                case BundledAssetGroupSchema.BundleCompressionMode.LZ4:
                    compresion = BuildCompression.LZ4;
                    break;
                case BundledAssetGroupSchema.BundleCompressionMode.Uncompressed:
                    compresion = BuildCompression.Uncompressed;
                    break;
            }

            return compresion;
        }

        /// <summary>
        /// Get the compressions settings for the specified asset bundle.
        /// </summary>
        /// <param name="identifier">The identifier of the asset bundle.</param>
        /// <returns>The compression setting for the asset group.  If the group is not found, the default compression is used.</returns>
        public override BuildCompression GetCompressionForIdentifier(string identifier)
        {
            string groupGuid;
            if (m_bundleToAssetGroup.TryGetValue(identifier, out groupGuid))
            {
                var group = m_settings.FindGroup(g => g != null && g.Guid == groupGuid);
                if (group != null)
                {
                    var abSchema = group.GetSchema<BundledAssetGroupSchema>();
                    if (abSchema != null)
                        return abSchema.GetBuildCompressionForBundle(identifier);
                    else
                        Debug.LogWarningFormat("Bundle group {0} does not have BundledAssetGroupSchema.", group.name);
                }
                else
                {
                    Debug.LogWarningFormat("Unable to find group with guid {0}", groupGuid);
                }
            }
            return base.GetCompressionForIdentifier(identifier);
        }
    }
}
