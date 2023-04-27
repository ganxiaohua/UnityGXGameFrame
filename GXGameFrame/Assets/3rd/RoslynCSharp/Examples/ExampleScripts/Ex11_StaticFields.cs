using UnityEngine;

namespace RoslynCSharp.Example
{
    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and then modify the value of a static field.
    /// </summary>
    public class Ex11_StaticFields : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            static string exampleField = ""Hello World"";
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


            // Get the field value from a field with the name 'exampleField' and cast it to a string value
            // Note that only static fields can be accessed via a 'ScriptType' instance. A 'ScriptProxy' is required for instance fields
            string fieldString = (string)type.FieldsStatic["exampleField"];

            // Check that the string we read has the expected value
            Debug.Log(fieldString == "Hello World");

            // Set a field value with the name 'exampleField'
            // An exception may occur if the assigned type canot be implicitly converted to the actual field type
            type.FieldsStatic["exampleField"] = "Goodbye World";
            

            // The safe version will handle any exceptions thrown when trying to access the field and return null if that is the case.
            fieldString = (string)type.SafeFieldsStatic["exampleField"];

            type.SafeFieldsStatic["exampleField"] = fieldString;
        }
    }
}