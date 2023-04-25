using UnityEngine;

namespace RoslynCSharp.Example
{
    public class Ex16b_InstanceBehaviourBroadcast : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example : MonoBehaviour
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
            type.CreateInstance(gameObject);

            // Send an instance broadcast which will cause all instance methods with matching names and argument lists in the domain to be invoked on mono behaviour scripts.
            // Note that we can pass arguments if required or none at all, but the argument list must match in order for the broadcast to invoke the target method.
            // Note that broadcast methods will not generate any errors or exceptions. Invocations will fail silently if an error occurs.
            domain.BroadcastActiveScene("ExampleMethod", "World");

            // We can also broadcast all scenes or a specific scene using Broadcast(Scene, string).
            domain.BroadcastAllScenes("ExampleMethod", "World");
        }
    }
}
