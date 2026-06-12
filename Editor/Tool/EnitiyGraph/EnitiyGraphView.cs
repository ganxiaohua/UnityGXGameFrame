using System.Text;
using System;
using GameFrame.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class EntityGraphView : EditorEntity
    {
        private static readonly Color EntityColor = new Color(0.36f, 0.40f, 0.46f, 1f);
        private static readonly Color EffEntityColor = new Color(0.00f, 0.74f, 0.92f, 1f);
        private static readonly Color WorldColor = new Color(1.00f, 0.72f, 0.22f, 1f);

        private GeneralGraphView generalGraphView;

        private EntityInfos entityInfos;

        private EditorWindow editorWindow;

        private VisualElement graphContainer;

        private DoubleMap<Node, EntityNode> nodeDic;

        private int flootHeght;

        private int flootWidth;

        private EntityNode selectEntityNode;

        private float lastTime;

        public void Init(EditorWindow editorWindow)
        {
            Init(editorWindow, editorWindow.rootVisualElement);
        }

        public void Init(EditorWindow editorWindow, VisualElement graphContainer)
        {
            base.Init(editorWindow);
            flootHeght = 118;
            flootWidth = 280;
            nodeDic = new();
            this.editorWindow = editorWindow;
            this.graphContainer = graphContainer ?? editorWindow.rootVisualElement;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
        }

        public override void Show()
        {
            base.Show();
            if (generalGraphView == null)
            {
                generalGraphView = new GeneralGraphView();
                generalGraphView.Init();
                graphContainer.Add(generalGraphView);
            }

            generalGraphView.Show();
            FollowNode(null);
        }

        public override void Hide()
        {
            base.Hide();
            generalGraphView?.Hide();
        }

        public override void Update()
        {
            if (generalGraphView == null)
                return;

            // if ((m_CurUpdateTime += Time.deltaTime) >= 0.5f)
            // {
            //     m_CurUpdateTime = 0;
            // }

            if (selectEntityNode != null)
            {
                CreateNodeWithInfo(selectEntityNode);
            }

            generalGraphView.Update();
        }

        public override void Dispose()
        {
            if (generalGraphView != null)
            {
                graphContainer?.Remove(generalGraphView);
                RemoveAll();
                generalGraphView = null;
            }

            EditorApplication.playModeStateChanged -= PlayModeStateChange;
            ComponentView.Destroy();
            CapabilityView.Destroy();
            EntityView.Destroy();
            base.Dispose();
        }

        private void RemoveAll()
        {
            if (generalGraphView == null || nodeDic == null)
                return;

            generalGraphView.DeleteAllNode();
            nodeDic.Clear();
        }

        public void FollowNode(Node node)
        {
            if (node == null)
            {
                entityInfos ??= new EntityInfos();
                entityInfos.GetRootEntity();
                selectEntityNode = entityInfos.RootNode;
            }
            else if (nodeDic.TryGetValueKv(node, out EntityNode entityNode))
            {
                selectEntityNode = entityNode;
            }

            RemoveAll();
        }

        private void RemoveNode(Node node)
        {
            if (nodeDic.TryGetValueKv(node, out EntityNode entityNode))
            {
                if (entityNode.Entity.Parent is World)
                {
                    (((EffEntity) entityNode.Entity).Parent as World).RemoveChild((EffEntity) entityNode.Entity);
                }
                else
                {
                    var parent = (Entity) entityNode.Entity.Parent;
                    if (parent.Children.Contains(entityNode.Entity))
                    {
                        parent.RemoveChild(entityNode.Entity);
                    }
                    else
                    {
                        parent.RemoveComponent(entityNode.Entity.GetType());
                    }
                }

                generalGraphView.RemoveNode(entityNode.GraphNode);
                entityNode.GraphNode.Clear();
                nodeDic.RemoveByKey(node);
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
            // RemoveBeyondEntityNode();
        }

        private void CreateRoot(EntityNode root)
        {
            if (nodeDic.ContainsValue(root))
            {
                return;
            }

            var graphNode = generalGraphView.AddNode<GeneralGrophNode>();
            var graphNodeName = GetGraphNodeName(root);
            nodeDic.Add(graphNode, root);
            graphNode.Init(this, root, graphNodeName, GetNodeRect(root, graphNodeName));
            AddNodeActions(graphNode, root);
            _ = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            graphNode.SetColor(GetNodeColor(root));
        }

        private void CreateEntityNode(EntityNode parentNode)
        {
            for (int i = 0; i < parentNode.NextNodes.Count; i++)
            {
                var childNode = parentNode.NextNodes[i];
                if (nodeDic.ContainsValue(childNode))
                {
                    if (parentNode.GraphNode == null)
                    {
                        var parentPorts = CreateGraphNode(parentNode);
                        parentPorts.node.SetColor(GetNodeColor(parentNode));
                        if (!nodeDic.ContainsValue(parentNode))
                            nodeDic.Add(parentPorts.node, parentNode);
                    }

                    var port = childNode.GraphNode?.InPort;
                    if (port != null && !port.connected)
                        generalGraphView.AddEdgeByPorts(parentNode.GraphNode.OutPort, port, PickingMode.Ignore);
                    CreateEntityNode(childNode);
                    continue;
                }

                Rect localRect = GetNodeRect(childNode, GetGraphNodeName(childNode));
                Rect graphViewRect = generalGraphView.viewport.worldBound;
                float scale = generalGraphView.contentViewContainer.transform.scale.x;
                Rect worldBound = generalGraphView.contentViewContainer.worldBound;
                Rect rectView = new Rect(localRect.x * scale + worldBound.x, localRect.y * scale + worldBound.y, localRect.width, localRect.height);
                if (!graphViewRect.Overlaps(rectView))
                {
                    CreateEntityNode(childNode);
                    continue;
                }

                CreateEntityNode(parentNode, childNode);
            }
        }

        private void CreateEntityNode(EntityNode parentNode, EntityNode childNode)
        {
            var nodesPort = CreateGraphNode(childNode);
            if (parentNode.GraphNode != null)
                generalGraphView.AddEdgeByPorts(parentNode.GraphNode.OutPort, nodesPort.inPort, PickingMode.Ignore);
            nodesPort.node.SetColor(GetNodeColor(childNode));
            nodeDic.Add(nodesPort.node, childNode);
            CreateEntityNode(childNode);
        }

        private (GeneralGrophNode node, Port inPort) CreateGraphNode(EntityNode node)
        {
            var graphNode = generalGraphView.AddNode<GeneralGrophNode>();
            var graphNodeName = GetGraphNodeName(node);
            Rect localRect = GetNodeRect(node, graphNodeName);
            graphNode.Init(this, node, graphNodeName, localRect);
            AddNodeActions(graphNode, node);

            var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
            _ = graphNode.AddProt("", typeof(bool), Direction.Output);
            node.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            return (graphNode, inPort);
        }

        private void AddNodeActions(GeneralGrophNode graphNode, EntityNode node)
        {
            graphNode.AddButton("关注", FollowNode);
            graphNode.AddButton("删除", RemoveNode);
            graphNode.AddButton("组件", (x) => { ShowComponent(node); });
            if (node.Entity is EffEntity effEntity && effEntity.world is ECCWorld)
            {
                graphNode.AddButton("能力", (x) => { ShowCapability(node); });
            }
        }

        private string GetGraphNodeName(EntityNode node)
        {
            if (node?.Entity == null)
                return "Unknown Entity";

            return string.IsNullOrEmpty(node.Entity.Name)
                    ? node.Entity.GetType().Name
                    : $"{node.Entity.GetType().Name} ({node.Entity.Name})";
        }

        private Rect GetNodeRect(EntityNode node, string graphNodeName)
        {
            float width = Mathf.Clamp(graphNodeName.Length * 8f + 120f, 220f, 360f);
            return new Rect(node.Floor * flootWidth, 120f + node.Grid * flootHeght, width, 112f);
        }

        private Color GetNodeColor(EntityNode node)
        {
            if (node?.Entity is World)
                return WorldColor;

            return node?.Entity is EffEntity ? EffEntityColor : EntityColor;
        }

        private void RemoveBeyondEntityNode()
        {
            Rect graphViewRect = generalGraphView.viewport.worldBound;
            var list = nodeDic.Keys;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector2 pos = new Vector2(list[i].worldBound.x, list[i].worldBound.y);
                if (!graphViewRect.Contains(pos))
                {
                    generalGraphView.RemoveNode(list[i]);
                    nodeDic.RemoveByKey(list[i]);
                    list.RemoveAt(i);
                }
            }
        }


        public void ShowComponent(EntityNode selectEntityNode)
        {
            if (selectEntityNode.Entity is EffEntity ecs)
            {
                ComponentView.Init(ecs);
            }
            else
            {
                EntityView.Init(selectEntityNode.Entity);
            }
        }

        public void ShowCapability(EntityNode selectEntityNode)
        {
            if (selectEntityNode.Entity is EffEntity ecs && ecs.world is ECCWorld)
            {
                CapabilityView.Init(ecs);
            }
        }

        private void PlayModeStateChange(PlayModeStateChange playModeStateChange)
        {
            nodeDic?.Clear();
            entityInfos = null;
        }

        public bool FindNodeComp(string name)
        {
            if (entityInfos == null || string.IsNullOrWhiteSpace(name))
                return false;

            return FindNode(entityInfos.RootNode, name.Trim(), true);
        }

        public bool FindNodecapability(string name)
        {
            if (entityInfos == null || string.IsNullOrWhiteSpace(name))
                return false;

            return FindNode(entityInfos.RootNode, name.Trim(), false);
        }

        private bool FindNode(EntityNode node, string name, bool isComp)
        {
            if (node?.Entity == null)
                return false;

            if (IsNodeMatch(node, name))
            {
                if (isComp)
                {
                    ShowComponent(node);
                    return true;
                }

                if (node.Entity is EffEntity effEntity && effEntity.world is ECCWorld)
                {
                    ShowCapability(node);
                    return true;
                }

                return false;
            }

            if (node.NextNodes == null || node.NextNodes.Count == 0)
                return false;

            foreach (var nextNode in node.NextNodes)
            {
                if (FindNode(nextNode, name, isComp))
                    return true;
            }

            return false;
        }

        private bool IsNodeMatch(EntityNode node, string name)
        {
            string entityName = node.Entity.Name ?? string.Empty;
            string typeName = node.Entity.GetType().Name;
            return entityName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
