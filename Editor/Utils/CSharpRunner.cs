using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using Microsoft.CSharp;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public static class CSharpRunner
    {
        public static readonly string Program = $"{nameof(CSharpRunner)}Program";

        public static async UniTask Execute(string usings, string code, string[] referencedAssemblies = null)
        {
            await UniTask.SwitchToThreadPool();

            string fullScript =
                    $@"{usings}
public class {Program}
{{
    public static async void Main()
    {{
        await UniTask.Yield();
        {code}
    }}
}}";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            if (referencedAssemblies == null)
            {
                parameters.ReferencedAssemblies.AddRange(GetReferences().ToArray());
            }
            else
            {
                parameters.ReferencedAssemblies.AddRange(referencedAssemblies);
            }

            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, fullScript);

            await UniTask.SwitchToMainThread();

            if (results.Errors.HasErrors)
            {
                int lineOffset = 6;
                for (int i = 0; i < usings.Length; i++)
                    if (usings[i] == '\n')
                        lineOffset++;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    var error = results.Errors[i];
                    // filter error message: "The predefined type `<Type>' is defined multiple times"
                    if (error.ErrorNumber == "CS1685")
                        continue;
                    sb.AppendLine($"Error({error.ErrorNumber}): {error.ErrorText}(at <color=#4B7AF3>Console line:{error.Line - lineOffset}</color>)");
                }

                sb.AppendLine();
                sb.AppendLine(fullScript);

                Debug.LogError(sb.ToString());
            }
            else
            {
                Assembly assembly = results.CompiledAssembly;
                Type program = assembly.GetType(Program);
                MethodInfo main = program.GetMethod("Main");
                main?.Invoke(null, null);
            }

            provider.Dispose();
        }

        public static List<string> GetReferences()
        {
            var list = new List<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic && !list.Contains(assembly.Location) && !string.IsNullOrEmpty(assembly.Location))
                {
                    if (FilterReferenceFile(assembly.Location))
                        continue;
                    list.Add(assembly.Location);
                }
            }

            list.Sort();
            return list;
        }

        private static bool FilterReferenceFile(string file)
        {
            if (file.EndsWith("mscorlib.dll"))
                return true;
            return false;
        }
    }
}