using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GameFrame.Runtime.UnityUI
{
    public sealed partial class UISystem
    {
        public RectTransform Root { get; private set; }
        public Canvas RootCanvas { get; private set; }
        public IKeyboardHandler KeyboardHandler { get; set; } = new NoKeyboardHandler();

        private RectTransform inactiveRoot;
        private CanvasGroup rootGroup;
        private GameObject ownedEventSystem;
        private bool globalInputEnabled = true;

        public void Initialize()
        {
            if (Root != null) return;
            var frameRoot = GXGameFrame.Instance.RootEntity;
            if (frameRoot == null) throw new InvalidOperationException("Call GXGameFrame.Init before using UnityUI.");
            entityOwner = frameRoot.GetComponent<UIRootComponents>();
            disposing = false;
            globalInputEnabled = true;
            var go = new GameObject("UnityUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(CanvasGroup), typeof(GraphicRaycaster));
            UnityEngine.Object.DontDestroyOnLoad(go);
            Root = (RectTransform)go.transform;
            RootCanvas = go.GetComponent<Canvas>();
            RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            rootGroup = go.GetComponent<CanvasGroup>();
            var staging = new GameObject("Initializing", typeof(RectTransform));
            inactiveRoot = (RectTransform)staging.transform;
            inactiveRoot.SetParent(Root, false);
            staging.SetActive(false);
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                ownedEventSystem = new GameObject("UnityUI EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                UnityEngine.Object.DontDestroyOnLoad(ownedEventSystem);
            }
        }

        public void SetGlobalEnable(bool enableRender, bool enableInput)
        {
            Initialize();
            rootGroup.alpha = enableRender ? 1 : 0;
            rootGroup.interactable = enableInput;
            rootGroup.blocksRaycasts = enableInput;
            globalInputEnabled = enableInput;
            foreach (var panel in panels) ApplyGlobalSettings(panel);
            if (!enableInput && EventSystem.current != null)
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if (selected != null && selected.transform.IsChildOf(Root)) EventSystem.current.SetSelectedGameObject(null);
            }
        }

        internal void ApplyGlobalSettings(Panel panel) => panel.ApplyGlobalSettings(globalInputEnabled);

        public bool IsDisplayTopmost(Panel panel)
        {
            if (!panel.Visible || panel.Root == null || !panel.Root.gameObject.activeInHierarchy) return false;
            foreach (var other in visiblePanels)
            {
                if (other == panel || (other.Flags & PanelFlag.NonTopmost) != 0 || other.Root == null) continue;
                if (!other.Root.gameObject.activeInHierarchy || other.Root.IsChildOf(panel.Root)) continue;
                if (CompareDisplayOrder(other, panel) > 0) return false;
            }
            return true;
        }

        private static int CompareDisplayOrder(Panel left, Panel right)
        {
            var leftTop = left;
            var rightTop = right;
            while (leftTop.Parent != null) leftTop = leftTop.Parent;
            while (rightTop.Parent != null) rightTop = rightTop.Parent;
            var order = leftTop.Canvas.sortingOrder.CompareTo(rightTop.Canvas.sortingOrder);
            if (order != 0) return order;
            Transform a = left.Root;
            Transform b = right.Root;
            if (a.IsChildOf(b)) return 1;
            if (b.IsChildOf(a)) return -1;
            var aDepth = GetDepth(a);
            var bDepth = GetDepth(b);
            while (aDepth > bDepth) { a = a.parent; aDepth--; }
            while (bDepth > aDepth) { b = b.parent; bDepth--; }
            while (a.parent != b.parent) { a = a.parent; b = b.parent; }
            return a.GetSiblingIndex().CompareTo(b.GetSiblingIndex());
        }

        private static int GetDepth(Transform node)
        {
            var depth = 0;
            while (node.parent != null) { node = node.parent; depth++; }
            return depth;
        }

        private void DestroyRoot()
        {
            if (Root != null) UnityEngine.Object.Destroy(Root.gameObject);
            if (ownedEventSystem != null) UnityEngine.Object.Destroy(ownedEventSystem);
            Root = null;
            RootCanvas = null;
            inactiveRoot = null;
            rootGroup = null;
            ownedEventSystem = null;
        }
    }
}
