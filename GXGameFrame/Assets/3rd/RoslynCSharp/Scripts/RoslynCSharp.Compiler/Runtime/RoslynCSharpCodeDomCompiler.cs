#define ROSLYNCSHARP_NO_CODEDOM

#if !ROSLYNCSHARP_NO_CODEDOM
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynCSharp.Compiler;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;

namespace RoslynCSharp.CodeDom.Compiler
{
    public class RoslynCSharpCodeDomCompiler : ICodeCompiler
    {
        // Private
        private string outputDirectory = string.Empty;

        // Properties
        public string OutputDirectory
        {
            get { return outputDirectory; }
            set
            {
                outputDirectory = value;

                // Dont allow null path
                if (outputDirectory == null)
                    outputDirectory = string.Empty;
            }
        }

        // Methods
        public CompilerResults CompileAssemblyFromFile(CompilerParameters parameters, string fileName)
        {
            // Convert parameters
            CSharpParseOptions parseOption = GetParseOptions(parameters);
            CSharpCompilationOptions compilationOptions = GetCompilationOptions(parameters);

            // Parse the file
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = RoslynCSharpCompiler.ParseFile(fileName, parseOption);

            // Build references
            string[] references = GetCompilationReferences(parameters);

            // Generate compilation object
            Compilation compileObject = RoslynCSharpCompiler.CreateCompilationObject(parameters.OutputAssembly, references, syntaxTrees, compilationOptions);
            
            // Emit compilation object to target stream
            using (Stream outputStream = GetOutputStream(parameters))
            {
                // Emit compilation object
                CompilationResult result = RoslynCSharpCompiler.EmitCompilationObject(compileObject, outputStream);

                // Convert results
                return GetCompilerResults(result);
            }
        }

        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters parameters, string[] fileNames)
        {
            // Convert parameters
            CSharpParseOptions parseOption = GetParseOptions(parameters);
            CSharpCompilationOptions compilationOptions = GetCompilationOptions(parameters);

            // Parse the file
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = RoslynCSharpCompiler.ParseFiles(fileNames, parseOption);

            // Build references
            string[] references = GetCompilationReferences(parameters);

            // Generate compilation object
            Compilation compileObject = RoslynCSharpCompiler.CreateCompilationObject(parameters.OutputAssembly, references, syntaxTrees, compilationOptions);

            // Emit compilation object to target stream
            using (Stream outputStream = GetOutputStream(parameters))
            {
                // Emit compilation object
                CompilationResult result = RoslynCSharpCompiler.EmitCompilationObject(compileObject, outputStream);

                // Convert results
                return GetCompilerResults(result);
            }
        }

        public CompilerResults CompileAssemblyFromSource(CompilerParameters parameters, string source)
        {
            // Convert parameters
            CSharpParseOptions parseOption = GetParseOptions(parameters);
            CSharpCompilationOptions compilationOptions = GetCompilationOptions(parameters);

            // Parse the file
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = RoslynCSharpCompiler.ParseSource(source, parseOption);

            // Build references
            string[] references = GetCompilationReferences(parameters);

            // Generate compilation object
            Compilation compileObject = RoslynCSharpCompiler.CreateCompilationObject(parameters.OutputAssembly, references, syntaxTrees, compilationOptions);

            // Emit compilation object to target stream
            using (Stream outputStream = GetOutputStream(parameters))
            {
                // Emit compilation object
                CompilationResult result = RoslynCSharpCompiler.EmitCompilationObject(compileObject, outputStream);

                // Convert results
                return GetCompilerResults(result);
            }
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters parameters, string[] sources)
        {
            // Convert parameters
            CSharpParseOptions parseOption = GetParseOptions(parameters);
            CSharpCompilationOptions compilationOptions = GetCompilationOptions(parameters);

            // Parse the file
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = RoslynCSharpCompiler.ParseSources(sources, parseOption);

            // Build references
            string[] references = GetCompilationReferences(parameters);

            // Generate compilation object
            Compilation compileObject = RoslynCSharpCompiler.CreateCompilationObject(parameters.OutputAssembly, references, syntaxTrees, compilationOptions);

            // Emit compilation object to target stream
            using (Stream outputStream = GetOutputStream(parameters))
            {
                // Emit compilation object
                CompilationResult result = RoslynCSharpCompiler.EmitCompilationObject(compileObject, outputStream);

                // Convert results
                return GetCompilerResults(result);
            }
        }

        #region NotSupported
        public CompilerResults CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit compilationUnit)
        {
            throw new NotSupportedException("Use compile from file or compile from source");
        }

        public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
        {
            throw new NotSupportedException("Use compile from file or compile from source");
        }
        #endregion

        private CSharpParseOptions GetParseOptions(CompilerParameters parameters)
        {
            // Language version
            string languageValue = GetParameterOption(parameters, "/languageversion");
            LanguageVersion version = LanguageVersion.Default;

            if(languageValue != null)
            {
                decimal specifiedVersion;
                bool success = decimal.TryParse(languageValue, out specifiedVersion);

                if(success == true)
                {
                    foreach (LanguageVersion languageVersion in Enum.GetValues(typeof(LanguageVersion)))
                    {
                        // Get enum string value
                        string enumString = languageVersion.ToString();

                        // Check for options with 'CSharp' in
                        if (enumString.Contains("CSharp") == false)
                            continue;

                        // Remove string value
                        enumString = enumString.Replace("CSharp", string.Empty);

                        // Convert to string and check for equality
                        if(specifiedVersion.ToString().Replace(".", "_") == enumString)
                        {
                            version = languageVersion;
                            break;
                        }
                    }
                }
            }

            // Define options
            string defineValue = GetParameterOption(parameters, "/define");
            List<string> defineSymbols = new List<string>();

            if(defineValue != null)
            {
                // Find all defines
                string[] split = defineValue.Split(';');

                // Process each define
                foreach(string defineOption in split)
                {
                    // Skip empty symbols
                    if (string.IsNullOrEmpty(defineOption) == true)
                        continue;

                    // Add to define list
                    defineSymbols.Add(defineOption);
                }
            }

            // Create parse options
            return new CSharpParseOptions(
                version,
                DocumentationMode.None,
                SourceCodeKind.Regular,
                defineSymbols);
        }

        private CSharpCompilationOptions GetCompilationOptions(CompilerParameters parameters)
        {
            // Find the output type we want
            OutputKind outputKind = (parameters.GenerateExecutable == true) ? 
                OutputKind.WindowsApplication : 
                OutputKind.DynamicallyLinkedLibrary;

            // Check for optimize
            OptimizationLevel optimize = (GetParameterOption(parameters, "/optimize") != null)
                ? OptimizationLevel.Release
                : OptimizationLevel.Debug;

            // Check for unsafe
            bool allowUnsafe = (GetParameterOption(parameters, "/unsafe") != null)
                ? true
                : false;

            // Check for platform
            Platform platform = Platform.AnyCpu;

            bool allowConcurrentCompile = true;

            // Create the output options
            return new CSharpCompilationOptions(
                outputKind,
                false,                                                          // Suppressed diagnostics
                null,                                                           // Module name
                null,                                                           // Main type name
                null,                                                           // Script class name
                null,                                                           // Using
                optimize,                                                       // Optimize level
                false,                                                          // Check overflow
                allowUnsafe,                                                    // Allow unsafe
                null,                                                           // Krypto key container
                null,                                                           // Krypto key file
                default(System.Collections.Immutable.ImmutableArray<byte>),     // Krypto public key
                null,                                                           // Delay sign
                platform,                                                       // Platform
                ReportDiagnostic.Default,                                       // Diagnostic option
                parameters.WarningLevel,                                        // Warning level
                null,                                                           // Specific diagnostic options
                allowConcurrentCompile,                                         // Allow concurrent compile
                false,                                                          // Deterministic
                null,                                                           // XML reference resolver
                null,                                                           // Source reference resolver
                null,                                                           // Metadata reference resolver
                null,                                                           // Assembly identity comparer
                null,                                                           // Strong name provider
                false,                                                          // Public sign                                 
                MetadataImportOptions.Public);                                  // Meta import options            
        }

        private CompilerResults GetCompilerResults(CompilationResult result)
        {
            CompilerResults compilerResults = new CompilerResults(null);

            // Setup results
            compilerResults.NativeCompilerReturnValue = 0;
            compilerResults.PathToAssembly = result.OutputFile;
            compilerResults.CompiledAssembly = result.OutputAssembly;

            foreach(CompilationError error in result.Errors)
            {
                // Skip info messages
                if (error.IsInfo == true)
                    continue;

                // Create a compiler error
                CompilerError compilerError = new CompilerError(
                    error.SourceFile, 
                    error.SourceLine, 
                    error.SourceColumn, 
                    error.Code, 
                    error.Message);

                // Set warning flag
                compilerError.IsWarning = error.IsWarning;

                // Register the error
                compilerResults.Errors.Add(compilerError);
            }

            return compilerResults;
        }

        private Stream GetOutputStream(CompilerParameters parameters)
        {
            Stream outStream = null;

            // Check for generate in memory
            if(parameters.GenerateInMemory == true)
            {
                // Create a memory stream for the output
                outStream = new MemoryStream();
            }
            else
            {
                // Get correct extension for module type
                string extension = ".dll";

                if (parameters.GenerateExecutable == true)
                    extension = ".exe";

                // Build the output filepath
                string filePath = Path.Combine(outputDirectory, parameters.OutputAssembly + extension);

                // Create a file stream for the output
                outStream = new FileStream(filePath, FileMode.Create);
            }

            return outStream;
        }

        private string[] GetCompilationReferences(CompilerParameters parameters)
        {
            // Allocate references array
            string[] references = new string[parameters.ReferencedAssemblies.Count];

            // Add each reference
            for(int i = 0; i < references.Length; i++)
            {
                // Store the reference
                references[i] = parameters.ReferencedAssemblies[i];
            }

            return references;
        }

        private string GetParameterOption(CompilerParameters parameters, string targetOption)
        {
            // Split by space
            string[] split = parameters.CompilerOptions.Split(' ');

            // Check all options
            foreach(string option in split)
            {
                // Check for matching option
                if(option.IndexOf(targetOption) == 0)
                {
                    // Find character
                    int index = option.IndexOf(':');

                    // Check for value
                    if (index >= 0)
                    {
                        // Return value
                        return option.Remove(0, index);
                    }
                    else
                        return option;
                }
            }

            // No option found
            return null;
        }
    }
}
#endif