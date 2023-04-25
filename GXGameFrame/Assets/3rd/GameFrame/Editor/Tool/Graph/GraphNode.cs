using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphView : GraphView
    {
        private EnitiyInfos m_EnitiyInfos;

        private DialogueNode RootGraphNode;

        // 在构造函数里，对GraphView进行一些初始的设置
        public DialogueGraphView()
        {
            m_EnitiyInfos = new EnitiyInfos();
            // 允许对Graph进行Zoom in/out
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 允许拖拽Content
            this.AddManipulator(new ContentDragger());
            // 允许Selection里的内容
            this.AddManipulator(new SelectionDragger());
            // GraphView允许进行框选
            this.AddManipulator(new RectangleSelector());
            RootGraphNode = AddNode("根节点", new Vector2(100, 150));
            AddPort(RootGraphNode, Direction.Output, "");
            RootGraphNode.RefreshExpandedState();
            RootGraphNode.RefreshPorts();
            RefreshAllEnitity();
        }

        private void RefreshAllEnitity()
        {
            m_EnitiyInfos.GetAllEnitiy();
            m_EnitiyInfos.RootNode.GraphNode = RootGraphNode;
            CreateEnitiyNode(m_EnitiyInfos.RootNode);
        }

        private void CreateEnitiyNode(EnitiyNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                var graphNode = AddNode(enititnode.entity.GetType().Name, new Vector2(enititnode.Floor * 200, 150 + enititnode.Grid * 100));
                var inPort = AddPort(graphNode, Direction.Input, "");
                var outPort = AddPort(graphNode, Direction.Output, "");
                enititnode.GraphNode = graphNode;
                graphNode.RefreshExpandedState();
                graphNode.RefreshPorts();
                AddEdgeByPorts(node.GraphNode.OutPort, inPort);
                CreateEnitiyNode(enititnode);
            }
        }

        private void AddEdgeByPorts(Port _outputPort, Port _inputPort)
        {
            Edge tempEdge = new Edge()
            {
                output = _outputPort,
                input = _inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            Add(tempEdge);
        }

        private DialogueNode AddNode(string name, Vector2 pos)
        {
            var node = GenEntryPointNode(name, pos);
            AddElement(node);
            return node;
        }

        private Port AddPort(DialogueNode node, Direction direction, string name)
        {
            var port = GenPortForNode(node, direction);
            port.portName = name;
            node.outputContainer.Add(port);
            if (direction == Direction.Input)
            {
                node.InPort = port;
            }
            else if (direction == Direction.Output)
            {
                node.OutPort = port;
            }

            return port;
        }

        private DialogueNode GenEntryPointNode(string name, Vector2 xy)
        {
            DialogueNode node = new DialogueNode
            {
                title = name,
                GUID = Guid.NewGuid().ToString(),
                Entry = true
            };
            node.SetPosition(new Rect(x: xy.x, y: xy.y, width: 100, height: 150));
            return node;
        }

        private Port GenPortForNode(Node n, Direction portDir, Port.Capacity capacity = Port.Capacity.Multi)
        {
            return n.InstantiatePort(Orientation.Horizontal, portDir, capacity, typeof(float));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            // 继承的GraphView里有个Property：ports, 代表graph里所有的port
            ports.ForEach((port) =>
            {
                // 对每一个在graph里的port，进行判断，这里有两个规则：
                // 1. port不可以与自身相连
                // 2. 同一个节点的port之间不可以相连
                if (port != startPort && port.node != startPort.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            // 在我理解，这个函数就是把所有除了startNode里的port都收集起来，放到了List里
            // 所以这个函数能让StartNode的Output port与任何其他的Node的Input port相连（output port应该默认不能与output port相连吧）
            return compatiblePorts;
        }
    }


    // 创建dialogue graph的底层节点类
    public class DialogueNode : Node
    {
        public string GUID;
        public string Text;
        public bool Entry = false;
        public Port InPort;
        public Port OutPort;
    }
}