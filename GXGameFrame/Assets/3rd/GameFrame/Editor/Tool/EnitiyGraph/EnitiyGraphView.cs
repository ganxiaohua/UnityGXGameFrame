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

        private DoubleMap<Node, EntityNode> m_NodeDic;

        private int m_FlootHeght;

        private int m_FlootWidth;

        private EntityNode mSelectEntityNode;

        private float m_LastTime;

        public void Init(EditorWindow editorWindow)
        {
            base.Init(editorWindow);
            m_FlootHeght = 200;
            m_FlootWidth = 200;
            m_NodeDic = new();
            m_EditorWindow = editorWindow;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
        }

        public override void Show()
        {
            base.Show();
            if (m_GeneralGraphView == null)
            {
                m_GeneralGraphView = new GeneralGraphView();
                m_GeneralGraphView.Init();
                m_EditorWindow.rootVisualElement.Add(m_GeneralGraphView);
            }

            m_GeneralGraphView.Show();
            FollowNode(null);
        }

        public override void Hide()
        {
            base.Hide();
            m_GeneralGraphView.Hide();
        }

        public override void Update()
        {
            // if ((m_CurUpdateTime += Time.deltaTime) >= 0.5f)
            // {
            //     m_CurUpdateTime = 0;
            // }

            if (mSelectEntityNode != null)
            {
                CreateNodeWithInfo(mSelectEntityNode);
            }

            m_GeneralGraphView.Update();
        }

        public override void Clear()
        {
            if (m_GeneralGraphView == null) return;
            m_EditorWindow.rootVisualElement.Remove(m_GeneralGraphView);
            RemoveAll();
            m_GeneralGraphView = null;
            EditorApplication.playModeStateChanged -= PlayModeStateChange;
            ComponentView.Destroy();
            base.Clear();
        }

        private void RemoveAll()
        {
            m_GeneralGraphView.DeleteAllNode();
            m_NodeDic.Clear();
        }

        public void FollowNode(Node node)
        {
            if (node == null)
            {
                mEntityInfos ??= new EntityInfos();
                mEntityInfos.GetRootEntity();
                mSelectEntityNode = mEntityInfos.RootNode;
            }
            else if (m_NodeDic.TryGetValueKv(node, out EntityNode entityNode))
            {
                mSelectEntityNode = entityNode;
            }

            RemoveAll();
        }

        private void RemoveNode(Node node)
        {
            if (m_NodeDic.TryGetValueKv(node, out EntityNode entityNode))
            {
                (((ECSEntity) entityNode.Entity).Parent as World).RemoveChild((ECSEntity) entityNode.Entity);
                m_GeneralGraphView.RemoveElement(entityNode.GraphNode);
                entityNode.GraphNode.Clear();
                m_NodeDic.RemoveByKey(node);
            }

            FollowNode(null);
        }

        private void CreateNodeWithInfo(EntityNode rootNode)
        {
            if (rootNode == null || rootNode.Entity == null)
            {
                return;
            }

            CreateRoot(rootNode);
            CreateEntityNode(rootNode);
        }

        private void CreateRoot(EntityNode root)
        {
            if (m_NodeDic.ContainsValue(root))
            {
                return;
            }

            var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
            graphNode.AddButton("关注", FollowNode);
            graphNode.AddButton("删除", RemoveNode);
            var graphNodeName = string.IsNullOrEmpty(root.Entity.Name)
                ? root.Entity.GetType().Name
                : $"{root.Entity.GetType().Name} ({root.Entity.Name})";
            m_NodeDic.Add(graphNode, root);
            graphNode.Init(this, root, graphNodeName, new Rect(root.Floor * (m_FlootHeght + 50), m_FlootHeght + root.Grid * 100, m_FlootWidth - 50, 100));
            var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            m_NodeDic.Add(graphNode, root);
            graphNode.SetColor(root.Entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
        }

        private void CreateEntityNode(EntityNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                if (m_NodeDic.ContainsValue(enititnode))
                {
                    CreateEntityNode(enititnode);
                    continue;
                }

                Rect localRect = new Rect(enititnode.Floor * (m_FlootHeght + 50), m_FlootHeght + enititnode.Grid * 100, m_FlootWidth - 50, 100);
                Rect graphViewRect = m_GeneralGraphView.viewport.worldBound;
                float scale = m_GeneralGraphView.contentViewContainer.transform.scale.x;
                Rect worldBound = m_GeneralGraphView.contentViewContainer.worldBound;
                Rect rectView = new Rect(localRect.x * scale + worldBound.x, localRect.y * scale + worldBound.y, localRect.width, localRect.height);
                if (!graphViewRect.Overlaps(rectView))
                {
                    continue;
                }

                var graphNode = m_GeneralGraphView.AddNode<GeneralGrophNode>();
                graphNode.AddButton("关注", FollowNode);
                graphNode.AddButton("删除", RemoveNode);
                var graphNodeName = string.IsNullOrEmpty(enititnode.Entity.Name)
                    ? enititnode.Entity.GetType().Name
                    : $"{enititnode.Entity.GetType().Name} ({enititnode.Entity.Name})";
                graphNode.Init(this, enititnode, graphNodeName, localRect);
                var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
                var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
                enititnode.GraphNode = graphNode;
                graphNode.RefreshExpandedState();
                graphNode.RefreshPorts();
                m_GeneralGraphView.AddElement(graphNode);
                m_GeneralGraphView.AddEdgeByPorts(node.GraphNode.OutPort, inPort, PickingMode.Ignore);
                graphNode.SetColor(enititnode.Entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
                m_NodeDic.Add(graphNode, enititnode);
                CreateEntityNode(enititnode);
            }
        }

        private void RemoveEntityNode()
        {
            Rect graphViewRect = m_GeneralGraphView.viewport.worldBound;
            var list = m_NodeDic.Keys;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector2 pos = new Vector2(list[i].worldBound.x, list[i].worldBound.y);
                if (!graphViewRect.Contains(pos))
                {
                    m_GeneralGraphView.RemoveNode(list[i]);
                    m_NodeDic.RemoveByKey(list[i]);
                    list.RemoveAt(i);
                }
            }
        }


        public void ShowComponent(EntityNode selectEntityNode)
        {
            if (selectEntityNode.Entity is ECSEntity ecs)
            {
                ComponentView.Init(ecs);
            }
        }

        private void PlayModeStateChange(PlayModeStateChange playModeStateChange)
        {
            m_NodeDic.Clear();
            mEntityInfos = null;
        }

        public void FindNode(string name)
        {
            if(mEntityInfos == null) return;
            FindNode(mEntityInfos.RootNode, name);
        }

        private void FindNode(EntityNode node, string name)
        {
            if (string.Equals(node.Entity.Name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                ShowComponent(node);
            }
            else if(node.NextNodes == null || node.NextNodes.Count == 0)
            {
                return;
            }
            else
            {
                foreach (var nextNode in node.NextNodes)
                {
                    FindNode(nextNode, name);
                }
            }

        }
    }
}