using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace RoslynCSharp.Compiler
{
    public struct AssemblyReference : IMetadataReferenceProvider
    {
        // Private
        private IMetadataReferenceProvider reference;
        private string assemblyName;
        private AppDomain domain;

        // Properties
        public MetadataReference CompilerReference
        {
            get { return reference.CompilerReference; }
        }

        // Constructor
        public AssemblyReference(string assemblyName, AppDomain domain = null)
        {
            // Check for null
            if (assemblyName == null)
                throw new ArgumentNullException(nameof(assemblyName));

            // Get valid domain
            if (domain == null)
                domain = AppDomain.CurrentDomain;

            reference = null;

            foreach(Assembly assembly in domain.GetAssemblies())
            {
                if(assembly.GetName().Name == assemblyName)
                {
                    reference = new AssemblyReferenceFromAssemblyObject(assembly);
                    break;
                }
            }

            if (reference == null)
                throw new ArgumentException(string.Format("Failed to resolve assembly reference '{0}'. Ensure that the assembly is loaded and that the specified name is correct", assemblyName));

            this.assemblyName = assemblyName;
            this.domain = domain;
        }

        // Methods
        public static IMetadataReferenceProvider FromNameOrFile(string assemblyNameOrFilePath, AppDomain searchDomain = null)
        {
            // Check for valid reference path
            if (File.Exists(assemblyNameOrFilePath) == false)
            {
                // Strip extensions etc
                string assemblyName = Path.GetFileNameWithoutExtension(assemblyNameOrFilePath);

                // Create reference
                return new AssemblyReference(assemblyName, searchDomain);
            }
            else
            {
                return new AssemblyReferenceFromFile(assemblyNameOrFilePath);
            }
        }

        public static IMetadataReferenceProvider FromAssembly(Assembly assembly)
        {
            return new AssemblyReferenceFromAssemblyObject(assembly);
        }

        public static IMetadataReferenceProvider FromStream(Stream assemblyStream)
        {
            return new AssemblyReferenceFromStream(assemblyStream);
        }

        public static IMetadataReferenceProvider FromImage(byte[] assemblyImage)
        {
            return new AssemblyReferenceFromImage(assemblyImage);
        }
    }

    public static class AssemblyReferenceExtensions
    {
        public static bool TryResolveReference(this IMetadataReferenceProvider provider)
        {
            MetadataReference r;
            Exception e;
            return TryResolveReference(provider, out r, out e);
        }

        public static bool TryResolveReference(this IMetadataReferenceProvider provider, out MetadataReference reference, out Exception error)
        {
            reference = null;
            error = null;

            // Check for invalid provider
            if (provider == null)
                return false;

            try
            {
                reference = provider.CompilerReference;
                return true;
            }
            catch (Exception e)
            {
                string providerMsg = (reference != null) ? reference.ToString() : (provider != null) ? provider.ToString() : "Unknown";

                error = new TargetException(string.Format("Failed to resolve assembly reference '{0}'", providerMsg), e);
                return false;
            }
        }
    }
}
