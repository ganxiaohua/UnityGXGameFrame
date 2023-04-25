using UnityEngine;
using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using Trivial.CodeSecurity;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoslynCSharp
{
    /// <summary>
    /// The main settings for Roslyn C#.
    /// </summary>
    public sealed class RoslynCSharp : ScriptableObject
    {
        /// <summary>
        /// The amount of detail that should be logged to the uNity console.
        /// </summary>
        public enum LogDetail
        {
            /// <summary>
            /// Dont log any messages.
            /// </summary>
            None,
            /// <summary>
            /// Only log errors.
            /// </summary>
            Errors,
            /// <summary>
            /// Only log warnings and errors.
            /// </summary>
            Warnings,
            /// <summary>
            /// Log all types of messages.
            /// </summary>
            Info,
        }

        // Private
        private static RoslynCSharp settings = null;

        [SerializeField, HideInInspector]
        private bool securityCheckCode = true;

        [SerializeField, HideInInspector]
        private bool allowPInvoke = false;

        [SerializeField, HideInInspector]
        private CodeSecurityRestrictions securityRestrictions = new CodeSecurityRestrictions();

        // Compiler options
        [SerializeField, HideInInspector]
        private LogDetail logDetail = LogDetail.Errors;

        [SerializeField, HideInInspector]
        private bool allowUnsafeCode = false;

        [SerializeField, HideInInspector]
        private bool allowOptimizeCode = true;

        [SerializeField, HideInInspector]
        private bool allowConcurrentCompile = true;

        [SerializeField, HideInInspector]
        private bool deterministic = false;

        [SerializeField, HideInInspector]
        private bool generateInMemory = true;

        [SerializeField, HideInInspector]
        private bool generateSymbols = true;

        [SerializeField, HideInInspector]
        private int warningLevel = 4;

        [SerializeField, HideInInspector]
        private LanguageVersion languageVersion = LanguageVersion.Default;

        [SerializeField, HideInInspector]
        private Platform targetPlatform = Platform.AnyCpu;

        [SerializeField, HideInInspector]
        private List<string> refeferences = new List<string>();

        [SerializeField, HideInInspector]
        private List<AssemblyReferenceAsset> referenceAssets = new List<AssemblyReferenceAsset>();

        [SerializeField, HideInInspector]
        private List<string> defineSymbols = new List<string>();

        // Hot Reloading
        [SerializeField, HideInInspector]
        private bool allowHotReloading = true;

        [SerializeField, HideInInspector]
        private bool hotReloadCopySerializedFields = true;

        [SerializeField, HideInInspector]
        private bool hotReloadCopyNonSerializedFields = true;

        [SerializeField, HideInInspector]
        private bool hotReloadDestroyOriginalScript = true;

        [SerializeField, HideInInspector]
        private bool hotReloadDisableOriginalScript = true;

        [SerializeField, HideInInspector]
        private bool hotReloadSecurityCheckCode = false;

        [SerializeField, HideInInspector]
        private bool hotReloadUseCSharpProjectReferences = true;

        //[SerializeField]//, HideInInspector]
        //public RoslynCSharpSecurityAllowance securityAllowance = null;

        // Public
        /// <summary>
        /// The default name of the settings asset.
        /// </summary>
        public const string settingsName = "RoslynCSharpSettings";

        // Properties
        /// <summary>
        /// Get the active settings.
        /// This will cause the settings to be loaded from resources if they are not already loaded.
        /// </summary>
        public static RoslynCSharp Settings
        {
            get
            {
                // Check for playing
                //if (Application.isPlaying == false)
                //    throw new InvalidOperationException("You should only access the settings at runtime. if you need to change the settings in editor then use 'LoadAsset' and 'SaveAsset' static methods");

                // Try to load from resources
                if (settings == null)
                    LoadResources();

                return settings;
            }
        }

        /// <summary>
        /// Should loaded or compiled code be security checked.
        /// </summary>
        public bool SecurityCheckCode
        {
            get { return securityCheckCode; }
            set { securityCheckCode = value; }
        }

        /// <summary>
        /// Should external pinvoke calls be allowed.
        /// It is highly recommended that this options remains disabled to prevent external unverified code from running.
        /// </summary>
        public bool AllowPInvoke
        {
            get { return allowPInvoke; }
            set { allowPInvoke = value; }
        }

        /// <summary>
        /// The restrictions that are used to security check code.
        /// </summary>
        public CodeSecurityRestrictions SecurityRestrictions
        {
            get { return securityRestrictions; }
        }

        /// <summary>
        /// The log level used to determine which types of messages get logged to the console.
        /// </summary>
        public LogDetail LogLevel
        {
            get { return logDetail; }
            set { logDetail = value; }
        }

        /// <summary>
        /// Should the compiler allow unsafe code to be compiled.
        /// </summary>
        public bool AllowUnsafeCode
        {
            get { return allowUnsafeCode; }
            set { allowUnsafeCode = value; }
        }

        /// <summary>
        /// Should the compiler optimize the output.
        /// </summary>
        public bool AllowOptimizeCode
        {
            get { return allowOptimizeCode; }
            set { allowOptimizeCode = value; }
        }

        /// <summary>
        /// Should the compiler use multiple threads to compile the code.
        /// </summary>
        public bool AllowConcurrentCompile
        {
            get { return allowConcurrentCompile; }
            set { allowConcurrentCompile = value; }
        }

        public bool Deterministic
        {
            get { return deterministic; }
            set { deterministic = value; }
        }

        /// <summary>
        /// Should the compiler generate the output in memory or write it to the file system.
        /// </summary>
        public bool GenerateInMemory
        {
            get { return generateInMemory; }
            set { generateInMemory = value; }
        }

        /// <summary>
        /// Should the debug symbols pdb file be generated.
        /// When enabled, a pdb file or memory image will be generated depending upon the value of <see cref="GenerateInMemory"/>.
        /// </summary>
        public bool GenerateSymbols
        {
            get { return generateSymbols; }
            set { generateSymbols = value; }
        }

        /// <summary>
        /// The current compiler warning level.
        /// </summary>
        public int WarningLevel
        {
            get { return warningLevel; }
            set { warningLevel = value; }
        }

        /// <summary>
        /// The target C# language version that should be supported.
        /// </summary>
        public LanguageVersion LanguageVersion
        {
            get { return languageVersion; }
            set { languageVersion = value; }
        }

        /// <summary>
        /// The target platform architecture.
        /// </summary>
        public Platform TargetPlatform
        {
            get { return targetPlatform; }
            set { targetPlatform = value; }
        }

        /// <summary>
        /// A list of assembly references used by the compiler.
        /// </summary>
        public IList<string> References
        {
            get { return refeferences; }
        }

        /// <summary>
        /// A list of assembly reference assets used by the compiler.
        /// </summary>
        public IList<AssemblyReferenceAsset> ReferenceAssets
        {
            get { return referenceAssets; }
        }

        /// <summary>
        /// A list of define symbols used by the compiler.
        /// </summary>
        public IList<string> DefineSymbols
        {
            get { return defineSymbols; }
        }


        /// <summary>
        /// Should hot reloading be enabled while in play mode
        /// </summary>
        public bool AllowHotReloading
        {
            get { return allowHotReloading; }
            set { allowHotReloading = value; }
        }

        /// <summary>
        /// Should serialized fields be copied when a script is hot reloaded
        /// </summary>
        public bool HotReloadCopySerializedFields
        {
            get { return hotReloadCopySerializedFields; }
            set { hotReloadCopySerializedFields = value; }
        }

        /// <summary>
        /// Should non serialized fields be copied when a script is hot reloaded
        /// </summary>
        public bool HotReloadCopyNonSerializedFields
        {
            get { return hotReloadCopyNonSerializedFields; }
            set { hotReloadCopyNonSerializedFields = value; }
        }

        /// <summary>
        /// Should the original script be destroyed when a script is hot reloaded
        /// </summary>
        public bool HotReloadDestroyOriginalScript
        {
            get { return hotReloadDestroyOriginalScript; }
            set { hotReloadDestroyOriginalScript = value; }
        }

        /// <summary>
        /// Should the original script be disabled when a script is hot reloaded
        /// </summary>
        public bool HotReloadDisableOriginalScript
        {
            get { return hotReloadDisableOriginalScript; }
            set { hotReloadDisableOriginalScript = value; }
        }

        /// <summary>
        /// Should code security verification checks run on reloaded code. 
        /// It is recommended that you disable this option for quicker reload times.
        /// </summary>
        public bool HotReloadSecurityCheckCode
        {
            get { return hotReloadSecurityCheckCode; }
            set { hotReloadSecurityCheckCode = value; }
        }

        /// <summary>
        /// Should reference assemblies by automatically detected by parsing the .csproj file generated by Unity.
        /// It is recommended that you enable this option to avoid missing reference errors when attempting to hot reload code at runtime.
        /// </summary>
        public bool HotReloadUseCSharpProjectReferences
        {
            get { return hotReloadUseCSharpProjectReferences; }
            set { hotReloadUseCSharpProjectReferences = value; }
        }

        // Methods
        /// <summary>
        /// Load the settings from the resources folder.
        /// </summary>
        public static void LoadResources()
        {
            // Try to load from resources
            settings = Resources.Load<RoslynCSharp>(settingsName);

            // Check for error
            if(settings == null)
            {
                settings = CreateInstance<RoslynCSharp>();
                Debug.LogWarningFormat("Failed to load settings asset '{0}' from resources. Default values will be used", settingsName);
            }
        }

        /// <summary>
        /// Log a message to the Unity console if the <see cref="LogLevel"/> allows info messages.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="args">The string format arguments</param>
        public static void Log(string format, params object[] args)
        {
            if (Settings.LogLevel >= LogDetail.Info)
            {
                if (args.Length == 0)
                {
                    Debug.Log(format);
                }
                else
                {
                    Debug.LogFormat(format, args);
                }
            }
        }

        /// <summary>
        /// Log a warning to the Unity console if the <see cref="LogLevel"/> allows warning messages.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="args">The string format arguments</param>
        public static void LogWarning(string format, params object[] args)
        {
            if(settings.LogLevel >= LogDetail.Warnings)
            {
                if(args.Length == 0)
                {
                    Debug.LogWarning(format);
                }
                else
                {
                    Debug.LogWarningFormat(format, args);
                }
            }
        }

        /// <summary>
        /// Log an error to the Unity console if the <see cref="LogLevel"/> allows error messages.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="args">The string format arguments</param>
        public static void LogError(string format, params object[] args)
        {
            if(settings.LogLevel >= LogDetail.Errors)
            {
                if(args.Length == 0)
                {
                    Debug.LogError(format);
                }
                else
                {
                    Debug.LogErrorFormat(format, args);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Load the settings from a project asset.
        /// Editor only method. 
        /// </summary>
        /// <returns>The loaded settings asset or null if the asset was not found</returns>
        public static RoslynCSharp LoadAsset()
        {
            // Try to find the asset
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(RoslynCSharp).Name);

            if(guids.Length == 0)
            {
                Debug.LogWarningFormat("Failed to load settings asset '{0}'", typeof(RoslynCSharp));
                return null;
            }

            // Get the asset path
            string loadPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            // Load the asset
            return AssetDatabase.LoadAssetAtPath<RoslynCSharp>(loadPath);
        }

        /// <summary>
        /// Save the specified settings to a project asset.
        /// The asset will be saved to its loaded location or if this is the frist time creating the asset it will be placed in the folder 'Assets/Resources/'.
        /// Editor only method.
        /// </summary>
        /// <param name="settings">The settings to save</param>
        public static void SaveAsset(RoslynCSharp settings)
        {
            // Check for null settings
            if (settings == null)
            {
                Debug.LogWarning("Failed to save settings because they have been destroyed");
                return;
            }

            // Mark as changed
            EditorUtility.SetDirty(settings);

            // Do not save assets when a build is running - This can cause SIGSEGV and hard crash
            if (BuildPipeline.isBuildingPlayer == true)
                return;

            if (AssetDatabase.Contains(settings) == false)
            {
                // Create the folder first
                if(AssetDatabase.IsValidFolder("Assets/Resources") == false)
                    AssetDatabase.CreateFolder("Assets", "Resources");

                // Create the asset
                AssetDatabase.CreateAsset(settings, "Assets/Resources/" + settingsName + ".asset");
            }
        }
#endif
    }
}
