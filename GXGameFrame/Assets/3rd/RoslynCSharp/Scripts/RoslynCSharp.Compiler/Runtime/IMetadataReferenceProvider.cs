using Microsoft.CodeAnalysis;

namespace RoslynCSharp.Compiler
{
    public interface IMetadataReferenceProvider
    {
        // Properties
        MetadataReference CompilerReference { get; }
    }
}
