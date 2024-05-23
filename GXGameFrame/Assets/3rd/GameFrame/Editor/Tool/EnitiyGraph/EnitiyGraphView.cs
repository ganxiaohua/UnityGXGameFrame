﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class EntityGraphView : EditorEntity
    {
        private GeneralGraphView m_GeneralGraphView;

        private EntityInfos mEntityInfos;

        private EditorWindow m_EditorWindow;

        private Dictionary<Node, EntityNode> m_NodeDic;

        private int m_FlootHeght;

        private int m_FlootWidth;

        private EntityNode mSelectEntityNode;

        private GeneralGraphGroup m_Group;

        private float m_LastTime;

        private ComponentView m_ComponentView;

        public void Init(EditorWindow editorWindow)
        {
            base.Init(null);
            m_FlootHeght = 200;
            m_FlootWidth = 200;
            m_GeneralGraphView = new GeneralGraphView();
            m_GeneralGraphView.Init();
            m_NodeDic = new();
            m_EditorWindow = editorWindow;
            editorWindow.rootVisualElement.Add(m_GeneralGraphView);
            EditorApplication.playModeStateChanged += PlayModeStateChange;
        }

        public override void Show()
        {
            base.Show();
            m_GeneralGraphView.Show();
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
            m_GeneralGraphView.Clear();
            m_Group?.Clear();
            m_Group = null;
            m_ComponentView?.Close();
            m_ComponentView = null;
            EditorApplication.playModeStateChanged -= PlayModeStateChange;
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
                mEntityInfos ??= new EntityInfos();
                mEntityInfos.GetRootEntity();
                CreateNodeWithInfo(mEntityInfos.RootNode);
            }
            else if (m_NodeDic.TryGetValue(node, out EntityNode entityNode))
            {
                CreateNodeWithInfo(entityNode);
            }
        }
        
        private void RemoveNode(Node node)
        {
            if (m_NodeDic.TryGetValue(node, out EntityNode entityNode))
            {
                (((ECSEntity)entityNode.entity).Parent as Context).RemoveChild((ECSEntity)entityNode.entity);
                m_GeneralGraphView.RemoveElement(entityNode.GraphNode);
                entityNode.GraphNode.Clear();
                m_NodeDic.Remove(node);
            }
        }

        private void CreateNodeWithInfo(EntityNode rootNode)
        {
            if (rootNode == null || rootNode.entity == null)
            {
                return;
            }

            CreateRoot(rootNode);
            CreateEntityNode(rootNode);
        }

        private void CreateRoot(EntityNode root)
        {
            var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
            graphNode.AddButton("关注", FollowNode);
            graphNode.AddButton("删除", RemoveNode);
            var graphNodeName = string.IsNullOrEmpty(root.entity.Name)
                ? root.entity.GetType().Name
                : $"{root.entity.GetType().Name} ({root.entity.Name})";
            m_NodeDic.Add(graphNode, root);
            graphNode.Init(this, root, graphNodeName, new Rect(root.Floor * (m_FlootHeght + 50), m_FlootHeght + root.Grid * 100, m_FlootWidth - 50, 100));
            var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            graphNode.SetColor(root.entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
        }


        private void CreateEntityNode(EntityNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
                var graphNodeName = string.IsNullOrEmpty(enititnode.entity.Name)
                    ? enititnode.entity.GetType().Name
                    : $"{enititnode.entity.GetType().Name} ({enititnode.entity.Name})";
                graphNode.AddButton("关注", FollowNode);
                graphNode.AddButton("删除", RemoveNode);
                m_NodeDic.Add(graphNode, enititnode);
                graphNode.Init(this, enititnode, graphNodeName,
                    new Rect(enititnode.Floor * (m_FlootHeght + 50), m_FlootHeght + enititnode.Grid * 100, m_FlootWidth - 50, 100));
                var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
                var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
                enititnode.GraphNode = graphNode;
                graphNode.RefreshExpandedState();
                graphNode.RefreshPorts();
                m_GeneralGraphView.AddElement(graphNode);
                m_GeneralGraphView.AddEdgeByPorts(node.GraphNode.OutPort, inPort, PickingMode.Ignore);
                CreateEntityNode(enititnode);
                graphNode.SetColor(enititnode.entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
            }
        }

        public void ShowComponent(EntityNode selectEntityNode)
        {
            mSelectEntityNode = selectEntityNode;
            if (mSelectEntityNode.entity is ECSEntity ecs)
            {
                List<int> comIndexs = ecs.ECSComponentArray.Indexs;
                List<ECSComponent> ecsComponents = new List<ECSComponent>();
                foreach (var index in comIndexs)
                {
                    ECSComponent ecsComponent = ecs.GetComponent(index);
                    ecsComponents.Add(ecsComponent);
                }
                ComponentView.Init(ecsComponents,ecs);
            }
        }

        private void PlayModeStateChange(PlayModeStateChange playModeStateChange)
        {
            m_NodeDic.Clear();
            mEntityInfos = null;
            // m_GeneralGraphView.Clear();
            m_Group?.Clear();
        }
    }
}