using System;
using System.Collections;
using System.Collections.Generic;

namespace RoslynCSharp.Editor
{
    public sealed class TreeNode
    {
        // Type
        private class NodeSorter : IComparer<TreeNode>
        {
            public int Compare(TreeNode x, TreeNode y)
            {
                return string.Compare(x.name, y.name);
            }
        }

        // Private
        private static readonly NodeSorter comparer = new NodeSorter();

        private string name = null;
        private int hierarchyDepth = 0;

        // Internal
        internal List<TreeNode> childNodes = new List<TreeNode>();

        // Public
        public bool expanded = false;
        public bool selected = true;
        public object userData = null;

        // Properties
        public string Name
        {
            get { return name; }
        }

        public int HierarchyDepth
        {
            get { return hierarchyDepth; }
        }

        // Constructor
        public TreeNode(string name)
        {
            this.name = name;
        }

        // Methods
        public void SortNodes()
        {
            childNodes.Sort(comparer);

            foreach (TreeNode node in childNodes)
                node.SortNodes();
        }

        public TreeNode GetOrCreateChildNode(string childName)
        {
            foreach (TreeNode node in childNodes)
                if (node.name == childName)
                    return node;

            TreeNode newNode = new TreeNode(childName);

            newNode.hierarchyDepth = hierarchyDepth + 1;
            childNodes.Add(newNode);

            return newNode;
        }

        public bool HasChildren()
        {
            return HasChildren(this);
        }

        public static bool HasChildren(TreeNode node)
        {
            // Check for no children
            if (node.childNodes.Count == 0)
                return false;

            foreach (TreeNode childNode in node.childNodes)
            {

                // Reccursive call
                if (HasChildren(childNode) == true)
                    return true;
            }

            // Default case
            return false;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
