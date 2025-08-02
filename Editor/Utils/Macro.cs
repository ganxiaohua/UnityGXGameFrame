using UnityEditor;

namespace GameFrame.Editor
{
    public static class Macro
    {
        public const string ShowAssert = "ShowAssert";

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
    }
}