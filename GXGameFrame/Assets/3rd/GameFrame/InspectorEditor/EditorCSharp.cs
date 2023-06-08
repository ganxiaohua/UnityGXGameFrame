using System;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;

#if UNITY_EDITOR

public static class EditorCSharp
{
    public static Assembly EditorAssembly;

    public static Assembly Get()
    {
        if (EditorAssembly == null)
        {
            byte[] bytes = File.ReadAllBytes("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll");
            EditorAssembly = Assembly.Load(bytes);
        }

        return EditorAssembly;
    }

    public static string GetEditorString_FirstScenePath()
    {
        Type myType = Get().GetType("GameFrame.Editor.EditorString");
        FieldInfo myField = myType.GetField("FirstScenePath", BindingFlags.Static | BindingFlags.Public);
        var value = myField.GetValue(null);
        return (string)value;
    }
    
    public static string GetEditorString_UIScriptsPath()
    {
        Type myType = Get().GetType("GameFrame.Editor.EditorString");
        FieldInfo myField = myType.GetField("UIScriptsPath", BindingFlags.Static | BindingFlags.Public);
        var value = myField.GetValue(null);
        return (string)value;
    }
}
#endif