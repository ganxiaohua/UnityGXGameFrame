using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Build
{
    /// <summary>
    /// Naming conventions for the built-in shader bundle name prefix.
    /// </summary>
    public enum ShaderBundleNaming
    {
        /// <summary>
        /// Set the built-in shader bundle name prefix to the hash of the project name.
        /// </summary>
        ProjectName,
        /// <summary>
        /// Set the built-in shader bundle name prefix to the guid of the default group.
        /// </summary>
        DefaultGroupGuid,
        /// <summary>
        /// Set the built-in shader bundle name prefix to the user specified value.
        /// </summary>
        Custom
    }
}

