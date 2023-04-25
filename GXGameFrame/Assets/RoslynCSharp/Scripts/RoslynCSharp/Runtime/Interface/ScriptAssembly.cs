using Microsoft.CodeAnalysis;
using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Trivial.CodeSecurity;

namespace RoslynCSharp
{
    // Types
    public interface IScriptCompiledAssembly
    {
        // Methods
        void MarkAsRuntimeCompiled(CompilationResult compileResult);
    }

    public abstract class ScriptAssembly : IMetadataReferenceProvider
    {
        // Private
        private static List<ScriptType> matchedTypes = new List<ScriptType>();

        private Dictionary<string, ScriptType> scriptTypes = null;
        private ScriptType mainType = null;        
        private bool isSecurityValidated = false;
        private int securityValidatedHash = -1;
        private CodeSecurityReport securityReport = null;

        // Protected
        protected string assemblyPath = null;
        protected string assemblySymbolsPath = null;
        protected byte[] assemblyImage = null;
        protected byte[] assemblySymbolsImage = null;

        // Properties
        /// <summary>
        /// Get the <see cref="ScriptDomain"/> that this <see cref="ScriptAssembly"/> is currently loaded in.  
        /// </summary>
        public abstract ScriptDomain Domain { get; }

        /// <summary>
        /// Get the <see cref="Assembly"/> that this <see cref="ScriptAssembly"/> wraps.  
        /// </summary>
        public abstract Assembly SystemAssembly { get; }

        /// <summary>
        /// Gets the main type for the assembly. This will usually return the first defined class type in the assembly which is especially useful for assemblies that only define a single type.
        /// </summary>
        public virtual ScriptType MainType
        {
            get
            {
                if(mainType == null)
                {
                    LoadScriptAssemblyTypes();

                    ScriptType defaultMain = null;

                    // Select first entry
                    foreach(ScriptType type in scriptTypes.Values)
                    {
                        defaultMain = type;
                        break;
                    }

                    // Check for class types
                    ScriptType overrideMain = scriptTypes.Values
                        .Where(t => t.SystemType.IsClass == true && t.Name != "<Module>")
                        .FirstOrDefault();

                    // Set main type
                    mainType = (overrideMain != null)
                        ? overrideMain
                        : defaultMain;
                }
                return mainType;
            }
        }

        /// <summary>
        /// Get the name of the wrapped assembly.
        /// </summary>
        public virtual string Name
        {
            get { return SystemAssembly.GetName().Name; }
        }

        /// <summary>
        /// Get the full name of the wrapped assembly.
        /// </summary>
        public virtual string FullName
        {
            get { return SystemAssembly.FullName; }
        }

        /// <summary>
        /// Get the version of the wrapped assembly.
        /// </summary>
        public virtual Version Version
        {
            get { return SystemAssembly.GetName().Version; }
        }

        /// <summary>
        /// Get the location of the loaded system assembly.
        /// </summary>
        public virtual string AssemblyPath
        {
            get { return assemblyPath; }
        }
        
        /// <summary>
        /// Get the location of the loaded debug symbols file.
        /// </summary>
        public virtual string AssemblySymbolsPath
        {
            get { return assemblySymbolsPath; }
        }

        /// <summary>
        /// Get the raw assembly image data of the loaded system assembly.
        /// </summary>
        public virtual byte[] AssemblyImage
        {
            get { return assemblyImage; }
        }

        /// <summary>
        /// Get the raw debug symbols image data for the loaded assembly.
        /// </summary>
        public virtual byte[] AssemblySymbolsImage
        {
            get { return assemblySymbolsImage; }
        }

        /// <summary>
        /// Get the metadata reference that can be used to add this assembly as a compiler reference.
        /// </summary>
        public virtual MetadataReference CompilerReference
        {
            get
            {
                if (AssemblyImage != null)
                    return AssemblyReference.FromImage(AssemblyImage).CompilerReference;

                return AssemblyReference.FromNameOrFile(AssemblyPath).CompilerReference;
            }
        }

        /// <summary>
        /// Returns true if this assembly was compiled at runtime by the Roslyn compiler service.
        /// </summary>
        public abstract bool IsRuntimeCompiled { get; }

        /// <summary>
        /// Get the DateTime when this assembly was runtime compiled.
        /// </summary>
        public abstract DateTime RuntimeCompiledTime { get; }

        /// <summary>
        /// Get the compilation result for the assembly if it was runtime compiled.
        /// </summary>
        public abstract CompilationResult CompileResult { get; }

        /// <summary>
        /// Returns true if this assembly has passed security verification.
        /// </summary>
        public virtual bool IsSecurityValidated
        {
            get { return isSecurityValidated; }
        }

        /// <summary>
        /// Get the code security report for this assembly if it was security verified.
        /// </summary>
        public virtual CodeSecurityReport SecurityReport
        {
            get { return securityReport; }
        }

        // Construction
        protected abstract void ConstructInstance(ScriptDomain domain, Assembly systemAssembly);

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1})", nameof(ScriptAssembly), SystemAssembly);
        }

        /// <summary>
        /// Run security verification on this assembly using the specified security restrictions.
        /// </summary>
        /// <param name="restrictions">The restrictions used to verify the assembly</param>
        /// <returns>True if the assembly passes security verification or false if it fails</returns>
        public bool SecurityCheckAssembly(CodeSecurityRestrictions restrictions)
        {
            CodeSecurityReport report;
            return SecurityCheckAssembly(restrictions, out report);
        }

        /// <summary>
        /// Run security verification on this assembly using the specified security restrictions and output a security report
        /// </summary>
        /// <param name="restrictions">The restrictions used to verify the assembly</param>
        /// <param name="report">The security report generated by the assembly checker</param>
        /// <returns>True if the assembly passes security verification or false if it fails</returns>
        public virtual bool SecurityCheckAssembly(CodeSecurityRestrictions restrictions, out CodeSecurityReport report)
        {
            // Skip checks
            if (isSecurityValidated == true && restrictions.RestrictionsHash == securityValidatedHash)
            {
                report = securityReport;
                return true;
            }

            // Create the security engine
            CodeSecurityEngine securityEngine = CreateSecurityEngine();

            // Check for already checked
            if (securityEngine == null)
            {
                report = securityReport;
                return isSecurityValidated;
            }

            // Must dispose once finished
            using (securityEngine)
            {
                // Run code valdiation
                isSecurityValidated = securityEngine.SecurityCheckAssembly(restrictions, out securityReport);

                // Check for verified
                if (isSecurityValidated == true)
                {
                    // Store the hash so that the same restirctions will not need to run again
                    securityValidatedHash = restrictions.RestrictionsHash;
                }
                else
                {
                    securityValidatedHash = -1;
                }

                report = securityReport;
                return isSecurityValidated;
            }
        }

        /// <summary>
        /// Create a code security engine that will be used to security verify the assembly code.
        /// </summary>
        /// <returns></returns>
        protected virtual CodeSecurityEngine CreateSecurityEngine()
        {
            if(AssemblyImage != null)
            {
                return new CodeSecurityEngine(AssemblyImage, AssemblySymbolsImage);
            }

            return new CodeSecurityEngine(AssemblyPath);//, AssemblySymbolsPath);
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type with the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type with the specified name is defined</returns>
        public virtual bool HasType(string name)
        {
            return FindType(name) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines one or more types that inherit from the specified type.
        /// The specified type may be a base class or interface type.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritace chain</param>
        /// <returns>True if there are one or more defined types that inherit from the specified type</returns>
        public virtual bool HasSubTypeOf(Type subType)
        {
            return FindSubTypeOf(subType) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defined one or more types that inherit from the specified generic type.
        /// The specified generic type may be a base class or interface type.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>True if there are one or more defined types that inherit from the specified generic type</returns>
        public bool HasSubTypeOf<T>()
        {
            return HasSubTypeOf(typeof(T));
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public virtual bool HasSubTypeOf(Type subType, string name)
        {
            return FindSubTypeOf(subType, name) != null;
        }

        /// <summary>
        /// Returns true if this <see cref="ScriptAssembly"/> defines a type that inherits from the specified genric type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>True if a type that inherits from the specified type and has the specified name is defined</returns>
        public bool HasSubTypeOf<T>(string name)
        {
            return HasSubTypeOf(typeof(T), name);
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> with the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public virtual ScriptType FindType(string name)
        {
            LoadScriptAssemblyTypes();

            // Try to find the type
            Type type = SystemAssembly.GetType(name, false, false);

            // Check for error
            if (type == null)
            {
                return null;
            }

            // Get the cached script type
            return scriptTypes[type.FullName];
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> from the specified system type.
        /// </summary>
        /// <param name="type">The system type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public virtual ScriptType FindType(Type type)
        {
            LoadScriptAssemblyTypes();

            // Check for error
            if (type == null)
                return null;

            // Get the cached script type
            return scriptTypes[type.FullName];
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified base type.
        /// If there is more than one type that inherits from the specified base type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubTypesOf(Type, bool)"/>. 
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public virtual ScriptType FindSubTypeOf(Type subType, bool includeNonPublic = true, bool findNestedTypes = true)
        {
            LoadScriptAssemblyTypes();

            // Find all types in the assembly
            foreach (ScriptType type in scriptTypes.Values)
            {
                // Check for non-public discoverability
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for skip nested types
                if (type.IsNestedType == true && findNestedTypes == false)
                    continue;

                // Check for subtype
                if (type.IsSubTypeOf(subType) == true)
                {
                    // Return first occurence
                    return type;
                }
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type. 
        /// If there is more than one type that inherits from the specified generic type, then the first matching type will be returned.
        /// If you want to find all types then use <see cref="FindAllSubTypesOf{T}(bool)"/>.
        /// </summary>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf<T>(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            return FindSubTypeOf(typeof(T), includeNonPublic, findNestedTypes);
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified base type and matches the specified name.
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public virtual ScriptType FindSubTypeOf(Type subType, string name)
        {
            LoadScriptAssemblyTypes();

            // Find a type with the specified name
            ScriptType type = FindType(name);

            // Check for error
            if (type == null)
                return null;

            // Make sure the identifier type is a subclass
            if (type.IsSubTypeOf(subType) == true)
                return type;

            return null;
        }

        /// <summary>
        /// Attempts to find a type defined in this <see cref="ScriptAssembly"/> that inherits from the specified generic type and matches the specified name. 
        /// Depending upon settings, name comparison may or may not be case sensitive.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <param name="name">The name of the type to look for</param>
        /// <returns>An instance of <see cref="ScriptType"/> representing the found type or null if the type could not be found</returns>
        public ScriptType FindSubTypeOf<T>(string name)
        {
            return FindSubTypeOf(typeof(T), name);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherits from the specified type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public virtual ScriptType[] FindAllSubTypesOf(Type subType, bool includeNonPublic = true, bool findNestedTypes = true)
        {
            LoadScriptAssemblyTypes();

            // Use shared list
            matchedTypes.Clear();

            // Find all types
            foreach (ScriptType type in scriptTypes.Values)
            {
                // Check for non-public discovery
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for skip nested types
                if (type.IsNestedType == true && findNestedTypes == false)
                    continue;

                // Make sure the type is a Unity object
                if (type.IsSubTypeOf(subType) == true)
                {
                    // Add type
                    matchedTypes.Add(type);
                }
            }

            // Get the array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from the specified generic type.
        /// If there are no types that inherit from the specified type then the return value will be an empty array.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllSubTypesOf<T>(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            return FindAllSubTypesOf(typeof(T), includeNonPublic, findNestedTypes);
        }

        /// <summary>
        /// Returns an array of all defined types in this <see cref="ScriptAssembly"/>. 
        /// </summary>
        /// <returns>An array of <see cref="ScriptType"/> representing all types defined in this <see cref="ScriptAssembly"/></returns>
        public virtual ScriptType[] FindAllTypes(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            LoadScriptAssemblyTypes();

            // Use shared array
            matchedTypes.Clear();
            matchedTypes.AddRange(scriptTypes.Values);

            // Remove nested types
            if (findNestedTypes == false)
                matchedTypes.RemoveAll(t => t.IsNestedType == true);

            // Get as array
            return matchedTypes.ToArray();
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.Object"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.Object"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllUnityTypes(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            return FindAllSubTypesOf<UnityEngine.Object>(includeNonPublic, findNestedTypes);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.MonoBehaviour"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllMonoBehaviourTypes(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            return FindAllSubTypesOf<UnityEngine.MonoBehaviour>(includeNonPublic, findNestedTypes);
        }

        /// <summary>
        /// Attempts to find all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.ScriptableObject"/>.  
        /// If there are no types that inherit from <see cref="UnityEngine.ScriptableObject"/> then the return value will be an empty array.
        /// </summary>
        /// <returns>(Not Null) An array of <see cref="ScriptType"/> or an empty array if no matching type was found</returns>
        public ScriptType[] FindAllScriptableObjectTypes(bool includeNonPublic = true, bool findNestedTypes = true)
        {
            return FindAllSubTypesOf<UnityEngine.ScriptableObject>(includeNonPublic, findNestedTypes);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherits from the specified type.
        /// </summary>
        /// <param name="subType">The type to check for in the inheritance chain</param>
        /// <param name="includeNonPublic">Should the search include non public types</param>
        /// <returns>Enumerable of matching results</returns>
        public virtual IEnumerable<ScriptType> EnumerateAllSubTypesOf(Type subType, bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            LoadScriptAssemblyTypes();

            foreach (ScriptType type in scriptTypes.Values)
            {
                // Check for visible
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for skip nested types
                if (type.IsNestedType == true && enumerateNestedTypes == false)
                    continue;

                // Check for sub type
                if (type.IsSubTypeOf(subType) == true)
                    yield return type;
            }
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from the specified generic type.
        /// </summary>
        /// <typeparam name="T">The generic type to check for in the inheritance chain</typeparam>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllSubTypesOf<T>(bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            return EnumerateAllSubTypesOf(typeof(T), includeNonPublic, enumerateNestedTypes);
        }

        /// <summary>
        /// Enumerate all defined types in this <see cref="ScriptAssembly"/>. 
        /// </summary>
        /// <returns>Enumerable of all results</returns>
        public virtual IEnumerable<ScriptType> EnumerateAllTypes(bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            LoadScriptAssemblyTypes();

            foreach (ScriptType type in scriptTypes.Values)
            {
                // Check for visible
                if (includeNonPublic == false)
                    if (type.IsPublic == false)
                        continue;

                // Check for skip nested types
                if (type.IsNestedType == true && enumerateNestedTypes == false)
                    continue;

                // Return type
                yield return type;
            }
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.Object"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllUnityTypes(bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            return EnumerateAllSubTypesOf<UnityEngine.Object>(includeNonPublic, enumerateNestedTypes);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.MonoBehaviour"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllMonoBehaviourTypes(bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            return EnumerateAllSubTypesOf<UnityEngine.MonoBehaviour>(includeNonPublic, enumerateNestedTypes);
        }

        /// <summary>
        /// Enumerate all types defined in this <see cref="ScriptAssembly"/> that inherit from <see cref="UnityEngine.ScriptableObject"/>.  
        /// </summary>
        /// <returns>Enumerable of matching results</returns>
        public IEnumerable<ScriptType> EnumerateAllScriptableObjectTypes(bool includeNonPublic = true, bool enumerateNestedTypes = true)
        {
            return EnumerateAllSubTypesOf<UnityEngine.ScriptableObject>(includeNonPublic, enumerateNestedTypes);
        }

        protected abstract ScriptType CreateRootScriptType(Type systemType);

        private void LoadScriptAssemblyTypes()
        {
            if (scriptTypes == null)
            {
                scriptTypes = new Dictionary<string, ScriptType>();

                Type[] types = SystemAssembly.GetTypes();

                foreach(Type type in types)
                {
                    if(type.IsNested == false)
                    {
                        scriptTypes.Add(type.FullName, CreateRootScriptType(type));
                    }
                }
            }
        }

        public static T CreateScriptAssembly<T>(ScriptDomain domain, Assembly systemAssembly, string assemblyPath = null, string assemblySymbolsPath = null, byte[] assemblyImage = null, byte[] assemblySymbolsImage = null, CompilationResult compileResult = null) where T : ScriptAssembly, new()
        {
            T assembly = new T();
            assembly.ConstructInstance(domain, systemAssembly);

            assembly.assemblyPath = assemblyPath;
            assembly.assemblySymbolsPath = assemblySymbolsPath;
            assembly.assemblyImage = assemblyImage;
            assembly.assemblySymbolsImage = assemblySymbolsImage;

            // Mark as runtime compiled
            if (assembly is IScriptCompiledAssembly)
                (assembly as IScriptCompiledAssembly).MarkAsRuntimeCompiled(compileResult);

            return assembly;
        }
    }
}
