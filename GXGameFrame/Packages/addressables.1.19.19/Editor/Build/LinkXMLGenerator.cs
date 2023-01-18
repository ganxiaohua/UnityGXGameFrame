using System;
using System.Collections.Generic;

namespace UnityEditor.AddressableAssets.Build
{
    /// <summary>
    /// This can be used to create a LinkXml for your build.  This will ensure that the desired runtime types are packed into the build.
    /// </summary>
    [Obsolete("UnityEditor.AddressableAssets.Build.LinkXmlGenerator is obsolete. Use UnityEditor.Build.Pipeline.Utilities.LinkXmlGenerator instead.")]
    public class LinkXmlGenerator : UnityEditor.Build.Pipeline.Utilities.LinkXmlGenerator
    {
#pragma warning disable CA1061 // Do not hide base class methods
        /// <inheritdoc />
        public new void AddTypes(params Type[] types) { base.AddTypes(types); }
        /// <inheritdoc />
        public new void AddTypes(IEnumerable<Type> types) { base.AddTypes(types); }
        /// <inheritdoc />
        public new void SetTypeConversion(Type a, Type b) { base.SetTypeConversion(a, b); }
        /// <inheritdoc />
        public new void Save(string path) { base.Save(path); }
#pragma warning restore CA1061 // Do not hide base class methods
    }
}
