using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.Runtime.UnityUI
{
    public abstract partial class Panel : SimpleEntity, IVersions, IInitializeSystem, IUpdateSystem
    {
        public abstract string Package { get; }
        public new abstract string Name { get; }

        /// <summary>AssetManager address, or a Resources-relative path without extension for Builtin panels.</summary>
        public abstract string PrefabPath { get; }

        public int Versions { get; set; }
        public virtual PanelMode Mode => PanelMode.Normal;
        public virtual PanelFlag Flags => PanelFlag.None;
        public virtual int SortingOrder => BuiltinPanelOrder.Normal;
        public virtual string AudioOnShow => null;
        public virtual float LifetimeAfterHide => (Flags & PanelFlag.Persistent) != 0 || Parent != null
            ? float.PositiveInfinity : 5;

        public PanelState PanelState { get; private set; }
        public bool Visible => PanelState == PanelState.Open;
        public new Panel Parent { get; private set; }
        public List<Panel> Childs { get; } = new();
        public DefaultAssetReference AssetReference { get; private set; }
        public RectTransform Root { get; private set; }
        public RectTransform ParentHoldComponent { get; private set; }
        public Canvas Canvas { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        private UniqueTimer destroyTimer;
        private int canvasSortingOrder;
        private PanelInputGate inputGate;
        public UniqueTimer DestroyTimer => destroyTimer ??= new UniqueTimer(() => UISystem.Instance.DestroyPanel(this));

        public void OnInitialize()
        {
            PanelState = PanelState.UnInitialize;
            State = IEntity.EntityState.None;
            Versions = 0;
            AssetReference = ReferencePool.Acquire<DefaultAssetReference>();
        }

        // Bind before user initialization so a failed override can still be disposed safely.
        internal void BindRoot(RectTransform root)
        {
            Root = root;
            root.gameObject.SetActive(false);
            root.SetParent(UISystem.Instance.Root, false);
            if (!root.TryGetComponent(out Canvas canvas)) canvas = root.gameObject.AddComponent<Canvas>();
            if (!root.TryGetComponent(out CanvasGroup group)) group = root.gameObject.AddComponent<CanvasGroup>();
            if (!root.TryGetComponent<GraphicRaycaster>(out _)) root.gameObject.AddComponent<GraphicRaycaster>();
            Canvas = canvas;
            CanvasGroup = group;
            group.ignoreParentGroups = false;
            // A sorting-override Canvas stops Graphic's ancestor raycast-filter traversal.
            // Keep an independent input filter on the panel, preserving its local CanvasGroup.
            inputGate = root.gameObject.AddComponent<PanelInputGate>();
            UISystem.Instance.ApplyGlobalSettings(this);
            canvasSortingOrder = SortingOrder;
            group.alpha = 1;
            ApplyRootLayout();
#if UNITY_EDITOR
            RegisterDebugInspector();
#endif
        }

        internal void ApplyGlobalSettings(bool enableInput)
        {
            if (inputGate != null) inputGate.InputEnabled = enableInput;
        }

        private void ApplyRootLayout()
        {
            if (Mode == PanelMode.Embed || Mode == PanelMode.Pop) return;
            Root.anchorMin = Vector2.zero;
            Root.anchorMax = Vector2.one;
            Root.offsetMin = Vector2.zero;
            Root.offsetMax = Vector2.zero;
            Root.localScale = Vector3.one;
        }

        public void SetParent(Panel parent, RectTransform holdComponent = null)
        {
            RequireInitialized();
            if (holdComponent == null && parent != null) holdComponent = parent.Root;
            if (parent == Parent && holdComponent == ParentHoldComponent) return;
            if (parent != null)
            {
                // Parent initialization may be awaiting this child before it can complete.
                if (parent.Root == null || parent.PanelState == PanelState.Destroy)
                    throw new InvalidOperationException("The parent panel has no live view.");
                if (Mode == PanelMode.Mono) throw new InvalidOperationException("A Mono panel cannot be a child panel.");
                if (parent == this || parent.Root.IsChildOf(Root))
                    throw new InvalidOperationException("A panel cannot be parented to itself or its descendants.");
                if (holdComponent != parent.Root && !holdComponent.IsChildOf(parent.Root))
                    throw new ArgumentException("The container must belong to the parent panel.", nameof(holdComponent));
            }
            else if (holdComponent != null)
                throw new ArgumentException("A container requires a parent panel.", nameof(holdComponent));

            RememberCanvasSorting();
            Parent?.Childs.Remove(this);
            Parent = parent;
            ParentHoldComponent = holdComponent;
            parent?.Childs.Add(this);
            Root.SetParent(holdComponent != null ? holdComponent : UISystem.Instance.Root, false);
            ApplyCanvasSorting();
            ApplyRootLayout();
        }

        public void AddChild(Panel child, RectTransform holdComponent = null) => child.SetParent(this, holdComponent);

        public void SetVersionDirty()
        {
            if (PanelState == PanelState.Destroy) throw new InvalidOperationException("Panel is destroyed.");
            Versions++;
        }

        /// <summary>Bind controls here. Do not call Show or Hide during initialization.</summary>
        public virtual UniTask OnInitializeAsync(RectTransform root, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();
            if (root != Root || root == null || PanelState != PanelState.UnInitialize)
                throw new InvalidOperationException("Panel initialization must be performed by UISystem.");
            return UniTask.CompletedTask;
        }

        public virtual void OnInitializeComplete()
        {
            if (PanelState != PanelState.UnInitialize) throw new InvalidOperationException("Panel is already initialized.");
            PanelState = PanelState.Hide;
        }

        public virtual void OnBeforeShow() { }

        public virtual void OnShow(object args = null)
        {
            RequireInitialized();
            Versions++;
            destroyTimer?.Cancel();
            PanelState = PanelState.Open;
            State = IEntity.EntityState.IsRunning;
            DoShowAnimation();
        }

        protected virtual void DoShowAnimation()
        {
            if (Root.parent == null) Root.SetParent(UISystem.Instance.Root, false);
            Root.gameObject.SetActive(true);
            ApplyCanvasSorting();
        }

        private void ApplyCanvasSorting()
        {
            // A newly added Canvas does not accept sorting overrides until activated.
            if (!Root.gameObject.activeInHierarchy) return;
            Canvas.overrideSorting = Parent == null;
            if (Parent == null) Canvas.sortingOrder = canvasSortingOrder;
        }

        private void RememberCanvasSorting()
        {
            if (Parent == null && Root.gameObject.activeInHierarchy)
                canvasSortingOrder = Canvas.sortingOrder;
        }

        public virtual void OnRestore()
        {
            RequireInitialized();
            Versions++;
            destroyTimer?.Cancel();
            PanelState = PanelState.Open;
            State = IEntity.EntityState.IsRunning;
            DoRestoreAnimation();
        }

        protected virtual void DoRestoreAnimation() => DoShowAnimation();

        public virtual void OnHide()
        {
            RequireInitialized();
            RememberCanvasSorting();
            Versions++;
            PanelState = PanelState.Hide;
            State = IEntity.EntityState.IsHide;
            DoHideAnimation();
        }

        protected virtual void DoHideAnimation() => Root.gameObject.SetActive(false);
        public virtual void OnAfterHide() { }
        public virtual void OnDestroySuppressed() { }

        public override void Dispose()
        {
            Versions++;
            destroyTimer?.Cancel();
            Parent?.Childs.Remove(this);
            Parent = null;
            ParentHoldComponent = null;
            if (Root != null)
            {
                Root.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(Root.gameObject);
            }
            Root = null;
            Canvas = null;
            CanvasGroup = null;
            inputGate = null;
            if (AssetReference != null) ReferencePool.Release(AssetReference);
            AssetReference = null;
            PanelState = PanelState.Destroy;
            base.Dispose();
        }

        internal void RequireInitialized()
        {
            if (PanelState == PanelState.UnInitialize || PanelState == PanelState.Destroy)
                throw new InvalidOperationException($"Panel {GetType().Name} is {PanelState}.");
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (Visible) Update(elapseSeconds, realElapseSeconds);
        }

        protected virtual void Update(float elapseSeconds, float realElapseSeconds) { }
    }
}
