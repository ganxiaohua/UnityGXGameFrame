//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Trivial.CodeSecurity;
//using UnityEditor;
//using UnityEngine;

//namespace RoslynCSharp.Editor
//{
//    public class SecuritySettingsDrawer
//    {
//        // Private
//        private RoslynCSharpSecurityAllowance securityAllowance = null;
//        private TreeView treeView = null;
//        private Vector2 scroll = Vector2.zero;
//        private float requiredHeight = 0f;

//        // Constructor
//        public SecuritySettingsDrawer(RoslynCSharpSecurityAllowance securityAllowance)
//        {
//            // Check for null
//            if (securityAllowance == null)
//                throw new ArgumentNullException(nameof(securityAllowance));

//            this.securityAllowance = securityAllowance;
//        }

//        // Methods
//        public void OnDrawSettings()
//        {
//            if (treeView == null)
//                BuildTreeView();

//            // Draw tree view
//            bool changed = TreeViewDrawer.DrawTreeView(ref scroll, ref requiredHeight, treeView);

//            // Check for changed
//            if(changed == true)
//            {
//                securityAllowance.AddAssemblyAllowance("").
//            }
//        }

//        public void OnDrawAssemblyButtons()
//        {
//            int widthStretch = 310;

//            // Button layout
//            GUILayout.Space(10);
//            if (Screen.width > widthStretch)
//                GUILayout.BeginHorizontal();

//            // Select assembly button
//            if (GUILayout.Button("Select Assembly File", GUILayout.Height(30)) == true)
//            {
//                string path = EditorUtility.OpenFilePanel("Open Assembly File", "Assets", "dll");

//                if (string.IsNullOrEmpty(path) == false)
//                {
//                    // Check for file exists
//                    if (File.Exists(path) == false)
//                    {
//                        Debug.LogError("Assembly file does not exist: " + path);
//                        return;
//                    }

//                    // Add assembly name
//                    securityAllowance.AddedAssemblies.Add(Assembly.ReflectionOnlyLoadFrom(path).GetName().Name);

//                    // Refresh tree view
//                    BuildTreeView();

//                    //// Set file path
//                    //asset.UpdateAssemblyReference(path, Path.GetFileNameWithoutExtension(path));

//                    //// Mark as dirty
//                    //EditorUtility.SetDirty(asset);
//                }
//            }

//            if (GUILayout.Button("Select Loaded Assembly", GUILayout.Height(30)) == true)
//            {
//                GenericMenu menu = new GenericMenu();

//                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
//                {
//                    string menuName = asm.FullName;

//                    if (menuName.StartsWith("Unity") == true)
//                        menuName = "Untiy Assemblies/" + menuName;
//                    else if (menuName.StartsWith("System") == true)
//                        menuName = "System Assemblies/" + menuName;

//                    menu.AddItem(new GUIContent(menuName), false, (object value) =>
//                    {
//                        // Get the selected assembly
//                        Assembly selectedAsm = (Assembly)value;

//                        // Check for location
//                        if (string.IsNullOrEmpty(selectedAsm.Location) == true || File.Exists(selectedAsm.Location) == false)
//                        {
//                            Debug.LogError("The selectged assembly could not be referenced because its source location could not be determined. Please add the assembly using the full path!");
//                            return;
//                        }

//                        // Add assembly name
//                        securityAllowance.AddedAssemblies.Add(selectedAsm.GetName().Name);

//                        // Refresh tree view
//                        BuildTreeView();
//                        // Update the assembly
//                        //asset.UpdateAssemblyReference(selectedAsm.Location, selectedAsm.FullName);

//                        // Mark as dirty
//                        //EditorUtility.SetDirty(asset);
//                    }, asm);
//                }

//                // SHow the menu
//                menu.ShowAsContext();
//            }

//            if (Screen.width > widthStretch)
//                GUILayout.EndHorizontal();
//        }

//        private void BuildTreeView()
//        {
//            treeView = new TreeView("Assemblies");

//            // Process all assemblies
//            foreach(string addedAssembly in securityAllowance.AddedAssemblies)
//            {
//                // Try to resolve assembly
//                Assembly asm = null;// AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == addedAssembly).FirstOrDefault();

//                try
//                {
//                    asm = AppDomain.CurrentDomain.Load(new AssemblyName(addedAssembly));
//                }
//                catch { }

//                if(asm != null)
//                {
//                    // Create the node
//                    TreeNode node = treeView.GetOrCreateNode("Assemblies/" + asm.GetName().Name);

//                    // Set on state
//                    node.selected = true;


//                    // Process types
//                    foreach (Type type in asm.GetTypes())
//                    {
//                        // Check for non public
//                        if (type.IsPublic == false)
//                            continue;

//                        // Check for exception
//                        if (typeof(Exception).IsAssignableFrom(type) == true)
//                            continue;

//                        // Add namespace
//                        TreeNode namespaceNode = node.GetOrCreateChildNode(type.Namespace);

//                        // Set on state


//                        // Add type
//                        TreeNode typeNode = namespaceNode.GetOrCreateChildNode(type.Name);


//                        // Process members
//                        foreach (MemberInfo member in type.GetMembers())
//                        {
//                            // Check for non-public
//                            if(member is PropertyInfo ||
//                                member is EventInfo ||
//                                member is ConstructorInfo ||
//                                (member is FieldInfo && ((FieldInfo)member).IsPublic == false) ||
//                                (member is MethodBase && ((MethodBase)member).IsPublic == false))
//                            {
//                                continue;
//                            }

//                            // Check for object members
//                            if (member.DeclaringType == typeof(object))
//                                continue;

//                            typeNode.GetOrCreateChildNode(member.ToString());
//                        }
//                    }
//                }
//                else
//                {
//                    treeView.Root.GetOrCreateChildNode(asm.GetName().Name + " (Assembly Missing!)");
//                }
//            }

//            // Sort tree view
//            treeView.SortNodes();

//            // Process all
//            //foreach(CodeSecurityAssemblyAllowance assembly in securityAllowance.AllowedAssemblies)
//            //{
//            //    // Create the node
//            //    TreeNode node = treeView.GetOrCreateNode("Assemblies/" + assembly.AssemblyName);

//            //    // Set on state
//            //    node.selected = true;
//            //}
//        }
//    }
//}
