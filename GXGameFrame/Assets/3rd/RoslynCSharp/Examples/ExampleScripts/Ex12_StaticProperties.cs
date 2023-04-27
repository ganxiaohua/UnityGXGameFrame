using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and then modify the value of a static property.
    /// </summary>
    public class Ex12_StaticProperties : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            static string exampleField = ""Hello World"";

            static string ExampleProperty
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


            // Get the property value from a property with the name 'ExampleProperty' and cast it to a string value
            // Note that only static properties can be accessed via a 'ScriptType' instance. A 'ScriptProxy' is required for instance properties
            string propertyString = (string)type.PropertiesStatic["ExampleProperty"];

            // Check that the string we read has the expected value
            Debug.Log(propertyString == "Hello World");

            // Set a property value with the name 'ExampleProperty'
            // Note that this will throw an 'TargetException' because the specified property does not define a 'set' accessor
            type.PropertiesStatic["ExampleProperty"] = "Goodbye World";


            // The safe version will handle any exceptions thrown when trying to access the property and return null if that is the case.
            propertyString = (string)type.SafePropertiesStatic["ExampleProperty"];

            type.SafePropertiesStatic["ExampleProperty"] = propertyString;
        }
    }
}