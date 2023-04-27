using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and then modify an instance property value of a script proxy object.
    /// </summary>
    public class Ex16_InstanceProperties : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            string exampleField = ""Hello World"";

            string ExampleProperty
            {
                get { return exampleField; }
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


            // Get the property value from a property with the name 'ExampleProperty' and cast it to a string value
            string propertyString = (string)proxy.Properties["ExampleProperty"];

            // Check that the string we read has the expected value
            Debug.Log(propertyString == "Hello World");

            // Set a property value with the name 'ExampleProperty'
            // Note that this will throw an 'TargetException' because the specified property does not define a 'set' accessor
            proxy.Properties["ExampleProperty"] = "Goodbye World";


            // The safe version will handle any exceptions thrown when trying to access the property and return null if that is the case.
            propertyString = (string)proxy.SafeProperties["ExampleProperty"];

            proxy.SafeProperties["ExampleProperty"] = propertyString;
        }
    }
}