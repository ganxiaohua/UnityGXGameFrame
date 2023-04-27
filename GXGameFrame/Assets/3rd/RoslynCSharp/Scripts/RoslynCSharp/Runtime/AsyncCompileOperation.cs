
using Microsoft.CodeAnalysis.CSharp;
using RoslynCSharp.Compiler;

namespace RoslynCSharp
{
    /// <summary>
    /// An awaitable object that contains state information for an asynchronous script compile request.
    /// </summary>
    public sealed class AsyncCompileOperation : AsyncOperation
    {
        // Types
        internal enum CompileType
        {
            CompileSource,
            CompileFile,
            CompileSyntaxTree,
        }

        // Private
        private object assemblyAccessLock = new object();
        private ScriptDomain compileDomain = null;
        private ScriptAssembly compileResult = null;
        private ScriptSecurityMode securityMode = 0;
        private bool isSecurityVerified = false;
        private CompileType sourceCompileType = 0;
        private string[] sourceOrFiles = null;
        private CSharpSyntaxTree[] syntaxTrees = null;
        private IMetadataReferenceProvider[] additionalReferences = null;

        // Properties
        /// <summary>
        /// Get the domain that the compiled assembly will be loaded into if successful.
        /// </summary>
        public ScriptDomain CompileDomain
        {
            get
            {
                lock(compileDomain)
                {
                    return compileDomain;
                }
            }
        }

        /// <summary>
        /// Get the main type of the compiled assembly.
        /// The return value will be null if the compile failed or the compiled script does not define atleast 1 type.
        /// </summary>
        public ScriptType CompiledType
        {
            get
            {
                lock(assemblyAccessLock)
                {
                    if (compileResult == null)
                        return null;

                    return compileResult.MainType;
                }
            }
        }

        /// <summary>
        /// Get the compiled assembly that the compiler generated.
        /// The return value will be null if the compile failed.
        /// </summary>
        public ScriptAssembly CompiledAssembly
        {
            get
            {
                lock(assemblyAccessLock)
                {
                    return compileResult;
                }
            }
        }

        /// <summary>
        /// Returns true if the loaded assembly has passed security verification.
        /// </summary>
        public bool IsSecurityVerified
        {
            get { return isSecurityVerified; }
        }

        // Constructor
        internal AsyncCompileOperation(ScriptDomain domain, CompileType compileMode, ScriptSecurityMode securityMode, string[] sourceOrFiles, CSharpSyntaxTree[] syntaxTrees, IMetadataReferenceProvider[] additionalReferenceAssemblies = null)
        {
            this.compileDomain = domain;
            this.sourceCompileType = compileMode;
            this.securityMode = securityMode;
            this.sourceOrFiles = sourceOrFiles;
            this.syntaxTrees = syntaxTrees;
            this.additionalReferences = additionalReferenceAssemblies;
        }

        // Methods
        /// <summary>
        /// Main entry point for async code.
        /// </summary>
        protected override void RunAsyncOperation()
        {
            ScriptAssembly result = null;

            lock(compileDomain)
            {
                switch(sourceCompileType)
                {
                    case CompileType.CompileSource:
                        {
                            if(sourceOrFiles.Length == 1)
                            {
                                // Compile the source
                                result = compileDomain.CompileAndLoadSource(sourceOrFiles[0], securityMode, additionalReferences);
                            }
                            else
                            {
                                // Compile the sources
                                result = compileDomain.CompileAndLoadSources(sourceOrFiles, securityMode, additionalReferences);
                            }
                            break;
                        }

                    case CompileType.CompileFile:
                        {
                            if(sourceOrFiles.Length == 1)
                            {
                                // Compile the file
                                result = compileDomain.CompileAndLoadFile(sourceOrFiles[0], securityMode, additionalReferences);
                            }
                            else
                            {
                                // Compile the files
                                result = compileDomain.CompileAndLoadFiles(sourceOrFiles, securityMode, additionalReferences);
                            }
                            break;
                        }

                    case CompileType.CompileSyntaxTree:
                        {
                            if(syntaxTrees.Length == 1)
                            {
                                // Compile the syntax tree
                                result = compileDomain.CompileAndLoadSyntaxTree(syntaxTrees[0], securityMode, additionalReferences);
                            }
                            else
                            {
                                // Compile all syntax trees
                                result = compileDomain.CompileAndLoadSyntaxTrees(syntaxTrees, securityMode, additionalReferences);
                            }
                            break;
                        }
                }
                // Store result
                lock(assemblyAccessLock)
                {
                    compileResult = result;
                }

                // Set successful flag
                isSuccessful = compileDomain.CompileResult.Success;

                // Set verified flag
                isSecurityVerified = (compileDomain.SecurityResult != null)
                    ? compileDomain.SecurityResult.IsSecurityVerified
                    : false;
            }
        }
    }
}
