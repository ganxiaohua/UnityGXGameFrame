using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace RoslynCSharp.Compiler
{
    public struct AssemblyReferenceFromStream : IMetadataReferenceProvider
    {
        // Private
        private Stream stream;

        // Properties
        public Stream Stream
        {
            get { return stream; }
        }

        public MetadataReference CompilerReference
        {
            get { return MetadataReference.CreateFromStream(stream); }
        }

        // Constructor
        public AssemblyReferenceFromStream(Stream assemblyStream)
        {
            // Check for null
            if (assemblyStream == null)
                throw new ArgumentNullException(nameof(assemblyStream));

            // Check for readable
            if (assemblyStream.CanRead == false)
                throw new ArgumentException("Assembly stream must be readable");

            this.stream = assemblyStream;
        }
    }
}
