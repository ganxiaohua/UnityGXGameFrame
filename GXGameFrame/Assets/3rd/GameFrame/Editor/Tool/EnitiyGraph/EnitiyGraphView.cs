using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GameFrame.Editor
{
    public class EnitiyGraphView : EditorEnitiy
    {
        private GeneralGraphView m_GeneralGraphView;

        private List<GeneralGrophNode> m_NodeList;

        private EnitiyInfos m_EnitiyInfos;

        private EditorWindow m_EditorWindow;

        public void Init(EditorWindow editorWindow)
        {
            base.Init();
            m_NodeList = new List<GeneralGrophNode>();
            m_GeneralGraphView = AddComponent<GeneralGraphView>();
            m_GeneralGraphView.Init();
            m_EditorWindow = editorWindow;
            editorWindow.rootVisualElement.Add(m_GeneralGraphView);
        }

        public override void Show()
        {
            base.Show();
            m_GeneralGraphView.Show();
            m_EnitiyInfos = new EnitiyInfos();
            m_EnitiyInfos.GetAllEnitiy();
            CreateNodeWithInfo();
        }

        public override void Hide()
        {
            base.Hide();
            m_GeneralGraphView.Hide();
        }

        public override void Clear()
        {
            base.Clear();
            m_EditorWindow.rootVisualElement.Remove(m_GeneralGraphView);
        }

        public void CreateNodeWithInfo()
        {
            CreateRoot(m_EnitiyInfos.RootNode);
            CreateEnitiyNode(m_EnitiyInfos.RootNode);
        }

        private void CreateRoot(EnitiyNode root)
        {
            var graphNode = AddChild<GeneralGrophNode>();
            var graphNodeName = root.entity.GetType().Name;
            graphNode.Init(graphNodeName, new Rect(root.Floor * 200, 150 + root.Grid * 100, 100, 150));
            var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            m_GeneralGraphView.AddElement(graphNode);
            m_NodeList.Add(graphNode);
        }

        private void CreateEnitiyNode(EnitiyNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                var graphNode = AddChild<GeneralGrophNode>();
                var graphNodeName = enititnode.entity.GetType().Name;
                graphNode.Init(graphNodeName, new Rect(enititnode.Floor * 200, 150 + enititnode.Grid * 100, 100, 150));
                var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
                var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
                enititnode.GraphNode = graphNode;
                graphNode.RefreshExpandedState();
                graphNode.RefreshPorts();
                m_GeneralGraphView.AddElement(graphNode);
                m_GeneralGraphView.AddEdgeByPorts(node.GraphNode.OutPort, inPort);
                CreateEnitiyNode(enititnode);
                m_NodeList.Add(graphNode);
            }
        }
    }
}