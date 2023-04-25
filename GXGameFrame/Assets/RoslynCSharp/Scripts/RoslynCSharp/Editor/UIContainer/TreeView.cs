using System;

namespace RoslynCSharp.Editor
{
    public class TreeView
    {
        // Private
        private string rootPath = null;
        private TreeNode rootNode = null;

        // Properties
        public TreeNode Root
        {
            get { return rootNode; }
        }

        // Constructor
        public TreeView(string rootPath)
        {
            this.rootPath = rootPath;
            this.rootNode = new TreeNode(rootPath);
        }

        // Methods
        public void SortNodes()
        {
            rootNode.SortNodes();
        }

        public TreeNode GetOrCreateNode(string nodePath)
        {
            // Check for child node
            if (nodePath.StartsWith(rootPath) == false)
                throw new ArgumentException("The specified path must be a child path of 'rootPath'");

            // Get the relative path
            string relative = nodePath.Remove(0, rootPath.Length).Trim('/');

            // Split all path levels
            string[] pathLevels = relative.Split('/');

            TreeNode current = rootNode;
            int index = 0;

            foreach (string pathLevel in pathLevels)
            {
                // Get or create the hierarchy node
                current = current.GetOrCreateChildNode(pathLevel);
                index++;
            }

            return current;
        }
    }
}
