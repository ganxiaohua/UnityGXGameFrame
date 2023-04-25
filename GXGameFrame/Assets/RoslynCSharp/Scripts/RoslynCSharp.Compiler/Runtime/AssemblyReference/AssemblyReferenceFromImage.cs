using Microsoft.CodeAnalysis;
using System;

namespace RoslynCSharp.Compiler
{
    public struct AssemblyReferenceFromImage : IMetadataReferenceProvider
    {
        // Private
        private byte[] image;

        // Properties
        public byte[] Image
        {
            get { return image; }
        }

        public MetadataReference CompilerReference
        {
            get { return MetadataReference.CreateFromImage(image); }
        }

        // Constructor
        public AssemblyReferenceFromImage(byte[] assemblyImage)
        {
            // Check for null
            if (assemblyImage == null)
                throw new ArgumentNullException(nameof(assemblyImage));

            this.image = assemblyImage;
        }        
    }
}
