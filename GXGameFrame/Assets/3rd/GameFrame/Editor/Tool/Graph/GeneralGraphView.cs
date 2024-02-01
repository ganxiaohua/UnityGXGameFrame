using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGraphView : GraphView
    {
        
        private List<Edge> m_EdgeList;

        private List<Node> m_NodeList;

        private float m_LastTime;

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
    }
}