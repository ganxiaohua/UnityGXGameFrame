using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GameFrame.Editor
{
    public class EnitiyGraphView : EditorEnitiy
    {
        private GeneralGraphView m_GeneralGraphView;


        private EnitiyInfos m_EnitiyInfos;

        private EditorWindow m_EditorWindow;

        private Dictionary<Node, EnitiyNode> m_NodeDic;

        public void Init(EditorWindow editorWindow)
        {
            base.Init();
            m_GeneralGraphView = AddComponent<GeneralGraphView>();
            m_GeneralGraphView.Init();
            m_NodeDic = new();
            m_EditorWindow = editorWindow;
            editorWindow.rootVisualElement.Add(m_GeneralGraphView);
        }

        public override void Show()
        {
            base.Show();
            m_GeneralGraphView.Show();
            m_EnitiyInfos = new EnitiyInfos();
            FollowNode(null);
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
            m_NodeDic.Clear();
        }

        public void RemoveAll()
        {
            m_GeneralGraphView.DeleteAllNode();
        }

        public void FollowNode(Node node)
        {
            RemoveAll();
            if (node == null)
            {
                m_EnitiyInfos.GetRootEnitiy();
                CreateNodeWithInfo(m_EnitiyInfos.RootNode);
            }
            else if (m_NodeDic.TryGetValue(node, out EnitiyNode enitiyNode))
            {
                CreateNodeWithInfo(enitiyNode);
            }
        }

        public void CreateNodeWithInfo(EnitiyNode rootNode)
        {
            CreateRoot(rootNode);
            CreateEnitiyNode(rootNode);
        }

        private void CreateRoot(EnitiyNode root)
        {
            var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
            graphNode.AddButton("关注", FollowNode);
            var graphNodeName = root.entity.GetType().Name;
            m_NodeDic.Add(graphNode, root);
            graphNode.Init(graphNodeName, new Rect(root.Floor * 200, 150 + root.Grid * 100, 100, 150));
            var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
        }

        private void CreateEnitiyNode(EnitiyNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
                var graphNodeName = enititnode.entity.GetType().Name;
                graphNode.AddButton("关注", FollowNode);
                m_NodeDic.Add(graphNode, enititnode);
                graphNode.Init(graphNodeName, new Rect(enititnode.Floor * 250, 150 + enititnode.Grid * 100, 100, 150));
                var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
                var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
                enititnode.GraphNode = graphNode;
                graphNode.RefreshExpandedState();
                graphNode.RefreshPorts();
                m_GeneralGraphView.AddElement(graphNode);
                m_GeneralGraphView.AddEdgeByPorts(node.GraphNode.OutPort, inPort);
                CreateEnitiyNode(enititnode);
            }
        }
    }
}