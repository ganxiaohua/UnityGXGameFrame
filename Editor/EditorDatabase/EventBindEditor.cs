using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameFrame.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrame.Editor
{
    public class MakeAutoEventBind
    {
        private static string MainText;
        private static string AddText;
        private static List<string> AllAddText = new();
        private static string UIBindText;
        private static readonly List<string> UIAddText = new();

        private static string MainTextSend;
        private static string AddTextSend;
        private static List<string> AllAddTextSend = new();
        private static HashSet<Type> SendEventHas = new();

        public static void AutoBindingEventScript()
        {
            if (EditorApplication.isPlaying)
                return;
            LoadText();
            LoadText2();
            AllAddText.Clear();
            UIAddText.Clear();
            AllAddTextSend.Clear();
            SendEventHas.Clear();
            var assembly = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assembly)
            {
                foreach (var name in EditorString.GetPaths("AssemblyNames"))
                {
                    if (item.GetName().Name != name) continue;
                    FindEntityClass(item);
                    break;
                }
            }

            // Event senders belong to their contracts, even when the selected UI backend
            // has no receiver yet. Also include interfaces implemented from other assemblies.
            foreach (var eventType in SendEventHas.OrderBy(type => type.FullName, StringComparer.Ordinal))
                AddEventSender(eventType);

            Create();
        }

        private static void LoadText()
        {
            string systemPath = EditorString.GetPath("GameFramePath") + "Editor/Text/EventBind.txt";
            string system = File.ReadAllText(systemPath);
            string[] text = system.Split('@', StringSplitOptions.None);
            MainText = text[0];
            AddText = text[1].Trim('\r', '\n') + Environment.NewLine;
            UIBindText = File.ReadAllText(EditorString.GetPath("GameFramePath") + "Editor/Text/EventBind.UI.txt");
        }

        private static void LoadText2()
        {
            string systemPath = EditorString.GetPath("GameFramePath") + "Editor/Text/EventSend.txt";
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
                if (IsEventInterface(tp)) SendEventHas.Add(tp);
                foreach (var eventType in tp.GetInterfaces())
                {
                    if (!IsEventInterface(eventType)) continue;
                    SendEventHas.Add(eventType);
                    if (!tp.IsClass) continue;
                    var binding = string.Format(AddText, tp.FullName, eventType.FullName);
                    (IsUIPanel(tp) ? UIAddText : AllAddText).Add(binding);
                }
            }
        }

        private static bool IsEventInterface(Type type)
            => type.IsInterface && type != typeof(IEvent) && typeof(IEvent).IsAssignableFrom(type);

        private static bool IsUIPanel(Type type)
        {
            // The inactive backend's base type is absent from the compiled assembly.
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.FullName == "GameFrame.Runtime.Panel" || current.FullName == "GameFrame.Runtime.UnityUI.Panel")
                    return true;
            }
            return false;
        }

        private static void AddEventSender(Type eventType)
        {
            var methods = eventType.GetMethods();
            if (methods.Length == 0) return;
            // Keep the existing first-method selection and generated sender signatures.
            var method = methods[0];
            var parameters = method.GetParameters();
            var declaration = string.Join(",", parameters.Select(parameter => parameter.ParameterType.FullName + " " + parameter.Name));
            var arguments = string.Join(",", parameters.Select(parameter => parameter.Name));
            AllAddTextSend.Add(string.Format(AddTextSend, eventType.Name.Substring(1), declaration, method.Name, arguments, eventType.FullName));
        }


        private static void Create()
        {
            AllAddText.Sort(StringComparer.Ordinal);
            UIAddText.Sort(StringComparer.Ordinal);
            var bigtext = string.Concat(AllAddText);
            var sendeventtext = string.Concat(AllAddTextSend);

            var eventBindOutPutPath = EditorString.GetPath("EventBindOutPutPath");
            var directory = Path.GetDirectoryName(eventBindOutPutPath);
            OpFile.CreateDirectory(directory);

            var eventSendOutPutPath = EditorString.GetPath("EventSendOutPutPath");
            directory = Path.GetDirectoryName(eventSendOutPutPath);
            OpFile.CreateDirectory(directory);

            File.WriteAllText(eventBindOutPutPath, string.Format(MainText, bigtext));
            var backend = GXGameFrame.UsesUnityUI ? "UnityUI" : "FGUI";
            var condition = GXGameFrame.UsesUnityUI ? "GAMEFRAME_USE_UNITYUI" : "!GAMEFRAME_USE_UNITYUI";
            var uiOutputPath = Path.Combine(Path.GetDirectoryName(eventBindOutPutPath),
                Path.GetFileNameWithoutExtension(eventBindOutPutPath) + "." + backend + ".cs");
            // Only replace the selected backend. Its counterpart must survive regeneration
            // while its types are excluded, so switching back needs no regeneration first.
            File.WriteAllText(uiOutputPath, string.Format(UIBindText, condition, string.Concat(UIAddText)));
            File.WriteAllText(eventSendOutPutPath, string.Format(MainTextSend, sendeventtext));
            AssetDatabase.Refresh();
            Debug.Log("生成系统绑定结束");
        }
    }
}
