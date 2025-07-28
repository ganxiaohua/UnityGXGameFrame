using System.Collections.Generic;
using System.Xml;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime.Editor.UI
{
    public partial class FGUIChecker
    {
        [HorizontalGroup("line1", 0.2f)]
        [Button("检查重复图片", buttonSize: 50)]
        void CheckRepeatImage()
        {
            Debug.Log("开始检测重复图片，原因是一个图集里面不能出现同名图片，只会将其中一个打进图集。");
            var set = new HashSet<string>();
            foreach (var (k, v) in packages)
            {
                var root = v.Path.Substring(0, v.Path.LastIndexOf("/"));
                XmlDocument xml = new XmlDocument();
                xml.Load(v.Path);

                var nodes = xml.SelectSingleNode("packageDescription").SelectSingleNode("resources").ChildNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node.Name == "image")
                    {
                        var nameValue = FindXmlAttr(node, "name");
                        var key = v.PackageName + "|" + nameValue;
                        if (set.Contains(key))
                        {
                            Debug.LogWarning($"包:{v.PackageName}, 存在重复图片 {nameValue}");
                        }
                        else
                        {
                            set.Add(key);
                        }
                    }
                }
            }

            Debug.Log("结束检测重复图片");
        }
    }
}