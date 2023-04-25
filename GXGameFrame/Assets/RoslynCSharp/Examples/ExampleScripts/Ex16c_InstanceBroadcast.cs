using UnityEngine;

namespace RoslynCSharp.Example
{
    public class Ex16c_InstanceBroadcast : MonoBehaviour
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

        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");

            // Compile and load code
            ScriptType type = domain.CompileAndLoadMainSource(sourceCode);

            // We need to create an instance to receive the broadcast
            type.CreateInstance();

            // Send an instance broadcast which will cause all instance methods with matching names and argument lists in the domain to be invoked on non-mono behaviour scripts.
            // Note that we can pass arguments if required or none at all, but the argument list must match in order for the broadcast to invoke the target method.
            // Note that broadcast methods will not generate any errors or exceptions. Invocations will fail silently if an error occurs.
            // Note that we need to pass a baseType filter. This can be used to only clal the method on types that derive from a particular base type, but you can also pass' typeof(object)' if no filter is required.
            domain.BroadcastInstance(typeof(object), "ExampleMethod", "World");
        }
    }
}
