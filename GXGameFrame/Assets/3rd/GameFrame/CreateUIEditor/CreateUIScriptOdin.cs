#if UNITY_EDITOR
using FairyGUI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class FairyData
{
    [FormerlySerializedAs("path")] [ReadOnly]
    public string Path;

    [FormerlySerializedAs("luaName")] public string FieldName;

    [FormerlySerializedAs("typeName")] [ReadOnly] [HideLabel] [VerticalGroup("Type")]
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

    [Title("默认路径")] [ReadOnly] [HideLabel] public string dafaultPath;

    private const string codePath = "/GxGame/Scripts/UI";
    private static string saveCodePath;

    private int Index;
    private int NamingLength;

    private UIPanel UIPanel;

    private Regex luaNamePattern = new Regex("[^a-zA-Z0-9]");

    private string[] m_ExportTypes = {"GButton", "GList", "GRichTextField", "GTextField", "GComponent", "GLoader", "GTextInput", "GProgressBar"};

    [Title("自定义路径选择")] [HideLabel] [FolderPath(ParentFolder = "Assets/GxGame/Scripts/UI", AbsolutePath = true)] [OnValueChanged("ButtonBind")]
    public string custom_Path;

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
        EditorSceneManager.OpenScene("Assets/GXGame/Scenes/Init.unity");
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
        dafaultPath = (Application.dataPath + codePath).Replace("\\", "/"); //初始化默认路径

        custom_Path = dafaultPath;

        if (string.IsNullOrEmpty(className))
        {
            className = "UI" + this.gameObject.GetComponent<UIPanel>().componentName; //默认类名
        }

        CheckSavePath();
        BindInit(); //默认进行绑定
    }


    /// <summary>
    /// 检测存储路径
    /// </summary>
    private void CheckSavePath()
    {
        if (string.IsNullOrEmpty(custom_Path))
        {
            saveCodePath = dafaultPath;
        }
        else
        {
            saveCodePath = custom_Path;
        }
    }


    /// <summary>
    /// 绑定初始化
    /// </summary>
    private void BindInit()
    {
        NamingLength = 0;
        Index = 0;
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
                    Index++;
                    luaName += Index.ToString();
                }

                if (NamingLength < luaName.Length)
                {
                    NamingLength = luaName.Length;
                }

                removeDuplicate.Add(luaName);

                FairyData item = new FairyData();
                item.Path = path;
                item.FieldName = luaName;
                item.TypeName = child.GetType().ToString().Replace("FairyGUI.", "");
                item.Com = child.displayObject.gameObject;
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

        CheckSavePath();
        try
        {
            if (File.Exists($"{saveCodePath}/{className}.cs"))
            {
                if (EditorUtility.DisplayDialog("警告！", className + "已经存在是否覆盖View组件获取脚本.(逻辑脚本会保留)", "覆盖", "取消"))
                {
                    ViewFunc(saveCodePath, className);
                }
            }
            else
            {
                LogicFunc(saveCodePath, className);
                ViewFunc(saveCodePath, className);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }


        void ViewFunc(string codepath, string classname)
        {
            string fields = "";
            for (int i = 0; i < bindList.Count; i++)
            {
                var v = bindList[i];
                string name = v.FieldName;
                string path = v.Path;
                string typeName = v.TypeName;
                fields += CreateEnitiyAuto.CreateUIAutoComText(typeName, name, path, typeName);
            }
            CreateEnitiyAuto.CreateUIViewAutoText(codepath, classname, fields);
        }

        void LogicFunc(string codepath, string classname)
        {
            CreateEnitiyAuto.WriteEnitit((CreateEnitiyAuto.InheritedObject) 0b11111, classname, true, codepath, UIPanel.packageName);
            CreateEnitiyAuto.CreateUIViewText(codepath, classname, UIPanel.packageName, UIPanel.componentName);
            CreateEnitiyAuto.CreateUIDataText(codepath, classname);

        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
    }
}

#endif