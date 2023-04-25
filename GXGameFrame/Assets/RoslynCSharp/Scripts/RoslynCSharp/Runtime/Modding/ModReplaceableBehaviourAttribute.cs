using System;

namespace RoslynCSharp.Modding
{
    /// <summary>
    /// Add this attribute to a component type to allow it to be replaced with an externally compiled mod script.
    /// Used in conjuection with the <see cref="ModScriptReplacer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ModReplaceableBehaviourAttribute : Attribute
    {
        // Private
        private string replaceScriptName = "";
        private Type requireBaseType = null;
        private Type[] requireInterfaceTypes = null;

        // Properties
        /// <summary>
        /// The name of the script override. The external script must define a class which matches this type name exactly in order to be replaced.
        /// </summary>
        public string ReplaceScriptName
        {
            get { return replaceScriptName; }
        }

        /// <summary>
        /// An optional base type that the external class must inherit from in order for script replacement to occur.
        /// </summary>
        public Type RequireBaseType
        {
            get { return requireBaseType; }
        }

        /// <summary>
        /// An optional array of interface types that the external class must implement in order for script replacement to occur.
        /// </summary>
        public Type[] RequireInterfaceTypes
        {
            get { return requireInterfaceTypes; }
        }

        // Constructor
        /// <summary>
        /// Crfeate a new instance.
        /// </summary>
        /// <param name="replaceScriptName">The replacement script name</param>
        /// <param name="requireBaseType">An optional base type that the external script must inherit from, or null if a required base type is not needed</param>
        /// <param name="requireInterfaceTypes">An optional array of interface types that the external script must implement, or an empty array if interface implementations are not needed</param>
        public ModReplaceableBehaviourAttribute(string replaceScriptName = "", Type requireBaseType = null, params Type[] requireInterfaceTypes)
        {
            this.replaceScriptName = replaceScriptName;
            this.requireBaseType = requireBaseType;
            this.requireInterfaceTypes = requireInterfaceTypes;
        }
    }
}
