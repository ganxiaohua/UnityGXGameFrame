using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RoslynCSharp.Compiler
{
    public struct AssemblyReferenceFromAssemblyObject : IMetadataReferenceProvider
    {
        // Private
        private AssemblyReferenceFromFile reference;
        private Assembly assembly;

        // Properties
        public MetadataReference CompilerReference
        {
            get { return reference.CompilerReference; }
        }

        // Constructor
        public AssemblyReferenceFromAssemblyObject(Assembly assembly)
        {
            // Check for null
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            // Check for assembly path set
            if (string.IsNullOrEmpty(assembly.Location) == true)
                throw new ArgumentException("The specified assembly is not referencable because it's 'Location' property is empty. You will need to reference the assembly explicitly by filepath, filestream or assembly image data");

            this.assembly = assembly;
            this.reference = new AssemblyReferenceFromFile(assembly.Location);
        }
    }
}
