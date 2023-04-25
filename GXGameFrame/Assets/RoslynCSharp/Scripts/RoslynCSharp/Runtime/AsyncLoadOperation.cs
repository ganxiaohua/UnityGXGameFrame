
using System.Reflection;

namespace RoslynCSharp
{
    /// <summary>
    /// An awaitable object that contains state information for an asynchronous assembly load request.
    /// </summary>
    public sealed class AsyncLoadOperation : AsyncOperation
    {
        // Types
        private enum AssemblyLoadType
        {
            LoadByName,
            LoadByPath,
            LoadFromBytes,
        }

        // Private
        private object assemblyAccessLock = new object();
        private ScriptDomain loadDomain = null;
        private ScriptAssembly loadResult = null;
        private ScriptSecurityMode securityMode = 0;
        private bool isSecurityVerified = false;

        private AssemblyLoadType loadType = 0;
        private AssemblyName asmName = null;
        private string asmPath = null;
        private string symbolPath = null;
        private byte[] asmBytes = null;
        private byte[] symbolBytes = null;

        // Properties
        /// <summary>
        /// Get the domain that the assembly will be loaded into if successful.
        /// </summary>
        public ScriptDomain LoadDomain
        {
            get
            {
                lock(loadDomain)
                {
                    return loadDomain;
                }
            }
        }

        /// <summary>
        /// Get the assembly that has been loaded.
        /// The return value will be null if the load failed or if the assembly failed code verification.
        /// </summary>
        public ScriptAssembly LoadedAssembly
        {
            get
            {
                lock (assemblyAccessLock)
                {
                    return loadResult;
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
        internal AsyncLoadOperation(ScriptDomain domain, AssemblyName assemblyName, ScriptSecurityMode securityMode)
        {
            this.loadDomain = domain;
            this.asmName = assemblyName;
            this.loadType = AssemblyLoadType.LoadByName;
            this.securityMode = securityMode;
        }

        internal AsyncLoadOperation(ScriptDomain domain, string assemblyPath, ScriptSecurityMode securityMode, string symbolPath = null)
        {
            this.loadDomain = domain;
            this.asmPath = assemblyPath;
            this.loadType = AssemblyLoadType.LoadByPath;
            this.securityMode = securityMode;
            this.symbolPath = symbolPath;
        }

        internal AsyncLoadOperation(ScriptDomain domain, byte[] assemblyBytes, ScriptSecurityMode securityMode, byte[] symbolBytes = null)
        {
            this.loadDomain = domain;
            this.asmBytes = assemblyBytes;
            this.loadType = AssemblyLoadType.LoadFromBytes;
            this.securityMode = securityMode;
            this.symbolBytes = symbolBytes;
        }

        // Methods
        /// <summary>
        /// Main entry point for async code.
        /// </summary>
        protected override void RunAsyncOperation()
        {
            ScriptAssembly result = null;

            lock(loadDomain)
            {
                switch(loadType)
                {
                    case AssemblyLoadType.LoadByName:
                        {
                            result = loadDomain.LoadAssembly(asmName, securityMode);
                            break;
                        }

                    case AssemblyLoadType.LoadByPath:
                        {
                            if (symbolPath != null)
                            {
                                result = loadDomain.LoadAssemblyWithSymbols(asmPath, symbolPath, securityMode);
                            }
                            else
                            {
                                result = loadDomain.LoadAssembly(asmPath, securityMode);
                            }
                            break;
                        }

                    case AssemblyLoadType.LoadFromBytes:
                        {
                            if (symbolBytes != null)
                            {
                                result = loadDomain.LoadAssemblyWithSymbols(asmBytes, symbolBytes, securityMode);
                            }
                            else
                            {
                                result = loadDomain.LoadAssembly(asmBytes, securityMode);
                            }
                            break;
                        }
                }
                // Store result
                lock (assemblyAccessLock)
                {
                    loadResult = result;
                }
            }

            // Set successful flag
            isSuccessful = result != null;

            // Set verified flag
            isSecurityVerified = (loadDomain.SecurityResult != null)
                ? loadDomain.SecurityResult.IsSecurityVerified
                : false;
        }
    }
}
