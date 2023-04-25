using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoslynCSharp.ExampleScripts
{
    public class Ex16a_InstanceEvents : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using System;
        using UnityEngine;
        class Example
        {
            event Func<string, int> exampleEvent;

            void TriggerExampleEvent(string msg)
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

            // Create an instance ot the Example type
            ScriptProxy proxy = type.CreateInstance();

            // Add an event listener for our target event - Generic parameters must be supplied for generic listeners methods that have a return type
            proxy.SafeEvents["exampleEvent"].AddListener<string, int>(CustomEventHandler);

            // Call the method to trigger an event in user code
            proxy.SafeCall("TriggerExampleEvent", "Hello World");
        }

        private int CustomEventHandler(string msg)
        {
            Debug.Log("Passed string: " + msg);
            return 123;
        }
    }
}
