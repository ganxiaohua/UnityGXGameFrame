#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FairyGUI;
using GameFrame.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;

[Serializable]
public class FairyData
{
    public string ParentName;

    public string Path;

    public string FieldName;

    public string TypeName;
}

public class CreateUICode
{
    private class Parent
    {
        public Parent(string parentFiled, GComponent go)
        {
            ParentFiled = parentFiled;
            Go = go;
        }

        public string ParentFiled;
        public GComponent Go;
    }

    private UIPanel uiPanel;

    private string[] exportTypes =
        {"GButton", "GList", "GRichTextField", "GTextField", "GComponent", "GLoader", "GTextInput", "GProgressBar", "GLabel", "GGroup"};

    private string className;

    private List<FairyData> bindList = new List<FairyData>();

    private string savePath;

    public void ButtonCreateScript(UIPanel panel)
    {
        EditorInit(panel);
        CreateScript();
    }

    public void ButtonBackMain()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(EditorString.FirstScenePath);
    }

    private void EditorInit(UIPanel panel)
    {
        uiPanel = panel;
        string componentName = panel.componentName;
        className = "UI" + componentName;
        savePath = EditorString.UIScriptsPath + "/" + className;
        BindInit(); //默认进行绑定
    }


    /// <summary>
    /// 绑定初始化
    /// </summary>
    private void BindInit()
    {
        bindList.Clear();
        var panel = uiPanel;
        var ui = panel.ui;
        Parent p = new Parent("Root", ui);
        uiPanel = panel;
        AutoBindComponent(p);
    }


    /// <summary>
    /// 绑定组件
    /// </summary>
    /// <param name="parent"></param>
    private void AutoBindComponent(Parent parent)
    {
        if (parent == null || parent.Go == null)
            return;
        GObject[] childs = parent.Go.GetChildren();
        foreach (GObject child in childs)
        {
            if (string.IsNullOrEmpty(child.name) || !exportTypes.Contains(child.GetType().Name))
                continue;
            if (child.displayObject != null)
            {
                string path = parent.ParentFiled + "_" + child.name;
                path = path.Replace("Root_", "");
                FairyData item = new FairyData();
                item.Path = child.name;
                item.FieldName = path;
                item.TypeName = child.GetType().Name;
                item.ParentName = parent.ParentFiled;
                bindList.Add(item);
                if (child is GComponent)
                {
                    Parent parents = new Parent(path, child as GComponent);
                    AutoBindComponent(parents);
                }
            }
        }
    }


    private void CreateScript()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        if (!File.Exists(savePath + "/" + className + ".cs"))
        {
            LogicFunc(savePath, className);
        }

        ViewFunc(savePath, className);


        void ViewFunc(string codepath, string classname)
        {
            string Text = "";
            for (int i = 0; i < bindList.Count; i++)
            {
                var v = bindList[i];
                Text += CreateEntityAuto.CreateUIAutoComText(v.TypeName, v.FieldName, v.Path, v.ParentName);
            }

            CreateEntityAuto.CreateUIViewAutoText(codepath, classname, Text);
        }

        void LogicFunc(string codepath, string classname)
        {
            CreateEntityAuto.CreateUIMain(codepath, classname, uiPanel.packageName, uiPanel.componentName);
            CreateEntityAuto.CreateUIViewText(codepath, classname, uiPanel.packageName, uiPanel.componentName);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
    }
}

#endif