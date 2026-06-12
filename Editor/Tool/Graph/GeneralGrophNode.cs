using System;
using System.Collections.Generic;
using GameFrame.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGrophNode : Node
    {
        private static readonly Color AccentColor = new Color(0.00f, 0.74f, 0.92f, 1f);
        private static readonly Color DarkCard = new Color(0.12f, 0.135f, 0.16f, 1f);
        private static readonly Color LightCard = new Color(0.94f, 0.95f, 0.97f, 1f);
        private static readonly Color DarkTitle = new Color(0.145f, 0.16f, 0.185f, 1f);
        private static readonly Color LightTitle = new Color(0.86f, 0.88f, 0.91f, 1f);
        private static readonly Color DarkText = new Color(0.91f, 0.93f, 0.96f, 1f);
        private static readonly Color LightText = new Color(0.12f, 0.13f, 0.15f, 1f);
        private static readonly Color DarkMutedText = new Color(0.62f, 0.67f, 0.72f, 1f);
        private static readonly Color LightMutedText = new Color(0.34f, 0.36f, 0.40f, 1f);

        public string GUID;
        public bool Entry = false;
        public Port InPort;
        public Port OutPort;
        private EntityGraphView mParent;
        private EntityNode mData;
        private List<string> options = new List<string> { };
        private int selectedIndex = 0;
        private List<Action<Node>> buttonList = new List<Action<Node>>();

        private static Color CardColor => EditorGUIUtility.isProSkin ? DarkCard : LightCard;
        private static Color TitleColor => EditorGUIUtility.isProSkin ? DarkTitle : LightTitle;
        private static Color PrimaryText => EditorGUIUtility.isProSkin ? DarkText : LightText;
        private static Color SecondaryText => EditorGUIUtility.isProSkin ? DarkMutedText : LightMutedText;

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
            ApplyNodeStyle();
            DrawEntitySummary();
            this.RegisterCallback<MouseDownEvent>(OnNodeSelected);
            var triggerButton = new Button(ShowDropdownMenu) 
            { 
                    text = "操作"
            };
            triggerButton.style.width = 46f;
            triggerButton.style.height = 22f;
            triggerButton.style.marginRight = 6f;
            triggerButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleButtonContainer.Add(triggerButton);
        }

        public void Show()
        {
        }

        public void Hide()
        {
        }

        private void ApplyNodeStyle()
        {
            expanded = true;
            style.minWidth = 220f;
            style.borderTopWidth = 1f;
            style.borderRightWidth = 1f;
            style.borderBottomWidth = 1f;
            style.borderLeftWidth = 4f;
            style.borderTopColor = new Color(1f, 1f, 1f, 0.08f);
            style.borderRightColor = new Color(1f, 1f, 1f, 0.08f);
            style.borderBottomColor = new Color(0f, 0f, 0f, 0.38f);
            style.borderLeftColor = AccentColor;
            style.backgroundColor = CardColor;

            titleContainer.style.height = 32f;
            titleContainer.style.backgroundColor = TitleColor;
            titleContainer.style.paddingLeft = 8f;
            titleContainer.style.paddingRight = 4f;

            mainContainer.style.backgroundColor = CardColor;
            mainContainer.style.paddingBottom = 6f;
            extensionContainer.style.backgroundColor = CardColor;
            inputContainer.style.paddingLeft = 8f;
            inputContainer.style.paddingRight = 8f;
            outputContainer.style.paddingLeft = 8f;
            outputContainer.style.paddingRight = 8f;
        }

        private void DrawEntitySummary()
        {
            extensionContainer.Clear();

            var summary = new VisualElement();
            summary.style.flexDirection = FlexDirection.Column;
            summary.style.paddingLeft = 10f;
            summary.style.paddingRight = 10f;
            summary.style.paddingTop = 6f;
            summary.style.paddingBottom = 2f;

            summary.Add(CreateSummaryLabel(GetEntityMetaText(), false));
            summary.Add(CreateSummaryLabel($"子节点 {mData?.NextNodes?.Count ?? 0}", true));

            extensionContainer.Add(summary);
        }

        private Label CreateSummaryLabel(string text, bool muted)
        {
            var label = new Label(text);
            label.style.color = muted ? SecondaryText : PrimaryText;
            label.style.fontSize = muted ? 11 : 12;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            return label;
        }

        private string GetEntityMetaText()
        {
            if (mData?.Entity == null)
                return "Unknown Entity";

            string typeName = mData.Entity.GetType().Name;
            if (mData.Entity is EffEntity effEntity)
                return $"{typeName}   ID:{effEntity.ID}   State:{effEntity.State}";

            return typeName;
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
            if (options.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("暂无操作"));
                menu.ShowAsContext();
                return;
            }

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
            label.style.color = SecondaryText;
            label.style.fontSize = 11;

            this.mainContainer.Add(label);
        }

        public void AddButton(string name, Action<Node> action)
        {
            if (action == null)
                return;

            buttonList.Add(action);
            options.Add(name);
        }

        public Port AddProt(string name, Type type, Direction portDir, Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Horizontal, portDir, capacity, type);
            port.portName = name;
            port.portColor = AccentColor;
            DisableManualConnection(port);
            if (portDir == Direction.Input)
            {
                InPort = port;
                inputContainer.Add(port);
            }
            else if (portDir == Direction.Output)
            {
                OutPort = port;
                outputContainer.Add(port);
            }

            return port;
        }

        private static void DisableManualConnection(Port port)
        {
            port.allowMultiDrag = false;
            port.capabilities &= ~Capabilities.Selectable;
            port.pickingMode = PickingMode.Ignore;
            if (port.edgeConnector != null)
                port.edgeConnector.target = null;
        }

        public void SetColor(Color color)
        {
            Color titleColor = new Color(color.r, color.g, color.b, EditorGUIUtility.isProSkin ? 0.34f : 0.24f);
            style.borderLeftColor = color;
            titleContainer.style.backgroundColor = titleColor;
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
