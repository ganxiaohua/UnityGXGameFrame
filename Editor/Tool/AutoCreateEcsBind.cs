using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace GameFrame.Editor
{
    enum CreateAuto
    {
        Class,
        Add,
        AddParameter,
        Get,
        Set,
        EventClass,
        ComponentsMain,
        ComponentsSub,
        ComponentsTypeMain,
        ComponentsTypeSub,
    }

    public class AutoCreate
    {
        private static Dictionary<CreateAuto, string> s_TextDictionary;
        private static Dictionary<Type, string> s_EventDictionary;
        private static StringBuilder s_EventSb = new StringBuilder(1024);

        public static void AutoCreateScript()
        {
            OpFile.DeleteFilesInDirectory(EditorString.ECSOutPutPath);
            LoadText();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            var number = 0;
            foreach (var item in assembly)
            {
                foreach (var name in EditorString.AssemblyNames)
                {
                    if (item.GetName().Name != name) continue;
                    number+=  FindAllECSCom(item);
                    break;
                }
            }
            CreateComponents(number);
            CreateEvent();
            AssetDatabase.Refresh();
        }

        private static void LoadText()
        {
            string add = EditorString.GameFramePath + "Editor/Text/ECS/Add.txt";
            string cls = EditorString.GameFramePath + "Editor/Text/ECS/Class.txt";
            string addparameter = EditorString.GameFramePath + "Editor/Text/ECS/AddParameter.txt";
            string get = EditorString.GameFramePath + "Editor/Text/ECS/Get.txt";
            string set = EditorString.GameFramePath + "Editor/Text/ECS/Set.txt";
            string ECSALLComponents = EditorString.GameFramePath + "Editor/Text/ECS/Components.txt";

            string stradd = File.ReadAllText(add);
            string strcls = File.ReadAllText(cls);
            string straddparameter = File.ReadAllText(addparameter);
            string strget = File.ReadAllText(get);
            string strset = File.ReadAllText(set);
            string strAllComponents = File.ReadAllText(ECSALLComponents);
            if (s_EventDictionary == null)
            {
                s_EventDictionary = new Dictionary<Type, string>();
            }

            s_EventDictionary.Clear();
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
            s_TextDictionary.Add(CreateAuto.ComponentsMain,strAllComponents);
        }

        private static int FindAllECSCom(Assembly assembly)
        {
            s_EventSb.Clear();
            Type[] types = assembly.GetTypes();
            int number = 0;
            foreach (var tp in types)
            {
                if (typeof(ECSComponent).IsAssignableFrom(tp) && tp.IsClass)
                {
                    var vb = tp.GetCustomAttribute<ViewBindAttribute>();
                    if (vb != null)
                    {
                        AddEvent(tp, vb.BindType);
                    }

                    number++;
                    CreateCshap(tp);
                }
            }

            return number;
        }

        //创建c#脚本
        private static void CreateCshap(Type type)
        {
            if (type.Name == nameof(ECSComponent))
            {
                return;
            }

            string ECSComponentName = "ECSEntity";
            var vb = type.GetCustomAttribute<AssignBindAttribute>();
            if (vb != null)
            {
                ECSComponentName = vb.BindType.FullName;
            }

            FieldInfo[] variable = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] propertyInfos = type.GetProperties();
            string typeName = type.Name;
            string typeFullName = type.FullName;
            StringBuilder sb = new StringBuilder(1024);
            string abcls = s_TextDictionary[CreateAuto.Class];
            string abAdd = string.Format(s_TextDictionary[CreateAuto.Add], typeName, ECSComponentName,typeFullName);
            string abGet = string.Format(s_TextDictionary[CreateAuto.Get], typeName, typeFullName, ECSComponentName);
            string abSet = "";
            string addParameter = "";
            string fieldName = "";
            string fieldTypeName = "";
            if (propertyInfos.Length > 0)
            {
                 fieldName = propertyInfos[0].Name;
                 fieldTypeName = propertyInfos[0].PropertyType.FullName;
            }else if (variable.Length > 0)
            {
                 fieldName = variable[0].Name;
                 fieldTypeName = variable[0].FieldType.FullName;
            }

            if (!string.IsNullOrEmpty(fieldName))
            {
                if (fieldTypeName == "String")
                {
                    fieldTypeName = "string";
                }

                if (fieldTypeName.Contains("List"))
                {
                    fieldTypeName = $"System.Collections.Generic.List<{variable[0].FieldType.GenericTypeArguments[0].FullName}>";
                }

                addParameter = string.Format(s_TextDictionary[CreateAuto.AddParameter], typeName, fieldTypeName, typeFullName, fieldName, ECSComponentName);
                string evetString = "";
                if (s_EventDictionary.TryGetValue(type, out evetString))
                {
                }

                abSet = string.Format(s_TextDictionary[CreateAuto.Set], typeName, fieldTypeName, typeFullName, fieldName, evetString, ECSComponentName);
            }

            sb.Append(abAdd);
            sb.Append(addParameter);
            sb.Append(abGet);
            sb.Append(abSet);
            string lastText = string.Format(abcls, typeName, sb.ToString());
            CreateDirectory(EditorString.ECSOutPutPath);
            File.WriteAllText($"{EditorString.ECSOutPutPath}{typeName}Auto.cs", lastText);
        }

        private static void AddEvent(Type type, Type bindType)
        {
            MethodInfo[] methods = bindType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            string funcName = methods[0].Name;
            s_EventDictionary.Add(type, $"View view = ecsEntity.GetView();\n" +
                                        $"        if (view == null) return null;\n" +
                                        $"        (({bindType.FullName}) (view.Value)).{funcName}(p);");
        }

        private static void CreateEvent()
        {
            if (s_EventSb.Length == 0)
                return;
            string last = string.Format(s_TextDictionary[CreateAuto.EventClass], s_EventSb);
            File.WriteAllText($"{EditorString.ECSOutPutPath}ViewBindEventAuto.cs", last);
        }
        
        private static void CreateComponents(int number)
        {
            var str = string.Format(s_TextDictionary[CreateAuto.ComponentsMain], number);
            File.WriteAllText($"{EditorString.ECSOutPutPath}Components.cs", str);
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