using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public class MakeAutoEventBind
    {
        private static string MainText;
        private static string AddText;
        private static List<string> AllAddText = new();

        private static string MainTextSend;
        private static string AddTextSend;
        private static List<string> AllAddTextSend = new();
        private static HashSet<Type> SendEventHas = new();

        public static void AutoCreateScript()
        {
            LoadText();
            LoadText2();
            AllAddText.Clear();
            AllAddTextSend.Clear();
            SendEventHas.Clear();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assembly)
            {
                foreach (var name in  EditorString.AssemblyNames)
                {
                    if (item.GetName().Name != name) continue;
                    FindEntityClass(item); 
                    break;
                }
            }

            Create();
        }

        private static void LoadText()
        {
            string systemPath = EditorString.GameFramePath + "Editor/Text/EventBind.txt";
            string system = File.ReadAllText(systemPath);
            string[] text = system.Split('@', StringSplitOptions.None);
            MainText = text[0];
            AddText = text[1];
        }

        private static void LoadText2()
        {
            string systemPath = EditorString.GameFramePath + "Editor/Text/EventSend.txt";
            string system = File.ReadAllText(systemPath);
            string[] text = system.Split('@', StringSplitOptions.None);
            MainTextSend = text[0];
            AddTextSend = text[1];
        }

        private static void FindEntityClass(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var tp in types)
            {
                if (typeof(IEvent).IsAssignableFrom(tp) && tp.IsClass)
                {
                    var ImplementedInterfaces = tp.GetTypeInfo().ImplementedInterfaces;
                    foreach (var item in ImplementedInterfaces)
                    {
                        if (typeof(IEvent).IsAssignableFrom(item) && item.IsInterface && item != typeof(IEvent))
                        {
                            string str = string.Format(AddText, tp.FullName, item.FullName);
                            AllAddText.Add(str);
                            //获得事件的方法 和参数
                            if (SendEventHas.Contains(item))
                            {
                                return;
                            }

                            string parameter = "";
                            {
                            } //{1}
                            string typex = item.Name.Substring(1, item.Name.Length - 1); //{0}
                            MethodInfo[] ins = item.GetMethods();
                            ParameterInfo[] parmeters = ins[0].GetParameters();
                            string method = ins[0].Name; //{2}
                            string paremwairte = ""; //{3}
                            foreach (ParameterInfo parmeter in parmeters)
                            {
                                parameter += parmeter.ParameterType.FullName + " " + parmeter.Name + ",";
                                paremwairte += parmeter.Name + ",";
                            }

                            if (!string.IsNullOrEmpty(parameter))
                                parameter = parameter.Substring(0, parameter.Length - 1);
                            if (!string.IsNullOrEmpty(paremwairte))
                                paremwairte = paremwairte.Substring(0, paremwairte.Length - 1);
                            str = string.Format(AddTextSend, typex, parameter, method, paremwairte);
                            AllAddTextSend.Add(str);
                            SendEventHas.Add(item);
                        }
                    }
                }
            }
        }


        private static void Create()
        {
            string bigtext = "";
            foreach (var item in AllAddText)
            {
                bigtext += item;
            }

            string sendeventtext = "";
            foreach (var item in AllAddTextSend)
            {
                sendeventtext += item;
            }

            File.WriteAllText(EditorString.EventBindOutPutPath, string.Format(MainText, bigtext));
            File.WriteAllText(EditorString.EventSendOutPutPath, string.Format(MainTextSend, sendeventtext));
            AssetDatabase.Refresh();
            Debug.Log("生成系统绑定结束");
        }
    }
}