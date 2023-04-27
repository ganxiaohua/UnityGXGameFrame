using System;
using System.Collections.Generic;

namespace RoslynCSharp.Compiler
{
    public sealed class AssemblyReferenceException : Exception
    {
        // Private
        private Exception[] referenceExceptions = null;

        // Internal
        internal static readonly string msg = "One or more assembly references could not be resolved. See ReferenceExceptions for more information";

        // Properties
        public Exception[] ReferenceExceptions
        {
            get { return referenceExceptions; }
        }

        // Constructor
        public AssemblyReferenceException(ICollection<Exception> allExceptions)
            : base(msg)
        {
            referenceExceptions = new Exception[allExceptions.Count];

            int index = 0;

            foreach (Exception e in allExceptions)
            {
                referenceExceptions[index] = e;
                index++;
            }
        }
    }
}
