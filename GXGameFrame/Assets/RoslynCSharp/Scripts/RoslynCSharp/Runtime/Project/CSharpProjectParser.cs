using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RoslynCSharp.Project
{
    internal sealed class CSharpProjectParser
    {
        // Private
        private static readonly XNamespace scheme = "http://schemas.microsoft.com/developer/msbuild/2003";

        private string assemblyName = "";
        private List<string> sources = null;
        private List<string> references = null;
        private List<string> projectReferences = null;
        private List<string> defines = null;
        private Exception parseException = null;

        // Properties
        public string AssemblyName
        {
            get { return assemblyName; }
        }

        public IReadOnlyList<string> Sources
        {
            get { return sources; }
        }

        public IReadOnlyList<string> References
        {
            get { return references; }
        }

        public IReadOnlyList<string> ProjectReferences
        {
            get { return projectReferences; }
        }

        public IReadOnlyList<string> Defines
        {
            get { return defines; }
        }

        public Exception ParseException
        {
            get { return parseException; }
            internal set { parseException = value; }
        }

        // Methods
        public bool ParseCSharpProject(XmlReader reader)
        {
            XDocument doc = XDocument.Load(reader);

            // Parse assembly name
            if (ParseAssemblyName(doc) == false)
                return false;

            // Parse all sources
            if (ParseSourceFiles(doc) == false)
                return false;

            // Parse references
            if (ParseReferences(doc) == false)
                return false;

            // Parse project references
            if (ParseProjectReferences(doc) == false)
                return false;

            // Parse defines
            if (ParseDefines(doc) == false)
                return false;

            // COmpleted without error
            return true;
        }

        private bool ParseAssemblyName(XDocument document)
        {
            try
            {
                // Find the node
                string name = document
                    .Descendants()
                    .SingleOrDefault(r => r.Name.LocalName == "AssemblyName")
                    .Value;

                // Assign the value
                this.assemblyName = name;
            }
            catch(Exception e)
            {
                // Node error
                this.parseException = e;
                return false;
            }

            return true;
        }

        private bool ParseSourceFiles(XDocument document)
        {
            try
            {
                // Find the node
                IEnumerable<string> sources = document
                    .Element(scheme + "Project")
                    .Elements(scheme + "ItemGroup")
                    .Elements(scheme + "Compile")
                    .Select(r => r.FirstAttribute.Value);

                // Add all
                this.sources = new List<string>(sources);
            }
            catch(Exception e)
            {
                // Node error
                this.parseException = e;
                return false;
            }

            return true;
        }

        private bool ParseReferences(XDocument document)
        {
            try
            {
                // Find the node
                IEnumerable<string> references = document
                    .Element(scheme + "Project")
                    .Elements(scheme + "ItemGroup")
                    .Elements(scheme + "Reference")
                    .Select(r => string.IsNullOrEmpty(r.Value) ? r.FirstAttribute.Value : r.Value);

                // Add all references
                this.references = new List<string>(references.Select(r => r.Trim(' ', '\t', '"', '\n')));
            }
            catch(Exception e)
            {
                this.parseException = e;
                return false;
            }

            return true;
        }

        private bool ParseProjectReferences(XDocument document)
        {
            try
            {
                // Find the node
                IEnumerable<string> projectReferences = document
                    .Element(scheme + "Project")
                    .Elements(scheme + "ItemGroup")
                    .Elements(scheme + "ProjectReference")
                    .Select(r => r.FirstAttribute.Value);

                // Add all project references
                this.projectReferences = new List<string>(projectReferences.Select(r => r.Trim(' ', '\t', '"', '\n')));
            }
            catch (Exception e)
            {
                this.parseException = e;
                return false;
            }

            return true;
        }

        private bool ParseDefines(XDocument document)
        {
            try
            {
                // Find the node
                IEnumerable<string> defines = document
                    .Element(scheme + "Project")
                    .Elements(scheme + "PropertyGroup")
                    .Where(r => r.FirstAttribute != null)
                    //.Where(r => r.FirstAttribute.Value.Contains("Release"))
                    .Elements(scheme + "DefineConstants")
                    .Select(r => r.Value);

                // Add all defines
                this.defines = new List<string>();

                // Split by delimiter
                foreach (string define in defines)
                    this.defines.AddRange(define.Split(';'));
            }
            catch (Exception e)
            {
                this.parseException = e;
                return false;
            }

            return true;
        }
    }
}
