using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Security;
using System.Collections.Generic;

using RoslynCSharp.Compiler;
using Trivial.CodeSecurity;
using RoslynCSharp.Implementation;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine.SceneManagement;
using RoslynCSharp.Project;
using System.Linq;

namespace RoslynCSharp
{
    /// <summary>
    /// The security mode used when loading and compiling assemblies.
    /// </summary>
    public enum ScriptSecurityMode
    {
        /// <summary>
        /// Use the security mode from the Roslyn C# settings window.
        /// </summary>
        UseSettings,
        /// <summary>
        /// Security verification will be skipped.
        /// </summary>
        EnsureLoad,
        /// <summary>
        /// Secutiry verification will be used.
        /// </summary>
        EnsureSecurity,
    }

    /// <summary>
    /// A <see cref="ScriptDomain"/> acts as a container for all code that is loaded dynamically at runtime.
    /// The main responsiblility of the domin is to separate pre-compiled game code from runtime-loaded code. 
    /// As a result, you will only be able to access types from the domain that were loaded at runtime.
    /// Any pre-compiled game code will be ignored.
    /// Any assemblies or scripts that are loaded into the domain at runtime will remain until the application exits so you should be careful to avoid loading too many assemblies.
    /// You would typically load user code at statup in a 'Load' method which would then exist and execute until the game exits.
    /// Multiple domain instances may be created but you should note that all runtime code will be loaded into the current application domain. The <see cref="ScriptDomain"/> simply masks the types that are visible.
    /// </summary>
    public class ScriptDomain : IDisposable
    {
        // Private
        private static List<ScriptDomain> activeDomains = new List<ScriptDomain>();
        private static List<ScriptAssembly> matchedAssemblies = new List<ScriptAssembly>();
        private static ScriptDomain active = null;

        private string name = null;
        private AppDomain sandbox = null;
        private ScriptExecution execution = new ScriptExecution();
        private List<ScriptAssembly> loadedAssemblies = new List<ScriptAssembly>();
        private RoslynCSharpCompiler sharedCompiler = null;
        private CodeSecurityReport securityResult = null;
        private CompilationResult compileResult = null;

        // Properties
        /// <summary>
        /// Get the active <see cref="ScriptDomain"/>.
        /// By default the last created domain will be active but you can also make a specific domain active using <see cref="MakeDomainActive(ScriptDomain)"/>.
        /// </summary>
        public static ScriptDomain Active
        {
            get { return active; }
        }

        /// <summary>
        /// Get the name of the domain.
        /// </summary>
        public string Name
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                return name;
            }
        }

        /// <summary>
        /// Get the app domain that this <see cref="ScriptDomain"/> manages.
        /// </summary>
        public AppDomain SandboxDomain
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                return sandbox;
            }
        }

        /// <summary>
        /// Get the <see cref="ScriptExecution"/> for this domain where all executing instances are accessible.
        /// </summary>
        public ScriptExecution Execution
        {
            get { return execution; }
        }

        /// <summary>
        /// Get all assemblies loaded into this domain.
        /// </summary>
        public ScriptAssembly[] Assemblies
        {
            get
            {
                // Check for disposed
                CheckDisposed();

                lock (this)
                {
                    return loadedAssemblies.ToArray();
                }
            }
        }

        /// <summary>
        /// Get all assemblies loaded into this domain that have been compiled at runtime using the Roslyn runtime compiler service.
        /// </summary>
        public ScriptAssembly[] CompiledAssemblies
        {
            get
            {
                // Check for disposed
                CheckDisposed();

                // Use shared list for result
                matchedAssemblies.Clear();

                lock (this)
                {
                    // Check all assemblies
                    foreach (ScriptAssembly assembly in loadedAssemblies)
                    {
                        // Add to result list
                        if (assembly.IsRuntimeCompiled == true)
                            matchedAssemblies.Add(assembly);
                    }
                }

                // Get as array
                return matchedAssemblies.ToArray();
            }
        }

        /// <summary>
        /// Enumerate all assemblies loaded into this domain.
        /// </summary>
        public IEnumerable<ScriptAssembly> EnumerateAssemblies
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                lock (this)
                {
                    return loadedAssemblies;
                }
            }
        }

        /// <summary>
        /// Enumerate all assemblies loaded into this domain that have been compiled at runtime using the Roslyn runtime compiler service.
        /// </summary>
        public IEnumerable<ScriptAssembly> EnumerateCompiledAssemblies
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                lock (this)
                {
                    foreach (ScriptAssembly assembly in loadedAssemblies)
                    {
                        if (assembly.IsRuntimeCompiled == true)
                            yield return assembly;
                    }
                }
            }
        }

        /// <summary>
        /// Get the Roslyn runtime compiler service associated with this domain.
        /// This value will be null if the compiler service has not been initialized.
        /// </summary>
        public RoslynCSharpCompiler RoslynCompilerService
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                return sharedCompiler;
            }
        }

        /// <summary>
        /// Get the last compilation report as a result of compiling an assembly.
        /// </summary>
        public CompilationResult CompileResult
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                return compileResult;
            }
        }

        /// <summary>
        /// Get the last security report as a result of loading or compiling an assembly.
        /// </summary>
        public CodeSecurityReport SecurityResult
        {
            get { return securityResult; }
        }

        /// <summary>
        /// Returns true if the Roslyn compiler service is initialized and ready to recieve compile requests.
        /// </summary>
        public bool IsCompilerServiceInitialized
        {
            get
            {
                // Check for disposed
                CheckDisposed();
                return sharedCompiler != null;
            }
        }

        /// <summary>
        /// Has this domain been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return sandbox == null; }
        }

        // Constructor
        private ScriptDomain(string name, AppDomain sandboxDomain = null)
        {
            // Store the name
            this.name = name;

            // Store the domain
            sandbox = sandboxDomain;

            // Revert to current domain
            if (sandboxDomain == null)
            {
                // Create the app domain
                this.sandbox = AppDomain.CurrentDomain;
            }

            // Add active domain
            activeDomains.Add(this);
        }

        // Methods
        #region AssemblyLoad
        /// <summary>
        /// Attempts to load a managed assembly from the specified resources path into the sandbox app domain.
        /// The target asset must be a <see cref="TextAsset"/> in order to be loaded successfully. 
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="resourcePath">The file name of path relative to the 'Resources' folder without the file extension</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <exception cref="SecurityException">The assembly breaches the imposed security restrictions</exception>
        public ScriptAssembly LoadAssemblyFromResources(string resourcePath, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Try to load resource
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);

            // Check for error
            if (asset == null)
                throw new DllNotFoundException(string.Format("Failed to load dll from resources path '{0}'", resourcePath));
            
            // Get the asset bytes and call through
            return LoadAssembly(asset.bytes, securityMode);
        }

        /// <summary>
        /// Attempts to load the specified managed assembly into the sandbox app domain.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="fullPath">The full path to the .dll file</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        public ScriptAssembly LoadAssembly(string fullPath, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Create an assembly name object
            AssemblyName name = AssemblyName.GetAssemblyName(fullPath);

            // Load the assembly
            Assembly assembly = sandbox.Load(name);

            // Create script assembly
            return RegisterAssemblyPath(assembly, securityMode, fullPath);
        }

        /// <summary>
        /// Attempts to load the specified managed assembly into the sandbox app domain along with pdb debug symbols.
        /// Use this method if you want to be able to debug the specified assembly.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="assemblyPath">The full path to the .dll file</param>
        /// <param name="symbolsPath">The full path to the .pdb symbols file</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        public ScriptAssembly LoadAssemblyWithSymbols(string assemblyPath, string symbolsPath, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Load assembly and symbols
            byte[] asmBytes = File.ReadAllBytes(assemblyPath);
            byte[] symbolBytes = File.ReadAllBytes(symbolsPath);

            // Load the assembly
            Assembly assembly = sandbox.Load(asmBytes, symbolBytes);

            // Create script assembly
            return RegisterAssemblyPath(assembly, securityMode, assemblyPath, symbolsPath);
        }

        /// <summary>
        /// Attempts to load the specified managed assembly into the sandbox app domain.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="name">The <see cref="AssemblyName"/> representing the assembly to load</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        public ScriptAssembly LoadAssembly(AssemblyName name, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Load the assembly
            Assembly assembly = sandbox.Load(name);

            // Create script assembly
            return RegisterAssembly(assembly, securityMode);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified raw bytes.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="assemblyBytes">The raw data representing the file structure of the managed assembly, The result of <see cref="File.ReadAllBytes(string)"/> for example.</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        public ScriptAssembly LoadAssembly(byte[] assemblyBytes, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Load the assembly
            Assembly assembly = sandbox.Load(assemblyBytes);

            // Create script assembly
            return RegisterAssemblyImage(assembly, securityMode, assemblyBytes);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified raw bytes along with debug symbols.
        /// Use this method if you want to be able to debug the specified assembly.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="assemblyBytes">The raw data representing the file structure of the managed assembly, The result of <see cref="File.ReadAllBytes(string)"/> for example.</param>
        /// <param name="symbolBytes">The raw data representing the pdb debug symbols for the managed assembly</param>
        /// <returns>An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if an error occurs</returns>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        public ScriptAssembly LoadAssemblyWithSymbols(byte[] assemblyBytes, byte[] symbolBytes, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            // Load the assembly
            Assembly assembly = sandbox.Load(assemblyBytes, symbolBytes);

            // Create script assembly
            return RegisterAssemblyImage(assembly, securityMode, assemblyBytes, symbolBytes);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified filepath asynchronously.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncLoadOperation.LoadDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="fullPath">The filepath to the managed assembly</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An awaitable async operation object that contains state information for the load request</returns>
        public AsyncLoadOperation LoadAssemblyAsync(string fullPath, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            return new AsyncLoadOperation(this, fullPath, securityMode);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified filepath asynchronously.
        /// Use this method if you want to be able to debug the specified assembly.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncLoadOperation.LoadDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="assemblyPath">The filepath to the managed assembly</param>
        /// <param name="symbolsPath">The filepath for the pdb debug symbols</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An awaitable async operation object that contains state information for the load request</returns>
        public AsyncLoadOperation LoadAssemblyWithSymbolsAsync(string assemblyPath, string symbolsPath, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            return new AsyncLoadOperation(this, assemblyPath, securityMode, symbolsPath);
        }

        /// <summary>
        /// Attempts to load a managed assembly with the specified name asynchronously.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncLoadOperation.LoadDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="name">The name of the assembly to load</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An awaitable async operation object that contains state information for the load request</returns>
        public AsyncLoadOperation LoadAssemblyAsync(AssemblyName name, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            return new AsyncLoadOperation(this, name, securityMode);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified raw bytes asynchronously.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncLoadOperation.LoadDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="assemblyBytes">A byte array containing the managed assembly imagae data</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An awaitable async operation object that contains state information for the load request</returns>
        public AsyncLoadOperation LoadAssemblyAsync(byte[] assemblyBytes, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            return new AsyncLoadOperation(this, assemblyBytes, securityMode);
        }

        /// <summary>
        /// Attempts to load a managed assembly from the specified raw bytes asynchronously.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncLoadOperation.LoadDomain"/> to get the output from the code validation request.
        /// Use this method if you want to be able to debug the specified assembly.
        /// </summary>
        /// <param name="assemblyBytes">A byte array containing the managed assembly imagae data</param>
        /// <param name="symbolBytes">A byte array containing the raw assembly pdb debug symbol image data</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>An awaitable async operation object that contains state information for the load request</returns>
        public AsyncLoadOperation LoadAssemblyWithSymbolsAsync(byte[] assemblyBytes, byte[] symbolBytes, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Check for disposed
            CheckDisposed();

            return new AsyncLoadOperation(this, assemblyBytes, securityMode, symbolBytes);
        }

        /// <summary>
        /// Attempts to load the managed assembly at the specified location.
        /// Any exceptions throw while loading will be caught.
        /// </summary>
        /// <param name="fullPath">The full path to the .dll file</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occurred</returns>
        public bool TryLoadAssembly(string fullPath, out ScriptAssembly result, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Even though this method is safe we cannot allow access to a disposed domain
            CheckDisposed();

            try
            {
                // Call through
                result = LoadAssembly(fullPath, securityMode);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to load a managed assembly with the specified name.
        /// Any exceptions thrown while loading will be caught.
        /// </summary>
        /// <param name="name">The <see cref="AssemblyName"/> of the assembly to load</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occurred</returns>
        public bool TryLoadAssembly(AssemblyName name, out ScriptAssembly result, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Even though this method is safe we cannot allow access to a disposed domain
            CheckDisposed();

            try
            {
                // Call through
                result = LoadAssembly(name, securityMode);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to load a managed assembly from the raw assembly data.
        /// Any exceptions thrown while loading will be caught.
        /// </summary>
        /// <param name="data">The raw data representing the file structure of the managed assembly, The result of <see cref="File.ReadAllBytes(string)"/> for example.</param>
        /// <param name="result">An instance of <see cref="ScriptAssembly"/> representing the loaded assembly or null if the load failed</param>
        /// <param name="securityMode">The security mode which determines whether code validation will run</param>
        /// <returns>True if the assembly was loaded successfully or false if an error occured</returns>
        public bool TryLoadAssembly(byte[] data, out ScriptAssembly result, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings)
        {
            // Even though this method is safe we cannot allow access to a disposed domain
            CheckDisposed();

            try
            {
                // Call through
                result = LoadAssembly(data, securityMode);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
        #endregion

        #region AssemblyCompile
        /// <summary>
        /// Compile and load the speciied C# source code string.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// This does the same as <see cref="CompileAndLoadSource(string, ScriptSecurityMode)"/> but returns the main type of the <see cref="ScriptAssembly"/> for convenience.        /// 
        /// </summary>
        /// <param name="cSharpSource">The string containing C# source code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The main type of the compiled assembly or null if the compile failed, security validation failed or there was main type</returns>
        public ScriptType CompileAndLoadMainSource(string cSharpSource, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Send compile request
            ScriptAssembly assembly = CompileAndLoadSource(cSharpSource, securityMode, additionalReferenceAssemblies);

            // Try to get main type
            if (assembly != null && assembly.MainType != null)
                return assembly.MainType;

            return null;
        }

        /// <summary>
        /// Compile and load the specified C# source file.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// This does the same as <see cref="CompileAndLoadFileAsync(string, ScriptSecurityMode)"/> but returns the main type of the <see cref="ScriptAssembly"/> for convenience.
        /// </summary>
        /// <param name="cSharpFile">The filepath to a file containing C# code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The main type of the compiled assembly or null if the compile failed, security validation failed or there was no main type</returns>
        public ScriptType CompileAndLoadMainFile(string cSharpFile, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Send compile request
            ScriptAssembly assembly = CompileAndLoadFile(cSharpFile, securityMode, additionalReferenceAssemblies);

            // Try to get main type
            if (assembly != null && assembly.MainType != null)
                return assembly.MainType;

            return null;
        }

        /// <summary>
        /// Compile and load the specified C# syntax tree.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// This does the same as <see cref="CompileAndLoadSyntaxTree(CSharpSyntaxTree, ScriptSecurityMode, IMetadataReferenceProvider[])"/> but returns the main type of the <see cref="ScriptAssembly"/> for convenience.
        /// </summary>
        /// <param name="cSharpFile">The C# syntax tree to compile</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The main type of the compiled assembly or null if the compile failed, security validation failed or there was no main type</returns>
        public ScriptType CompileAndLoadMainSyntaxTree(CSharpSyntaxTree syntaxTree, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Send compile request
            ScriptAssembly assembly = CompileAndLoadSyntaxTree(syntaxTree, securityMode, additionalReferenceAssemblies);

            // Try to get main type
            if (assembly != null && assembly.MainType != null)
                return assembly.MainType;

            return null;
        }

        /// <summary>
        /// Compile and load the specified C# source code string.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpSource">The string containing C# source code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadSource(string cSharpSource, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock(this)
            {
                // Compile from source
                compileResult = sharedCompiler.CompileFromSource(cSharpSource, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# source file.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpFile">The filepath to a file containing C# code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadFile(string cSharpFile, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {                
                // Compile from file
                compileResult = sharedCompiler.CompileFromFile(cSharpFile, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load all C# source files in a given directory.
        /// Using the searchPattern and searchOption parameters, you can control whether nested directories are scanned, as well as filter by file name and extension.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="directoryPath">The directory path to scan</param>
        /// <param name="searchPattern">The file matching filter used to find source files inside the given directory. The default option '*.cs' should be fine in most cases for finding .cs source files</param>
        /// <param name="searchOption">Specify whether top level or nested directories should be scanned</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <param name="additionalReferenceAssemblies"></param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadDirectory(string directoryPath, string searchPattern = "*.cs", SearchOption searchOption = SearchOption.TopDirectoryOnly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            // Get file paths
            string[] cSharpFilePaths = Directory.GetFiles(directoryPath, searchPattern, searchOption);

            lock(this)
            {
                // Compile from file
                compileResult = sharedCompiler.CompileFromFiles(cSharpFilePaths, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# syntax tree.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpFile">The C# syntax tree to compile</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadSyntaxTree(CSharpSyntaxTree syntaxTree, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {
                // Compile from file
                compileResult = sharedCompiler.CompileFromSyntaxTree(new Microsoft.CodeAnalysis.SyntaxTree[] { syntaxTree }, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# source code strings.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpSources">An array of C# source code strings</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadSources(string[] cSharpSources, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {
                // Compile from source
                compileResult = sharedCompiler.CompileFromSources(cSharpSources, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# source files.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpFiles">An array of filepaths to C# source files</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compil or security verification failed</returns>
        public ScriptAssembly CompileAndLoadFiles(string[] cSharpFiles, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {
                // Compile from file
                compileResult = sharedCompiler.CompileFromFiles(cSharpFiles, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# syntax tree.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="syntaxTrees">An array of CSharp syntax trees</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadSyntaxTrees(CSharpSyntaxTree[] syntaxTrees, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {
                // Compile from file
                compileResult = sharedCompiler.CompileFromSyntaxTree(syntaxTrees, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# project file (.csproj).
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpProjectFile">A file path to the .csproj file to compile</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadCSharpProject(string cSharpProjectFile, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock(this)
            {
                // Compile project
                CompileFromCSharpProject(cSharpProjectFile, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# project.
        /// Use <see cref="CompileResult"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpProject">The <see cref="CSharpProject"/> to compile</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>The compiled and loaded assembly or null if the compile or security verification failed</returns>
        public ScriptAssembly CompileAndLoadCSharpProject(CSharpProject cSharpProject, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            lock (this)
            {
                // Compile project
                CompileFromCSharpProject(cSharpProject, additionalReferenceAssemblies);

                // Log to console
                LogCompilerOutputToConsole();

                // Load assembly
                Assembly asm = compileResult.LoadCompiledAssembly(sandbox);

                // Security check
                return RegisterAssembly<ScriptCompiledAssemblyImpl>(asm, securityMode, CompileResult);
            }
        }

        /// <summary>
        /// Compile and load the specified C# source string asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpSource">The string containing C# source code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state information about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadSourceAsync(string cSharpSource, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileSource, securityMode, new string[] { cSharpSource }, null, additionalReferenceAssemblies);
        }

        /// <summary>
        /// Compile and load the specified C# source file asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpFile">The filepath to the C# source file</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state information about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadFileAsync(string cSharpFile, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileFile, securityMode, new string[] { cSharpFile }, null, additionalReferenceAssemblies);
        }

        /// <summary>
        /// Compile and load the specified C# syntax tree asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="syntaxTree">The C# syntax tree to compilee</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state information about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadSyntaxTreeAsync(CSharpSyntaxTree syntaxTree, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileSyntaxTree, securityMode, null, new CSharpSyntaxTree[] { syntaxTree }, additionalReferenceAssemblies);
        }

        /// <summary>
        /// Compile and load the specified C# source strings asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpSources">An array of strings containgin C# source code</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state infomration about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadSourcesAsync(string[] cSharpSources, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileSource, securityMode, cSharpSources, null, additionalReferenceAssemblies);
        }

        /// <summary>
        /// Compile and load the specified C# source files asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="cSharpFiles">An array of filepaths to C# source files</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state information about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadFilesAsync(string[] cSharpFiles, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileFile, securityMode, cSharpFiles, null, additionalReferenceAssemblies);
        }

        /// <summary>
        /// Compile and load the specified C# syntax trees asynchronously.
        /// Use <see cref="CompileResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the compile request.
        /// Use <see cref="SecurityResult"/> of <see cref="AsyncCompileOperation.CompileDomain"/> to get the output from the code validation request.
        /// </summary>
        /// <param name="syntaxTrees">An array of C# syntax trees to compile</param>
        /// <param name="securityMode">The code validation used to verify the code</param>
        /// <returns>An awaitable async operation object containing state information about the compile request</returns>
        public AsyncCompileOperation CompileAndLoadSyntaxTreesAsync(CSharpSyntaxTree[] syntaxTrees, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Make sure the compiler is initialized and the domain is valid
            CheckDisposed();
            CheckCompiler();

            return new AsyncCompileOperation(this, AsyncCompileOperation.CompileType.CompileSyntaxTree, securityMode, null, syntaxTrees, additionalReferenceAssemblies);
        }
        #endregion

        public void CompileFromSource(string cSharpSource, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromSource(cSharpSource, additionalReferenceAssemblies);
        }

        public void CompileFromSources(string[] cSharpSources, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromSources(cSharpSources, additionalReferenceAssemblies);
        }

        public void CompileFromFile(string cSharpFile, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromFile(cSharpFile, additionalReferenceAssemblies);
        }

        public void CompileFromFiles(string[] cSharpFiles, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromFiles(cSharpFiles, additionalReferenceAssemblies);
        }

        public void CompileFromSyntaxTree(CSharpSyntaxTree syntaxTree, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromSyntaxTree(new Microsoft.CodeAnalysis.SyntaxTree[] { syntaxTree }, additionalReferenceAssemblies);
        }

        public void CompileFromSyntaxTrees(CSharpSyntaxTree[] syntaxTrees, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            compileResult = sharedCompiler.CompileFromSyntaxTree(syntaxTrees, additionalReferenceAssemblies);
        }

        public void CompileFromCSharpProject(string cSharpProjectFile, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Try to parse project file
            CSharpProjectFile projectFile = CSharpProjectFile.ParseFile(cSharpProjectFile);

            // Send compile request 
            CompileFromCSharpProject(projectFile, additionalReferenceAssemblies);
        }

        public void CompileFromCSharpProject(CSharpProject project, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            // Send the compile request
            compileResult = sharedCompiler.CompileFromFiles(
                project.Sources.ToArray(),
                project.GetMetadataReferences());
        }

        #region Broadcast
        /// <summary>
        /// Attempt to invoke a static method for all types loaded into this domain.
        /// Only static method will be considered by this broadcast. Use <see cref="Broadcast(Scene, string)"/> or <see cref="BroadcastInstance(Type, string)"/> to broadcast an instance method.
        /// </summary>
        /// <param name="methodName">The nake of the static method to invoke</param>
        public void StaticBroadcast(string methodName)
        {
            // Process all loaded assemblies
            foreach(ScriptAssembly assembly in EnumerateAssemblies)
            {
                // Process all types
                foreach (ScriptType type in assembly.EnumerateAllTypes())
                {
                    // Try to invoke method
                    type.SafeCallStatic(methodName);
                }
            }
        }

        /// <summary>
        /// Attempt to invoke a static method for all types loaded into this domain.
        /// Only static method will be considered by this broadcast. Use <see cref="Broadcast(Scene, string, object[])"/> or <see cref="BroadcastInstance(Type, string, object[])"/> to broadcast an instance method.
        /// </summary>
        /// <param name="methodName">The nake of the static method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching static methods must have a compatible argument list otherwise the broadcast will fail for that particular call</param>
        public void StaticBroadcast(string methodName, params object[] args)
        {
            // Process all loaded assemblies
            foreach (ScriptAssembly assembly in EnumerateAssemblies)
            {
                // Process all types
                foreach (ScriptType type in assembly.EnumerateAllTypes())
                {
                    // Try to invoke method
                    type.SafeCallStatic(methodName, args);
                }
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono bheaivour instances loaded into this domain that exist in the active loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string)"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        public void BroadcastActiveScene(string methodName)
        {
            Broadcast(SceneManager.GetActiveScene(), methodName);
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono bheaivour instances loaded into this domain that exist in the active loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void BroadcastActiveScene(string methodName, params object[] args)
        {
            Broadcast(SceneManager.GetActiveScene(), methodName, args);
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the active loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string)"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        public void BroadcastActiveScene(Type baseType, string methodName)
        {
            Broadcast(SceneManager.GetActiveScene(), baseType, methodName);
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the active loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void BroadcastActiveScene(Type baseType, string methodName, params object[] args)
        {
            Broadcast(SceneManager.GetActiveScene(), baseType, methodName, args);
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in any loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string)"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        public void BroadcastAllScenes(string methodName)
        {
            for(int i = 0; i < SceneManager.sceneCount; i++)
            {
                Broadcast(SceneManager.GetSceneAt(i), methodName);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in any loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void BroadcastAllScenes(string methodName, params object[] args)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Broadcast(SceneManager.GetSceneAt(i), methodName, args);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in any loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string)"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        public void BroadcastAllScenes(Type baseType, string methodName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Broadcast(SceneManager.GetSceneAt(i), baseType, methodName);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in any loaded scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void BroadcastAllScenes(Type baseType, string methodName, params object[] args)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Broadcast(SceneManager.GetSceneAt(i), baseType, methodName, args);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the specified scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="targetScene">The scene used to filter the broadcast. All executing behaviour instances in this scene matching the criteria will receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void Broadcast(Scene targetScene, string methodName)
        {
            // Process all executing behaviour instances
            foreach(ScriptProxy proxy in execution.BehaviourProxies)
            {
                // Get behaviour instance
                MonoBehaviour behaviour = proxy.GetInstanceAs<MonoBehaviour>(true);

                // Check for same scene
                if (targetScene.name == behaviour.gameObject.scene.name)
                    proxy.SafeCall(methodName);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the specified scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// </summary>
        /// <param name="targetScene">The scene used to filter the broadcast. All executing behaviour instances in this scene matching the criteria will receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void Broadcast(Scene targetScene, string methodName, params object[] args)
        {
            // Process all executing behaviour instances
            foreach (ScriptProxy proxy in execution.BehaviourProxies)
            {
                // Get behaviour instance
                MonoBehaviour behaviour = proxy.GetInstanceAs<MonoBehaviour>(true);

                // Check for same scene
                if (targetScene.name == behaviour.gameObject.scene.name)
                    proxy.SafeCall(methodName, args);
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the specified scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string)"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="targetScene">The scene used to filter the broadcast. All executing behaviour instances in this scene matching the criteria will receive the broadcast</param>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        public void Broadcast(Scene targetScene, Type baseType, string methodName)
        {
            // Check for non-monobehaviour base type - this method only support script components
            if (typeof(MonoBehaviour).IsAssignableFrom(baseType) == false)
                return;
            
            // Process all executing behaviour instances
            foreach (ScriptProxy proxy in execution.BehaviourProxies)
            {
                // Check for sub type
                if (proxy.ScriptType.IsSubTypeOf(baseType) == true)
                {
                    // Get behaviour instance
                    MonoBehaviour behaviour = proxy.GetInstanceAs<MonoBehaviour>(true);

                    // Check for same scene
                    if (targetScene.name == behaviour.gameObject.scene.name)
                        proxy.SafeCall(methodName);
                }
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing mono behaviour instances loaded into this domain that exist in the specified scene.
        /// Only monobehaviour components will be considered by this broadcast. Use <see cref="BroadcastInstance(Type, string, object[])"/> if you want to broadcast to non-monobehaviour instances.
        /// Only behaviour types deriving from the specified base type will be considered.
        /// </summary>
        /// <param name="targetScene">The scene used to filter the broadcast. All executing behaviour instances in this scene matching the criteria will receive the broadcast</param>
        /// <param name="baseType">A type deriving from monobehaviour that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void Broadcast(Scene targetScene, Type baseType, string methodName, params object[] args)
        {
            // Check for non-monobehaviour base type - this method only support script components
            if (typeof(MonoBehaviour).IsAssignableFrom(baseType) == false)
                return;

            // Process all executing behaviour instances
            foreach (ScriptProxy proxy in execution.BehaviourProxies)
            {
                // Check for sub type
                if (proxy.ScriptType.IsSubTypeOf(baseType) == true)
                {
                    // Get behaviour instance
                    MonoBehaviour behaviour = proxy.GetInstanceAs<MonoBehaviour>(true);

                    // Check for same scene
                    if (targetScene.name == behaviour.gameObject.scene.name)
                        proxy.SafeCall(methodName, args);
                }
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing non-unity instances (Normal managed instances created using 'new').
        /// Only instances deriving from the specified base type will be considered. Pass 'typeof(object)' to use no filter at all.
        /// </summary>
        /// <param name="baseType">A type that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        public void BroadcastInstance(Type baseType, string methodName)
        {
            // Process all executing instances
            foreach(ScriptProxy proxy in execution.InstanceProxies)
            {
                // Check for sub type
                if(proxy.ScriptType.IsSubTypeOf(baseType) == true)
                {
                    // Call the method
                    proxy.SafeCall(methodName);
                }
            }
        }

        /// <summary>
        /// Attempt to invoke an instance method for all executing non-unity instances (Normal managed instances created using 'new').
        /// Only instances deriving from the specified base type will be considered. Pass 'typeof(object)' to use no filter at all.
        /// </summary>
        /// <param name="baseType">A type that executing types must inherit from in order to receive the broadcast</param>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="args">The argument list for the target method. All matching instances must have a compatible argument list otherwise the broadcast will fail for that particular instance</param>
        public void BroadcastInstance(Type baseType, string methodName, params object[] args)
        {
            // Process all executing instances
            foreach (ScriptProxy proxy in execution.InstanceProxies)
            {
                // Check for sub type
                if (proxy.ScriptType.IsSubTypeOf(baseType) == true)
                {
                    // Call the method
                    proxy.SafeCall(methodName, args);
                }
            }
        }
        #endregion

        /// <summary>
        /// Dispose of this domain.
        /// This will cause the target app domain to be unloaded if it is not the default app domain.
        /// The domain will be unusable after disposing.
        /// </summary>
        public void Dispose()
        {
            if (sandbox != null)
            {
                bool isUnityEditorDomain = false;

                // Check for editor domain - Do not unload this domain as it can crash editor
                if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor)
                    isUnityEditorDomain = true;

                // Unload app domain
                if (sandbox.IsDefaultAppDomain() == false && isUnityEditorDomain == false)
                    AppDomain.Unload(sandbox);

                // Remove from active list
                activeDomains.Remove(this);

                lock (this)
                {
                    loadedAssemblies.Clear();
                }

                sandbox = null;
                sharedCompiler = null;
                securityResult = null;
                compileResult = null;
            }
        }

        /// <summary>
        /// Initializes the Roslyn compiler service if it has not yet been initialized.
        /// </summary>
        public void InitializeCompilerService()
        {
            // Check if the compiler is initialized
            if (sharedCompiler == null)
            {
                // Create the compiler
                sharedCompiler = new RoslynCSharpCompiler(true, true, Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary, Microsoft.CodeAnalysis.CSharp.LanguageVersion.Default, sandbox);

                // Setup compiler
                ApplyCompilerServiceSettings();
            }
        }

        /// <summary>
        /// Causes the Roslyn C# settings to be loaded and applied to the Roslyn compiler service.
        /// This requires the compiler service to be initialized otherwise it will do nothing.
        /// </summary>
        public void ApplyCompilerServiceSettings()
        {
            // Check for no compiler
            if (sharedCompiler == null)
                return;

            // Load the settings
            RoslynCSharp settings = RoslynCSharp.Settings;

            // Setup compiler values
            sharedCompiler.AllowUnsafe = settings.AllowUnsafeCode;
            sharedCompiler.AllowOptimize = settings.AllowOptimizeCode;
            sharedCompiler.AllowConcurrentCompile = settings.AllowConcurrentCompile;
            sharedCompiler.Deterministic = settings.Deterministic;
            sharedCompiler.GenerateInMemory = settings.GenerateInMemory;
            sharedCompiler.GenerateSymbols = settings.GenerateSymbols; // NOT SUPPORTED ON MONO
            sharedCompiler.WarningLevel = settings.WarningLevel;
            sharedCompiler.LanguageVersion = settings.LanguageVersion;
            sharedCompiler.TargetPlatform = settings.TargetPlatform;

            // Setup reference paths
            sharedCompiler.ReferenceAssemblies.Clear();
            foreach (string reference in settings.References)
                sharedCompiler.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(reference));

            // Setup define symbols
            sharedCompiler.DefineSymbols.Clear();
            foreach (string define in settings.DefineSymbols)
                sharedCompiler.DefineSymbols.Add(define);
        }

        /// <summary>
        /// Log the last output of the Roslyn compiler to the Unity console.
        /// </summary>
        public void LogCompilerOutputToConsole()
        {
            // Check for no report
            if (compileResult == null)
                return;
            
            bool loggedHeader = false;

            // Simple function to only output the header when one or more errors, warnings or infos will be logged
            Action logHeader = () =>
            {
                if (loggedHeader == false)
                {
                    RoslynCSharp.Log("__Roslyn Compile Output__");
                    loggedHeader = true;
                }
            };

            // Process report
            foreach (CompilationError error in compileResult.Errors)
            {
                if(error.IsError == true)
                {
                    // Log as error
                    logHeader();
                    RoslynCSharp.LogError(error.ToString());
                }
                else if(error.IsWarning == true)
                {
                    // Log as warning
                    logHeader();
                    RoslynCSharp.LogWarning(error.ToString());
                }
                else if(error.IsInfo == true)
                {
                    logHeader();
                    RoslynCSharp.Log(error.ToString());
                }
            }
        }

        private void CheckDisposed()
        {
            // Check for our sandbox domain
            if(sandbox == null)
                throw new ObjectDisposedException("The 'ScriptDomain' has already been disposed");
        }

        private void CheckCompiler()
        {
            // Check for our compiler service
            if (sharedCompiler == null)
                throw new Exception("The compiler service has not been initialized");
        }

        public ScriptAssembly RegisterAssembly(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, CompilationResult compileResult = null)
        {
            return RegisterAssembly<ScriptAssemblyImpl>(systemAssembly, securityMode, compileResult);
        }

        public ScriptAssembly RegisterAssembly<T>(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, CompilationResult compileResult = null) where T : ScriptAssembly, new()
        {
            // Check for null assembly
            if (systemAssembly == null)
                return null;

            string imagePath = (compileResult != null) ? compileResult.OutputFile : null;
            string symbolPath = (compileResult != null) ? compileResult.OutputPDBFile : null;

            byte[] imageStore = (imagePath != null && File.Exists(imagePath) == true) ? File.ReadAllBytes(imagePath) : null;
            byte[] symbolStore = (symbolPath != null && File.Exists(symbolPath) == true) ? File.ReadAllBytes(symbolPath) : null;

            return RegisterAssemblyImpl(ScriptAssembly.CreateScriptAssembly<T>(this, systemAssembly, imagePath, symbolPath, imageStore, symbolStore, compileResult), securityMode);
        }

        public ScriptAssembly RegisterAssemblyPath(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, string assemblyPath = null, string assemblySymbolsPath = null, CompilationResult compileResult = null)
        {
            return RegisterAssemblyPath<ScriptAssemblyImpl>(systemAssembly, securityMode, assemblyPath, assemblySymbolsPath, compileResult);
        }

        public ScriptAssembly RegisterAssemblyPath<T>(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, string assemblyPath = null, string assemblySymbolsPath = null, CompilationResult compileResult = null) where T : ScriptAssembly, new()
        {
            // Check for null assembly
            if (systemAssembly == null)
                return null;

            byte[] imageStore = (assemblyPath != null && File.Exists(assemblyPath) == true) ? File.ReadAllBytes(assemblyPath) : null;
            byte[] symbolStore = (assemblySymbolsPath != null && File.Exists(assemblySymbolsPath) == true) ? File.ReadAllBytes(assemblySymbolsPath) : null;

            return RegisterAssemblyImpl(ScriptAssembly.CreateScriptAssembly<T>(this, systemAssembly, assemblyPath, assemblySymbolsPath, imageStore, symbolStore, compileResult), securityMode);
        }

        public ScriptAssembly RegisterAssemblyImage(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, byte[] assemblyImage = null, byte[] assemblySymbolsImage = null, CompilationResult compileResult = null)
        {
            return RegisterAssemblyImage<ScriptAssemblyImpl>(systemAssembly, securityMode, assemblyImage, assemblySymbolsImage, compileResult);
        }

        public ScriptAssembly RegisterAssemblyImage<T>(Assembly systemAssembly, ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings, byte[] assemblyImage = null, byte[] assemblySymbolsImage = null, CompilationResult compileResult = null) where T : ScriptAssembly, new()
        {
            // Check for null assembly
            if (systemAssembly == null)
                return null;

            return RegisterAssemblyImpl(ScriptAssembly.CreateScriptAssembly<T>(this, systemAssembly, null, null, assemblyImage, assemblySymbolsImage, compileResult), securityMode);
        }

        private ScriptAssembly RegisterAssemblyImpl(ScriptAssembly scriptAssembly, ScriptSecurityMode securityMode)
        {
            // Check for ensure security mode
            bool performSecurityCheck = (securityMode == ScriptSecurityMode.EnsureSecurity);

            // Get value from settings
            if (securityMode == ScriptSecurityMode.UseSettings)
                performSecurityCheck = RoslynCSharp.Settings.SecurityCheckCode;

            // Check for security checks
            if (performSecurityCheck == true)
            {
                CodeSecurityRestrictions restrictions = RoslynCSharp.Settings.SecurityRestrictions;

                // Use pinvoke option
                restrictions.AllowPInvoke = RoslynCSharp.Settings.AllowPInvoke;

                // Perform code validation
                if (scriptAssembly.SecurityCheckAssembly(restrictions, out securityResult) == false)
                {
                    // Log the error
                    RoslynCSharp.LogError(securityResult.GetSummaryText());
                    RoslynCSharp.LogError(securityResult.GetAllText(true));
                    // Dont load the assembly
                    return null;
                }
                else
                    RoslynCSharp.Log(securityResult.GetSummaryText());
            }

            lock (this)
            {
                // Register with domain
                this.loadedAssemblies.Add(scriptAssembly);
            }

            // Return result
            return scriptAssembly;
        }

        /// <summary>
        /// Creates a new <see cref="ScriptDomain"/> into which assemblies and scripts may be loaded.
        /// </summary>
        /// <returns>A new instance of <see cref="ScriptDomain"/></returns>
        public static ScriptDomain CreateDomain(string domainName, bool initCompiler = true, bool makeActiveDomain = true, AppDomain sandboxDomain = null)
        {
            // Create a new named domain
            ScriptDomain domain = new ScriptDomain(domainName, sandboxDomain);

            // Load the roslyn settings - do this now because the next load request could be from a worker thread
            RoslynCSharp.LoadResources();

            // Check for compiler
            if (initCompiler == true)
                domain.InitializeCompilerService();
           
            // Make domain active
            if (makeActiveDomain == true)
                MakeDomainActive(domain);

            return domain;
        }

        /// <summary>
        /// Attempt to find a domain with the specified name.
        /// </summary>
        /// <param name="domainName">The domain name to search for</param>
        /// <returns>A domain with the specified name or null if no matching domain was found</returns>
        public static ScriptDomain FindDomain(string domainName)
        {
            foreach(ScriptDomain domain in activeDomains)
            {
                if (domain.name == domainName)
                    return domain;
            }

            // Domain not found
            return null;
        }

        /// <summary>
        /// Set the specified domain as the active domain.
        /// The active domain is used when resolving script types from an unspecified source.
        /// </summary>
        /// <param name="domain">The domain to make active</param>
        public static void MakeDomainActive(ScriptDomain domain)
        {
            // Check for null domain
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            // Make active
            active = domain;
        }

        /// <summary>
        /// Set the domain with the specified name as the active domain.
        /// The active domain is used when resolving script types from an unspecified source.
        /// </summary>
        /// <param name="domainName">The name of the domain to make active</param>
        public static void MakeDomainActive(string domainName)
        {
            // Find domain with name
            ScriptDomain domain = FindDomain(domainName);

            // Make active
            if (domain != null)
                MakeDomainActive(domain);
        }
    }
}
