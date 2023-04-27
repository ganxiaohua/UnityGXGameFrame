using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RoslynCSharp.Compiler
{
    public sealed class CompilationResult : IMetadataReferenceProvider
    {
        // Private
        private bool success = false;
        private AssemblyOutput assembly = new AssemblyOutput();
        private CompilationError[] errors = null;

        // Properties
        public MetadataReference CompilerReference
        {
            get
            {
                // Only successful compiles can be referenced
                if (success == false)
                    throw new InvalidDataException("Cannot get matadata reference from compliation result because the compile was unsuccessful");

                // Create a file reference
                if (OutputFile != null)
                    return new AssemblyReferenceFromFile(OutputFile).CompilerReference;

                // Create a memory reference
                if (OutputAssemblyImage != null)
                    return new AssemblyReferenceFromImage(OutputAssemblyImage).CompilerReference;

                return null;
            }
        }

        public bool Success
        {
            get { return success; }
        }

        public string OutputFile
        {
            get { return assembly.AssemblyFilePath; }
            internal set { assembly.AssemblyFilePath = value; }
        }

        public string OutputPDBFile
        {
            get { return assembly.AssemblyPDBFilePath; }
            internal set { assembly.AssemblyPDBFilePath = value; }
        }

        public byte[] OutputAssemblyImage
        {
            get { return assembly.AssemblyImage; }
            internal set { assembly.AssemblyImage = value; }
        }

        public byte[] OutputPDBImage
        {
            get { return assembly.AssemblyPDBImage; }
            internal set { assembly.AssemblyPDBImage = value; }
        }

        public Assembly OutputAssembly
        {
            get { return assembly.OutputAssembly; }
            internal set { assembly.OutputAssembly = value; }
        }

        public CompilationError[] Errors
        {
            get { return errors; }
        }

        public int ErrorCount
        {
            get
            {
                int count = 0;

                foreach (CompilationError error in errors)
                    if (error.IsError == true)
                        count++;

                return count;
            }
        }

        public int WarningCount
        {
            get
            {
                int count = 0;

                foreach (CompilationError error in errors)
                    if (error.IsWarning == true)
                        count++;

                return count;
            }
        }

        public int InfoCount
        {
            get
            {
                int count = 0;

                foreach (CompilationError error in errors)
                    if (error.IsInfo == true)
                        count++;

                return count;
            }
        }

        internal AssemblyOutput AssemblyOutput
        {
            get { return assembly; }
        }

        // Constructor
        internal CompilationResult(bool success, IEnumerable<Diagnostic> diagnostics)
        {
            this.success = success;
            CreateErrors(diagnostics);
        }

        // Methods
        public Assembly LoadCompiledAssembly(AppDomain loadDomain = null)
        {
            // Only load if the compilation was successful
            if (success == false)
                return null;
            
            // Check for already loaded
            if (OutputAssembly != null)
                return OutputAssembly;

            // Get valid domain
            if (loadDomain == null)
                loadDomain = AppDomain.CurrentDomain;

            if (string.IsNullOrEmpty(OutputFile) == false)
            {                
                // Check for symbols
                if(string.IsNullOrEmpty(OutputPDBFile) == true)
                {
                    // Create assembly name info
                    AssemblyName asmName = AssemblyName.GetAssemblyName(OutputFile);

                    // Try to load assembly from path
                    OutputAssembly = loadDomain.Load(asmName);
                }
                else
                {
                    // Try to load assembly with symbols
                    OutputAssembly = loadDomain.Load(OutputAssemblyImage, OutputPDBImage);
                }
            }
            else if(OutputAssemblyImage != null)
            {
                // Check for symbols
                if(OutputPDBImage == null)
                {
                    // Try to load assembly without symbols
                    OutputAssembly = loadDomain.Load(OutputAssemblyImage);
                }
                else
                {
                    // Try to load assembly with symbols
                    OutputAssembly = loadDomain.Load(OutputAssemblyImage, OutputPDBImage);
                }
            }

            return OutputAssembly;
        }

        private void CreateErrors(IEnumerable<Diagnostic> diagnostics)
        {
            // Create temp list
            List<CompilationError> errors = new List<CompilationError>();

            // Process all diagnostics
            foreach(Diagnostic diagnostic in diagnostics)
            {
                // Useless warnings relating to binding higher assembly versions to lower versions
                switch(diagnostic.Id)
                {
                    case "CS1701":
                    case "CS1702":
                        continue;
                }

                // Dont register hidden diagnostics
                if (diagnostic.Severity == DiagnosticSeverity.Hidden)
                    continue;

                // Create diagnostic error
                errors.Add(new CompilationError(diagnostic));
            }

            // Store member errors
            this.errors = errors.ToArray();
        }
    }
}
