using System;
using Trivial.ImGUI;
using UnityEditor;
using UnityEngine;

namespace RoslynCSharp.Editor
{
    public sealed class InputDialog : ImEditorWindow
    {
        public enum DialogResult
        {
            Accept,
            Cancel,
        }

        // Private
        private string heading = string.Empty;
        private string content = string.Empty;
        private string input = string.Empty;
        private Action<string> acceptCallback = null;

        private bool requestClose = false;

        // Public
        public static readonly Vector2 defaultSize = new Vector2(320, 140);

        // Methods
        public static void ShowDialog(string title, string content, Action<string> acceptCallback)
        {
            // Show the dialog
            InputDialog dialog = ShowWindow<InputDialog>();

            // Listen for update
            EditorApplication.update += dialog.Tick;

            // Update dialog window
            dialog.heading = title;
            dialog.content = content;
            dialog.acceptCallback = acceptCallback;


            // Find the center of the screen
            Vector2 center = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);

            // Show as dialog
            dialog.ShowAsDropDown(new Rect(center - (defaultSize / 2), Vector2.zero), defaultSize);
        }

        public void CloseDialog(DialogResult result)
        {
            // Check for cancel
            if (result == DialogResult.Cancel)
                input = null;

            // Trigger callback
            if (acceptCallback != null && string.IsNullOrEmpty(input) == false)
                acceptCallback(input);

            // Close the window
            // NOTE: Cannot call `EditorWindow.Close` direct from `OnLostFocus` callstack as it appears to crash the editor or throw NullReferenceException (Inside Unity codebase) in some versions.
            // Use a simple flag workaround to close the window on the next editor updat.
            requestClose = true;
        }

        public override void OnImGUI()
        {
            titleContent.text = "Input Required";

            // Heading layout
            ImGUILayout.BeginLayout(ImGUILayoutType.HorizontalCentered);
            {
                // Title label
                ImGUI.SetNextStyle(ImGUIStyle.LargeLabel);
                ImGUILayout.Label(heading);
            }
            ImGUILayout.EndLayout();

            // Separator
            ImGUILayout.Separator();

            // Small space
            ImGUILayout.Space(10);

            // Content label
            ImGUI.SetNextStyle(ImGUIStyle.WrappedLabel);
            ImGUILayout.Label(content);

            // Push to bottom
            ImGUILayout.Space();

            // Input field
            input = ImGUILayout.TextField(input);

            // Small space
            ImGUILayout.Space(10);

            // Dialog buttons
            ImGUILayout.BeginLayout(ImGUILayoutType.HorizontalCentered);
            {
                ImGUI.SetNextWidth(80);
                if (ImGUILayout.Button("Accept") == true)
                {
                    CloseDialog(DialogResult.Accept);
                }

                ImGUI.SetNextWidth(80);
                if (ImGUILayout.Button("Cancel") == true)
                {
                    CloseDialog(DialogResult.Cancel);
                }
            }
            ImGUILayout.EndLayout();

            // Small space
            ImGUILayout.Space(10);
        }

        private void OnLostFocus()
        {
            CloseDialog(DialogResult.Cancel);
        }

        private void Tick()
        {
            if (requestClose == true)
            {
                // Reset flag
                requestClose = false;

                // Remove listener
                EditorApplication.update -= Tick;

                // Close the window
                Close();
            }
        }
    }
}
