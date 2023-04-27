using RoslynCSharp.Compiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace RoslynCSharp.Project
{
    /// <summary>
    /// Represents a .csproj file contents and provides access to common project properties such as sources, references and more.
    /// </summary>
    public class CSharpProject
    {
        // Private        
        private CSharpProjectParser parser = new CSharpProjectParser();

        // Protected
        protected IMetadataReferenceProvider[] compilerReferences = null;

        // Properties
        internal CSharpProjectParser Parser
        {
            get { return parser; }
        }

        /// <summary>
        /// Get the name of the assembly for the C# project.
        /// </summary>
        public string AssemblyName
        {
            get { return parser.AssemblyName; }
        }

        /// <summary>
        /// Get a collection of all C# source file paths included in this project.
        /// </summary>
        public IReadOnlyList<string> Sources
        {
            get { return parser.Sources; }
        }

        /// <summary>
        /// Get a collection of all C# reference assembly paths included in this project.
        /// The 'HintPath' node for the reference will be used if it is available.
        /// </summary>
        public IReadOnlyList<string> References
        {
            get { return parser.References; }
        }

        /// <summary>
        /// Get a collection of all C# project names that are referenced in this project.
        /// Project references are used to reference another .csproj.
        /// </summary>
        public IReadOnlyList<string> ProjectReferences
        {
            get { return parser.ProjectReferences; }
        }

        /// <summary>
        /// Get a collection of all compiler define symbols that are used in this project.
        /// </summary>
        public IReadOnlyList<string> Defines
        {
            get { return parser.Defines; }
        }

        /// <summary>
        /// Get an exception that wa thrown while trying to parse the project.
        /// </summary>
        public Exception ParseException
        {
            get { return parser.ParseException; }
            protected internal set { parser.ParseException = value; }
        }

        // Constructor
        private protected CSharpProject() { }

        // Methods
        /// <summary>
        /// Get all references for this project as <see cref="IMetadataReferenceProvider"/> that can be passed straight to the compiler.
        /// </summary>
        /// <returns></returns>
        public virtual IMetadataReferenceProvider[] GetMetadataReferences()
        {
            if (compilerReferences == null)
            {
                // Get references
                IReadOnlyList<string> references = References;

                // Create arrays
                compilerReferences = new IMetadataReferenceProvider[references.Count];

                // Process all references
                for (int i = 0; i < references.Count; i++)
                {
                    // Create the reference
                    compilerReferences[i] = AssemblyReference.FromNameOrFile(references[i]);
                }
            }

            return compilerReferences;
        }

        /// <summary>
        /// Parse a .csproj from the specified string containing the raw xml data.
        /// This method will thrown an exception if anything goes wrong while parsing.
        /// </summary>
        /// <param name="csharpProjectText">A string containing .csproj formatted data</param>
        /// <returns>A <see cref="CSharpProject"/> for the specified project string</returns>
        public static CSharpProject ParseText(string csharpProjectText)
        {
            CSharpProject project;

            // Try to parse the project text
            if (TryParseText(csharpProjectText, out project) == false)
            {
                // Throw exception on error
                if (project.ParseException != null)
                    throw project.ParseException;

                return null;
            }

            return project;
        }

        /// <summary>
        /// Try to parse a .csproj from the specified string containing the raw xml data.
        /// This method will not throw an excepion. Use <see cref="ParseException"/> to access any exception that were thrown.
        /// </summary>
        /// <param name="csharpProjectText">A string containing .csproj formatted data</param>
        /// <param name="project">A <see cref="CSharpProject"/> for the specified project string</param>
        /// <returns>True if parsing was successful or false if not</returns>
        public static bool TryParseText(string csharpProjectText, out CSharpProject project)
        {
            // Create the project
            project = new CSharpProject();

            try
            {
                // Create reader
                using (XmlReader reader = XmlReader.Create(new StringReader(csharpProjectText)))
                {
                    // Try to parse
                    project.Parser.ParseCSharpProject(reader);

                    // Check for exception
                    if (project.ParseException != null)
                    {
                        project = null;
                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                project.ParseException = e;
            }

            return true;
        }
    }
}
