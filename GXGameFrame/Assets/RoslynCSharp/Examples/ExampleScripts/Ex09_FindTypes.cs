using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and then use a number of find methods to get types from the compiled assembly.
    /// </summary>
    public class Ex09_FindTypes : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example : MonoBehaviour
        {
            static void ExampleMethod(string input)
            {
                Debug.Log(""Hello "" + input);
            }
        }";

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");


            // Compile and load code - Note that we use 'CompileAndLoadMainSource' which is the same as 'CompileAndLoadSource' but returns the main type in the compiled assembly
            ScriptAssembly assembly = domain.CompileAndLoadSource(sourceCode, ScriptSecurityMode.UseSettings);


            // Should the search include types or members that are private or internal
            bool includeNonPublic = true;


            // Find all types in the assembly
            foreach (ScriptType type in assembly.FindAllTypes(includeNonPublic))
                Debug.Log("FindAllTypes: " + type.FullName);
        
            // Find all types in the assembly without allocating return arrays
            foreach (ScriptType type in assembly.EnumerateAllTypes(includeNonPublic))
                Debug.Log("EnumerateAllTypes: " + type.FullName);


            // Find all types that inherit from a specified type
            foreach (ScriptType type in assembly.EnumerateAllSubTypesOf<MonoBehaviour>(includeNonPublic))
                Debug.Log("EnumerateSubTypesOf<MonoBehaviour>: " + type.FullName);

            // Alternative shorthand for built in unity base types
            assembly.EnumerateAllMonoBehaviourTypes(includeNonPublic);
            assembly.EnumerateAllScriptableObjectTypes(includeNonPublic);

            // Find all unity types, ie: types that inherit from UnityEngine.Object
            assembly.EnumerateAllUnityTypes(includeNonPublic);
            
        }
    }
}