using System;
using UnityEngine;

namespace RoslynCSharp.Example
{
#pragma warning disable 0414

    /// <summary>
    /// An example script that shows how to setup a script domain for compiling and loading external code.
    /// </summary>
    public class Ex01_CreateScriptDomain : MonoBehaviour
    {
        private ScriptDomain domain = null;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Should the script domain initialize the roslyn compiler service. If this is disabled then you will not be able to compile C# code, only load managed assemblies.
            bool initCompiler = true;

            // Should the script domain be made active when created. The active domain can be accessed via 'ScriptDomain.Active'
            bool makeActiveDomain = true;

            // The app domain where all code should be loaded and executed. Note that you will need to manually handle domain assembly references
            // You can also pass null to 'CreateDomain'
            AppDomain appDomain = AppDomain.CurrentDomain;

            // Create a script domain. We use this domain to compile and load scripts and assemblies
            domain = ScriptDomain.CreateDomain("Example Domain", initCompiler, makeActiveDomain, appDomain);
        }
    }
}
