using Microsoft.CodeAnalysis;
using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RoslynCSharp.Implementation
{
    internal class ScriptAssemblyImpl : ScriptAssembly
    {
        // Private
        private ScriptDomain domain = null;
        private Assembly systemAssembly = null;        

        // Properties
        public override ScriptDomain Domain
        {
            get { return domain; }
        }

        public override Assembly SystemAssembly
        {
            get { return systemAssembly; }
        }

        public override MetadataReference CompilerReference
        {
            get
            {
                if (AssemblyImage != null)
                    return AssemblyReference.FromImage(AssemblyImage).CompilerReference;

                return AssemblyReference.FromNameOrFile(AssemblyPath).CompilerReference;
            }
        }

        public override bool IsRuntimeCompiled
        {
            get { return false; }
        }

        public override DateTime RuntimeCompiledTime
        {
            get { return DateTime.MinValue; }
        }

        public override CompilationResult CompileResult
        {
            get { return null; }
        }

        // Construction
        protected override void ConstructInstance(ScriptDomain domain, Assembly systemAssembly)
        {
            this.domain = domain;
            this.systemAssembly = systemAssembly;

            if (string.IsNullOrEmpty(systemAssembly.Location) == false && File.Exists(systemAssembly.Location) == true)
                this.assemblyImage = File.ReadAllBytes(systemAssembly.Location);
        }

        // Methods
        protected override ScriptType CreateRootScriptType(Type systemType)
        {
            return ScriptType.CreateScriptType<ScriptTypeImpl>(this, null, systemType);
        }
    }
}
