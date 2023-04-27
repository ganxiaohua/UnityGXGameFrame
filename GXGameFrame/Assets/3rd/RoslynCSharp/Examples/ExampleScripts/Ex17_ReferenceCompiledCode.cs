using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynCSharp.Compiler;
using System.IO;
using UnityEngine;

namespace RoslynCSharp.Example
{
    public class Ex17_ReferenceCompiledCode : MonoBehaviour
    {
        // Private
        private ScriptDomain domain = null;
        private const string sourceCodeA = @"
        using UnityEngine;
        public class Example
        {
            public void LogToConsole(string arg)
            {
                Debug.Log(arg);
            }
        }";

        private const string sourceCodeB = @"
        using UnityEngine;
        public class ReferenceExample
        {
            public static void SayHello()
            {
                Example refClass = new Example();
                refClass.LogToConsole(""Hello World"");
            }
        }";

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Create domain
            domain = ScriptDomain.CreateDomain("Example Domain");


            // Compile and load the first batch of source code.
            // The public types in this source code will be accessible to any assemblies that reference it.
            ScriptAssembly assemblyA = domain.CompileAndLoadSource(sourceCodeA, ScriptSecurityMode.UseSettings);

            // Compile and load the second batch of source code.
            // Note that we pass 'assemblyA' as part of the third argument 'additionalAssemblyReferences'. This will allow the code we are compiling to access any public types defined in assemblyA.
            // We could also add many more reference assemblies if required by providing a bigger array of references.
            ScriptAssembly assemblyB = domain.CompileAndLoadSource(sourceCodeB, ScriptSecurityMode.UseSettings, new IMetadataReferenceProvider[] { assemblyA });

            // Call the static method 'SayHello' which will call the method 'LogToConsole' which is defined in assemblyA
            assemblyB.MainType.SafeCallStatic("SayHello");

            // Note that there are many other ways to add assembly references. 
            // Any type that implements 'IMetadataAssemblyProvider' can be passed including RoslynCSharp.ScriptAssembly, and RoslynCSharp.Compiler.CompilationResult.
            // You can also use the 'RoslynCSharp.Compiler.AssemblyReference' type to reference other assemblies in a few different ways. All of the following AssemblyReference method calls return an IMetadataReferenceProvider value.

            // Get a metadata reference from a System.Reflection.Assembly which is already loaded. Note that the 'Location' property of the assembly cannot be empty otherwise this will fail.
            AssemblyReference.FromAssembly(typeof(object).Assembly);

            // Get a metadata reference from an assembly image data array. The source array can come from anywhere as long as it is a valid assembly image.
            byte[] bytes = File.ReadAllBytes("C:/Assemblies/MyAssembly.dll");
            AssemblyReference.FromImage(bytes);

            // Get a metadata reference from a System.IO.Stream containing assembly image data.
            Stream assemblyStream = File.OpenRead("C:/Assemblies/MyAssembly.dll");
            AssemblyReference.FromStream(assemblyStream);

            // Get a metadata reference from a loaded assembly with the specified name or an assembly at the specified path. Note that the 'Location' property of the loaded assembly cannot be empty if providing only and assembly name.
            AssemblyReference.FromNameOrFile("mscorlib");
            AssemblyReference.FromNameOrFile("C:/Assemblies/MyAssembly.dll");
        }
    }
}
