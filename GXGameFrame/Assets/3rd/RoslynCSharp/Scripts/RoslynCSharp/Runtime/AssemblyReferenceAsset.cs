using System;
using UnityEngine;
using RoslynCSharp.Compiler;
using Microsoft.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace RoslynCSharp
{
    [CreateAssetMenu(fileName = "Assembly Reference Asset", menuName = "Roslyn C#/Assembly Reference Asset")]
    public class AssemblyReferenceAsset : ScriptableObject, IMetadataReferenceProvider, ISerializationCallbackReceiver
    {
        // Private
        [SerializeField, HideInInspector]
        private string assemblyName = "";

        [SerializeField, HideInInspector]
        private string assemblyPath = "";

        [SerializeField, HideInInspector]
        private byte[] assemblyImage = null;

        [SerializeField, HideInInspector]
        private long lastWriteTimeTicks = 0;

        private DateTime lastWriteTime = DateTime.Now;

        // Properties
        public MetadataReference CompilerReference
        {
            get { return GetReferences(); }
        }

        public string AssemblyName
        {
            get { return assemblyName; }
        }

        public string AssemblyPath
        {
            get { return assemblyPath; }
        }

        public byte[] AssemblyImage
        {
            get { return assemblyImage; }
        }

        public DateTime LastWriteTime
        {
            get { return lastWriteTime; }
        }

        public bool IsValid
        {
            get { return assemblyImage != null && assemblyImage.Length > 0; }
        }

        // Methods
        public void UpdateAssemblyReference(string referencePath, string assemblyName)
        {
            // Check for null
            if (referencePath == null) throw new ArgumentNullException(nameof(referencePath));
            if (referencePath == string.Empty) throw new ArgumentException("Path cannot be empty");
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));

            // Reset old values
            this.assemblyName = "";
            this.assemblyPath = "";
            this.assemblyImage = new byte[0];

            // Update the assembly
            if(File.Exists(referencePath) == true)
            {
                // Try to get relative
                //referencePath = Path.GetRe

                // Load the data
                this.assemblyName = assemblyName;
                this.assemblyPath = referencePath;
                this.assemblyImage = File.ReadAllBytes(referencePath);
                this.lastWriteTime = File.GetLastWriteTime(referencePath);
            }
        }

        public override string ToString()
        {
            string asmName = assemblyName;

            if (string.IsNullOrEmpty(asmName) == true)
                asmName = "<Invalid Assembly>";

            return string.Format("{0}({1})", nameof(AssemblyReferenceAsset), asmName);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Store last write time
            lastWriteTimeTicks = lastWriteTime.Ticks;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Create the last write time
            lastWriteTime = new DateTime(lastWriteTimeTicks);

            // Check for outdated - we may need to reload the serialized assembly image
            if(File.Exists(assemblyPath) == true)
            {
                // Get the last write time
                DateTime lastTime = File.GetLastWriteTime(assemblyPath);

                // Check for newer file
                if(lastTime > lastWriteTime)
                {
                    // We need to reload the data
                    UpdateAssemblyReference(assemblyPath, assemblyName);
                }
            }
        }

        private MetadataReference GetReferences()
        {
            if(File.Exists(assemblyPath) == false)
            {
                if(assemblyImage != null && assemblyImage.Length > 0)
                {
                    // Create refererence from image
                    return AssemblyReference.FromImage(assemblyImage).CompilerReference;
                }
            }
            else
            {
                // Create reference from path
                return AssemblyReference.FromNameOrFile(assemblyPath).CompilerReference;
            }

            throw new Exception("Assembly reference asset is invalid!");
        }
    }
}
