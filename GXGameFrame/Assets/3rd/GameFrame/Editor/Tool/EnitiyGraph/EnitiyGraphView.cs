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
                (((ECSEntity) entityNode.Entity).Parent as World).RemoveChild((ECSEntity) entityNode.Entity);
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
            graphNode.SetColor(root.Entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
        }

        private void CreateEntityNode(EntityNode node)
        {
            for (int i = 0; i < node.NextNodes.Count; i++)
            {
                var enititnode = node.NextNodes[i];
                if (nodeDic.ContainsValue(enititnode))
                {
                    CreateEntityNode(enititnode);
                    continue;
                }

                Rect localRect = new Rect(enititnode.Floor * (flootHeght + 50), flootHeght + enititnode.Grid * 100, flootWidth - 50, 100);
                Rect graphViewRect = generalGraphView.viewport.worldBound;
                float scale = generalGraphView.contentViewContainer.transform.scale.x;
                Rect worldBound = generalGraphView.contentViewContainer.worldBound;
                Rect rectView = new Rect(localRect.x * scale + worldBound.x, localRect.y * scale + worldBound.y, localRect.width, localRect.height);
                if (!graphViewRect.Overlaps(rectView))
                {
                    continue;
                }

                var graphNode = generalGraphView.AddNode<GeneralGrophNode>();
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
                generalGraphView.AddElement(graphNode);
                generalGraphView.AddEdgeByPorts(node.GraphNode.OutPort, inPort, PickingMode.Ignore);
                graphNode.SetColor(enititnode.Entity is ECSEntity ? new Color(0.5f, 0.2f, 0.1f) : Color.gray);
                nodeDic.Add(graphNode, enititnode);
                CreateEntityNode(enititnode);
            }
        }

        private void RemoveEntityNode()
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
            if (selectEntityNode.Entity is ECSEntity ecs)
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