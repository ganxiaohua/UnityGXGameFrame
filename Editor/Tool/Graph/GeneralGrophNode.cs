using System;
using System.Collections.Generic;
using UnityEditor;
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
        private EntityGraphView mParent;
        private EntityNode mData;
        private List<string> options = new List<string> { };
        private int selectedIndex = 0;
        private List<Action<Node>> buttonList = new List<Action<Node>>();

        public void Init(EntityGraphView parent, EntityNode data, string text, Rect rect)
        {
            mParent = parent;
            mData = data;
            UseDefaultStyling();
            GUID = Guid.NewGuid().ToString();
            title = text;
            layer = 1;
            buttonList.Clear();
            SetPosition(rect);
            this.RegisterCallback<MouseDownEvent>(OnNodeSelected);
            var triggerButton = new Button(ShowDropdownMenu) 
            { 
                    text = "选项",
                    style = { width = 30, height = 20 } // 自定义尺寸
            };
            titleButtonContainer.Add(triggerButton);
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
        
        private void ShowDropdownMenu()
        {
            GenericMenu menu = new GenericMenu();
            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                menu.AddItem(new GUIContent(options[i]), 
                        false, 
                        () => OnSelectOption(index));
            }
            menu.ShowAsContext();
        }
        
        private void OnSelectOption(int index)
        {
            selectedIndex = index;
            buttonList[index].Invoke(this);
        }

        public void AddChildCount(int count)
        {
            Label label = new Label($"子节点数量:{count}");

            this.mainContainer.Add(label);
        }

        public void AddButton(string name, Action<Node> action)
        {
            buttonList.Add(action);
            options.Add(name);
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
            // if (e.button == 0)
            // {
            //     mParent.ShowComponent(mData);
            // }
        }
    }
}