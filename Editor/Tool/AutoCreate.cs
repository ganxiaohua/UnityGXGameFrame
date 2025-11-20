using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    enum CreateAuto
    {
        Class,
        Add,
        AddParameter,
        Get,
        Set,
        ComponentsMain,
        Capability
    }

    public partial class AutoCreate
    {
        private static Dictionary<CreateAuto, string> s_TextDictionary;
        private static Dictionary<Type, string> s_ViewBindDictionary;
        private static StringBuilder tempStr = new StringBuilder(1024);

        public static void AutoAllScript()
        {
            if (EditorApplication.isPlaying)
                return;
            OpFile.DeleteFilesInDirectory(EditorString.GetPath("CompOutPutPath"));
            LoadText();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            var number = 0;
            foreach (var item in assembly)
            {
                foreach (var name in EditorString.GetPaths("AssemblyNames"))
                {
                    if (item.GetName().Name != name) continue;
                    number += FindAllECSCom(item);
                    break;
                }
            }

            CreateComponents(number);
            CreateCapabiltys();
            AssetDatabase.Refresh();
            Debugger.Log("生成完毕");
        }

        private static void LoadText()
        {
            var GameFramePath = EditorString.GetPath("GameFramePath");
            string add = GameFramePath + "Editor/Text/ECS/Add.txt";
            string cls = GameFramePath + "Editor/Text/ECS/Class.txt";
            string addparameter = GameFramePath + "Editor/Text/ECS/AddParameter.txt";
            string get = GameFramePath + "Editor/Text/ECS/Get.txt";
            string set = GameFramePath + "Editor/Text/ECS/Set.txt";
            string ECSALLComponents = GameFramePath + "Editor/Text/ECS/Components.txt";
            string ECSALLCapabiltys = GameFramePath + "Editor/Text/ECS/Capabiltys.txt";

            string stradd = File.ReadAllText(add);
            string strcls = File.ReadAllText(cls);
            string straddparameter = File.ReadAllText(addparameter);
            string strget = File.ReadAllText(get);
            string strset = File.ReadAllText(set);
            string strAllComponents = File.ReadAllText(ECSALLComponents);
            string strAllCapabiltys = File.ReadAllText(ECSALLCapabiltys);
            if (s_ViewBindDictionary == null)
            {
                s_ViewBindDictionary = new Dictionary<Type, string>();
            }

            s_ViewBindDictionary.Clear();
            if (s_TextDictionary == null)
            {
                s_TextDictionary = new Dictionary<CreateAuto, string>();
            }

            s_TextDictionary.Clear();
            s_TextDictionary.Add(CreateAuto.Class, strcls);
            s_TextDictionary.Add(CreateAuto.Add, stradd);
            s_TextDictionary.Add(CreateAuto.AddParameter, straddparameter);
            s_TextDictionary.Add(CreateAuto.Get, strget);
            s_TextDictionary.Add(CreateAuto.Set, strset);
            s_TextDictionary.Add(CreateAuto.ComponentsMain, strAllComponents);
            s_TextDictionary.Add(CreateAuto.Capability, strAllCapabiltys);
        }

        private static int FindAllECSCom(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            int number = 0;
            foreach (var tp in types)
            {
                if (typeof(EffComponent).IsAssignableFrom(tp) && tp.IsClass)
                {
                    var vb = tp.GetCustomAttribute<ViewBindAttribute>();
                    if (vb != null)
                    {
                        AddViewBind(tp, vb.BindType);
                    }

                    number++;
                    CreateCompCshap(tp);
                }
            }

            return number;
        }

        //创建c#脚本
        private static void CreateCompCshap(Type type)
        {
            if (type.Name == nameof(EffComponent))
            {
                return;
            }

            string ECSComponentName = nameof(EffEntity);
            var vb = type.GetCustomAttribute<AssignBindAttribute>();
            if (vb != null)
            {
                ECSComponentName = vb.BindType.FullName;
            }

            if (type.FullName.Contains("GamePlay.Runtime.BeUseFuncComp"))
            {
                Debug.Log("xxxxxx");
            }

            FieldInfo[] variable = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertyInfos = type.GetProperties();
            string typeName = type.Name;
            string typeFullName = type.FullName;
            tempStr.Clear();
            string abcls = s_TextDictionary[CreateAuto.Class];
            string abAdd = string.Format(s_TextDictionary[CreateAuto.Add], typeName, ECSComponentName, typeFullName);
            string abGet = string.Format(s_TextDictionary[CreateAuto.Get], typeName, typeFullName, ECSComponentName);
            string abSet = "";
            string addParameter = "";
            string fieldName = "";
            string fieldTypeName = "";
            if (propertyInfos.Length > 0)
            {
                fieldName = propertyInfos[0].Name;
                fieldTypeName = propertyInfos[0].PropertyType.ToString();
            }
            else if (variable.Length > 0)
            {
                fieldName = variable[0].Name;
                fieldTypeName = variable[0].FieldType.ToString();
            }

            fieldTypeName = fieldTypeName.Replace("[]", "!!");
            fieldTypeName = fieldTypeName.Replace("`3[", "<").Replace("`2[", "<").Replace("`1[", "<").Replace("]", ">");
            fieldTypeName = fieldTypeName.Replace("!!", "[]");
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (fieldTypeName == "String")
                {
                    fieldTypeName = "string";
                }

                addParameter = string.Format(s_TextDictionary[CreateAuto.AddParameter], typeName, fieldTypeName, typeFullName, fieldName, ECSComponentName);
                string evetString = "";
                if (s_ViewBindDictionary.TryGetValue(type, out evetString))
                {
                }

                abSet = string.Format(s_TextDictionary[CreateAuto.Set], typeName, fieldTypeName, typeFullName, fieldName, evetString, ECSComponentName);
            }

            tempStr.Append(abAdd);
            tempStr.Append(addParameter);
            tempStr.Append(abGet);
            tempStr.Append(abSet);
            string lastText = string.Format(abcls, typeName, tempStr.ToString());
            CreateDirectory(EditorString.GetPath("CompOutPutPath"));
            File.WriteAllText($"{EditorString.GetPath("CompOutPutPath")}{typeName}Auto.cs", lastText);
        }

        private static void AddViewBind(Type type, Type bindType)
        {
            MethodInfo[] methods = bindType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            string funcName = methods[0].Name;
            s_ViewBindDictionary.Add(type, $"View view = effEntity.GetView();\n" +
                                           $"        if (view == null) return null;\n" +
                                           $"        (({bindType.FullName}) (view.Value)).{funcName}(p);");
        }


        private static void CreateComponents(int number)
        {
            var str = string.Format(s_TextDictionary[CreateAuto.ComponentsMain], number);
            File.WriteAllText($"{EditorString.GetPath("CompOutPutPath")}Components.cs", str);
        }

        private static void CreateDirectory(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}