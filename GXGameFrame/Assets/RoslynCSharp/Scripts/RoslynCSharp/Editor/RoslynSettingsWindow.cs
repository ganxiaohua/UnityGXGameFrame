using Trivial.ImGUI;
using UnityEditor;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using UnityEngine;
using Trivial.CodeSecurity;

namespace RoslynCSharp.Editor
{
    [CustomEditor(typeof(RoslynCSharp))]
    public class RoslynSettingsWindow : ImInspectorWindow<RoslynCSharp>
    {
        // Private
        private const int labelWidth = 160;
        private const int negativeVerticalSpaceSize = 28;

        private RoslynCSharp settings = null;
        private int selectedReference = -1;
        private int selectedReferenceAsset = -1;
        private int selectedDefineSymbol = -1;
        private int selectedIllegalReferenceAllow = -1;
        private int selectedIllegalReferenceDeny = -1;
        private int selectedIllegalNamespaceAllow = -1;
        private int selectedIllegalNamespaceDeny = -1;
        private int selectedIllegalTypeAllow = -1;
        private int selectedIllegalTypeDeny = -1;
        private int selectedIllegalMemberAllow = -1;
        private int selectedIllegalMemberDeny = -1;
        private int selectedTab = 0;

        //private SecuritySettingsDrawer securityDrawer = null;

        // Methods
        [MenuItem("Tools/Roslyn C#/Settings")]
        public static void ShowWindow()
        {
            // Try to load the asset
            RoslynCSharp settings = RoslynCSharp.LoadAsset();

            // Check for no asset
            if (settings == null)
                settings = CreateInstance<RoslynCSharp>();

            // Select the item
            Selection.activeObject = settings;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Load settings from project
            settings = RoslynCSharp.LoadAsset();

            // Use default settings
            if (settings == null)
                settings = CreateInstance<RoslynCSharp>();




            CodeSecurityAllowance s = new CodeSecurityAllowance();

            s.AddAssemblyAllowance("mscorlib, 1.0.0.0");
            s.AddAssemblyAllowance("testAsm");

            //securityDrawer = new SecuritySettingsDrawer(s);
        }

        protected override void OnDisable()
        { 
            base.OnDisable();

            // Save settings to project
            RoslynCSharp.SaveAsset(settings);
        }

        public override void OnImGUI()
        {
            // 
            OnImGUIContent();
        }

        public void OnImGUIContent()
        {
            // General
            ImGUI.SetNextStyle(ImGUIStyle.BoldLabel);
            ImGUILayout.Label("General");

            ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
            {
                // Label
                ImGUI.SetNextWidth(labelWidth);
                ImGUILayout.Label("Log Level:");

                // Toggle
                settings.LogLevel = (RoslynCSharp.LogDetail)ImGUILayout.EnumPopup(settings.LogLevel);
            }
            ImGUILayout.EndLayout();


            ImGUILayout.Space(10);
            ImGUILayout.Separator();


            // Display a toolbar
            ImGUI.ToolbarItem("Compiler");
            ImGUI.ToolbarItem("Security");
            ImGUI.ToolbarItem("Hot Reload");

            ImGUILayout.Space(5);
            ImGUILayout.BeginLayout(ImGUILayoutType.HorizontalCentered);
            {
                ImGUI.SetNextWidth(220);
                selectedTab = ImGUILayout.Toolbar(selectedTab);
            }
            ImGUILayout.EndLayout();
            ImGUILayout.Space(10);


            // Compiler tab
            if (selectedTab == 0)
            {
                // Allow Unsafe
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Allow Unsafe Code:");

                    // Toggle
                    settings.AllowUnsafeCode = ImGUILayout.Toggle(settings.AllowUnsafeCode);
                }
                ImGUILayout.EndLayout();

                // Allow optimize
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Allow Optimize Code:");

                    // Toggle
                    settings.AllowOptimizeCode = ImGUILayout.Toggle(settings.AllowOptimizeCode);
                }
                ImGUILayout.EndLayout();

                // Allow concurrent
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Allow Concurrent Compile:");

                    // Toggle
                    settings.AllowConcurrentCompile = ImGUILayout.Toggle(settings.AllowConcurrentCompile);
                }
                ImGUILayout.EndLayout();

                // Deterministic
                // Allow concurrent
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Deterministic:");

                    // Toggle
                    settings.Deterministic = ImGUILayout.Toggle(settings.Deterministic);
                }
                ImGUILayout.EndLayout();

                // Generate in memory
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Generate in Memory:");

                    // Toggle
                    settings.GenerateInMemory = ImGUILayout.Toggle(settings.GenerateInMemory);
                }
                ImGUILayout.EndLayout();

                // Generate symbols
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Generate Symbols:");

                    // Toggle
                    settings.GenerateSymbols = ImGUILayout.Toggle(settings.GenerateSymbols);
                }
                ImGUILayout.EndLayout();

                // Warning level
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Warning Level:");

                    // Popup
                    for (int i = 0; i < 5; i++)
                        ImGUI.PopupItem(i.ToString());

                    settings.WarningLevel = ImGUILayout.Popup(settings.WarningLevel);
                }
                ImGUILayout.EndLayout();

                // Language version
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Language Version:");

                    // Toggle
                    settings.LanguageVersion = (LanguageVersion)ImGUILayout.EnumPopup(settings.LanguageVersion);
                }
                ImGUILayout.EndLayout();

                // Target platform
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Target Platform:");

                    // Toggle
                    settings.TargetPlatform = (Platform)ImGUILayout.EnumPopup(settings.TargetPlatform);
                }
                ImGUILayout.EndLayout();

                // References
                selectedReference = ImGUILayout.Listbox<string>("References", settings.References, selectedReference, () =>
                {
                    // Get input from the user
                    InputDialog.ShowDialog("Add Assembly Reference", "Enter the name or path of the assembly to reference", (string input) =>
                    {
                        // Add the new reference
                        if (settings.References.Contains(input) == false)
                        {
                            settings.References.Add(input);
                            Repaint();
                        }
                    });
                    return null;
                }, OnListItemImGUI);

                //// Reference assets
                //selectedReferenceAsset = ImGUILayout.Listbox<AssemblyReferenceAsset>("Reference Assets", settings.ReferenceAssets, selectedReferenceAsset, () =>
                //{
                //    // Get input from user
                //    InputObjectDialog.ShowDialog<AssemblyReferenceAsset>("Add Assembly Reference Asset", "Select the assembly reference asset from your project", (UnityEngine.Object input) =>
                //    {
                //        // Get asset
                //        AssemblyReferenceAsset asset = input as AssemblyReferenceAsset;

                //        // Add the new reference
                //        if(asset != null && settings.ReferenceAssets.Contains(asset) == false)
                //        {
                //            settings.ReferenceAssets.Add(asset);
                //            Repaint();
                //        }
                //    });
                //    return null;
                //}, OnListItemImGUI);

                // Defines
                selectedDefineSymbol = ImGUILayout.Listbox<string>("Define Symbols", settings.DefineSymbols, selectedDefineSymbol, () =>
                {
                    // Get input from the user
                    InputDialog.ShowDialog("Add Define Symbol", "Enter the name of the scripting define symbol", (string input) =>
                    {
                        // Add the new reference
                        if (settings.DefineSymbols.Contains(input) == false)
                        {
                            settings.DefineSymbols.Add(input);
                            Repaint();
                        }
                    });
                    return null;
                }, OnListItemImGUI);
            }
            // Security tab
            else if (selectedTab == 1)
            {
                // Security check code
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Security Check Code:");

                    // Toggle
                    settings.SecurityCheckCode = ImGUILayout.Toggle(settings.SecurityCheckCode);
                }
                ImGUILayout.EndLayout();                

                // Allow pInvoke
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Allow PInvoke:");

                    // Toggle
                    settings.AllowPInvoke = ImGUILayout.Toggle(settings.AllowPInvoke);
                }
                ImGUILayout.EndLayout();
                ImGUILayout.Space(10);


                // Disable if security check is not enabled
                ImGUI.PushEnabledVisualState(settings.SecurityCheckCode);
                {
                    // Heading
                    ImGUI.SetNextStyle(ImGUIStyle.BoldLabel);
                    ImGUILayout.Label("Assembly Reference Restrictions");

                    // Default behaviour
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Default Behaviour:");

                        // Field
                        settings.SecurityRestrictions.AssemblyReferences.DefaultBehaviour = (CodeSecurityRestrictions.CodeSecurityBehaviour)ImGUILayout.EnumPopup(settings.SecurityRestrictions.AssemblyReferences.DefaultBehaviour);
                    }
                    ImGUILayout.EndLayout();

                    // Use regex
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Use Regex Matching:");

                        // Field
                        settings.SecurityRestrictions.AssemblyReferences.UseRegexMatch = ImGUILayout.Toggle(settings.SecurityRestrictions.AssemblyReferences.UseRegexMatch);
                    }
                    ImGUILayout.EndLayout();

                    ImGUILayout.BeginLayout(Screen.width > 550 ? ImGUILayoutType.Horizontal : ImGUILayoutType.Vertical);
                    {
                        selectedIllegalReferenceAllow = ImGUILayout.Listbox("Whitelist", settings.SecurityRestrictions.AssemblyReferences.AllowEntries, selectedIllegalReferenceAllow, () =>
                        {
                            InputDialog.ShowDialog("Add Assembly Reference", "Enter the name of the allowable assembly reference. The reference name should be the assembly name only, For example: 'mscorlib'", (string input) =>
                            {
                            // Add new entry
                            settings.SecurityRestrictions.AssemblyReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Allow);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);

                        selectedIllegalReferenceDeny = ImGUILayout.Listbox("Blacklist", settings.SecurityRestrictions.AssemblyReferences.DenyEntries, selectedIllegalReferenceDeny, () =>
                        {
                            InputDialog.ShowDialog("Add Assembly Reference", "Enter the name of the illegal assembly reference. The reference name should be the assembly name only, For example: 'mscorlib'", (string input) =>
                            {
                            // Add new entry
                            settings.SecurityRestrictions.AssemblyReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Deny);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);
                    }
                    ImGUILayout.EndLayout();
                    ImGUILayout.Space(10);


                    // Heading
                    ImGUI.SetNextStyle(ImGUIStyle.BoldLabel);
                    ImGUILayout.Label("Namespace Reference Restrictions");

                    // Default behaviour
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Default Behaviour:");

                        // Field
                        settings.SecurityRestrictions.NamespaceReferences.DefaultBehaviour = (CodeSecurityRestrictions.CodeSecurityBehaviour)ImGUILayout.EnumPopup(settings.SecurityRestrictions.NamespaceReferences.DefaultBehaviour);
                    }
                    ImGUILayout.EndLayout();

                    // Use regex
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Use Regex Matching:");

                        // Field
                        settings.SecurityRestrictions.NamespaceReferences.UseRegexMatch = ImGUILayout.Toggle(settings.SecurityRestrictions.NamespaceReferences.UseRegexMatch);
                    }
                    ImGUILayout.EndLayout();

                    ImGUILayout.BeginLayout(Screen.width > 550 ? ImGUILayoutType.Horizontal : ImGUILayoutType.Vertical);
                    {
                        selectedIllegalNamespaceAllow = ImGUILayout.Listbox("Whitelist", settings.SecurityRestrictions.NamespaceReferences.AllowEntries, selectedIllegalNamespaceAllow, () =>
                        {
                            InputDialog.ShowDialog("Add Namespace Reference", "Enter the name of the allowable namespace. You can also use wildcards to specify that all child namespaces should also be included, For example: 'System.IO.*'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.NamespaceReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Allow);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);

                        selectedIllegalNamespaceDeny = ImGUILayout.Listbox("Blacklist", settings.SecurityRestrictions.NamespaceReferences.DenyEntries, selectedIllegalNamespaceDeny, () =>
                        {
                            InputDialog.ShowDialog("Add Namespace Reference", "Enter the name of the illegal namespace. You can also use wildcards to specify that all child namespaces should also be included, For example: 'System.IO.*'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.NamespaceReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Deny);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);
                    }
                    ImGUILayout.EndLayout();
                    ImGUILayout.Space(10);


                    // Heading
                    ImGUI.SetNextStyle(ImGUIStyle.BoldLabel);
                    ImGUILayout.Label("Type Reference Restrictions");

                    // Default behaviour
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Default Behaviour:");

                        // Field
                        settings.SecurityRestrictions.TypeReferences.DefaultBehaviour = (CodeSecurityRestrictions.CodeSecurityBehaviour)ImGUILayout.EnumPopup(settings.SecurityRestrictions.TypeReferences.DefaultBehaviour);
                    }
                    ImGUILayout.EndLayout();

                    // Use regex
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Use Regex Matching:");

                        // Field
                        settings.SecurityRestrictions.TypeReferences.UseRegexMatch = ImGUILayout.Toggle(settings.SecurityRestrictions.TypeReferences.UseRegexMatch);
                    }
                    ImGUILayout.EndLayout();

                    ImGUILayout.BeginLayout(Screen.width > 550 ? ImGUILayoutType.Horizontal : ImGUILayoutType.Vertical);
                    {
                        selectedIllegalTypeAllow = ImGUILayout.Listbox("Whitelist", settings.SecurityRestrictions.TypeReferences.AllowEntries, selectedIllegalTypeAllow, () =>
                        {
                            InputDialog.ShowDialog("Add Type Reference", "Enter the name of the allowable type. The full type name should be specified excluding the assembly name, For example: 'System.AppDomain'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.TypeReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Allow);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);

                        selectedIllegalTypeDeny = ImGUILayout.Listbox("Blacklist", settings.SecurityRestrictions.TypeReferences.DenyEntries, selectedIllegalTypeDeny, () =>
                        {
                            InputDialog.ShowDialog("Add Type Reference", "Enter the name of the illegal type. The full type name should be specified excluding the assembly name, For example: 'System.AppDomain'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.TypeReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Deny);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);
                    }
                    ImGUILayout.EndLayout();
                    ImGUILayout.Space(10);


                    // Heading
                    ImGUI.SetNextStyle(ImGUIStyle.BoldLabel);
                    ImGUILayout.Label("Member Reference Restrictions");

                    // Default behaviour
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Default Behaviour:");

                        // Field
                        settings.SecurityRestrictions.MemberReferences.DefaultBehaviour = (CodeSecurityRestrictions.CodeSecurityBehaviour)ImGUILayout.EnumPopup(settings.SecurityRestrictions.MemberReferences.DefaultBehaviour);
                    }
                    ImGUILayout.EndLayout();

                    // Use regex
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Use Regex Matching:");

                        // Field
                        settings.SecurityRestrictions.MemberReferences.UseRegexMatch = ImGUILayout.Toggle(settings.SecurityRestrictions.MemberReferences.UseRegexMatch);
                    }
                    ImGUILayout.EndLayout();

                    ImGUILayout.BeginLayout(Screen.width > 550 ? ImGUILayoutType.Horizontal : ImGUILayoutType.Vertical);
                    {
                        selectedIllegalMemberAllow = ImGUILayout.Listbox("Whitelist", settings.SecurityRestrictions.MemberReferences.AllowEntries, selectedIllegalMemberAllow, () =>
                        {
                            InputDialog.ShowDialog("Add Member Reference", "Enter the name of the allowable member including the type name. The full type name should be specified followed by the member name separated by '.', For example: 'UnityEngine.Application.Quit'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.MemberReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Allow);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);

                        selectedIllegalMemberDeny = ImGUILayout.Listbox("Blacklist", settings.SecurityRestrictions.MemberReferences.DenyEntries, selectedIllegalMemberDeny, () =>
                        {
                            InputDialog.ShowDialog("Add Type Reference", "Enter the name of the illegal member. The full type name should be specified followed by the member name separated by '.', For example: 'UnityEngine.Application.Quit'", (string input) =>
                            {
                                // Add new entry
                                settings.SecurityRestrictions.MemberReferences.AddEntryName(input, CodeSecurityRestrictions.CodeSecurityBehaviour.Deny);
                                Repaint();
                            });

                            return null;
                        }, OnListItemImGUI);
                    }
                    ImGUILayout.EndLayout();

                    //selectedIllegalReference = ImGUILayout.Listbox<string>("References", settings.SecurityRestrictions.References, selectedIllegalReference, ()  =>
                    //{
                    //    // Get input from the user
                    //    InputDialog.ShowDialog("Add Reference", "Enter the name of the illegal reference including the '.dll' file extension, For example: 'UnityEditor.dll'", (string input) =>
                    //    {
                    //        // Add the new reference
                    //        settings.SecurityRestrictions.AddAssemblyReference(input);
                    //        Repaint();
                    //    });

                    //    // Dont add item by default
                    //    return null;
                    //}, OnListItemImGUI);

                    //// Move under list
                    //ImGUILayout.Space(-negativeVerticalSpaceSize);

                    //// Illegal references
                    //ImGUI.SetNextWidth(Screen.width - 100);
                    //ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    //{
                    //    // Small indent
                    //    ImGUILayout.Space(5);

                    //    // Label
                    //    ImGUI.SetNextTooltip("Blacklist mode will ensure that all references are not used whereas whitelist mode will only allow the use of the specified references");
                    //    ImGUILayout.Label("Restriction Type:");

                    //    // Popup
                    //    settings.SecurityRestrictions.ReferencesMode = (SecurityRestrictions.SecurityRestrictionMode)ImGUILayout.EnumPopup(settings.SecurityRestrictions.ReferencesMode);
                    //}
                    //ImGUILayout.EndLayout();
                    ImGUILayout.Space(10);

                    //securityDrawer.OnDrawSettings();

                    // Illegal namespaces
                    //selectedIllegalNamespace = ImGUILayout.Listbox<string>("Namespaces", settings.SecurityRestrictions.Namespaces, selectedIllegalNamespace, () => 
                    //{
                    //    // Get input from the user
                    //    InputDialog.ShowDialog("Add Namespace", "Enter the name of the illegal namespace. Wildcards can be used to also exclude child namespaces, For example: 'System.IO.*'", (string input) =>
                    //    {
                    //        // Add the new reference
                    //        settings.SecurityRestrictions.AddNamespace(input);
                    //        Repaint();
                    //    });

                    //    // Dont add item by default
                    //    return null;
                    //}, OnListItemImGUI);

                    //// Move under list
                    //ImGUILayout.Space(-negativeVerticalSpaceSize);

                    //ImGUI.SetNextWidth(Screen.width - 100);
                    //ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    //{
                    //    // Small indent
                    //    ImGUILayout.Space(5);

                    //    // Label
                    //    ImGUI.SetNextTooltip("Blacklist mode will ensure that all namespaces are not used whereas whitelist mode will only allow the use of the specified namespaces");
                    //    ImGUILayout.Label("Restriction Type:");

                    //    // Popup
                    //    settings.SecurityRestrictions.NamespacesMode = (SecurityRestrictions.SecurityRestrictionMode)ImGUILayout.EnumPopup(settings.SecurityRestrictions.NamespacesMode);
                    //}
                    //ImGUILayout.EndLayout();
                    ImGUILayout.Space(10);




                    // Illegal references
                    //selectedIllegalType = ImGUILayout.Listbox<string>("Types", settings.SecurityRestrictions.TypeNames, selectedIllegalType, () =>
                    //{
                    //    // Get input from the user
                    //    InputDialog.ShowDialog("Add Type", "Enter the name of the illegal type. Assembly qualified type names are preferred however you may be able to use the full type name including namespace, For example: 'System.AppDomain'", (string input) =>
                    //    {
                    //        // Add the new reference
                    //        settings.SecurityRestrictions.AddType(input);
                    //        Repaint();
                    //    });

                    //    // Dont add item by default
                    //    return null;
                    //}, OnListItemImGUI);

                    //// Move under list
                    //ImGUILayout.Space(-negativeVerticalSpaceSize);

                    //ImGUI.SetNextWidth(Screen.width - 100);
                    //ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    //{
                    //    // Small indent
                    //    ImGUILayout.Space(5);

                    //    // Label
                    //    ImGUI.SetNextTooltip("Blacklist mode will ensure that all types are not used whereas whitelist mode will only allow the use of the specified types");
                    //    ImGUILayout.Label("Restriction Type:");

                    //    // Popup
                    //    settings.SecurityRestrictions.TypesMode = (SecurityRestrictions.SecurityRestrictionMode)ImGUILayout.EnumPopup(settings.SecurityRestrictions.TypesMode);
                    //}
                    //ImGUILayout.EndLayout();
                }
                ImGUI.PopVisualState();
            }
            else if(selectedTab == 2)
            {
                // Enable hot reloading
                ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                {
                    // Label
                    ImGUI.SetNextWidth(labelWidth);
                    ImGUILayout.Label("Enable Hot Reloading:");

                    // Toggle
                    settings.AllowHotReloading = ImGUILayout.Toggle(settings.AllowHotReloading);
                }
                ImGUILayout.EndLayout();

                if(settings.AllowHotReloading == true)
                {
                    ImGUILayout.HelpBox("Hot reloading is a runtime feature that can auto-compile and load scripts that are edited while in play mode. It is useful for quick iteration on detached components and is fully automatic. It is highly recommmended that you disable 'Auto-Refresh' in the Unity editor if you use this feature");
                }

                ImGUI.PushEnabledVisualState(settings.AllowHotReloading);
                {
                    // Copy serialized fields
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Copy Serialized Fields:");

                        // Toggle
                        settings.HotReloadCopySerializedFields = ImGUILayout.Toggle(settings.HotReloadCopySerializedFields);
                    }
                    ImGUILayout.EndLayout();

                    // Copy non serialized fields
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Copy Non-Serialized Fields:");

                        // Toggle
                        settings.HotReloadCopyNonSerializedFields = ImGUILayout.Toggle(settings.HotReloadCopyNonSerializedFields);
                    }
                    ImGUILayout.EndLayout();

                    // Destroy original script
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUILayout.Label("Destroy Original Script:");

                        // Toggle
                        settings.HotReloadDestroyOriginalScript = ImGUILayout.Toggle(settings.HotReloadDestroyOriginalScript);
                    }
                    ImGUILayout.EndLayout();

                    ImGUI.PushEnabledVisualState(settings.HotReloadDestroyOriginalScript == false);
                    {
                        // Disable original script
                        ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                        {
                            // Label
                            ImGUI.SetNextWidth(labelWidth);
                            ImGUILayout.Label("Disable Original Script:");

                            // Toggle
                            settings.HotReloadDisableOriginalScript = ImGUILayout.Toggle(settings.HotReloadDisableOriginalScript);
                        }
                        ImGUILayout.EndLayout();
                    }
                    ImGUI.PopVisualState();

                    // Security check code
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUI.SetNextTooltip("Disable this option for faster reload times");
                        ImGUILayout.Label("Security Check Code:");

                        // Toggle
                        settings.HotReloadSecurityCheckCode = ImGUILayout.Toggle(settings.HotReloadSecurityCheckCode);
                    }
                    ImGUILayout.EndLayout();

                    // Parse project
                    ImGUILayout.BeginLayout(ImGUILayoutType.Horizontal);
                    {
                        // Label
                        ImGUI.SetNextWidth(labelWidth);
                        ImGUI.SetNextTooltip("Enable this option and assembly references will be supplied to the compiler automatically by parsing the .csproj file that Unity generates");
                        ImGUILayout.Label("Parse .csproj References:");

                        // Toggle
                        settings.HotReloadUseCSharpProjectReferences = ImGUILayout.Toggle(settings.HotReloadUseCSharpProjectReferences);
                    }
                    ImGUILayout.EndLayout();
                }
                ImGUI.PopVisualState();
            }
        }

        private void OnListItemImGUI(string item)
        {
            ImGUILayout.Label(item);
        }

        private void OnListItemImGUI(AssemblyReferenceAsset item)
        {
            ImGUILayout.Label(item.AssemblyName);
        }
    }
}
