using System.Reflection;

namespace RoslynCSharp.Compiler
{
    public sealed class AssemblyOutput
    {
        // Private
        private Assembly outputAssembly = null;
        private string assemblyFilePath = null;
        private string assemblyPDBFilePath = null;
        private byte[] assemblyImage = null;
        private byte[] assemblyPDBImage = null;
        private bool isPatched = false;

        // Properties
        public Assembly OutputAssembly
        {
            get { return outputAssembly; }
            internal set { outputAssembly = value; }
        }

        public bool HasFilePath
        {
            get { return assemblyFilePath != null; }
        }

        public string AssemblyFilePath
        {
            get { return assemblyFilePath; }
            internal set { assemblyFilePath = value; }
        }

        public string AssemblyPDBFilePath
        {
            get { return assemblyPDBFilePath; }
            internal set { assemblyPDBFilePath = value; }
        }

        public byte[] AssemblyImage
        {
            get { return assemblyImage; }
            internal set { assemblyImage = value; }
        }

        public byte[] AssemblyPDBImage
        {
            get { return assemblyPDBImage; }
            internal set { assemblyPDBImage = value; }
        }

        public bool IsPatched
        {
            get { return isPatched; }
        }

        // Constructor
        internal AssemblyOutput() { }

        // Methods
        public void PatchAssemblyFilePath(string newAssemblyFilePath)
        {
            this.assemblyFilePath = newAssemblyFilePath;
            this.isPatched = true;
        }

        public void PatchAssemblyPDBFilePath(string newAssemblyPDBFilePath)
        {
            this.assemblyPDBFilePath = newAssemblyPDBFilePath;
            this.isPatched = true;
        }

        public void PatchAssemblyImage(byte[] newAssemblyImage)
        {
            this.assemblyImage = newAssemblyImage;
            this.isPatched = true;
        }

        public void PatchAssemblyPDBImage(byte[] newAssemblyPDBImage)
        {
            this.assemblyPDBImage = newAssemblyPDBImage;
            this.isPatched = true;
        }
    }
}
