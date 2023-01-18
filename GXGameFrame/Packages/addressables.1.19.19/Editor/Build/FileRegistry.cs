using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditor.AddressableAssets.Build
{
    /// <summary>
    /// Use to contain files created during a build.
    /// </summary>
    public class FileRegistry
    {
        private readonly HashSet<string> m_FilePaths;

        /// <summary>
        /// Initializes a new file registry instance.
        /// </summary>
        public FileRegistry()
        {
            m_FilePaths = new HashSet<string>();
        }

        /// <summary>
        /// Retrieves all the stored file paths.
        /// </summary>
        /// <returns>Returns all file paths as an IEnumerable.</returns>
        public IEnumerable<string> GetFilePaths()
        {
            return new HashSet<string>(m_FilePaths);
        }

        /// <summary>
        /// Adds a file path to our set of file paths.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void AddFile(string path)
        {
            m_FilePaths.Add(path);
        }

        /// <summary>
        /// Removes a file path from our set of file paths.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void RemoveFile(string path)
        {
            m_FilePaths.Remove(path);
        }

        /// <summary>
        /// Given a bundle name, determine the file path for the bundle.
        /// </summary>
        /// <param name="bundleName">The name of the bundle.</param>
        /// <returns>The full file path.</returns>
        public string GetFilePathForBundle(string bundleName)
        {
            bundleName = Path.GetFileNameWithoutExtension(bundleName);
            return m_FilePaths.FirstOrDefault((entry) => entry.Contains(bundleName));
        }

        /// <summary>
        /// Replace an entry in the File Registry with a new bundle name.
        /// </summary>
        /// <param name="bundleName">The bundle name to replace.</param>
        /// <param name="newFileRegistryEntry">The new file registry bundle name.</param>
        /// <returns>Returns true if a successful replacement occured.</returns>
        public bool ReplaceBundleEntry(string bundleName, string newFileRegistryEntry)
        {
            if (!m_FilePaths.Contains(newFileRegistryEntry))
            {
                m_FilePaths.RemoveWhere((entry) => entry.Contains(bundleName));
                AddFile(newFileRegistryEntry);
                return true;
            }

            return false;
        }
    }
}
