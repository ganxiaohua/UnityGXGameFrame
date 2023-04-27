using UnityEngine;
using RoslynCSharp.Compiler;    // Required for access to compiler types such as 'CompilationError'

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string.
    /// </summary>
    public class Ex06_CompileFromSource : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            static void ExampleMethod()
            {
                Debug.Log(""Hello World"");
            }
        }";

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");


            // Compile and load code
            ScriptAssembly assembly = domain.CompileAndLoadSource(sourceCode, ScriptSecurityMode.UseSettings);


            // Check for compiler errors
            if(domain.CompileResult.Success == false)
            {
                // Get all errors
                foreach(CompilationError error in domain.CompileResult.Errors)
                {
                    if(error.IsError == true)
                    {
                        Debug.LogError(error.ToString());
                    }
                    else if(error.IsWarning == true)
                    {
                        Debug.LogWarning(error.ToString());
                    }
                }
            }
        }
    }
}