﻿#if UNITY_EDITOR
using System;
using System.IO;
using GameFrame;
using UnityEditor;
using UnityEngine;

public class CreateEnitiyAuto
{
    public static string EnitityTxtPath = "Assets/3rd/GameFrame/Editor/Text/EnitiyCreate/EnitiyCreate.txt";

    public static string UIStartText = "Assets/3rd/GameFrame/Editor/Text/UI/UIStartText.txt";

    public static string UIViewText = "Assets/3rd/GameFrame/Editor/Text/UI/UIViewText.txt";

    public static string UIViewAutoText = "Assets/3rd/GameFrame/Editor/Text/UI/UIViewAutoText.txt";

    public static string[] UIViewAutoTexts;

    [System.Flags]
    public enum InheritedObject
    {
        IStart = 1 << 1,
        IShow = 1 << 2,
        IHide = 1 << 3,
        IUpdate = 1 << 4,
        IClear = 1 << 5,
    }

    public static void WriteEnitit(InheritedObject inheritedobject, string componentName, bool isUI, string createPath, string uipackName = "")
    {
        var txt = File.ReadAllText(EnitityTxtPath);
        string[] text = txt.Split('#', StringSplitOptions.None);
        string componentObject = ComponentObjectIsUI(isUI, componentName);
        string scriptComponentInherit = "";
        string startSystem = "";
        string startSystemContent = StartIsUI(isUI, componentName, uipackName);
        string showSystem = "";
        string showSystemContent = ShowIsUI(isUI, componentName);
        string hideSystem = "";
        string hideSystemContent = HideIsUI(isUI, componentName);
        string updateSystem = "";
        string updateSystemContent = "";
        string clearSystem = "";
        string clearSystemContent = ClearIsUI(isUI, componentName);


        if ((inheritedobject & InheritedObject.IStart) == InheritedObject.IStart)
        {
            startSystem = string.Format(text[2], componentName, startSystemContent);
            scriptComponentInherit += "IStart,";
        }

        if ((inheritedobject & InheritedObject.IShow) == InheritedObject.IShow)
        {
            showSystem = string.Format(text[3], componentName, showSystemContent);
            scriptComponentInherit += "IShow,";
        }

        if ((inheritedobject & InheritedObject.IHide) == InheritedObject.IHide)
        {
            hideSystem = string.Format(text[4], componentName, hideSystemContent);
            scriptComponentInherit += "IHide,";
        }

        if ((inheritedobject & InheritedObject.IUpdate) == InheritedObject.IUpdate)
        {
            updateSystem = string.Format(text[5], componentName, updateSystemContent);
            scriptComponentInherit += "IUpdate,";
        }

        if ((inheritedobject & InheritedObject.IClear) == InheritedObject.IClear)
        {
            clearSystem = string.Format(text[6], componentName, clearSystemContent);
            scriptComponentInherit += "IClear,";
        }

        string scriptComponent = string.Format(text[0], componentName, scriptComponentInherit.Substring(0, scriptComponentInherit.Length - 1), componentObject);
        var scriptComponentSystem = string.Format(text[1], componentName, startSystem + showSystem + hideSystem + updateSystem + clearSystem);
        File.WriteAllText($"{createPath}/{componentName}.cs", scriptComponent);
        File.WriteAllText($"{createPath}/{componentName}System.cs", scriptComponentSystem);
        AssetDatabase.Refresh();
        Debug.Log("创建成功");
    }


    /// <summary>
    /// 如果是UI需要进行修改的
    /// </summary>
    public static string ComponentObjectIsUI(bool isUI, string componentName)
    {
        if (isUI)
        {
            return $"public {componentName}View {componentName}View;";
        }

        return "";
    }


    public static string StartIsUI(bool isUI, string componentName, string packName)
    {
        string[] fuiguid = AssetDatabase.FindAssets($"{packName}_fui t:textAsset");
        string path = AssetDatabase.GUIDToAssetPath(fuiguid[0]);
        var txt = File.ReadAllText(UIStartText);
        if (isUI)
        {
            return string.Format(txt, componentName, path.Replace(StaticText.UIPath, "").Replace("_fui.bytes", ""));
        }
        return "";
    }

    public static string ShowIsUI(bool isUI, string componentName)
    {
        if (isUI)
        {
            return $"self.{componentName}View.Show();";
        }

        return "";
    }

    public static string HideIsUI(bool isUI, string componentName)
    {
        if (isUI)
        {
            return $"self.{componentName}View.Hide();";
        }

        return "";
    }

    public static string ClearIsUI(bool isUI, string componentName)
    {
        if (isUI)
        {
            return $"ReferencePool.Release(self.{componentName}View);";
        }

        return "";
    }


    public static void CreateUIViewText(string createPath, string componentName, string FGUIPakeName, string FGUIClassName)
    {
        var txt = File.ReadAllText(UIViewText);
        txt = string.Format(txt, componentName, FGUIPakeName, FGUIClassName);
        File.WriteAllText($"{createPath}/{componentName}View.cs", txt);
    }

    public static string CreateUIAutoComText(string fguiComponentClassName, string fieldName, string Componentpath, string asClass)
    {
        if (UIViewAutoTexts == null)
        {
            var txt = File.ReadAllText(UIViewAutoText);
            UIViewAutoTexts = txt.Split('#', StringSplitOptions.None);
        }

        return string.Format(UIViewAutoTexts[1], fguiComponentClassName, fieldName, Componentpath, asClass);
    }

    public static void CreateUIViewAutoText(string createPath, string componentName, string content)
    {
        var txt = string.Format(UIViewAutoTexts[0], componentName, content);
        File.WriteAllText($"{createPath}/{componentName}ViewAuto.cs", txt);
    }
}
#endif