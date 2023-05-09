using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGraphView : GraphView, IEditorEnitiy
    {
        public int ID { get; set; }
        public IEditorEnitiy Parent { get; set; }

        public List<Edge> m_EdgeList;

        public void Init()
        {
            m_EdgeList = new List<Edge>();
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
        }

        /// <summary>
        /// 连线
        /// </summary>
        /// <param name="outputPort"></param>
        /// <param name="inputPort"></param>
        public void AddEdgeByPorts(Port outputPort, Port inputPort)
        {
            Edge tempEdge = new Edge()
            {
                output = outputPort,
                input = inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            m_EdgeList.Add(tempEdge);
            Add(tempEdge);
        }
        
        /// <summary>
        /// 将点位连线的回调
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter adapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach((port) =>
            {
                if (port != startPort && port.node != startPort.node)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }
    }
}