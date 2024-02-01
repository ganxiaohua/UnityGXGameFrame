using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class EnitiyGraphView : EditorEnitiy
    {
        private GeneralGraphView m_GeneralGraphView;

        private EnitiyInfos m_EnitiyInfos;

        private EditorWindow m_EditorWindow;

        private Dictionary<Node, EnitiyNode> m_NodeDic;

        private int m_FlootHeght;

        private int m_FlootWidth;

        public EnitiyNode SelectEnitiyNode;

        private GeneralGraphGroup group;

        private float m_LastTime;

        private List<string> tempComList = new List<string>();

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
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
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
            m_GeneralGraphView.Clear();
            group.Clear();
            group = null;
            EditorApplication.update -= OnEditorUpdate;
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
                m_EnitiyInfos.GetRootEnitiy();
                CreateNodeWithInfo(m_EnitiyInfos.RootNode);
            }
            else if (m_NodeDic.TryGetValue(node, out EnitiyNode enitiyNode))
            {
                CreateNodeWithInfo(enitiyNode);
            }
        }

        private void CreateNodeWithInfo(EnitiyNode rootNode)
        {
            if (rootNode == null || rootNode.entity == null)
            {
                return;
            }

            CreateRoot(rootNode);
            CreateEnitiyNode(rootNode);
        }

        private void CreateRoot(EnitiyNode root)
        {
            var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
            graphNode.AddButton("关注", FollowNode);
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


        private void CreateEnitiyNode(EnitiyNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
                var graphNodeName = string.IsNullOrEmpty(enititnode.entity.Name)
                    ? enititnode.entity.GetType().Name
                    : $"{enititnode.entity.GetType().Name} ({enititnode.entity.Name})";
                graphNode.AddButton("关注", FollowNode);
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
                CreateEnitiyNode(enititnode);
                graphNode.SetColor(enititnode.entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
            }
        }

        private void OnEditorUpdate()
        {
            if (Time.realtimeSinceStartup - m_LastTime > 0.1f && SelectEnitiyNode!=null)
            {
                m_LastTime = Time.realtimeSinceStartup;
                if (group == null)
                {
                    group = AddComponent<GeneralGraphGroup>(m_GeneralGraphView);
                }

                tempComList.Clear();
                if (SelectEnitiyNode.entity is ECSEntity ecs)
                {
                    List<int> comIndexs = ecs.ECSComponentArray.Indexs;
                    foreach (var index in comIndexs)
                    {
                        ECSComponent ecsComponent = ecs.GetComponent(index);

                        Type type = ecsComponent.GetType();

                        // 获取所有公共字段（包括静态和实例）
                        FieldInfo[] fields = type.GetFields();
                        string syr = type.Name;
                        foreach (var field in fields)
                        {
                            syr += ($" - {field.Name}: {field.GetValue(ecsComponent)}");
                        }

                        tempComList.Add(syr);
                    }

                    group.CreateList(tempComList, SelectEnitiyNode.GraphNode.GetPosition());
                }
            }
        }

        private void PlayModeStateChange(PlayModeStateChange playModeStateChange)
        {
            m_NodeDic.Clear();
            m_GeneralGraphView.Clear();
            group.Clear();
        }
    }
}