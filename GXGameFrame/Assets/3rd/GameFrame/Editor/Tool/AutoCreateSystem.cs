using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class AutoCreateSystem
    {
        private static string sGameFramePath = "Assets/3rd/GameFrame/";
        private static string OutPutPath = "Assets/3rd/GameFrame/Runtime/Core/Auto/SystemBindAuto.cs";
        private static string MainText;
        private static string AddText;
        private static List<string> AllAddText = new();

        [MenuItem("Tool/生成System绑定脚本",false,1)]
        public static void AutoCreateScript()
        {
            LoadText();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assembly)
            {
                if (item.GetName().Name == "Assembly-CSharp")
                {
                    FindEnitiyClass(item);
                }
            }

            Create();
        }

        public static void LoadText()
        {
            string systemPath = sGameFramePath + "Editor/Text/SystemBind.txt";
            string system = File.ReadAllText(systemPath);
            string[] text = system.Split('#', StringSplitOptions.None);
            MainText = text[0];
            AddText = text[1];
        }

        public static void FindEnitiyClass(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var tp in types)
            {
                PushDic(assembly, tp);
            }
        }

        private static void PushDic(Assembly assembly, Type type)
        {
            var vb = type.GetCustomAttribute<SystemBindAttribute>();
            if (vb != null)
            {
                Type baseType = type.BaseType;
                Type[] types = baseType.GenericTypeArguments;
                string enitiySystemType = type.FullName.Replace("+", ".");
                string enitiyType = types[0].Name;
                string systemType = "";
                var x = baseType.GetTypeInfo().ImplementedInterfaces;
                foreach (Type itemType in x)
                {
                    if (itemType.Name.Contains("IStartSystem"))
                    {
                        types = itemType.GenericTypeArguments;
                        if (types.Length >= 1)
                        {
                            systemType = $"IStartSystem{Typesjoint(types)}";
                        }
                        else
                        {
                            systemType = $"IStartSystem";
                        }
                    }
                    else if (itemType.Name.Contains("IUpdateSystem"))
                    {
                        systemType = $"IUpdateSystem";
                    }

                    else if (itemType.Name.Contains("IClearSystem"))
                    {
                        systemType = $"IClearSystem";
                    }

                    else if (itemType.Name.Contains("IClearSystem"))
                    {
                        systemType = $"IClearSystem";
                    }
                    else if (itemType.Name.Contains("IShowSystem"))
                    {
                        types = itemType.GenericTypeArguments;
                        if (types.Length >= 1)
                        {
                            systemType = $"IShowSystem{Typesjoint(types)}";
                        }
                        else
                        {
                            systemType = $"IShowSystem";
                        }
                    }
                    else if (itemType.Name.Contains("IHideSystem"))
                    {
                        systemType = $"IHideSystem";
                    }
                }

                AllAddText.Add(string.Format(AddText, $"typeof({enitiyType})", $"typeof({systemType})", $"typeof({enitiySystemType})"));
            }
        }

        private static string GetGenericityType(string typename, Type[] type)
        {
            if (!typename.Contains('`'))
            {
                return typename;
            }

            string name = typename.Split('`')[0];
            return name + Typesjoint(type);
        }

        private static string Typesjoint(Type[] types)
        {
            string typesName = "<";
            foreach (var item in types)
            {
                if (item.GenericTypeArguments.Length == 0)
                {
                    typesName += item.Name + ",";
                }
                else
                {
                    typesName += GetGenericityType(item.Name, item.GenericTypeArguments) + ",";
                }
            }

            typesName = typesName.Substring(0, typesName.Length - 1) + ">";
            return typesName;
        }

        private static void Create()
        {
            string bigtext = "";
            foreach (var item in AllAddText)
            {
                bigtext += item;
            }

            File.WriteAllText(OutPutPath, string.Format(MainText, bigtext));
            AssetDatabase.Refresh();
            Debug.Log("生成系统绑定结束");
        }
    }
}