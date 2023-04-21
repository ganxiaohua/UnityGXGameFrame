#if UNITY_EDITOR
using FairyGUI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[Serializable]
public class FairyData
{
    [ReadOnly]
    public string path;
    public string luaName;

    [ReadOnly]
    [HideLabel]
    [VerticalGroup("Type")]
    public string typeName;

    [ReadOnly]
    [HideLabel]
    [VerticalGroup("Type")]
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
    }

    [Title("默认路径")]
    [ReadOnly]
    [HideLabel]
    public string dafaultPath;

    private const string codePath = "/Lua/UI";
    private static string saveCodePath;

    private int Index;
    private int NamingLength;

    private Regex luaNamePattern = new Regex("[^a-zA-Z0-9]");

    private string[] m_ExportTypes = { "GButton", "GList", "GRichTextField", "GTextField", "GComponent", "GLoader", "GTextInput", "GProgressBar" };

    [Title("自定义路径选择")]
    [HideLabel]
    [FolderPath(ParentFolder = "Assets/Lua/UI", AbsolutePath = true)]
    [OnValueChanged("ButtonBind")]
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
        EditorSceneManager.OpenScene("Assets/Init.unity");
    }

    [LabelText("是否生成资源栏")]
    public bool CheckResource;

    [HorizontalGroup]
    [LabelText("类名:"), LabelWidth(40)]
    public string className;
    [HorizontalGroup]
    [Button("使用包体名作为脚本名", ButtonSizes.Medium)]
    public void DefaultClassName()
    {
        className = "UI" + this.gameObject.GetComponent<UIPanel>().componentName;  //默认类名
    }


    [ListDrawerSettings(NumberOfItemsPerPage = 20)]
    [PropertyOrder(1)]
    // [TableList, Searchable]
    public List<FairyData> bindList = new List<FairyData>();

    [OnInspectorInit("EditorInit")]
    private void EditorInit()
    {
        dafaultPath = (Application.dataPath + codePath).Replace("\\", "/");  //初始化默认路径

        custom_Path = dafaultPath;

        if (string.IsNullOrEmpty(className))
        {
            className = "UI" + this.gameObject.GetComponent<UIPanel>().componentName;  //默认类名
        }

        CheckSavePath();
        BindInit();  //默认进行绑定
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
        var ui = this.gameObject.GetComponent<UIPanel>().ui;
        Parent p = new Parent(null, ui);
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
                item.path = path;
                item.luaName = luaName;
                item.typeName = child.GetType().ToString().Replace("FairyGUI.", "");
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
            if (File.Exists($"{saveCodePath}/{className}.lua"))
            {
                if (EditorUtility.DisplayDialog("警告！", className + "已经存在是否覆盖View组件获取脚本.(逻辑脚本会保留)", "覆盖", "取消"))
                {
                    ViewLua(saveCodePath, className);
                }
            }
            else
            {
                LogicLua(saveCodePath, className);
                ViewLua(saveCodePath, className);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }


        void ViewLua(string codepath, string classname)
        {
            int tcount = (NamingLength + 3) / 4 + 1;
            using (StreamWriter sw = new StreamWriter($"{codepath}/View{classname}.lua"))
            {
                sw.WriteLine("---自动生成文件,不要手动修改");
                sw.WriteLine("");
                sw.WriteLine("local Components = {}");
                sw.WriteLine("function Components.GetComponents(self)");
                sw.WriteLine("\tlocal get = self._get");
                for (int i = 0; i < bindList.Count; i++)
                {
                    var v = bindList[i];
                    string name = v.luaName;
                    string path = v.path;
                    string typeName = v.typeName;
                    string annotation = "";
                    // if (!Array.Exists(m_ExportTypes, typeName.Equals))
                    // {
                    //     // annotation = "--";
                    // }

                    string funcName;
                    if (path.Contains("."))
                    {
                        funcName = $"self:GetChildByPath(\"{path}\")";
                    }
                    else
                    {
                        funcName = $"self:GetChild(\"{path}\")";
                    }
                    var innerName = $"_com{i}";
                    sw.WriteLine($"\t{annotation}get.{name.PadRight(NamingLength)} = function() if not self.{innerName} then self.{innerName} = {funcName} end return self.{innerName} end --{typeName}");
                }

                sw.WriteLine("end\n");
                sw.WriteLine("return Components");
            }
        }

        void LogicLua(string codepath, string classname)
        {
            using (StreamWriter sw = new StreamWriter($"{codepath}/{classname}.lua"))
            {
                string viewpath = codepath.Replace(Application.dataPath + "/Lua/", "").Replace("/", ".") + ".View" +
                                  classname.Replace("/", ".");
                sw.WriteLine($"local DependenRecPackage = require(\"UI.DependenRecPackage\")");
                sw.WriteLine($"local Components = require(\"{viewpath}\")");
                sw.WriteLine("local UIPackage = FairyGUI.UIPackage");
                sw.WriteLine("local Base = require(\"UI.WindowBase\")");
                sw.WriteLine("local M = Base:Extend()");
                if (CheckResource)
                {
                    sw.WriteLine("local UITop = require(\"UI.Common.UI.UITop\")");
                }
                sw.WriteLine("M.DependenRec = nil--关联资源");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("function M:OnInit()");
                sw.WriteLine("\tBase.OnInit(self)");
                sw.WriteLine("\tComponents.GetComponents(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("function M:OnShown()");
                sw.WriteLine("\tBase.OnShown(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("function M:DoShowAnimation()");
                sw.WriteLine("\tBase.DoShowAnimation(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("function M:DoHideAnimation()");
                sw.WriteLine("\tBase.DoHideAnimation(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("function M:OnHide()");
                sw.WriteLine("\tBase.OnHide(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("function M:Dispose()");
                sw.WriteLine("\tBase.Dispose(self)");
                sw.WriteLine("end");
                sw.WriteLine("");
                sw.WriteLine("return M");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("提示", "代码生成完毕", "OK");
    }

}

#endif

