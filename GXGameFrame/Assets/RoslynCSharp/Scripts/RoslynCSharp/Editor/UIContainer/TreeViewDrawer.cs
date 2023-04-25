using System;
using UnityEditor;
using UnityEngine;

namespace RoslynCSharp.Editor
{
    public static class TreeViewDrawer
    {
        // Private
        private const int lineHeight = 15;

        // Methods
        public static bool DrawTreeView(ref Vector2 scroll, ref float requiredHeight, TreeView treeView)
        {
            Rect drawArea = new Rect(0f, 50, Screen.width, 300f);

            return DrawTreeView(drawArea, ref scroll, ref requiredHeight, treeView);
        }

        public static bool DrawTreeView(in Rect area, ref Vector2 scroll, ref float requiredHeight, TreeView treeView)
        {
            // Get starting line
            Rect lineRect = new Rect(0, -lineHeight, area.width, lineHeight);

            // Rect funt
            Rect NextRect()
            {
                lineRect.y += lineHeight;
                return lineRect;
            };

            bool changed = false;

            // Scroll area
            scroll = GUI.BeginScrollView(area, scroll, new Rect(0f, 0f, area.width, requiredHeight));
            {
                // Get clipping rect
                Rect clipRect = new Rect(scroll.x, scroll.y, area.width, area.height);

                // Draw the tree view
                changed = DrawTreeViewNode(area, clipRect, NextRect, scroll, treeView.Root);
            }
            GUI.EndScrollView();

            // Update required height
            requiredHeight = lineRect.y + lineHeight * 2;

            return changed;
        }

        public static bool DrawTreeViewNode(in Rect area, in Rect clip, Func<Rect> nextRect, in Vector2 scroll, TreeNode node)
        {
            bool changed = false;

            // Get the line rect
            Rect lineRect = nextRect();

            // Culling - skip any items not on screen
            bool displayItem = FastVisibilityCheck(clip, lineRect);

            //displayItem = true;
            // Check if item is inside view area and should not be culled
            if(displayItem == true)
            {
                // Update indent amount
                lineRect.x += 16f * node.HierarchyDepth;

                // Create rects
                Rect foldoutRect = new Rect(lineRect.x, lineRect.y, 16, lineRect.height);
                Rect toggleRect = new Rect(lineRect.x + 16, lineRect.y, 16, lineRect.height);
                Rect labelRect = new Rect(lineRect.x + 40, lineRect.y, lineRect.width - 40, lineRect.height);
                Rect hintRect = new Rect(lineRect.x + lineRect.width - 260, lineRect.y, 260, lineRect.height);

                // Check for children
                if (node.childNodes.Count > 0)
                {
                    // Check for expended
                    node.expanded = EditorGUI.Foldout(foldoutRect, node.expanded, GUIContent.none);
                }

                int total = 0;
                int selected = 0;

                // Get number of selected children
                GetSelectedChildCount(node, out total, out selected);

                bool isOn = total > 0 && total == selected;
                bool isMixed = selected > 0 && total != selected;

                // Display toggle
                EditorGUI.showMixedValue = isMixed;
                bool result = EditorGUI.Toggle(toggleRect, isOn);
                EditorGUI.showMixedValue = false;

                // Check for changed
                if(result != isOn)
                {
                    // Select / deselect all
                    SetAllChildrenSelected(node, result);
                    changed = true;
                }

                // Draw label
                GUI.Label(labelRect, node.Name, EditorStyles.miniLabel);
            }

            // Draw all children
            if (node.expanded == true)
            {
                int childCount = node.childNodes.Count;

                for (int i = 0; i < childCount; i++)
                {
                    DrawTreeViewNode(area, clip, nextRect, scroll, node.childNodes[i]);
                }
            }

            return changed;
        }

        private static void GetSelectedChildCount(TreeNode node, out int childCount, out int selectedCount)
        {
            childCount = 0;
            selectedCount = 0;

            int immediateChildCount = node.childNodes.Count;

            for(int i = 0; i < immediateChildCount; i++)
            {
                // Check children first - depth first
                int total, selected;
                GetSelectedChildCount(node.childNodes[i], out total, out selected);

                childCount += total;
                selectedCount += selected;
            }
            
            // Check for this node checked
            childCount++;

            if (node.selected == true)
                selectedCount++;
        }

        private static void SetAllChildrenSelected(TreeNode node, bool selected)
        {
            int childCount = node.childNodes.Count;

            for(int i = 0; i < childCount; i++)
            {
                // Set children first - depth first
                SetAllChildrenSelected(node.childNodes[i], selected);
            }

            // Set state
            node.selected = selected;
        }

        private static bool FastVisibilityCheck(in Rect viewArea, in Rect elementRect)
        {
            // We can make some assumptions here to improve performance
            if (elementRect.y < viewArea.y || elementRect.yMax > viewArea.yMax)
                return false;

            return true;
        }
    }
}
