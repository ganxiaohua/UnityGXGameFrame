using System;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;

#if UNITY_EDITOR

public static class EditorCSharp
{
    public static Assembly EditorAssembly;

    public static Assembly Get(string dllPath)
    {
        byte[] bytes = File.ReadAllBytes(dllPath);
        return Assembly.Load(bytes);
    }

    public static string GetEditorString_FirstScenePath()
    {
        Type myType = Get("Library/ScriptAssemblies/GameFrame.Editor.dll").GetType("GameFrame.Editor.EditorString");
        FieldInfo myField = myType.GetField("FirstScenePath", BindingFlags.Static | BindingFlags.Public);
        var value = myField.GetValue(null);
        return (string)value;
    }
    
    public static string GetEditorString_UIScriptsPath()
    {
        Type myType = Get("Library/ScriptAssemblies/GameFrame.Editor.dll").GetType("GameFrame.Editor.EditorString");
        FieldInfo myField = myType.GetField("UIScriptsPath", BindingFlags.Static | BindingFlags.Public);
        var value = myField.GetValue(null);
        return (string)value;
    }
    public static int GetProjectConfigData_ActivePlayModeIndex()
    {
        Type myType = Get("Library/ScriptAssemblies/Unity.Addressables.Editor.dll").GetType("UnityEditor.AddressableAssets.Settings.ProjectConfigData");
        PropertyInfo myField = myType.GetProperty("ActivePlayModeIndex", BindingFlags.Static | BindingFlags.Public);
        var value = myField.GetValue(null);
        return (int) value;
    }
}
#endif