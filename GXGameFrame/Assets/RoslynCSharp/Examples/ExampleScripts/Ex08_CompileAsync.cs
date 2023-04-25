using UnityEngine;
using RoslynCSharp.Compiler;    // Required for access to compiler types such as 'CompilationError'
using System.Collections;

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string asynchronously.
    /// </summary>
    public class Ex08_CompileAsync : MonoBehaviour
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
        public IEnumerator Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");


            // Compile and load code
            AsyncCompileOperation compileRequest = domain.CompileAndLoadSourceAsync(sourceCode, ScriptSecurityMode.UseSettings);

            // Wait for operation to complete
            yield return compileRequest;

            // Check for compiler errors
            if (compileRequest.IsSuccessful == false)
            {
                // Get all errors
                foreach (CompilationError error in compileRequest.CompileDomain.CompileResult.Errors)
                {
                    if (error.IsError == true)
                    {
                        Debug.LogError(error.ToString());
                    }
                    else if (error.IsWarning == true)
                    {
                        Debug.LogWarning(error.ToString());
                    }
                }
            }
        }
    }
}