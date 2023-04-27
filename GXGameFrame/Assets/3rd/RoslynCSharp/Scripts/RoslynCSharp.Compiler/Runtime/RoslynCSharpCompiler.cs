using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace RoslynCSharp.Compiler
{
    public class RoslynCSharpCompiler
    {
        // Private
        private AppDomain loadAssemblyDomain = null;
        private CSharpParseOptions parseOptions = null;
        private CSharpCompilationOptions compileOptions = null;

        private string outputDirectory = "";
        private string outputName = null;
        private string outputPDBExtension = ".pdb";
        private bool allowUnsafe = false;
        private bool allowOptimize = true;
        private bool allowConcurrentCompile = true;
        private bool deterministic = false;
        private bool generateInMemory = true;
        private bool generateSymbols = false;
        private int warningLevel = 4;
        private LanguageVersion languageVersion = LanguageVersion.Default;
        private OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary;
        private Platform targetPlatform = Platform.AnyCpu;
        private DebugInformationFormat debugSymbolType = DebugInformationFormat.PortablePdb;        

        private ObservableCollection<string> defineSymbols = new ObservableCollection<string>();
        private ObservableCollection<IMetadataReferenceProvider> referenceAssemblies = new ObservableCollection<IMetadataReferenceProvider>();
        private List<MetadataReference> referenceBuilder = new List<MetadataReference>();
        private List<Exception> referenceExceptions = new List<Exception>();
        private List<IAssemblyProcessor> assemblyProcessors = new List<IAssemblyProcessor>();
        private CompilationResult lastCompileResult = null;

        // Public
        public static bool loadCompiledAssemblies = false;

        // Properties
        public string OutputDirectory
        {
            get { return outputDirectory; }
            set { outputDirectory = value; }
        }        

        public string OutputName
        {
            get { return outputName; }
            set { outputName = value; }
        }

        public string OutputPDBExtension
        {
            get { return outputPDBExtension; }
            set { outputPDBExtension = value; }
        }

        public bool AllowUnsafe
        {
            get { return allowUnsafe; }
            set
            {
                allowUnsafe = value;
                UpdateCompilerOptions();
            }
        }

        public bool AllowOptimize
        {
            get { return allowOptimize; }
            set
            {
                allowOptimize = value;
                UpdateCompilerOptions();
            }
        }

        public bool AllowConcurrentCompile
        {
            get { return allowConcurrentCompile; }
            set
            {
                allowConcurrentCompile = value;
                UpdateCompilerOptions();
            }
        }

        public bool Deterministic
        {
            get { return deterministic; }
            set
            {
                deterministic = value;
                UpdateCompilerOptions();
            }
        }

        public bool GenerateInMemory
        {
            get { return generateInMemory; }
            set { generateInMemory = value; }
        }

        public bool GenerateSymbols
        {
            get { return generateSymbols; }
            set { generateSymbols = value; }
        }

        public int WarningLevel
        {
            get { return warningLevel; }
            set
            {
                warningLevel = value;
                UpdateCompilerOptions();
            }
        }

        public LanguageVersion LanguageVersion
        {
            get { return languageVersion; }
            set
            {
                languageVersion = value;
                UpdateParserOptions();
            }
        }

        public OutputKind OutputKind
        {
            get { return outputKind; }
            set
            {
                outputKind = value;
                UpdateCompilerOptions();
            }
        }

        public Platform TargetPlatform
        {
            get { return targetPlatform; }
            set
            {
                targetPlatform = value;
                UpdateCompilerOptions();
            }
        }

        public IList<string> DefineSymbols
        {
            get { return defineSymbols; }
        }

        public IList<IMetadataReferenceProvider> ReferenceAssemblies
        {
            get { return referenceAssemblies; }
        }

        public string DefaultOutputExtension
        {
            get
            {
                switch (outputKind)
                {
                    default:
                        return string.Empty;

                    case OutputKind.ConsoleApplication:
                    case OutputKind.NetModule:
                    case OutputKind.WindowsApplication:
                        return ".exe";

                    case OutputKind.DynamicallyLinkedLibrary:
                            return ".dll";
                }                
            }
        }

        public CompilationResult LastCompileResult
        {
            get { return lastCompileResult; }
        }

        public DebugInformationFormat DebugSymbolType
        {
            get { return debugSymbolType; }
            set { debugSymbolType = value; }
        }        

        // Constructor
        public RoslynCSharpCompiler(bool includeDefaultReferenceAssemblies = true, bool generateInMemory = true, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary, LanguageVersion languageVersion = LanguageVersion.Default, AppDomain loadAssemblyDomain = null)
        {
            this.generateInMemory = generateInMemory;
            this.outputKind = outputKind;
            this.languageVersion = languageVersion;
            this.loadAssemblyDomain = loadAssemblyDomain;

            // Update options
            UpdateParserOptions();
            UpdateCompilerOptions();

            // Add event listeners
            defineSymbols.CollectionChanged += (o, e) => UpdateParserOptions();
        }

        public RoslynCSharpCompiler(string outputName, bool includeDefaultReferenceAssemblies = true, bool generateInMemory = true, OutputKind outputKing = OutputKind.DynamicallyLinkedLibrary, LanguageVersion languageVersion = LanguageVersion.Default, AppDomain loadAssemblyDomain = null)
        {
            this.outputName = outputName;
            this.generateInMemory = generateInMemory;
            this.outputKind = OutputKind;
            this.languageVersion = LanguageVersion;
            this.loadAssemblyDomain = loadAssemblyDomain;

            // Update options
            UpdateParserOptions();
            UpdateCompilerOptions();

            // Add event listeners
            defineSymbols.CollectionChanged += (o, e) => UpdateParserOptions();
        }

        // Methods
        public CompilationResult CompileFromSource(string cSharpSource, IMetadataReferenceProvider[] additionalAssemblyReferences = null)
        {
            // Check for null
            if (cSharpSource == null)
                throw new ArgumentNullException(nameof(cSharpSource));

            // Parse the source
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = ParseSource(cSharpSource, parseOptions);

            // Generate compilation object
            return CompileFromSyntaxTree(syntaxTrees, additionalAssemblyReferences);
        }

        public CompilationResult CompileFromSources(string[] cSharpSources, IMetadataReferenceProvider[] additionalAssemblyReferences = null)
        {
            // Check for null
            for (int i = 0; i < cSharpSources.Length; i++)
                if (cSharpSources[i] == null)
                    throw new ArgumentNullException(string.Format("Source array element '{0}' is null", i));

            // Parse the sources
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = ParseSources(cSharpSources, parseOptions);

            // Generate the compilation object
            return CompileFromSyntaxTree(syntaxTrees, additionalAssemblyReferences);
        }

        public CompilationResult CompileFromFile(string cSharpFile, IMetadataReferenceProvider[] additionalAssemblyReferences = null)
        {
            // Check for null
            if (cSharpFile == null)
                throw new ArgumentNullException(nameof(cSharpFile));

            // Parse the source
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = ParseFile(cSharpFile, parseOptions);

            // Generate compilation object
            return CompileFromSyntaxTree(syntaxTrees, additionalAssemblyReferences);
        }

        public CompilationResult CompileFromFiles(string[] cSharpFiles, IMetadataReferenceProvider[] additionalAssemblyReferences = null)
        {
            // Check for null
            for (int i = 0; i < cSharpFiles.Length; i++)
                if (cSharpFiles[i] == null)
                    throw new ArgumentNullException(string.Format("Source array element '{0}' is null", i));

            // Parse the sources
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = ParseFiles(cSharpFiles, parseOptions);

            // Generate the compilation object
            return CompileFromSyntaxTree(syntaxTrees, additionalAssemblyReferences);
        }

        public CompilationResult CompileFromSyntaxTree(Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees, IMetadataReferenceProvider[] additionalAsemblyReferences)
        {
            string outputPath = null;

            // Generate assembly name
            string assemblyName = outputName;
            

            // Assign a guid name
            if (string.IsNullOrEmpty(assemblyName) == true)
                assemblyName = Guid.NewGuid().ToString();

            // Use the name only for internal assembly name
            string assemblyNameOnly = Path.ChangeExtension(assemblyName, null);

            // Check for extension
            if (Path.HasExtension(assemblyName) == false)
                assemblyName += DefaultOutputExtension;

            // Generate output name
            if(string.IsNullOrEmpty(outputDirectory) == true)
            {
                outputPath = assemblyName;
            }
            else
            {
                // Build output path
                outputPath = Path.Combine(outputDirectory, assemblyName);
            }

            // Build references
            Exception referenceException = null;
            MetadataReference[] references = UpdateReferences(additionalAsemblyReferences, out referenceException);

            // Check for referencing error
            if (referenceException != null)
                throw referenceException;


            // Create the compilation object
            Compilation compileObject = CreateCompilationObject(
                assemblyNameOnly, 
                references, 
                syntaxTrees, 
                compileOptions);
            
            // Create output folder if necessary
            DirectoryInfo parent = Directory.GetParent(outputPath);

            if (generateInMemory == false && parent.Exists == false)
                parent.Create();

            // Create output stream
            Stream outputStream = (generateInMemory == true)
                ? (Stream)new MemoryStream()
                : (Stream)new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            Stream outputPDBStream = null;

            if (generateSymbols == true)
            {
                string outputPDBPath = Path.ChangeExtension(outputPath, outputPDBExtension);

                outputPDBStream = (generateInMemory == true)
                    ? (Stream)new MemoryStream()
                    : (Stream)new FileStream(outputPDBPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            

            // Dispose of stream correctly
            using (outputStream)
            {
                using (outputPDBStream)
                {
                    // Emit the compile object
                    CompilationResult result = EmitCompilationObject(compileObject, outputStream, outputPDBStream, loadCompiledAssemblies, loadAssemblyDomain, debugSymbolType);

                    // Run processors
                    foreach(IAssemblyProcessor processor in assemblyProcessors)
                    {
                        try
                        {
                            processor.OnProcessAssembly(result.AssemblyOutput);
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                        }
                    }

                    // Store the result
                    lastCompileResult = result;

                    return result;
                }
            }
        }

        public void AddAssemblyProcessor(IAssemblyProcessor processor)
        {
            if(processor != null && assemblyProcessors.Contains(processor) == false)
                assemblyProcessors.Add(processor);
        }

        public void RemoveAssemblyProcessor(IAssemblyProcessor processor)
        {
            if (assemblyProcessors.Contains(processor) == true)
                assemblyProcessors.Remove(processor);
        }

        private void UpdateParserOptions()
        {
            // Update options
            parseOptions = new CSharpParseOptions(
                languageVersion,
                DocumentationMode.Parse,
                SourceCodeKind.Regular,
                defineSymbols);
        }

        private void UpdateCompilerOptions()
        {
            OptimizationLevel optimizeLevel = (allowOptimize == true)
                ? OptimizationLevel.Release
                : OptimizationLevel.Debug;

            // Create the output options
            compileOptions = new CSharpCompilationOptions(
                outputKind,
                false,                                                          // Suppressed diagnostics
                null,                                                           // Module name
                null,                                                           // Main type name
                null,                                                           // Script class name
                null,                                                           // Using
                optimizeLevel,                                                  // Optimize level
                false,                                                          // Check overflow
                allowUnsafe,                                                    // Allow unsafe
                null,                                                           // Krypto key container
                null,                                                           // Krypto key file
                default(System.Collections.Immutable.ImmutableArray<byte>),     // Krypto public key
                null,                                                           // Delay sign
                targetPlatform,                                                 // Platform
                ReportDiagnostic.Default,                                       // Diagnostic option
                warningLevel,                                                   // Warning level
                null,                                                           // Specific diagnostic options
                allowConcurrentCompile,                                         // Allow concurrent compile
                deterministic,                                                          // Deterministic
                null,                                                           // XML reference resolver
                null,                                                           // Source reference resolver
                null,                                                           // Metadata reference resolver
                null,                                                           // Assembly identity comparer
                null,                                                           // Strong name provider
                false,                                                          // Public sign                                 
                MetadataImportOptions.Public);                                  // Meta import options

        }

        private MetadataReference[] UpdateReferences(IMetadataReferenceProvider[] additionalReferences, out Exception error)
        {
            error = null;

            // Genrate references
            referenceBuilder.Clear();
            referenceExceptions.Clear();

            //UpdateReferencesFromProviderSource(defaultReferenceAssemblies, referenceBuilder, referenceExceptions);

            // Add standard references
            UpdateReferencesFromProviderSource(referenceAssemblies, referenceBuilder, referenceExceptions);

            // Add additional references
            if(additionalReferences != null)
                UpdateReferencesFromProviderSource(additionalReferences, referenceBuilder, referenceExceptions);

            // Generate an exception object if there are errors
            if (referenceExceptions.Count > 0)
                error = new AssemblyReferenceException(referenceExceptions);

            // Get final reference collection
            return referenceBuilder.ToArray();
        }            

        private void UpdateReferencesFromProviderSource(IEnumerable<IMetadataReferenceProvider> providerSource, IList<MetadataReference> references, IList<Exception> exceptions)
        {
            // Add standard references
            foreach (IMetadataReferenceProvider referenceProvider in providerSource)
            {
                if (referenceProvider == null)
                    continue;

                MetadataReference resolved;
                Exception exception;

                if (referenceProvider.TryResolveReference(out resolved, out exception) == true)
                {
                    references.Add(resolved);
                }
                else
                {
                    exceptions.Add(exception);
                }
            }
        }

        public static Microsoft.CodeAnalysis.SyntaxTree[] ParseSource(string cSharpSource, CSharpParseOptions parseOptions = null)
        {
            // Call through
            return ParseSources(new string[] { cSharpSource }, parseOptions);
        }

        public static Microsoft.CodeAnalysis.SyntaxTree[] ParseSources(string[] cSharpSources, CSharpParseOptions parseOptions = null)
        {
            // Check for no input
            if (cSharpSources.Length == 0)
                return new Microsoft.CodeAnalysis.SyntaxTree[0];

            // Allocate return result
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = new Microsoft.CodeAnalysis.SyntaxTree[cSharpSources.Length];

            // Parse all
            for(int i = 0; i < syntaxTrees.Length; i++)
            {
                // Parse all input sources
                syntaxTrees[i] = CSharpSyntaxTree.ParseText(cSharpSources[i], parseOptions, "", System.Text.Encoding.Default);
            }

            return syntaxTrees;
        }

        public static Microsoft.CodeAnalysis.SyntaxTree[] ParseFile(string cSharpFile, CSharpParseOptions parseOptions = null)
        {
            // Call through
            return ParseFiles(new string[] { cSharpFile }, parseOptions);
        }

        public static Microsoft.CodeAnalysis.SyntaxTree[] ParseFiles(string[] cSharpFiles, CSharpParseOptions parseOptions = null)
        {
            // Check for no input
            if (cSharpFiles.Length == 0)
                return new Microsoft.CodeAnalysis.SyntaxTree[0];

            // Allocate return result
            Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees = new Microsoft.CodeAnalysis.SyntaxTree[cSharpFiles.Length];

            // Parse all
            for(int i = 0; i < syntaxTrees.Length; i++)
            {
                // Check for valid file
                if (File.Exists(cSharpFiles[i]) == false)
                    throw new IOException(string.Format("The specified C# source file '{0}' does not exist", cSharpFiles[i]));

                // Open the file stream for reading
                using (Stream sourceStream = File.OpenRead(cSharpFiles[i]))
                {
                    // Create the text reader
                    using (TextReader sourceReader = new StreamReader(sourceStream))
                    {
                        // Parse the source
                        syntaxTrees[i] = CSharpSyntaxTree.ParseText(SourceText.From(sourceReader, (int)sourceStream.Length, System.Text.Encoding.Default), parseOptions, cSharpFiles[i]);
                    }
                }
            }

            return syntaxTrees;
        }

        public static Compilation CreateCompilationObject(string assemblyName, string[] references, Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees, CSharpCompilationOptions options = null)
        {
            // Require valid name
            if (string.IsNullOrEmpty(assemblyName) == true)
                throw new ArgumentException("A valid assembly name must be provided");

            // Require valid syntax tree
            if (syntaxTrees == null || syntaxTrees.Length == 0)
                throw new ArgumentException("You must provide at least one syntax tree for the creation of a compilation object");

            // Validate references
            if (references == null)
                references = new string[0];

            // Create references
            MetadataReference[] metaReferences = new MetadataReference[references.Length];

            for(int i = 0; i < metaReferences.Length; i++)
            {
                // Create the file reference
                metaReferences[i] = MetadataReference.CreateFromFile(references[i]);
            }

            // Call through to create object
            return CreateCompilationObject(assemblyName, metaReferences, syntaxTrees, options);
        }

        public static Compilation CreateCompilationObject(string assemblyName, MetadataReference[] references, Microsoft.CodeAnalysis.SyntaxTree[] syntaxTrees, CSharpCompilationOptions options = null)
        {
            // Require valid name
            if (string.IsNullOrEmpty(assemblyName) == true)
                throw new ArgumentException("A valid assembly name must be provided");

            // Require valid syntax tree
            if (syntaxTrees == null || syntaxTrees.Length == 0)
                throw new ArgumentException("You must provide at least one syntax tree for the creation of a compilation object");

            // Check for no references
            if (references == null)
                references = new MetadataReference[0];

            // Create the object
            return CSharpCompilation.Create(assemblyName, syntaxTrees, references, options);
        }

        public static CompilationResult EmitCompilationObject(Compilation emitObject, Stream targetStream, Stream targetPDBStream = null, bool loadCompiledAssembly = true, AppDomain loadAssemblyDomain = null, DebugInformationFormat debugFormat = DebugInformationFormat.PortablePdb)
        {
            // Check for valid arguments
            if (emitObject == null)
                throw new ArgumentNullException(nameof(emitObject));

            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            // Get the stream position
            long streamStart = targetStream.Position;
            long pdbStreamStart = (targetPDBStream == null) ? 0 : targetPDBStream.Position;

            EmitOptions emitOptions = null;

            // Create emit options
            if(targetPDBStream != null)
                emitOptions = new EmitOptions(false, debugFormat);

            // Emit to stream
            EmitResult emitResult = emitObject.Emit(targetStream, targetPDBStream, null, null, null, emitOptions);

            // Create complation result
            CompilationResult result = new CompilationResult(emitResult.Success, emitResult.Diagnostics);

            // Setup output path
            if (targetStream is FileStream)
            {
                FileStream fStream = targetStream as FileStream;
                
                // Get the file name
                result.OutputFile = fStream.Name;

                // Flush stream because we will be using it
                fStream.Flush();
            }

            // Check for assembly load
            if(result.Success == true)
            {
                // Load symbols
                if(targetPDBStream != null)
                {
                    // Setup output path
                    if (targetPDBStream is FileStream)
                        result.OutputPDBFile = (targetPDBStream as FileStream).Name;

                    // Check for seek support
                    if (targetPDBStream.CanSeek == true)
                    {
                        // Get the stream end position
                        long pdbStreamEnd = targetPDBStream.Position;

                        // Move to begining of stream
                        targetPDBStream.Seek(streamStart, SeekOrigin.Begin);


                        // Create a buffer to hold the assembly data
                        byte[] pdbBytes = new byte[pdbStreamEnd - streamStart];

                        // Read from stream
                        targetPDBStream.Read(pdbBytes, 0, pdbBytes.Length);

                        // Store output bytes
                        result.OutputPDBImage = pdbBytes;
                    }

                    // Close the stream
                    targetPDBStream.Dispose();
                }


                // The assembly image property should always be assigned
                LoadAssemblyImageFromStream(targetStream, streamStart, result);

                // Close the stream
                targetStream.Dispose();


                // We should load the assembly into the specified domain
                if (loadCompiledAssembly == true)
                {
                    // Get the load domain
                    AppDomain loadDomain = AppDomain.CurrentDomain;

                    // Use the specified domain for loading
                    if (loadAssemblyDomain != null)
                        loadDomain = loadAssemblyDomain;

                    // Load into runtime domain
                    if (string.IsNullOrEmpty(result.OutputFile) == false)
                    {
                        // Create the assembly name
                        AssemblyName asmName = new AssemblyName();
                        asmName.CodeBase = result.OutputFile;

                        // Load from file if the file was generated - This means that the assembly base path will be setup
                        result.OutputAssembly = loadDomain.Load(asmName);
                    }
                    else
                    {
                        // Load from memory - do not use file system
                        result.OutputAssembly = loadDomain.Load(result.OutputAssemblyImage);
                    }
                } // end load compiled assembly                
            }

            return result;
        }

        private static void LoadAssemblyImageFromStream(Stream targetStream, long streamStart, CompilationResult result)
        {
            // Check for seek support
            if (targetStream.CanSeek == false)
                throw new IOException("Unable to load assembly definition because the specified output stream is not seekable");


            // Get the stream end position
            long streamEnd = targetStream.Position;

            // Move to begining of stream
            targetStream.Seek(streamStart, SeekOrigin.Begin);


            // Create a buffer to hold the assembly data
            byte[] assemblyBytes = new byte[streamEnd - streamStart];

            // Read from stream
            targetStream.Read(assemblyBytes, 0, assemblyBytes.Length);

            // Store assembly image
            result.OutputAssemblyImage = assemblyBytes;
        }
    }
}
