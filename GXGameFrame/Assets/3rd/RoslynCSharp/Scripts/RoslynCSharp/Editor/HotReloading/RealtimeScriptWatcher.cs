using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using RoslynCSharp.Modding;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using RoslynCSharp.Project;
using RoslynCSharp.Compiler;
using UnityEditor.Compilation;

namespace RoslynCSharp.HotReloading
{
    public class RealtimeScriptWatcher
    {
        // Private
        private static bool reloadingFlag = false;

        private ScriptDomain domain = null;
        private FileSystemWatcher watcher = null;
        private Queue<FileSystemEventArgs> fileEvents = new Queue<FileSystemEventArgs>();
        
        // Constructor
        public RealtimeScriptWatcher(ScriptDomain domain, string folderPath)
        {
            this.domain = domain;
            this.watcher = new FileSystemWatcher(folderPath);
            this.watcher.Filter = "*.cs";
            this.watcher.IncludeSubdirectories = true;
            this.watcher.EnableRaisingEvents = true;

            // Add listener
            this.watcher.Changed += OnFileWatcherChanged;

            EditorApplication.update += OnEditorUpdate;
            SceneView.duringSceneGui += OnSceneView;
        }

        // Methods
        private void OnFileWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if(e.ChangeType == WatcherChangeTypes.Changed)
            {
                lock (fileEvents)
                {
                    fileEvents.Enqueue(e);
                }
            }
        }

        private void OnEditorUpdate()
        {
            while (fileEvents.Count > 0)
            {
                FileSystemEventArgs e = fileEvents.Dequeue();

                // Check for play mode
                if (Application.isPlaying == true)
                {
                    // Reload all scripts in order
                    OnReloadScript(e.FullPath);
                }
            }
        }

        private void OnSceneView(SceneView view)
        {
            if (reloadingFlag == true)
                GUILayout.Label("Hot Reloading...");
        }

        private void OnReloadScript(string path)
        {
            reloadingFlag = true;

            // Time the reload duration
            Stopwatch timer = Stopwatch.StartNew();

            // Use settings options
            ScriptSecurityMode securityMode = ScriptSecurityMode.UseSettings;

            // Check for disabled security checks
            if (RoslynCSharp.Settings.HotReloadSecurityCheckCode == false)
                securityMode = ScriptSecurityMode.EnsureLoad;


            // Check for parse project file
            if (RoslynCSharp.Settings.HotReloadUseCSharpProjectReferences == true)
            {
                CSharpProject projectFile = GetCSharpProjectForScript(path);

                // Try to get assembly csharp
                //if (CSharpProjectFile.TryParseUnityFile(UnityCSharpProjectFile.Assembly_CSharp, out projectFile) == false)
                //{
                //    Debug.LogWarning("Hot reload: Failed to parse Assembly-CSharp project file. Hot reload may fail due to missing references");
                //}
                //else
                if(projectFile == null)
                {
                    Debug.LogWarningFormat("Hot reload: Failed to parse project file for script: {0}. Hot reload may fail due to missing references", path);
                }
                else
                {
                    // Clear any current references
                    domain.RoslynCompilerService.ReferenceAssemblies.Clear();

                    // Add reference to assembly-csharp
                    domain.RoslynCompilerService.ReferenceAssemblies.Add(AssemblyReference.FromNameOrFile(projectFile.AssemblyName + ".dll"));

                    // Build references from .csproj file
                    foreach (IMetadataReferenceProvider reference in projectFile.GetMetadataReferences())
                        domain.RoslynCompilerService.ReferenceAssemblies.Add(reference);
                }
            }

            // Recompile the script
            ScriptAssembly asm = domain.CompileAndLoadFile(path, securityMode);

            // Check for success
            if(asm == null)
            {
                domain.LogCompilerOutputToConsole();
                return;
            }

            // Find the type for the changed source file
            Type mainMonoType = GetMainMonoTypeForSourceFile(path);

            // Find type with matching full name
            ScriptType reloadType = asm.FindType(mainMonoType);

            if(reloadType != null)
            {
                ScriptReplacerOptions options = ScriptReplacerOptions.DontRequireAttribute;

                // Setup options
                if (RoslynCSharp.Settings.HotReloadCopySerializedFields == true) options |= ScriptReplacerOptions.CopySerializeFields;
                if (RoslynCSharp.Settings.HotReloadCopyNonSerializedFields == true) options |= ScriptReplacerOptions.CopyNonSerializeFields;
                if (RoslynCSharp.Settings.HotReloadDestroyOriginalScript == true) options |= ScriptReplacerOptions.DestroyOriginalScript;
                if (RoslynCSharp.Settings.HotReloadDisableOriginalScript == true) options |= ScriptReplacerOptions.DisableOriginalScript;

                // Run the script replacement
                ModScriptReplacerReport report;
                ModScriptReplacer.ReplaceScriptsForActiveScene(reloadType, out report, options);

                // Check for any issues
                report.LogToConsole();
            }

            // Log reload info
            Debug.Log("Hot Reload: '" + path + "', Reload Time: " + timer.ElapsedMilliseconds.ToString() + "ms");

            reloadingFlag = false;
        }

        private Type GetMainMonoTypeForSourceFile(string path)
        {
            path = path.Replace("\\", "/");
            path = FileUtil.GetProjectRelativePath(path);

            // Load the mono script
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            // Get the mono class
            return script.GetClass();
        }

        private static CSharpProject GetCSharpProjectForScript(string scriptPath)
        {
            // Check for error
            if (string.IsNullOrEmpty(scriptPath) == true)
                return null;

            // Get relative path
            if(Path.IsPathRooted(scriptPath) == true)
            {
                // Convert to unix path
                string normalizeScriptPath = scriptPath.Replace('\\', '/');

                // Get relative path
                scriptPath = FileUtil.GetProjectRelativePath(normalizeScriptPath);

                // Check for failed - script does not exist inside project or path is invalid/incorrectly formatted
                if (string.IsNullOrEmpty(scriptPath) == true)
                    return null;
            }

            // Get the assembly name - This includes the .dll extension (We can assume that .csproj file has same name as assembly)
            string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);

            // Strip extension
            string assemblyNameOnly = Path.GetFileNameWithoutExtension(assemblyName);

            // Create the full path
            string cSharpProjectPath = Path.Combine(CSharpProjectFile.UnityProjectDirectory, assemblyNameOnly + ".csproj");



            CSharpProjectFile projectFile;
            
            // Check if file exists
            if(string.IsNullOrEmpty(cSharpProjectPath) == false && File.Exists(cSharpProjectPath) == true)
            {
                // Try to parse the file
                if (CSharpProjectFile.TryParseFile(cSharpProjectPath, out projectFile) == false)
                    projectFile = null;
            }
            // Fallback to assembly-csharp
            else
            {
                if (CSharpProjectFile.TryParseUnityFile(UnityCSharpProjectFile.Assembly_CSharp, out projectFile) == false)
                    projectFile = null;
            }

            return projectFile;
        }
    }
}
