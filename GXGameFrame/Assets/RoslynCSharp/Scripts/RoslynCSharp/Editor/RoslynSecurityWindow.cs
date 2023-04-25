//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Trivial.CodeSecurity;
//using UnityEditor;

//namespace RoslynCSharp.Editor
//{
//    public class RoslynSecurityWindow : EditorWindow
//    {
//        // Private
//        private RoslynCSharp settings = null;
//        //private SecuritySettingsDrawer securityDrawer = null;

//        // Methods
//        [MenuItem("Roslyn C#/Security")]
//        public static void ShowWindow()
//        {
//            GetWindow<RoslynSecurityWindow>();
//        }

//        public void OnEnable()
//        {
//            // Load settings from project
//            settings = RoslynCSharp.LoadAsset();

//            // Use default settings
//            if (settings == null)
//                settings = CreateInstance<RoslynCSharp>();

//            //RoslynCSharpSecurityAllowance s = new RoslynCSharpSecurityAllowance();

//            //s.AddedAssemblies.Add("mscorlib");

//            //securityDrawer = new SecuritySettingsDrawer(settings.securityAllowance);
//        }

//        public void OnGUI()
//        {
//            securityDrawer.OnDrawSettings();
//            securityDrawer.OnDrawAssemblyButtons();
//        }
//    }
//}
