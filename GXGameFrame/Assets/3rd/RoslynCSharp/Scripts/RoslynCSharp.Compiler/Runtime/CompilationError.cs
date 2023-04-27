using Microsoft.CodeAnalysis;

namespace RoslynCSharp.Compiler
{
    public sealed class CompilationError
    {
        // Private
        private Diagnostic diagnostic = null;
        private string code = null;
        private string message = null;
        private Location location = Location.None;
        private DiagnosticSeverity severity = DiagnosticSeverity.Info;
        private bool isWarningAsError = false;
        private bool isSupressed = false;

        // Properties
        public string Code
        {
            get { return code; }
        }

        public string Message
        {
            get { return message; }
        }

        public string SourceFile
        {
            get { return location.SourceTree.FilePath; }
        }

        public int SourceLine
        {
            get { return location.GetLineSpan().StartLinePosition.Line; }
        }

        public int SourceColumn
        {
            get { return location.GetLineSpan().StartLinePosition.Character; }
        }

        public bool IsInfo
        {
            get { return severity == DiagnosticSeverity.Info; }
        }

        public bool IsWarning
        {
            get { return severity == DiagnosticSeverity.Warning; }
        }

        public bool IsError
        {
            get { return severity == DiagnosticSeverity.Error; }
        }

        public bool IsWarningAsError
        {
            get { return isWarningAsError; }
        }

        public bool IsSuppressed
        {
            get { return isSupressed; }
        }

        // Internal
        internal CompilationError(Diagnostic diagnostic)
        {
            this.diagnostic = diagnostic;
            this.code = diagnostic.Id;
            this.message = diagnostic.GetMessage();
            this.location = diagnostic.Location;
            this.severity = diagnostic.Severity;
            this.isWarningAsError = diagnostic.IsWarningAsError;
            this.isSupressed = diagnostic.IsSuppressed;
        }

        // Methods
        public override string ToString()
        {
            return diagnostic.ToString();
        }
    }
}
