using UnityEngine;

namespace RoslynCSharp.Examples
{
    public class Ex12a_StaticEvents : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using System;
        using UnityEngine;
        class Example
        {
            static event Func<string, int> exampleEvent;

            static void TriggerExampleEvent(string msg)
            {
                Debug.Log(exampleEvent(msg));
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

            // Add an event listener for our target event - Generic parameters must be supplied for generic listeners methods that have a return type
            type.SafeEventsStatic["exampleEvent"].AddListener<string, int>(CustomEventHandler);

            // Call the method to trigger an event in user code
            type.SafeCallStatic("TriggerExampleEvent", "Hello World");
        }

        private int CustomEventHandler(string msg)
        {
            Debug.Log("Passed string: " + msg);
            return 123;
        }
    }
}
