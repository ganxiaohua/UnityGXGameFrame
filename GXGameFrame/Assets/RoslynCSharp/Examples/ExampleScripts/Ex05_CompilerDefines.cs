using System;
using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to access the compiler service and add a number of scripting define symbols.
    /// </summary>
    public class Ex05_CompilerDefines : MonoBehaviour
    {
        private ScriptDomain domain = null;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Add define symbols to compile code in different configurations
            domain.RoslynCompilerService.DefineSymbols.Add("DEBUG");
            domain.RoslynCompilerService.DefineSymbols.Add("UNITY_EDITOR");
        }
    }
}