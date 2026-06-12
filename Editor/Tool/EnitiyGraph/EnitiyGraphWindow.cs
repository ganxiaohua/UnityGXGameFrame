using GameFrame.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFrame.Editor
{
    public class DialogueGraphWindow : EditorWindow
    {
        private const float HeaderHeight = 76f;

        private static readonly Color AccentColor = new Color(0.00f, 0.74f, 0.92f, 1f);
        private static readonly Color CapabilityColor = new Color(1.00f, 0.72f, 0.22f, 1f);
        private static readonly Color IdleColor = new Color(0.36f, 0.40f, 0.46f, 1f);
        private static readonly Color DarkBackground = new Color(0.105f, 0.115f, 0.13f, 1f);
        private static readonly Color LightBackground = new Color(0.78f, 0.80f, 0.84f, 1f);
        private static readonly Color DarkPanel = new Color(0.145f, 0.16f, 0.185f, 1f);
        private static readonly Color LightPanel = new Color(0.90f, 0.91f, 0.93f, 1f);

        private EntityGraphView graphView;
        private VisualElement graphHost;
        private VisualElement emptyState;
        private Label statusPill;
        private Label subtitleLabel;
        private Label emptyTitleLabel;
        private Label emptyDetailLabel;
        private Label searchResultLabel;
        private TextField searchField;
        private Button refreshButton;
        private Button searchComponentButton;
        private Button searchCapabilityButton;

        private static Color BackgroundColor => EditorGUIUtility.isProSkin ? DarkBackground : LightBackground;
        private static Color PanelColor => EditorGUIUtility.isProSkin ? DarkPanel : LightPanel;
        private static Color PrimaryText => EditorGUIUtility.isProSkin ? new Color(0.91f, 0.93f, 0.96f, 1f) : new Color(0.12f, 0.13f, 0.15f, 1f);
        private static Color SecondaryText => EditorGUIUtility.isProSkin ? new Color(0.62f, 0.67f, 0.72f, 1f) : new Color(0.34f, 0.36f, 0.40f, 1f);

        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraphWindow>();
            window.titleContent = new GUIContent("实体审查");
            window.minSize = new Vector2(760f, 480f);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("实体审查");
            minSize = new Vector2(760f, 480f);
            BuildLayout();
            SyncGraphState();
        }

        private void Update()
        {
            SyncGraphState();
            graphView?.Update();
        }

        private void OnDisable()
        {
            DisposeGraph();
        }

        private void BuildLayout()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.backgroundColor = BackgroundColor;

            rootVisualElement.Add(CreateHeader());
            rootVisualElement.Add(CreateToolbar());

            graphHost = new VisualElement();
            graphHost.style.flexGrow = 1f;
            graphHost.style.minHeight = 0f;
            graphHost.style.position = Position.Relative;
            rootVisualElement.Add(graphHost);

            ShowEmptyState("等待运行", "进入 Play 模式后会显示当前实体层级。");
        }

        private VisualElement CreateHeader()
        {
            var header = new VisualElement();
            header.style.height = HeaderHeight;
            header.style.backgroundColor = PanelColor;
            header.style.borderTopWidth = 2f;
            header.style.borderTopColor = AccentColor;
            header.style.paddingLeft = 16f;
            header.style.paddingRight = 16f;
            header.style.paddingTop = 9f;

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;

            var textGroup = new VisualElement();
            textGroup.style.flexGrow = 1f;

            var titleLabel = new Label("Entity Graph");
            titleLabel.style.color = PrimaryText;
            titleLabel.style.fontSize = 18;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            subtitleLabel = new Label("Runtime hierarchy inspector");
            subtitleLabel.style.color = SecondaryText;
            subtitleLabel.style.fontSize = 11;
            subtitleLabel.style.marginTop = 3f;

            textGroup.Add(titleLabel);
            textGroup.Add(subtitleLabel);

            statusPill = new Label();
            StylePill(statusPill, IdleColor);
            titleRow.Add(textGroup);
            titleRow.Add(statusPill);

            var legend = new VisualElement();
            legend.style.flexDirection = FlexDirection.Row;
            legend.style.marginTop = 8f;
            legend.Add(CreateLegendItem("Entity", IdleColor));
            legend.Add(CreateLegendItem("EffEntity", AccentColor));
            legend.Add(CreateLegendItem("World", CapabilityColor));

            header.Add(titleRow);
            header.Add(legend);
            return header;
        }

        private VisualElement CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.backgroundColor = PanelColor;
            toolbar.style.paddingLeft = 12f;
            toolbar.style.paddingRight = 12f;
            toolbar.style.paddingTop = 7f;
            toolbar.style.paddingBottom = 7f;
            toolbar.style.borderTopWidth = 1f;
            toolbar.style.borderTopColor = new Color(1f, 1f, 1f, 0.06f);
            toolbar.style.borderBottomWidth = 1f;
            toolbar.style.borderBottomColor = new Color(0f, 0f, 0f, 0.28f);

            refreshButton = new Button(RefreshGraph) {text = "刷新"};
            StyleButton(refreshButton, AccentColor);

            searchField = new TextField();
            searchField.style.width = 220f;
            searchField.style.marginLeft = 10f;
            searchField.style.marginRight = 6f;
            searchField.tooltip = "输入实体名或类型名";
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                    SearchComponent();
            });

            searchComponentButton = new Button(SearchComponent) {text = "组件"};
            StyleButton(searchComponentButton, AccentColor);

            searchCapabilityButton = new Button(SearchCapability) {text = "能力"};
            StyleButton(searchCapabilityButton, CapabilityColor);

            searchResultLabel = new Label();
            searchResultLabel.style.color = SecondaryText;
            searchResultLabel.style.fontSize = 11;
            searchResultLabel.style.marginLeft = 10f;
            searchResultLabel.style.flexGrow = 1f;

            toolbar.Add(refreshButton);
            toolbar.Add(searchField);
            toolbar.Add(searchComponentButton);
            toolbar.Add(searchCapabilityButton);
            toolbar.Add(searchResultLabel);
            return toolbar;
        }

        private VisualElement CreateLegendItem(string text, Color color)
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.alignItems = Align.Center;
            item.style.marginRight = 18f;

            var swatch = new VisualElement();
            swatch.style.width = 10f;
            swatch.style.height = 10f;
            swatch.style.backgroundColor = color;
            swatch.style.marginRight = 6f;

            var label = new Label(text);
            label.style.color = SecondaryText;
            label.style.fontSize = 11;

            item.Add(swatch);
            item.Add(label);
            return item;
        }

        private void SyncGraphState()
        {
            bool playing = EditorApplication.isPlaying;
            bool sceneReady = playing && SceneFactory.GetPlayerScene() != null;

            if (!sceneReady)
            {
                if (graphView != null)
                    DisposeGraph();

                string title = playing ? "等待场景初始化" : "等待运行";
                string detail = playing ? "PlayerScene 创建后会自动加载实体图。" : "进入 Play 模式后会显示当前实体层级。";
                ShowEmptyState(title, detail);
                SetStatus(playing ? "Initializing" : "Stopped", playing ? CapabilityColor : IdleColor);
                SetToolbarEnabled(false);
                subtitleLabel.text = "Runtime hierarchy inspector";
                return;
            }

            if (graphView == null)
                CreateGraph();

            SetStatus(EditorApplication.isPaused ? "Paused" : "Live", EditorApplication.isPaused ? CapabilityColor : AccentColor);
            SetToolbarEnabled(true);
            subtitleLabel.text = "Play mode entity hierarchy";
        }

        private void CreateGraph()
        {
            graphHost.Clear();
            emptyState = null;
            graphView = new EntityGraphView();
            graphView.Init(this, graphHost);
            graphView.Show();
            searchResultLabel.text = string.Empty;
        }

        private void DisposeGraph()
        {
            graphView?.Dispose();
            graphView = null;
        }

        private void RefreshGraph()
        {
            graphView?.FollowNode(null);
            searchResultLabel.text = "已刷新实体图";
        }

        private void SearchComponent()
        {
            Search(true);
        }

        private void SearchCapability()
        {
            Search(false);
        }

        private void Search(bool component)
        {
            if (graphView == null)
                return;

            string keyword = searchField.value;
            bool found = component ? graphView.FindNodeComp(keyword) : graphView.FindNodecapability(keyword);
            searchResultLabel.text = found ? "已打开匹配实体" : "未找到匹配实体";
        }

        private void ShowEmptyState(string title, string detail)
        {
            if (graphHost == null || graphView != null)
                return;

            if (emptyState != null && emptyState.parent == graphHost)
            {
                emptyTitleLabel.text = title;
                emptyDetailLabel.text = detail;
                return;
            }

            graphHost.Clear();

            emptyState = new VisualElement();
            emptyState.style.flexGrow = 1f;
            emptyState.style.justifyContent = Justify.Center;
            emptyState.style.alignItems = Align.Center;

            emptyTitleLabel = new Label(title);
            emptyTitleLabel.style.color = PrimaryText;
            emptyTitleLabel.style.fontSize = 16;
            emptyTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            emptyDetailLabel = new Label(detail);
            emptyDetailLabel.style.color = SecondaryText;
            emptyDetailLabel.style.fontSize = 12;
            emptyDetailLabel.style.marginTop = 6f;

            emptyState.Add(emptyTitleLabel);
            emptyState.Add(emptyDetailLabel);
            graphHost.Add(emptyState);
        }

        private void SetStatus(string text, Color color)
        {
            statusPill.text = text;
            StylePill(statusPill, color);
        }

        private void SetToolbarEnabled(bool enabled)
        {
            refreshButton?.SetEnabled(enabled);
            searchField?.SetEnabled(enabled);
            searchComponentButton?.SetEnabled(enabled);
            searchCapabilityButton?.SetEnabled(enabled);
        }

        private void StylePill(Label label, Color color)
        {
            label.style.minWidth = 86f;
            label.style.height = 24f;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = PrimaryText;
            label.style.backgroundColor = new Color(color.r, color.g, color.b, 0.22f);
            label.style.borderBottomWidth = 2f;
            label.style.borderBottomColor = color;
        }

        private void StyleButton(Button button, Color color)
        {
            button.style.height = 24f;
            button.style.minWidth = 58f;
            button.style.marginRight = 6f;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.backgroundColor = new Color(color.r, color.g, color.b, 0.18f);
            button.style.borderBottomWidth = 2f;
            button.style.borderBottomColor = color;
        }
    }
}
