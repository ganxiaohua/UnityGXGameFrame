using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoslynCSharp.Editor
{
    [CustomEditor(typeof(AssemblyReferenceAsset))]
    public class AssemblyReferenceAssetInspector : UnityEditor.Editor
    {
        // Methods
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Get instance
            AssemblyReferenceAsset asset = target as AssemblyReferenceAsset;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
                style.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label("Assembly Info", style);

                // Line
                Rect area = GUILayoutUtility.GetLastRect();
                area.y += EditorGUIUtility.singleLineHeight + 5;
                area.height = 2;

                EditorGUI.DrawRect(area, new Color(0.2f, 0.2f, 0.2f, 0.4f));
                GUILayout.Space(10);

                EditorGUI.BeginDisabledGroup(true);
                {
                    // Assembly name
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("assemblyPath"));

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PrefixLabel("Last Write Time");
                        EditorGUILayout.TextField((asset.IsValid == true) ? asset.LastWriteTime.ToString() : string.Empty);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndVertical();


            int widthStretch = 310;

            // Button layout
            GUILayout.Space(10);
            if (Screen.width > widthStretch) 
                GUILayout.BeginHorizontal();

            // Select assembly button
            if(GUILayout.Button("Select Assembly File", GUILayout.Height(30)) == true)
            {
                string path = EditorUtility.OpenFilePanel("Open Assembly File", "Assets", "dll");

                if(string.IsNullOrEmpty(path) == false)
                {
                    // Check for file exists
                    if(File.Exists(path) == false)
                    {
                        Debug.LogError("Assembly file does not exist: " + path);
                        return;
                    }

                    // Use relative path if possible
                    string relativePath = path.Replace('\\', '/');
                    relativePath = FileUtil.GetProjectRelativePath(relativePath);

                    if (string.IsNullOrEmpty(relativePath) == false && File.Exists(relativePath) == true)
                        path = relativePath;

                    // Set file path
                    asset.UpdateAssemblyReference(path, Path.GetFileNameWithoutExtension(path));

                    // Mark as dirty
                    EditorUtility.SetDirty(asset);
                }
            }

            if(GUILayout.Button("Select Loaded Assembly", GUILayout.Height(30)) == true)
            {                
                GenericMenu menu = new GenericMenu();

                foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string menuName = asm.FullName;

                    if (menuName.StartsWith("Unity") == true)
                        menuName = "Untiy Assemblies/" + menuName;
                    else if (menuName.StartsWith("System") == true)
                        menuName = "System Assemblies/" + menuName;

                    menu.AddItem(new GUIContent(menuName), false, (object value) =>
                    {
                        // Get the selected assembly
                        Assembly selectedAsm = (Assembly)value;

                        // Check for location
                        if(string.IsNullOrEmpty(selectedAsm.Location) == true || File.Exists(selectedAsm.Location) == false)
                        {
                            Debug.LogError("The selectged assembly could not be referenced because its source location could not be determined. Please add the assembly using the full path!");
                            return;
                        }

                        string path = selectedAsm.Location;

                        // Use relative path if possible
                        string relativePath = path.Replace('\\', '/');
                        relativePath = FileUtil.GetProjectRelativePath(relativePath);

                        if (string.IsNullOrEmpty(relativePath) == false && File.Exists(relativePath) == true)
                            path = relativePath;

                        // Update the assembly
                        asset.UpdateAssemblyReference(path, selectedAsm.FullName);

                        // Mark as dirty
                        EditorUtility.SetDirty(asset);
                    }, asm);
                }

                // SHow the menu
                menu.ShowAsContext();
            }

            if(Screen.width > widthStretch)
                GUILayout.EndHorizontal();

            // Check for valid
            if(asset.IsValid == false)
            {
                EditorGUILayout.HelpBox("The assembly reference is not valid. Select a valid assembly path to reference", MessageType.Warning);
            }
            else if(File.Exists(asset.AssemblyPath) == false)
            {
                EditorGUILayout.HelpBox("The assembly path does not exists. Referencing will still work but any changes to the assembly will not be detected! Consider selecting a valid assembly path", MessageType.Warning);
            }
        }
    }
}
