using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CSharpProjectHelper
{
    private static bool RedirectOutput()
    {
        try
        {
            foreach (var file in Directory.GetFiles("./", "*.csproj", SearchOption.TopDirectoryOnly))
            {
                string text = File.ReadAllText(file, Encoding.UTF8);
                var match = Regex.Match(text, @"<OutputPath>(.*)</OutputPath>");
                string tmpOutput = match.Groups[1].ToString();
                if (!Directory.Exists(tmpOutput))
                    Directory.CreateDirectory(tmpOutput);
                string name = Path.GetFileName(tmpOutput.TrimEnd('\\'));
                string source = $"Library/ScriptAssemblies/{name}.dll";
                if (File.Exists(source))
                    File.Copy(source, $"{tmpOutput}\\{name}.dll", true);
                //text = Regex.Replace(text, @"<OutputPath>(.*)</OutputPath>", @"<OutputPath>Library\ScriptAssemblies\</OutputPath>");
                //File.WriteAllText(file, text, Encoding.UTF8);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return false;
    }
    
    [MenuItem("CSharpProject/ManualRedirectOutput")]
    private static void ManualRedirectOutput()
    {
        if (RedirectOutput())
        {
            EditorUtility.DisplayDialog("Tip", "Redirect Output Complete!", "ok");
        }
        else
        {
            EditorUtility.DisplayDialog("Tip", "Redirect Output Failed!", "ok");
        }
    }
    
    [MenuItem("CSharpProject/AutoRedirectOutput", true)]
    private static bool AutoRedirectOutputValidate()
    {
        Menu.SetChecked("CSharpProject/AutoRedirectOutput", EditorPrefs.HasKey("CSharpProject/AutoRedirectOutput"));
        return true;
    }

    [MenuItem("CSharpProject/AutoRedirectOutput")]
    private static void AutoRedirectOutput()
    {
        if (EditorPrefs.HasKey("CSharpProject/AutoRedirectOutput"))
            EditorPrefs.DeleteKey("CSharpProject/AutoRedirectOutput");
        else
            EditorPrefs.SetBool("CSharpProject/AutoRedirectOutput", true);
    }

    [InitializeOnLoadMethod]
    private static void AutoRedirectOutputOnLoad()
    {
        if (EditorPrefs.HasKey("CSharpProject/AutoRedirectOutput"))
        {
            RedirectOutput();
        }
    }
}
