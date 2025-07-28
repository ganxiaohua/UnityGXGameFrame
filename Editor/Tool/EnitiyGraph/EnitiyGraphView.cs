using GameFrame.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class EntityGraphView : EditorEntity
    {
        private GeneralGraphView generalGraphView;

        private EntityInfos entityInfos;

        private EditorWindow editorWindow;

        private DoubleMap<Node, EntityNode> nodeDic;

        private int flootHeght;

        private int flootWidth;

        private EntityNode selectEntityNode;

        private float lastTime;

        public void Init(EditorWindow editorWindow)
        {
            base.Init(editorWindow);
            flootHeght = 200;
            flootWidth = 200;
            nodeDic = new();
            this.editorWindow = editorWindow;
            EditorApplication.playModeStateChanged += PlayModeStateChange;
        }

        public override void Show()
        {
            base.Show();
            if (generalGraphView == null)
            {
                generalGraphView = new GeneralGraphView();
                generalGraphView.Init();
                editorWindow.rootVisualElement.Add(generalGraphView);
            }

            generalGraphView.Show();
            FollowNode(null);
        }

        public override void Hide()
        {
            base.Hide();
            generalGraphView.Hide();
        }

        public override void Update()
        {
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
            if (generalGraphView == null) return;
            editorWindow.rootVisualElement.Remove(generalGraphView);
            RemoveAll();
            generalGraphView = null;
            EditorApplication.playModeStateChanged -= PlayModeStateChange;
            ComponentView.Destroy();
            EntityView.Destroy();
            base.Dispose();
        }

        private void RemoveAll()
        {
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

                generalGraphView.RemoveElement(entityNode.GraphNode);
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
            graphNode.AddButton("关注", FollowNode);
            graphNode.AddButton("删除", RemoveNode);
            var graphNodeName = string.IsNullOrEmpty(root.Entity.Name)
                ? root.Entity.GetType().Name
                : $"{root.Entity.GetType().Name} ({root.Entity.Name})";
            nodeDic.Add(graphNode, root);
            graphNode.Init(this, root, graphNodeName, new Rect(root.Floor * (flootHeght + 50), flootHeght + root.Grid * 100, flootWidth - 50, 100));
            var outPort = graphNode.AddProt("", typeof(bool), Direction.Output);
            root.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            nodeDic.Add(graphNode, root);
            graphNode.SetColor(root.Entity is EffEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
        }

        private void CreateEntityNode(EntityNode parentNode)
        {
            for (int i = 0; i < parentNode.NextNodes.Count; i++)
            {
                var childNode = parentNode.NextNodes[i];
                if (nodeDic.ContainsValue(childNode))
                {
                    if (parentNode.GraphNode == null)
                        CreateGraphNode(parentNode);
                    var port = (Port) childNode.GraphNode.outputContainer.hierarchy[0];
                    if (!port.connected)
                        generalGraphView.AddEdgeByPorts(parentNode.GraphNode.OutPort, port, PickingMode.Ignore);
                    CreateEntityNode(childNode);
                    continue;
                }

                Rect localRect = new Rect(childNode.Floor * (flootHeght + 50), flootHeght + childNode.Grid * 100, flootWidth - 50, 100);
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
            nodesPort.node.SetColor(childNode.Entity is EffEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
            nodeDic.Add(nodesPort.node, childNode);
            CreateEntityNode(childNode);
        }

        private (GeneralGrophNode node, Port inPort) CreateGraphNode(EntityNode node)
        {
            Rect localRect = new Rect(node.Floor * (flootHeght + 50), flootHeght + node.Grid * 100, flootWidth - 50, 100);
            var graphNode = generalGraphView.AddNode<GeneralGrophNode>();
            graphNode.AddButton("关注", FollowNode);
            graphNode.AddButton("删除", RemoveNode);
            var graphNodeName = string.IsNullOrEmpty(node.Entity.Name)
                ? node.Entity.GetType().Name
                : $"{node.Entity.GetType().Name} ({node.Entity.Name})";
            graphNode.Init(this, node, graphNodeName, localRect);
            var inPort = graphNode.AddProt("", typeof(bool), Direction.Input);
            _ = graphNode.AddProt("", typeof(bool), Direction.Output);
            node.GraphNode = graphNode;
            graphNode.RefreshExpandedState();
            graphNode.RefreshPorts();
            generalGraphView.AddElement(graphNode);
            return (graphNode, inPort);
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

        private void PlayModeStateChange(PlayModeStateChange playModeStateChange)
        {
            nodeDic.Clear();
            entityInfos = null;
        }

        public void FindNode(string name)
        {
            if (entityInfos == null) return;
            FindNode(entityInfos.RootNode, name);
        }

        private void FindNode(EntityNode node, string name)
        {
            if (string.Equals(node.Entity.Name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                ShowComponent(node);
            }
            else if (node.NextNodes == null || node.NextNodes.Count == 0)
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