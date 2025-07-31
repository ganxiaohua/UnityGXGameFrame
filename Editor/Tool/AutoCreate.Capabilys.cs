using System;
using System.Collections.Generic;
using System.IO;
using GameFrame.Runtime;
using UnityEditor;

namespace GameFrame.Editor
{
    public partial class AutoCreate
    {
        private static List<CapabilityBase> capabilitylist = new();

        public static void AutoCapabilityScript()
        {
            LoadText();
            CreateCapabiltys();
            AssetDatabase.Refresh();
        }

        private static void CreateCapabiltys()
        {
            var assemblys = AppDomain.CurrentDomain.GetAssemblies();
            var number = 0;
            tempStr.Clear();
            capabilitylist.Clear();
            foreach (var assembly in assemblys)
            {
                foreach (var name in EditorString.AssemblyNames)
                {
                    if (assembly.GetName().Name != name) continue;
                    Type[] types = assembly.GetTypes();
                    foreach (var tp in types)
                    {
                        if (typeof(CapabilityBase).IsAssignableFrom(tp) && tp.IsClass && tp.Name != nameof(CapabilityBase))
                        {
                            var instance = (CapabilityBase) assembly.CreateInstance(tp.FullName);
                            capabilitylist.Add(instance);
                            number++;
                        }
                    }
                }
            }

            capabilitylist.Sort((x, y) => { return x.TickGroupOrder - y.TickGroupOrder; });
            int index = 0;
            foreach (var item in capabilitylist)
            {
                string updateMode = item.UpdateMode == CapabilitysUpdateMode.Update ? "IUpdateSystem" : "IFixedUpdateSystem";
                tempStr.Append(index == 0
                        ? $" var orderTid =  CapabilityID<{item.GetType().FullName},{updateMode}>.TID;\n"
                        : $"        orderTid =  CapabilityID<{item.GetType().FullName},{updateMode}>.TID;\n");
                index++;
            }

            var str = string.Format(s_TextDictionary[CreateAuto.Capability], tempStr, number);
            File.WriteAllText($"{EditorString.ECSOutPutPath}AllCapabilitys.cs", str);
            tempStr.Clear();
        }
    }
}