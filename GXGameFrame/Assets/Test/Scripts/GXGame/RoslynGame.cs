using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using UnityEngine;

public class RoslynGame : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        var domain = ScriptDomain.CreateDomain("Example Domain");
        var thisDomain = AppDomain.CurrentDomain;
        var Assemblies = thisDomain.GetAssemblies();
        List<IMetadataReferenceProvider> assemblyreference = new ();
        foreach (var Assemblie in Assemblies)
        {
            if (!Assemblie.IsDynamic && !string.IsNullOrEmpty(Assemblie.Location))
            {
                Debug.Log(Assemblie.Location);
                assemblyreference.Add(new AssemblyReferenceFromAssemblyObject(Assemblie));
            }
        }
        //这里不得先,因为systembind中是cspah程序集里面的,而我们要找的是新建程序集中的UIEnitity.
        ScriptAssembly assembly = domain.CompileAndLoadDirectory("Assets/Test/Scripts/GXGame/Sub", "*.cs", SearchOption.AllDirectories,ScriptSecurityMode.UseSettings,assemblyreference.ToArray());
        
        // ScriptAssembly assembly = domain.CompileAndLoadFile("Assets/Test/Scripts/GXGame/RoslynMan.cs", ScriptSecurityMode.UseSettings);

        if (domain.CompileResult.Success == false)
        {
            foreach (CompilationError error in domain.CompileResult.Errors)
            {
                if (error.IsError == true)
                {
                    Debug.LogError(error.ToString());
                }
                else if (error.IsWarning == true)
                {
                    Debug.LogWarning(error.ToString());
                }
            }
        }

        ReloadType = assembly.FindType("RoslynMan");
        ReloadType.CallStatic("Start");
    }

    private ScriptType ReloadType;
    // Update is called once per frame
    void Update()
    {
        ReloadType.CallStatic("Update");
    }
}