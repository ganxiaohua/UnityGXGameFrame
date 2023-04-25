using UnityEngine;

namespace RoslynCSharp.Example
{
    public class Ex12b_StaticBroadcast : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            static void ExampleMethod(string input)
            {
                Debug.Log(""Hello "" + input);
            }
        }";

        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Compile and load code - Note that we do not need the result object for broadcasts
            domain.CompileAndLoadMainSource(sourceCode);

            // Send a static broadcast which will cause all static methods with matching names and argument lists in the domain to be invoked
            // Note that we can pass arguments if required or none at all, but the argument list must match in order for the broadcast to invoke the target method.
            // Note that broadcast methods will not generate any errors or exceptions. Invocations will fail silently if an error occurs.
            domain.StaticBroadcast("ExampleMethod", "World");
        }
    }
}
