using UnityEngine;

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    /// <summary>
    /// An example script that shows how to load managed assemblies into an existing script domain.
    /// </summary>
    public class Ex02_LoadAssembly : MonoBehaviour
    {
        private ScriptDomain domain = null;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Security load mode
            // ScriptSecurityMode.EnsureLoad - Do not perform and code validation and just load the assembly
            // ScriptSecurityMode.EnsureSecurity - Perform full code validation and discard the assembly if verification fails
            // ScriptSecurityMode.UseSettings - Use the RoslynC# settings to determine which action to take
            ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings;

            // Load an assembly - The assembly may be verified using active security restriction depending upon the security mod specified
            // If an assembly fails verification then it will not be loaded and the load method will return null
            ScriptAssembly assembly = domain.LoadAssembly("path/to/assembly.dll", securityMode);
        }
    }
}
