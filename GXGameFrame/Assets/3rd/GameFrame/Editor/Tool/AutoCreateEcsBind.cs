﻿using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
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
        EventClass,
        Event,
    }

    public class AutoCreate
    {
        private static Dictionary<CreateAuto, string> s_TextDictionary;
        private static Dictionary<Type, string> s_EventDictionary;
        private static StringBuilder s_EventSb = new StringBuilder(1024);
        
        public static void AutoCreateScript()
        {
            LoadText();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assembly)
            {
                if (item.GetName().Name == "Assembly-CSharp")
                {
                    FindAllECSCom(item);
                }
            }
        }

        public static void LoadText()
        {
            string add =EditorString.GameFramePath + "Editor/Text/Add.txt";
            string cls = EditorString.GameFramePath + "Editor/Text/Class.txt";
            string addparameter = EditorString.GameFramePath + "Editor/Text/AddParameter.txt";
            string get = EditorString.GameFramePath + "Editor/Text/Get.txt";
            string set = EditorString.GameFramePath + "Editor/Text/Set.txt";
            string eventClass = EditorString.GameFramePath + "Editor/Text/EventClass.txt";
            string Event = EditorString.GameFramePath + "Editor/Text/Event.txt";
            string stradd = File.ReadAllText(add);
            string strcls = File.ReadAllText(cls);
            string straddparameter = File.ReadAllText(addparameter);
            string strget = File.ReadAllText(get);
            string strset = File.ReadAllText(set);
            string strEventClass = File.ReadAllText(eventClass);
            string strEvent = File.ReadAllText(Event);
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
            s_TextDictionary.Add(CreateAuto.EventClass, strEventClass);
            s_TextDictionary.Add(CreateAuto.Event, strEvent);
        }

        public static void FindAllECSCom(Assembly assembly)
        {
            s_EventSb.Clear();
            Type[] types = assembly.GetTypes();
            foreach (var tp in types)
            {
                if (typeof(IECSComponent).IsAssignableFrom(tp) && tp.IsClass)
                {
                    var vb = tp.GetCustomAttribute<ViewBindAttribute>();
                    if (vb != null)
                    {
                        AddEvent(tp);
                    }
                    CreateCSHAP(tp);
                }
            }

            CreateEvent();
        }

        //创建c#脚本
        public static void CreateCSHAP(Type type)
        {
            FieldInfo[] variable = type.GetFields();
            string typeName = type.Name;
            StringBuilder sb = new StringBuilder(1024);
            string abcls = s_TextDictionary[CreateAuto.Class];
            string abAdd = string.Format(s_TextDictionary[CreateAuto.Add], typeName);
            string abGet = string.Format(s_TextDictionary[CreateAuto.Get], typeName);
            string abSet = "";
            string addParameter = "";
            if (variable.Length > 0)
            {
                string fieldName = variable[0].Name;
                string fieldTypeName = variable[0].FieldType.Name;
                if (fieldTypeName == "String")
                {
                    fieldTypeName = "string";
                }

                addParameter = string.Format(s_TextDictionary[CreateAuto.AddParameter], typeName, fieldTypeName, fieldName);
                string evetString = "";
                if (s_EventDictionary.TryGetValue(type, out  evetString))
                {
                    
                }
                abSet = string.Format(s_TextDictionary[CreateAuto.Set], typeName, fieldTypeName, fieldName,evetString);
            }

            sb.Append(abAdd);
            sb.Append(addParameter);
            sb.Append(abGet);
            sb.Append(abSet);
            string lastText = string.Format(abcls, typeName, sb.ToString());
            CreateDirectory(EditorString.ECSOutPutPath);
            File.WriteAllText($"{EditorString.ECSOutPutPath}{typeName}Auto.cs", lastText);
            AssetDatabase.Refresh();
        }

        public static void AddEvent(Type type)
        {
            string typeName = type.Name;
            string str = string.Format(s_TextDictionary[CreateAuto.Event], typeName);
            s_EventSb.Append(str);
            s_EventDictionary.Add(type,$"ViewBindEventClass.{typeName}EntityComponentNumericalChange?.Invoke(p,ecsEntity);");
        }

        public static void CreateEvent()
        {
            if (s_EventSb.Length == 0)
                return;
            string last = string.Format(s_TextDictionary[CreateAuto.EventClass], s_EventSb);
            File.WriteAllText($"{EditorString.ECSOutPutPath}ViewBindEventAuto.cs", last);
        }

        public static void CreateDirectory(string path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}