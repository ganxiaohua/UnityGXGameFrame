using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and then call an instance method on a proxy object.
    /// </summary>
    public class Ex14_InstanceMethods : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            void ExampleMethod(string input)
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
            ScriptType type = domain.CompileAndLoadMainSource(sourceCode, ScriptSecurityMode.UseSettings);                       

            // Create an instance of 'Example'
            ScriptProxy proxy = type.CreateInstance();

            // Create an instance of 'Example' using the overload constructor
            proxy = type.CreateInstance();


            // Call the method called 'ExampleMethod' and pass the string argument 'World'
            // Note that any exceptions thrown by the target method will not be caught
            proxy.Call("ExampleMethod", "World");


            // Call the method called 'ExampleMethod' and pass the string argument 'Safe World'
            // Note that any exceptions thrown by the target method will handled as indicated by the 'Safe' name
            proxy.SafeCall("ExampleMethod", "Safe World");
        }
    }

    public class Ex14_InstanceMethods_Updated : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            void ExampleMethod(string input)
            {
                Debug.Log(""Hello "" + input);
            }
        }";

        public AssemblyReferenceAsset netStandardRef;
        public AssemblyReferenceAsset unityRef;

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Compile and load code - Note that we use 'CompileAndLoadMainSource' which is the same as 'CompileAndLoadSource' but returns the main type in the compiled assembly
            ScriptType type = domain.CompileAndLoadMainSource(sourceCode, ScriptSecurityMode.UseSettings, new Compiler.IMetadataReferenceProvider[]
            {
                netStandardRef,
                unityRef,
            });

            // Create an instance of 'Example'
            ScriptProxy proxy = type.CreateInstance();

            // Create an instance of 'Example' using the overload constructor
            proxy = type.CreateInstance();


            // Call the method called 'ExampleMethod' and pass the string argument 'World'
            // Note that any exceptions thrown by the target method will not be caught
            proxy.Call("ExampleMethod", "World");


            // Call the method called 'ExampleMethod' and pass the string argument 'Safe World'
            // Note that any exceptions thrown by the target method will handled as indicated by the 'Safe' name
            proxy.SafeCall("ExampleMethod", "Safe World");
        }
    }
}