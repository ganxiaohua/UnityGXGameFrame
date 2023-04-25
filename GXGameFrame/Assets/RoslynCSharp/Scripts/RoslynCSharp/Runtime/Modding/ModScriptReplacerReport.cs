using System.Collections.Generic;
using UnityEngine;

namespace RoslynCSharp.Modding
{
    /// <summary>
    /// Contains useful information for a script replacement request.
    /// Any issues when replacing the script will be logged here so that you can see why replacement failed.
    /// </summary>
    public sealed class ModScriptReplacerReport
    {
        // Private
        private List<string> replaceMessages = new List<string>();
        private List<string> replaceWarnings = new List<string>();
        private List<string> replaceErrors = new List<string>();

        // Properties
        /// <summary>
        /// Returns true if this report contains any info messages.
        /// </summary>
        public bool HasMessages
        {
            get { return replaceMessages.Count > 0; }
        }

        /// <summary>
        /// Returns true if this report contains any warnings.
        /// </summary>
        public bool HasWarnings
        {
            get { return replaceWarnings.Count > 0; }
        }

        /// <summary>
        /// Returns true if this report contains any errors.
        /// </summary>
        public bool HasErrors
        {
            get { return replaceErrors.Count > 0; }
        }

        /// <summary>
        /// Get all info messages in this report as a read only collection.
        /// </summary>
        public IReadOnlyList<string> Messages
        {
            get { return replaceMessages; }
        }

        /// <summary>
        /// Get all warnings in this report as a read only collection.
        /// </summary>
        public IReadOnlyList<string> Warnings
        {
            get { return replaceWarnings; }
        }

        /// <summary>
        /// Get all errors in this report as a read only collection.
        /// </summary>
        public IReadOnlyList<string> Errors
        {
            get { return replaceErrors; }
        }       

        // Methods
        /// <summary>
        /// Add an info message to the report.
        /// </summary>
        /// <param name="message">The message to add</param>
        public void AddMessage(string message)
        {
            replaceMessages.Add(message);
        }

        /// <summary>
        /// Add an info message to the report using string formatting.
        /// </summary>
        /// <param name="messageFormat">The format of the input string</param>
        /// <param name="args">formatting arguments</param>
        public void AddMessageFormat(string messageFormat, params object[] args)
        {
            replaceMessages.Add(string.Format(messageFormat, args));
        }

        /// <summary>
        /// Add a warning to the report.
        /// </summary>
        /// <param name="warningMessage">The warning to add</param>
        public void AddWarning(string warningMessage)
        {
            replaceWarnings.Add(warningMessage);
        }

        /// <summary>
        /// Add a warning message to the report using string formatting.
        /// </summary>
        /// <param name="warningFormat">The format of the input string</param>
        /// <param name="args">formatting arguments</param>
        public void AddWarningFormat(string warningFormat, params object[] args)
        {
            replaceWarnings.Add(string.Format(warningFormat, args));
        }

        /// <summary>
        /// Add an error to the report.
        /// </summary>
        /// <param name="errorMessage">The error to add</param>
        public void AddError(string errorMessage)
        {
            replaceErrors.Add(errorMessage);
        }

        /// <summary>
        /// Add an error to the report using string formatting.
        /// </summary>
        /// <param name="errorFormat">The format of the input string</param>
        /// <param name="args">Formatting arguments</param>
        public void AddErrorFormat(string errorFormat, params object[] args)
        {
            replaceErrors.Add(string.Format(errorFormat, args));
        }

        /// <summary>
        /// Log all messages, warnings and errors in this report to the Unity editor console window.
        /// </summary>
        public void LogToConsole()
        {
            if (HasWarnings == false && HasErrors == false)
                return;

            Debug.Log("__Mod Script Replacer Report__");

            foreach (string message in replaceMessages)
                Debug.Log(message);

            foreach (string warning in replaceWarnings)
                Debug.LogWarning(warning);

            foreach (string error in replaceErrors)
                Debug.LogError(error);
        }
    }
}
