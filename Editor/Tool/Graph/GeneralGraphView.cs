using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GameFrame.Runtime.Editor
{
    public class GeneralGraphView : GraphView
    {
        private List<Edge> edgeList;

        private List<Node> nodeList;

        private float lastTime;

        public void Init()
        {
            edgeList = new List<Edge>();
            nodeList = new();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 允许拖拽Content
            this.AddManipulator(new ContentDragger());
            // 允许Selection里的内容
            this.AddManipulator(new SelectionDragger());
            // GraphView允许进行框选
            this.AddManipulator(new RectangleSelector());
            this.StretchToParentSize();
        }

        public void Show()
        {
        }

        public void Hide()
        {
        }

        public void Update()
        {
        }

        public void DeleteAllNode()
        {
            foreach (var item in nodeList)
            {
                item.Clear();
                RemoveElement(item);
            }

            foreach (var item in edgeList)
            {
                RemoveElement(item);
            }

            nodeList.Clear();
            edgeList.Clear();
        }

        public T AddNode<T>() where T : Node, new()
        {
            T node = new T();
            AddElement(node);
            nodeList.Add(node);
            return node;
        }

        public void RemoveNode(Node node)
        {
            RemoveElement(node);
            nodeList.Remove(node);
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
            edgeList.Add(tempEdge);
            AddElement(tempEdge);
            tempEdge.pickingMode = picking;
        }
    }
}