using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace RoslynCSharp.Project
{
    /// <summary>
    /// Represents a predefined Unity project file (.csproj) that are generated automatically by Unity.
    /// </summary>
    public enum UnityCSharpProjectFile
    {
        /// <summary>
        /// The main assembly where all runtime scripts are added, unless assembly definition files are used.
        /// </summary>
        Assembly_CSharp,
        /// <summary>
        /// The first pass assembly where all runtime scripts located inside the 'Plugins' folder are added, unless assembly definition files are used.
        /// </summary>
        Assembly_CSharp_Firstpass,
        /// <summary>
        /// The main assembly where all editor scripts located inside the 'Editor' folder are added, unless assembly definition files are used.
        /// </summary>
        Assembly_CSharp_Editor,
        /// <summary>
        /// The first pass assembly where all editor scripts located inside the 'Editor/Plugins' folder are added, unless assembly definition files are used.
        /// </summary>
        Assembly_CSharp_Editor_Firstpass,
    }

    /// <summary>
    /// Represents a .csproj file at a specific file location.
    /// Provides access to common project properties such as sources, references and more.
    /// </summary>
    public class CSharpProjectFile : CSharpProject
    {
        // Private
        private static readonly Dictionary<UnityCSharpProjectFile, string> unityProjectFilesLookup = new Dictionary<UnityCSharpProjectFile, string>
        {
            { UnityCSharpProjectFile.Assembly_CSharp, "Assembly-CSharp" },
            { UnityCSharpProjectFile.Assembly_CSharp_Editor, "Assembly-CSharp-Editor" },
            { UnityCSharpProjectFile.Assembly_CSharp_Firstpass, "Assembly-CSharp-firstpass" },
            { UnityCSharpProjectFile.Assembly_CSharp_Editor_Firstpass, "Assembly-CSharp-Editor-firstpass" },
        };

        private string projectPath = "";
        private IMetadataReferenceProvider[] metadataReferences = null;
        private IMetadataReferenceProvider[] projectMetadataReferences = null;
        private CSharpProjectFile[] projectReferences = null;

        // Properties
        /// <summary>
        /// Get the folder path where the current Unity project is located.
        /// This will always be the parent folder to 'Assets'.
        /// </summary>
        public static string UnityProjectDirectory
        {
            get { return Directory.GetParent(Application.dataPath).FullName; }
        }

        /// <summary>
        /// Get the file path where this project file was loaded from.
        /// </summary>
        public string ProjectPath
        {
            get { return projectPath; }
        }

        /// <summary>
        /// Get the folder path where this project file exists.
        /// </summary>
        public string ProjectDirectory
        {
            get { return Directory.GetParent(projectPath).FullName; }
        }

        // Constructor
        private protected CSharpProjectFile(string projectPath)
        {
            this.projectPath = projectPath;
        }

        // Methods
        /// <summary>
        /// Get all project references for this project as <see cref="IMetadataReferenceProvider"/> that can be passed straight to the compiler.
        /// Project references will not be resolved by their full path, only name and file extension.
        /// </summary>
        /// <returns></returns>
        public override IMetadataReferenceProvider[] GetMetadataReferences()
        {
            // Check for no project references
            if (ProjectReferences.Count == 0)
                return base.GetMetadataReferences();

            if(metadataReferences == null)
            {
                metadataReferences = base.GetMetadataReferences()
                    .Concat(GetMetadataProjectReferencesOnly())
                    .ToArray();
            }
            return metadataReferences;
        }

        public IMetadataReferenceProvider[] GetMetadataProjectReferencesOnly()
        {
            if (projectMetadataReferences == null)
            {
                // Get all project references
                CSharpProjectFile[] projectReferences = GetProjectReferences();

                // Allocate return array
                projectMetadataReferences = new IMetadataReferenceProvider[projectReferences.Length];

                // Process all references
                for (int i = 0; i < projectReferences.Length; i++)
                {
                    // Create the reference
                    projectMetadataReferences[i] = AssemblyReference.FromNameOrFile(
                        projectReferences[i].AssemblyName + ".dll");
                }
            }

            return projectMetadataReferences;
        }

        /// <summary>
        /// Get all <see cref="CSharpProjectFile"/> that are referenced by this project.
        /// </summary>
        /// <returns></returns>
        public CSharpProjectFile[] GetProjectReferences()
        {
            if(projectReferences == null)
            {
                // Get the project references
                IReadOnlyList<string> references = ProjectReferences;

                // Allocate return array
                projectReferences = new CSharpProjectFile[references.Count];

                // Process all references
                for (int i = 0; i < references.Count; i++)
                {
                    string workingFolder = ProjectDirectory;

                    // Get the full path
                    string projectReferencePath = Path.Combine(workingFolder, references[i]);

                    // Parse the project file
                    projectReferences[i] = CSharpProjectFile.ParseFile(projectReferencePath);
                }
            }

            return projectReferences;
        }

        /// <summary>
        /// Parse a .csproj from the specified file path.
        /// This method will thrown an exception if anything goes wrong while parsing.
        /// </summary>
        /// <param name="filePath">The file path of the .csproj file to parse</param>
        /// <returns>A <see cref="CSharpProject"/> representing the loaded project file</returns>
        public static CSharpProjectFile ParseFile(string filePath)
        {
            CSharpProjectFile projectFile;

            // Try to parse the project file
            if (TryParseFile(filePath, out projectFile) == false)
            {
                // Throw exception on error
                if (projectFile.ParseException != null)
                    throw projectFile.ParseException;

                return null;
            }

            return projectFile;
        }

        /// <summary>
        /// Try to parse a .csproj from the specified file path.
        /// This method will not throw an excepion. Use <see cref="ParseException"/> to access any exception that were thrown.
        /// </summary>
        /// <param name="filePath">The file path of the .csproj file to parse</param>
        /// <param name="project">A <see cref="CSharpProject"/> representing the loaded project file</param>
        /// <returns>True if parsing was successful or false if not</returns>
        public static bool TryParseFile(string filePath, out CSharpProjectFile projectFile)
        {
            // Create the project
            projectFile = new CSharpProjectFile(filePath);

            try
            {
                // Create reader
                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    // Try to parse
                    projectFile.Parser.ParseCSharpProject(reader);

                    // Check for exception
                    if (projectFile.ParseException != null)
                    {
                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                projectFile.ParseException = e;
            }

            return true;
        }

        public static CSharpProjectFile ParseUnityFile(UnityCSharpProjectFile unityProjectFile)
        {
            return ParseFile(GetUnityProjectFileLocation(unityProjectFile));
        }

        public static bool TryParseUnityFile(UnityCSharpProjectFile unityProjectFile, out CSharpProjectFile projectFile)
        {
            return TryParseFile(GetUnityProjectFileLocation(unityProjectFile), out projectFile);
        }

        public static CSharpProjectFile ParseUnityFile(string assemblyNameOnly)
        {
            // Make sure extension is intact
            if (Path.HasExtension(assemblyNameOnly) == false)
                assemblyNameOnly += ".csproj";

            return ParseFile(Path.Combine(UnityProjectDirectory, assemblyNameOnly));
        }

        public static bool TryParseUnityFile(string assemblyNameOnly, out CSharpProjectFile projectFile)
        {
            // Make sure extension is intact
            if (Path.HasExtension(assemblyNameOnly) == false)
                assemblyNameOnly += ".csproj";

            return TryParseFile(Path.Combine(UnityProjectDirectory, assemblyNameOnly), out projectFile);
        }

        /// <summary>
        /// Try to get the file path for the specified Unity build in C# project.
        /// The return value will be null if the project file could not be found.
        /// This usually means that Unity has not generated the project yet, or your project does not require the specifed C# project to be created (No editor scripts for example).
        /// </summary>
        /// <param name="unityProjectFile">The <see cref="UnityCSharpProjectFile"/> to try and find the path for</param>
        /// <returns>A file path if the project was found or null if not</returns>
        public static string GetUnityProjectFileLocation(UnityCSharpProjectFile unityProjectFile)
        {
            string searchFolder = UnityProjectDirectory;

            // Find all project files
            string[] files = Directory.GetFiles(searchFolder, "*.csproj");

            foreach(string file in files)
            {
                // Get the file name only
                string fileNameOnly = Path.GetFileNameWithoutExtension(file);

                // Check for path
                if (string.Compare(unityProjectFilesLookup[unityProjectFile], fileNameOnly) == 0)
                    return file;
            }

            // Project not found
            return null;
        }
    }
}
