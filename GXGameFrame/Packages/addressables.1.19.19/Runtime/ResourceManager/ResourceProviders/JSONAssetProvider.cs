using System;
using System.ComponentModel;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// Converts JSON serialized text into the requested object.
    /// </summary>
    [DisplayName("JSON Asset Provider")]
    public class JsonAssetProvider : TextDataProvider
    {
        /// <summary>
        /// Converts raw text into requested object type via JSONUtility.FromJson.
        /// </summary>
        /// <param name="type">The object type the text is converted to.</param>
        /// <param name="text">The text to convert.</param>
        /// <returns>Returns the converted object.</returns>
        public override object Convert(Type type, string text)
        {
            return JsonUtility.FromJson(text, type);
        }
    }
}
