using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class GeneralGraphView : GraphView
    {
        private static readonly Color EdgeColor = new Color(0.00f, 0.74f, 0.92f, 0.78f);
        private static readonly Color DarkBackground = new Color(0.105f, 0.115f, 0.13f, 1f);
        private static readonly Color LightBackground = new Color(0.78f, 0.80f, 0.84f, 1f);

        private List<Edge> edgeList;

        private List<Node> nodeList;

        private GridBackground gridBackground;

        private bool isCreatingRuntimeEdge;

        private float lastTime;

        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? DarkBackground : LightBackground;

        public void Init()
        {
            edgeList = new List<Edge>();
            nodeList = new();
            style.flexGrow = 1f;
            style.backgroundColor = BackgroundColor;

            gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();
            graphViewChanged = OnGraphViewChanged;

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
            if (outputPort == null || inputPort == null)
                return;

            Edge tempEdge = new StraightLineEdge()
            {
                output = outputPort,
                input = inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            edgeList.Add(tempEdge);
            tempEdge.pickingMode = picking;
            StyleEdge(tempEdge);

            isCreatingRuntimeEdge = true;
            try
            {
                AddElement(tempEdge);
            }
            finally
            {
                isCreatingRuntimeEdge = false;
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (!isCreatingRuntimeEdge && change.edgesToCreate != null)
                change.edgesToCreate.Clear();

            return change;
        }

        private void StyleEdge(Edge edge)
        {
            edge.capabilities &= ~Capabilities.Selectable;
            edge.capabilities &= ~Capabilities.Deletable;
            edge.pickingMode = PickingMode.Ignore;
            edge.edgeControl.inputOrientation = Orientation.Horizontal;
            edge.edgeControl.outputOrientation = Orientation.Horizontal;
            edge.edgeControl.inputColor = EdgeColor;
            edge.edgeControl.outputColor = EdgeColor;
            edge.edgeControl.fromCapColor = EdgeColor;
            edge.edgeControl.toCapColor = EdgeColor;
            edge.edgeControl.capRadius = 0f;
            edge.edgeControl.drawFromCap = false;
            edge.edgeControl.drawToCap = false;
            edge.edgeControl.edgeWidth = 0;
            edge.edgeControl.interceptWidth = 12f;
            edge.MarkDirtyRepaint();
        }

        private sealed class StraightLineEdge : Edge
        {
            private const float EdgePadding = 80f;
            private static readonly Color ShadowColor = new Color(0f, 0f, 0f, 0.36f);
            private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.16f);

            public StraightLineEdge()
            {
                style.position = Position.Absolute;
                pickingMode = PickingMode.Ignore;
                generateVisualContent += OnGenerateVisualContent;
            }

            public override bool UpdateEdgeControl()
            {
                bool updated = base.UpdateEdgeControl();
                UpdateLineLayout();
                return updated;
            }

            protected override EdgeControl CreateEdgeControl()
            {
                var control = new EdgeControl
                {
                    inputOrientation = Orientation.Horizontal,
                    outputOrientation = Orientation.Horizontal,
                    capRadius = 0f,
                    drawFromCap = false,
                    drawToCap = false,
                    edgeWidth = 0,
                    interceptWidth = 12f
                };
                control.visible = false;
                control.pickingMode = PickingMode.Ignore;
                return control;
            }

            private void UpdateLineLayout()
            {
                if (output == null || input == null || parent == null)
                    return;

                Vector2 from = parent.WorldToLocal(output.GetGlobalCenter());
                Vector2 to = parent.WorldToLocal(input.GetGlobalCenter());
                float left = Mathf.Min(from.x, to.x) - EdgePadding;
                float right = Mathf.Max(from.x, to.x) + EdgePadding;
                float top = Mathf.Min(from.y, to.y) - EdgePadding;
                float bottom = Mathf.Max(from.y, to.y) + EdgePadding;

                SetPosition(new Rect(left, top, Mathf.Max(1f, right - left), Mathf.Max(1f, bottom - top)));
                MarkDirtyRepaint();
            }

            private void OnGenerateVisualContent(MeshGenerationContext context)
            {
                if (output == null || input == null || parent == null)
                    return;

                Vector2 from = GlobalToLocal(output.GetGlobalCenter());
                Vector2 to = GlobalToLocal(input.GetGlobalCenter());

                DrawLine(context.painter2D, from + Vector2.down, to + Vector2.down, ShadowColor, 5f);
                DrawLine(context.painter2D, from, to, EdgeColor, 3f);
                DrawLine(context.painter2D, from + Vector2.up, to + Vector2.up, HighlightColor, 1f);
            }

            private static void DrawLine(Painter2D painter, Vector2 from, Vector2 to, Color color, float width)
            {
                painter.lineWidth = width;
                painter.strokeColor = color;
                painter.lineCap = LineCap.Round;
                painter.lineJoin = LineJoin.Round;
                painter.BeginPath();
                painter.MoveTo(from);
                painter.LineTo(to);
                painter.Stroke();
            }

            private Vector2 GlobalToLocal(Vector2 worldPosition)
            {
                Vector3 local = worldTransform.inverse.MultiplyPoint3x4(worldPosition);
                return new Vector2(local.x, local.y);
            }
        }
    }
}
