using System;
using System.Text;
using Object = UnityEngine.Object;

namespace UnityEngine
{
    /// <summary>
    /// Used to restrict an AssetReference field or property to only allow items wil specific labels.  This is only enforced through the UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class AssetReferenceUIRestriction : Attribute
    {
        /// <summary>
        /// Validates that the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="obj">The Object to validate.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public virtual bool ValidateAsset(Object obj)
        {
            return true;
        }

        /// <summary>
        /// Validates that the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="path">The path to the asset in question.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public virtual bool ValidateAsset(string path)
        {
            return true;
        }
    }
    /// <summary>
    /// Used to restrict an AssetReference field or property to only allow items wil specific labels.  This is only enforced through the UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssetReferenceUILabelRestriction : AssetReferenceUIRestriction
    {
        /// <summary>
        /// Stores the labels allowed for the AssetReference.
        /// </summary>
        public string[] m_AllowedLabels;

        /// <summary>
        /// Stores the allowed labels formatted as a string.
        /// </summary>
        public string m_CachedToString;

        /// <summary>
        /// Creates a new AssetReferenceUILabelRestriction object.
        /// </summary>
        /// <param name="allowedLabels">The labels allowed for the AssetReference.</param>
        public AssetReferenceUILabelRestriction(params string[] allowedLabels)
        {
            m_AllowedLabels = allowedLabels;
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(Object obj)
        {
            return true;
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(string path)
        {
            return true;
        }

        /// <summary>
        /// Converts the information about the allowed labels to a formatted string.
        /// </summary>
        /// <returns>Returns information about the allowed labels as a string.</returns>
        public override string ToString()
        {
            if (m_CachedToString == null)
            {
                StringBuilder sb = new StringBuilder();
                bool first = true;
                foreach (var t in m_AllowedLabels)
                {
                    if (!first)
                        sb.Append(',');
                    first = false;
                    sb.Append(t);
                }
                m_CachedToString = sb.ToString();
            }
            return m_CachedToString;
        }
    }
}
