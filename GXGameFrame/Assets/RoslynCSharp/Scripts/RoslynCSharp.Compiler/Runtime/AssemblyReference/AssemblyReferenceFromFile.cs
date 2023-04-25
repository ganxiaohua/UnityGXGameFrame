using System;
using Microsoft.CodeAnalysis;

namespace RoslynCSharp.Compiler
{
    public struct AssemblyReferenceFromFile : IMetadataReferenceProvider
    {
        // Private
        private string filePath;

        // Properties
        public string FilePath
        {
            get { return filePath; }
        }

        public MetadataReference CompilerReference
        {
            get { return MetadataReference.CreateFromFile(filePath); }
        }

        // Constructor
        public AssemblyReferenceFromFile(string assemblyFile)
        {
            // Check for null
            if (assemblyFile == null)
                throw new ArgumentNullException(nameof(assemblyFile));

            this.filePath = assemblyFile;
        }
    }
}
