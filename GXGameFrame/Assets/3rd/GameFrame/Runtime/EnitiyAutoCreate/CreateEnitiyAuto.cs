#if UNITY_EDITOR
using System;
using System.IO;

public class CreateEnitiyAuto
{
    
    
    public static string UIMainUIText = "Assets/3rd/GameFrame/Editor/Text/UI/MainUI.txt";
    public static string UIViewText = "Assets/3rd/GameFrame/Editor/Text/UI/UIViewText.txt";
    public static string UIViewAutoText = "Assets/3rd/GameFrame/Editor/Text/UI/UIViewAutoText.txt";
    public static string[] UIViewAutoTexts;
    
    
    public static void CreateUIMain(string createPath, string componentName, string FGUIPakeName, string FGUIClassName)
    {
        var txt = File.ReadAllText(UIMainUIText);
        txt = string.Format(txt, componentName, FGUIPakeName, FGUIClassName);
        File.WriteAllText($"{createPath}/{componentName}Window.cs", txt);
    }
    
    public static void CreateUIViewText(string createPath, string componentName, string FGUIPakeName, string FGUIClassName)
    {
        var txt = File.ReadAllText(UIViewText);
        txt = string.Format(txt, componentName, FGUIPakeName, FGUIClassName);
        File.WriteAllText($"{createPath}/{componentName}View.cs", txt);
    }

    public static string CreateUIAutoComText(string componentName, string fieldName,string path)
    {
        if (UIViewAutoTexts == null)
        {
            var txt = File.ReadAllText(UIViewAutoText);
            UIViewAutoTexts = txt.Split('#', StringSplitOptions.None);
        }

        return string.Format(UIViewAutoTexts[1], componentName, fieldName,path);
    }

    public static void CreateUIViewAutoText(string createPath,string componentName, string content)
    {
        var txt = string.Format(UIViewAutoTexts[0],componentName, content);
        File.WriteAllText($"{createPath}/{componentName}ViewAuto.cs", txt);
    }
}
#endif