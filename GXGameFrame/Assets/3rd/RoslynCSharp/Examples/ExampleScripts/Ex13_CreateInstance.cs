using UnityEngine;

namespace RoslynCSharp.Example
{
#pragma warning disable 0219

    /// <summary>
    /// An example script that shows how to use the compiler service to compile and load a C# source code string and create an instance of the main type.
    /// </summary>
    public class Ex13_CreateInstance : MonoBehaviour
    {
        private ScriptDomain domain = null;
        private const string sourceCode = @"
        using UnityEngine;
        class Example
        {
            public Example() { }

            public Example(string arg)
            {
                Debug.Log(""Example_ctor: "" + arg);
            }
        }

        class ExampleBehaviour : MonoBehaviour
        {
            void Start()
            {
                Debug.Log(""ExampleBehaviour: Start"");
            }
        }

        class ExampleScriptable : ScriptableObject
        {
            void OnEnable()
            {
                Debug.Log(""ExampleScriptable: OnEnable"");
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


            // Find all types
            ScriptType type = assembly.FindType("Example");
            ScriptType behaviourType = assembly.FindSubTypeOf<MonoBehaviour>("ExampleBehaviour");
            ScriptType scriptableType = assembly.FindSubTypeOf<ScriptableObject>("ExampleScriptable");


            // Create an instance of 'Example'
            ScriptProxy proxy = type.CreateInstance();

            // Create an instance of 'Example' using the overload constructor
            proxy = type.CreateInstance(null, "Hello World");


            // Create an instance of 'ExampleBehaviour'
            // Note that we need to pass a game object to attach the component to because it inherits from 'MonoBehaviour' 
            // Failing to provide a valid game object will case the result to be null
            // Awake, OnEnable, Start and other Unity callbacks will be triggered as you would expect
            ScriptProxy behaviourProxy = behaviourType.CreateInstance(gameObject);

            // Both outputs will be 'true'
            Debug.Log(behaviourProxy.IsMonoBehaviour);
            Debug.Log(behaviourProxy.IsUnityObject);


            // Create an instance of 'ExampleScriptable'
            // this is much the same as creating a normal instance however it will be constructed using 'ScriptableObject.CreateInstance' because it inherits from 'ScriptableObject'
            ScriptProxy scriptableProxy = scriptableType.CreateInstance();

            // Both outputs will be 'true'
            Debug.Log(scriptableType.IsScriptableObject);
            Debug.Log(scriptableType.IsUnityObject);
        }
    }
}