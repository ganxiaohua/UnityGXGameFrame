using System;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR

public static class EditorCSharp
{
    public static Assembly EditorAssembly;

    public static Assembly Get(string dllPath)
    {
        byte[] bytes = File.ReadAllBytes(dllPath);
        return Assembly.Load(bytes);
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