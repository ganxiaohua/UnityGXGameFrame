using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGraphView : GraphView, IEditorEnitiy
    {
        public int ID { get; set; }
        public IEditorEnitiy Parent { get; set; }

        public List<Edge> m_EdgeList;

        public List<Node> m_NodeList;

        private UnityEditor.Experimental.GraphView.Group group;

        public void Init()
        {
            m_EdgeList = new List<Edge>();
            m_NodeList = new();
            this.StretchToParentSize();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 允许拖拽Content
            this.AddManipulator(new ContentDragger());
            // 允许Selection里的内容
            this.AddManipulator(new SelectionDragger());
            // GraphView允许进行框选
            this.AddManipulator(new RectangleSelector());
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
            DeleteAllNode();
            group = null;
        }

        public void DeleteAllNode()
        {
            foreach (var item in m_NodeList)
            {
                item.Clear();
                RemoveElement(item);
            }

            foreach (var item in m_EdgeList)
            {
                RemoveElement(item);
            }

            m_NodeList.Clear();
            m_EdgeList.Clear();
        }

        public T AddNode<T>() where T : Node, new()
        {
            T node = new T();
            AddElement(node);
            m_NodeList.Add(node);
            return node;
        }

        private void AddMsgBox(Rect pos)
        {
            group = new UnityEditor.Experimental.GraphView.Group();
            group.layer = 2;
            AddElement(group);
        }
        
        public void CreateList(List<string> dataList,Rect pos)
        {
            if (group == null)
                AddMsgBox(pos);
            group.Clear();
            var listContainer = new VisualElement();
            listContainer.style.flexDirection = FlexDirection.Column;
            foreach (var data in dataList)
            {
                var item = new Label(data);
                listContainer.Add(item);
            }
            group.Add(listContainer);
            group.SetPosition(new Rect(pos.x+pos.width+10, pos.y, group.resolvedStyle.width, group.resolvedStyle.height));
        }

        /// <summary>
        /// 连线
        /// </summary>
        /// <param name="outputPort"></param>
        /// <param name="inputPort"></param>
        public void AddEdgeByPorts(Port outputPort, Port inputPort, PickingMode picking)
        {
            Edge tempEdge = new Edge()
            {
                output = outputPort,
                input = inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            m_EdgeList.Add(tempEdge);
            AddElement(tempEdge);
            tempEdge.pickingMode = picking;
        }

        // /// <summary>
        // /// 将点位连线的回调
        // /// </summary>
        // /// <param name="startPort"></param>
        // /// <param name="adapter"></param>
        // /// <returns></returns>
        // public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
        // {
        //     List<Port> compatiblePorts = new List<Port>();
        //
        //     ports.ForEach((port) =>
        //     {
        //         if (port != startPort && port.node != startPort.node)
        //         {
        //             compatiblePorts.Add(port);
        //         }
        //     });
        //     return compatiblePorts;
        // }
    }
}