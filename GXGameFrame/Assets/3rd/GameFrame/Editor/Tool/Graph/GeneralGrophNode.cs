using System;
using System.Collections.Generic;
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
        private GeneralGraphView m_Parent;
        private EnitiyNode m_Data;

        public void Init(GeneralGraphView parent, EnitiyNode data, string text, Rect rect)
        {
            m_Parent = parent;
            m_Data = data;
            UseDefaultStyling();
            GUID = Guid.NewGuid().ToString();
            title = text;
            layer = 1;
            SetPosition(rect);
            this.RegisterCallback<MouseDownEvent>(OnNodeSelected);
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

        public void AddButton(string name, Action<Node> action)
        {
            Button btn = new Button(() => { action(this); });
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

        public void SetColor(Color color)
        {
            titleContainer.style.backgroundColor = color;
        }


        private void OnNodeSelected(MouseDownEvent e)
        {
            if (e.button == 0)
            {
                List<string> data = new List<string>();
                if (m_Data.entity is ECSEntity ecs)
                {
                    List<int> comIndexs = ecs.ECSComponentArray.Indexs;
                    foreach (var index in comIndexs)
                    {
                        data.Add(GXComponents.ComponentTypes[index].Name);
                    }
                    m_Parent.CreateList(data,this.GetPosition());
                }
            }
        }
    }
}