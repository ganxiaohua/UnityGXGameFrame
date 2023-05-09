using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.Editor
{
    public class GeneralGrophNode : Node, IEditorEnitiy
    {
        public int ID { get; set; }
        public IEditorEnitiy Parent { get; set; }
        public string GUID;
        public bool Entry = false;
        public Port InPort;
        public Port OutPort;

        public void Init(string text, Rect rect)
        {
            GUID = Guid.NewGuid().ToString();
            title = text;
            SetPosition(rect);
        }

        public void Show()
        {
        }

        public void Hide()
        {
        }

        public new void Clear()
        {
            base.Clear();
        }

        public Port AddProt(string name, Type type, Direction portDir, Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Horizontal, portDir, capacity, type);
            port.portName = name;
            outputContainer.Add(port);
            if (portDir == Direction.Input)
            {
                InPort = port;
            }
            else if (portDir == Direction.Output)
            {
                OutPort = port;
            }
            return port;
        }
    }
}