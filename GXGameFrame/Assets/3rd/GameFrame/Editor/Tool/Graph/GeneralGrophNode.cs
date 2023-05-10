using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGrophNode : Node
    {
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
            if (OutPort != null)
            {
                OutPort.Clear();
            }

            if (InPort != null)
            {
                InPort.Clear();
            }
        }

        public void AddButton(string name,Action<Node> action)
        {
            Button btn = new Button(() =>
            {
                action(this);
            });
            btn.text = name;
            titleContainer.Add(btn);
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