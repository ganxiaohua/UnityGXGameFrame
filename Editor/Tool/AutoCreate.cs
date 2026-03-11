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
        AddExternal,
        AddParameter,
        SetExternal,
        Get,
        Set,
        ComponentsMain,
        ComponentsDispose,
        Capability
    }

    public partial class AutoCreate
    {
        private static Dictionary<CreateAuto, string> sTextDictionary;

        // private static Dictionary<Type, string> sViewBindDictionary;
        private static List<Type> sEcsComList;
        private static StringBuilder sTempStr = new StringBuilder(1024);

        public static void AutoAllScript()
        {
            if (EditorApplication.isPlaying)
                return;
            OpFile.DeleteFilesInDirectory(EditorString.GetPath("CompOutPutPath"));
            LoadText();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assembly)
            {
                foreach (var name in EditorString.GetPaths("AssemblyNames"))
                {
                    if (item.GetName().Name != name) continue;
                    FindAllECSCom(item);
                    break;
                }
            }

            CreateComponents();
            CreateCapabiltys();
            // sViewBindDictionary.Clear();
            sTextDictionary.Clear();
            sTempStr.Clear();
            sEcsComList.Clear();
            AssetDatabase.Refresh();
            Debugger.Log("生成完毕");
        }

        private static void LoadText()
        {
            var GameFramePath = EditorString.GetPath("GameFramePath");
            string add = GameFramePath + "Editor/Text/ECS/Add.txt";
            string addExternal = GameFramePath + "Editor/Text/ECS/AddExternal.txt";
            string setExternal = GameFramePath + "Editor/Text/ECS/SetExternal.txt";
            string cls = GameFramePath + "Editor/Text/ECS/Class.txt";
            string addparameter = GameFramePath + "Editor/Text/ECS/AddParameter.txt";
            string get = GameFramePath + "Editor/Text/ECS/Get.txt";
            string set = GameFramePath + "Editor/Text/ECS/Set.txt";
            string allComponents = GameFramePath + "Editor/Text/ECS/Components.txt";
            string allCapabiltys = GameFramePath + "Editor/Text/ECS/Capabiltys.txt";
            string componentsDispose = GameFramePath + "Editor/Text/ECS/ComponentsDispose.txt";

            string stradd = File.ReadAllText(add);
            string straddaddExternal = File.ReadAllText(addExternal);
            string strcls = File.ReadAllText(cls);
            string straddparameter = File.ReadAllText(addparameter);
            string strget = File.ReadAllText(get);
            string strset = File.ReadAllText(set);
            string strsetExternal = File.ReadAllText(setExternal);
            string strAllComponents = File.ReadAllText(allComponents);
            string strAllCapabiltys = File.ReadAllText(allCapabiltys);
            string strComponentsDispose = File.ReadAllText(componentsDispose);
            //sViewBindDictionary ??= new Dictionary<Type, string>();
            //sViewBindDictionary.Clear();
            sEcsComList ??= new List<Type>();
            sEcsComList.Clear();
            sTextDictionary ??= new Dictionary<CreateAuto, string>();
            sTextDictionary.Clear();
            sTextDictionary.Add(CreateAuto.Class, strcls);
            sTextDictionary.Add(CreateAuto.Add, stradd);
            sTextDictionary.Add(CreateAuto.AddExternal, straddaddExternal);
            sTextDictionary.Add(CreateAuto.AddParameter, straddparameter);
            sTextDictionary.Add(CreateAuto.Get, strget);
            sTextDictionary.Add(CreateAuto.Set, strset);
            sTextDictionary.Add(CreateAuto.SetExternal, strsetExternal);
            sTextDictionary.Add(CreateAuto.ComponentsMain, strAllComponents);
            sTextDictionary.Add(CreateAuto.ComponentsDispose, strComponentsDispose);
            sTextDictionary.Add(CreateAuto.Capability, strAllCapabiltys);
        }

        private static void FindAllECSCom(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();

            foreach (var tp in types)
            {
                if (typeof(EffComponent).IsAssignableFrom(tp))
                {
                    // var vb = tp.GetCustomAttribute<ViewBindAttribute>();
                    // if (vb != null)
                    // {
                    //     AddViewBind(tp, vb.BindType);
                    // }

                    CreateCompCshap(tp);
                }
            }
        }

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

            sEcsComList.Add(type);
            FieldInfo[] variable = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertyInfos = type.GetProperties();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public |
                                                   BindingFlags.Instance |
                                                   BindingFlags.DeclaredOnly);
            string abSet = "";
            string typeName = type.Name;
            string typeFullName = type.FullName;
            sTempStr.Clear();
            string abcls = sTextDictionary[CreateAuto.Class];
            string abAdd = string.Format(sTextDictionary[CreateAuto.Add], typeName, ECSComponentName, typeFullName);
            string addExternal = string.Empty;
            string setAddEx = string.Empty;
            string abGet = string.Format(sTextDictionary[CreateAuto.Get], typeName, typeFullName, ECSComponentName);
            string addParameter = "";
            string fieldName = "";
            string fieldTypeName = "";
            bool equatable = false;
            Type fieldType = null;
            /***/
            foreach (MethodInfo method in methods)
            {
                if (method.Name.Contains("Init"))
                {
                    ParameterInfo[] parameters = method.GetParameters();

                    foreach (ParameterInfo param in parameters)
                    {
                        var fileType = param.ParameterType.FullName;
                        fileType = TypeNameHelper.GetFullNameWithoutAssemblyDetails(param.ParameterType);
                        addExternal = string.Format(sTextDictionary[CreateAuto.AddExternal], typeName, ECSComponentName, typeFullName, fileType);
                        break;
                    }
                }
                else if (method.Name.Contains("Set"))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    foreach (ParameterInfo param in parameters)
                    {
                        var fileType = param.ParameterType.FullName;
                        fileType = TypeNameHelper.GetFullNameWithoutAssemblyDetails(param.ParameterType);
                        setAddEx = string.Format(sTextDictionary[CreateAuto.SetExternal], typeName, ECSComponentName, typeFullName, fileType);
                        break;
                    }
                }
                else if (method.Name.Contains("Equatable"))
                {
                    equatable = true;
                }
            }

            /***/
            if (propertyInfos.Length > 0)
            {
                fieldName = propertyInfos[0].Name;
                fieldType = propertyInfos[0].PropertyType;
            }
            else if (variable.Length > 0)
            {
                fieldName = variable[0].Name;
                fieldType = variable[0].FieldType;
            }

            fieldTypeName = TypeNameHelper.GetFullNameWithoutAssemblyDetails(fieldType);
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (fieldTypeName == "String")
                {
                    fieldTypeName = "string";
                }

                addParameter = string.Format(sTextDictionary[CreateAuto.AddParameter], typeName, fieldTypeName, typeFullName, fieldName, ECSComponentName);
                // string evetString = "";
                // if (sViewBindDictionary.TryGetValue(type, out evetString))
                // {
                // }

                string equatable1 = string.Empty;
                if (equatable)
                {
                    equatable1 = "if (p->Equatable(param))";
                }

                abSet = string.Format(sTextDictionary[CreateAuto.Set], typeName, fieldTypeName, typeFullName, fieldName, ECSComponentName, equatable1);
            }

            if (string.IsNullOrEmpty(addExternal))
            {
                sTempStr.Append(abAdd);
                sTempStr.Append(addParameter);
            }
            else
            {
                sTempStr.Append(addExternal);
            }

            sTempStr.Append(abGet);
            sTempStr.Append(abSet);
            if (!string.IsNullOrEmpty(setAddEx))
                sTempStr.Append(setAddEx);
            string lastText = string.Format(abcls, typeName, sTempStr.ToString());
            CreateDirectory(EditorString.GetPath("CompOutPutPath"));
            File.WriteAllText($"{EditorString.GetPath("CompOutPutPath")}{typeName}Auto.cs", lastText);
        }

        // private static void AddViewBind(Type type, Type bindType)
        // {
        //     MethodInfo[] methods = bindType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        //     string funcName = methods[0].Name;
        //     sViewBindDictionary.Add(type, $"View view = effEntity.GetView();\n" +
        //                                   $"        if (view == null) return null;\n" +
        //                                   $"        (({bindType.FullName}) (view.Value)).{funcName}(p);");
        // }


        private static void CreateComponents()
        {
            var stra = sTextDictionary[CreateAuto.ComponentsMain];
            var strb = sTextDictionary[CreateAuto.ComponentsDispose];
            var astrings = stra.Split("@");
            var bstrings = strb.Split("@");
            string a = "";
            string b1 = "";
            string b2 = "";
            // bstrings[1] = bstrings[1].Substring(0, bstrings[1].Length - 1);
            foreach (var kv in sEcsComList)
            {
                a += string.Format(astrings[1], kv.FullName);
                b1 += string.Format(bstrings[1], kv.Name, kv.FullName);
                b2 += string.Format(bstrings[2], kv.Name, kv.FullName);
            }

            stra = string.Format(astrings[0], sEcsComList.Count, a);
            strb = string.Format(bstrings[0], b1, b2);
            CreateDirectory(EditorString.GetPath("CompOutPutPath"));
            File.WriteAllText($"{EditorString.GetPath("CompOutPutPath")}Components.cs", stra);
            File.WriteAllText($"{EditorString.GetPath("CompOutPutPath")}Components.DisposeComps.cs", strb);
        }

        private static void CreateDirectory(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // private static string FileTypeNameChange(string str)
        // {
        //     str = str.Replace("[]", "!!");
        //     str = str.Replace("`3[", "<").Replace("`2[", "<").Replace("`1[", "<").Replace("]", ">");
        //     str = str.Replace("!!", "[]");
        //     return str;
        // }
    }
}