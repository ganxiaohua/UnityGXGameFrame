using RoslynCSharp.Compiler;
using System;
using System.Reflection;
using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to access the compiler service and add a number of reference assemblies from memory or file.
    /// </summary>
    public class Ex04_CompilerReferences : MonoBehaviour
    {
        private ScriptDomain domain = null;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // We will be accessing the compiler service so make sure it is available
            // This is only a demonstration and passing 'true' to the 'CreateDomain' method will ensure that the compiler service is initialized
            if (domain.IsCompilerServiceInitialized == false)
                throw new InvalidOperationException("Compiler service is not initialized");


            // Add a reference to 'System.Core' - 'HashSet<>' is defined in 'System.Core.dll'
            Assembly systemCoreAssembly = typeof(System.Collections.Generic.HashSet<>).Assembly;

            // Add a compiler reference
            domain.RoslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromAssembly(systemCoreAssembly));



            // Add a reference to an unloaded assembly via file path
            string referenceAssemblyPath = "path/to/reference/assembly.dll";

            // Add a compiler reference
            domain.RoslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(referenceAssemblyPath));
        }
    }
}