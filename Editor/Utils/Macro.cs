using UnityEditor;

namespace GameFrame.Editor
{
    public static class Macro
    {
        public const string ShowAssert = "ShowAssert";
        public const string Tracked = "Tracked";
        public const string ShowEditorLine = "ShowEditorLine";

        [MenuItem("GX框架工具/宏定义/" + ShowAssert, true, 0)]
        private static bool CommonAssertDisableValidateV()
        {
            Menu.SetChecked("GX框架工具/宏定义/" + ShowAssert, MacroDefineHelper.Contains(ShowAssert));
            return true;
        }


        [MenuItem("GX框架工具/宏定义/" + ShowAssert, priority = 0)]
        private static void CommonAssertDisableValidate()
        {
            if (MacroDefineHelper.Contains(ShowAssert))
            {
                MacroDefineHelper.Disable(ShowAssert);
            }
            else
            {
                MacroDefineHelper.Enable(ShowAssert);
            }
        }


        [MenuItem("GX框架工具/宏定义/" + Tracked, true, 2)]
        private static bool TrackedDisableValidateV()
        {
            Menu.SetChecked("GX框架工具/宏定义/" + Tracked, MacroDefineHelper.Contains(Tracked));
            return true;
        }


        [MenuItem("GX框架工具/宏定义/" + Tracked, priority = 2)]
        private static void TrackedDisableValidate()
        {
            if (MacroDefineHelper.Contains(Tracked))
            {
                MacroDefineHelper.Disable(Tracked);
            }
            else
            {
                MacroDefineHelper.Enable(Tracked);
            }
        }


        [MenuItem("GX框架工具/宏定义/" + ShowEditorLine, true, 3)]
        private static bool ShowEditorLineDisableValidateV()
        {
            Menu.SetChecked("GX框架工具/宏定义/" + ShowEditorLine, MacroDefineHelper.Contains(ShowEditorLine));
            return true;
        }


        [MenuItem("GX框架工具/宏定义/" + ShowEditorLine, priority = 3)]
        private static void ShowEditorLineDisableValidate()
        {
            if (MacroDefineHelper.Contains(ShowEditorLine))
            {
                MacroDefineHelper.Disable(ShowEditorLine);
            }
            else
            {
                MacroDefineHelper.Enable(ShowEditorLine);
            }
        }


        [MenuItem("GX框架工具/UI/Auto Update Gen Code", true)]
        private static bool AutoUpdateValidate()
        {
            Menu.SetChecked("GX框架工具/UI/Auto Update Gen Code", UnityEditor.EditorPrefs.HasKey(EditorPrefsKey.KeyAuto));
            return true;
        }

        [MenuItem("GX框架工具/UI/Auto Update Gen Code")]
        private static void AutoUpdate()
        {
            if (UnityEditor.EditorPrefs.HasKey(EditorPrefsKey.KeyAuto))
                UnityEditor.EditorPrefs.DeleteKey(EditorPrefsKey.KeyAuto);
            else
                UnityEditor.EditorPrefs.SetBool(EditorPrefsKey.KeyAuto, true);
        }
    }
}