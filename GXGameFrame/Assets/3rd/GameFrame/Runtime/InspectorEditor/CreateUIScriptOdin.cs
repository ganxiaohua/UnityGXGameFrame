#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using FairyGUI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[Serializable]
public class FairyData
{
    [ReadOnly] public string Path;

    public string FieldName;

    public Stack<int> Index;

    [ReadOnly] [HideLabel] [VerticalGroup("Type")]
    public string TypeName;

    [ReadOnly] [HideLabel] [VerticalGroup("Type")]
    public GameObject Com;
}

public class CreateUIScriptOdin : MonoBehaviour
{
    private class Parent
    {
        public Parent(string path, GComponent go)
        {
            Path = path;
            Go = go;
        }

        public string Path;
        public GComponent Go;
        public UIPanel UIPanel;
    }

    private static string CodePath;
    private int index;
    private int namingLength;

    private UIPanel UIPanel;

    private Regex luaNamePattern = new Regex("[^a-zA-Z0-9]");

    private string[] exportTypes = {"GButton", "GList", "GRichTextField", "GTextField", "GComponent", "GLoader", "GTextInput", "GProgressBar"};

    public string SavePath;

    [Button("绑定组件"), ButtonGroup]
    public void ButtonBind()
    {
        BindInit();
    }

    [Button("清空绑定"), ButtonGroup, PropertyOrder(0)]
    public void ButtonBindClean()
    {
        bindList.Clear();
    }

    [Button("生成脚本"), ButtonGroup]
    public void ButtonCreateScript()
    {
        CreateScript();
    }

    [Button("返回主场景", ButtonSizes.Medium), GUIColor(1, 0.6f, 0.4f), PropertyOrder(20)]
    public void ButtonBackMain()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(EditorCSharp.GetEditorString_FirstScenePath());
    }

    [HorizontalGroup] [LabelText("类名:"), LabelWidth(40)]
    public string className;

    [HorizontalGroup]
    [Button("使用包体名作为脚本名", ButtonSizes.Medium)]
    public void DefaultClassName()
    {
        className = "UI" + this.gameObject.GetComponent<UIPanel>().componentName; //默认类名
    }


    [ListDrawerSettings(NumberOfItemsPerPage = 20)] [PropertyOrder(1)]
    // [TableList, Searchable]
    public List<FairyData> bindList = new List<FairyData>();


    [OnInspectorInit("EditorInit")]
    private void EditorInit()
    {
        string componentName = this.gameObject.GetComponent<UIPanel>().componentName;
        className = "UI" + componentName;
        SavePath = EditorCSharp.GetEditorString_UIScriptsPath() + "/" + className;
        BindInit(); //默认进行绑定
    }


    /// <summary>
    /// 绑定初始化
    /// </summary>
    private void BindInit()
    {
        namingLength = 0;
        index = 0;
        bindList.Clear();
        var panel = this.gameObject.GetComponent<UIPanel>();
        var ui = panel.ui;
        Parent p = new Parent(null, ui);
        p.UIPanel = panel;
        UIPanel = panel;
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
        // parent.Go.name
        GObject[] childs = parent.Go.GetChildren();
        List<string> removeDuplicate = new List<string>();
        foreach (GObject child in childs)
        {
            if (string.IsNullOrEmpty(child.name))
                continue;
            if (child.displayObject != null)
            {
                string path = (string.IsNullOrEmpty(parent.Path) ? "" : parent.Path + ".") + child.name;
                string luaName = path.Replace("_", "");
                luaName = luaNamePattern.Replace(luaName, "_");
                if (removeDuplicate.Contains(luaName))
                {
                    index++;
                    luaName += index.ToString();
                }

                if (namingLength < luaName.Length)
                {
                    namingLength = luaName.Length;
                }

                removeDuplicate.Add(luaName);

                FairyData item = new FairyData();
                item.Path = path;
                item.FieldName = luaName;
                item.TypeName = child.GetType().ToString().Replace("FairyGUI.", "");
                item.Com = child.displayObject.gameObject;
                item.Index = new Stack<int>();
                GComponent thisParent = child.parent;
                GObject thisChild = child;
                while (thisParent != null)
                {
                    int x = thisParent.GetChildIndex(thisChild);
                    thisChild = thisParent;
                    thisParent = thisParent.parent;
                    item.Index.Push(x);
                }

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
        if (bindList.Count <= 0)
        {
            EditorUtility.DisplayDialog("提示", "绑定控件列表为空，请绑定！！！", "OK");
            return;
        }

        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        if (File.Exists($"{SavePath}/{className}.cs"))
        {
            if (EditorUtility.DisplayDialog("警告！", className + "已经存在是否覆盖View组件获取脚本.(逻辑脚本会保留)", "覆盖", "取消"))
            {
                ViewFunc(SavePath, className);
            }
        }
        else
        {
            LogicFunc(SavePath, className);
            ViewFunc(SavePath, className);
        }


        void ViewFunc(string codepath, string classname)
        {
            string Text = "";
            for (int i = 0; i < bindList.Count; i++)
            {
                var v = bindList[i];
                Text += CreateEntityAuto.CreateUIAutoComText(v.TypeName, v.FieldName, v.Path);
            }

            CreateEntityAuto.CreateUIViewAutoText(codepath, classname, Text);
        }

        void LogicFunc(string codepath, string classname)
        {
            CreateEntityAuto.CreateUIMain(codepath, classname, UIPanel.packageName, UIPanel.componentName);
            CreateEntityAuto.CreateUIViewText(codepath, classname, UIPanel.packageName, UIPanel.componentName);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
    }
}

#endif