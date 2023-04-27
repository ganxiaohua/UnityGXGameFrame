using System.Collections;
using UnityEngine;

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    /// <summary>
    /// An example script that shows how to load managed assemblies into an existing script domain asynchronously.
    /// </summary>
    public class Ex03_LoadAssemblyAsync : MonoBehaviour
    {
        private ScriptDomain domain = null;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public IEnumerator Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Security load mode
            // ScriptSecurityMode.EnsureLoad - Do not perform and code validation and just load the assembly
            // ScriptSecurityMode.EnsureSecurity - Perform full code validation and discard the assembly if verification fails
            // ScriptSecurityMode.UseSettings - Use the RoslynC# settings to determine which action to take
            ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings;

            // Load an assembly async - The assembly may be verified using active security restriction depending upon the security mod specified
            // If an assembly fails verification then it will not be loaded and the load operation will contain failure information
            AsyncLoadOperation assemblyLoad = domain.LoadAssemblyAsync("path/to/assembly.dll", securityMode);

            // Wait for request to complete
            yield return assemblyLoad;

            // Get the loaded assembly - This will be null if the load or security validation failed
            ScriptAssembly assembly = assemblyLoad.LoadedAssembly;
        }
    }
}
