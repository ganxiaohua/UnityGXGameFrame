using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame.Runtime
{
    public abstract partial class Panel : SimpleEntity, IVersions
    {
        public abstract string Package { get; }

        public new abstract string Name { get; }

        public int Versions { get; set; }

        public virtual PanelMode Mode => PanelMode.Normal;

        public virtual PanelFlag Flags => PanelFlag.None;

        public virtual int SortingOrder => BuiltinPanelOrder.Normal;

        public virtual string AudioOnShow => null;

        public virtual float LifetimeAfterHide => (Flags & PanelFlag.Persistent) != 0 || Parent != null ? float.PositiveInfinity : 30;

        public new PanelState State { get; private set; }

        public bool Visible => State == PanelState.Open;

        public new Panel Parent { get; private set; }

        public List<Panel> Childs { get; private set; } = new List<Panel>();

        public DefaultAssetReference AssetReference { get; private set; } = new DefaultAssetReference();

        public GComponent Root { get; private set; }

        public GComponent ParentHoldComponent { get; private set; }

        private UniqueTimer _DestroyTimer;

        public UniqueTimer DestroyTimer
        {
            get
            {
                if (_DestroyTimer == null)
                {
                    _DestroyTimer = new UniqueTimer(() => UISystem.Instance.DestroyPanel(this));
#if UNITY_EDITOR
                    _DestroyTimer.Name = $"{Name} Destroy";
#endif
                }

                return _DestroyTimer;
            }
        }

        public void SetParent(Panel parent, GComponent holdComponent = null)
        {
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            if (holdComponent == null && parent != null) holdComponent = parent.Root;
            if (parent == Parent && holdComponent == ParentHoldComponent) return;
            if (Parent != null)
            {
                Parent.Childs.Remove(this);
                ParentHoldComponent.RemoveChild(Root);
            }

            if (parent != null)
            {
                Assert.AreNotEqual(PanelState.Destroy, parent.State, $"Panel({parent}) already destroyed");
                Assert.AreNotEqual(PanelMode.Mono, Mode, $"Child Panel({this}) Mode can't be Mono");
                parent.Childs.Add(this);
                Assert.IsTrue(parent.Root == holdComponent || parent.Root.IsAncestorOf(holdComponent), $"Panel({parent}) is not ancestor of hold component");
                holdComponent.AddChild(Root);
            }

            Parent = parent;
            ParentHoldComponent = holdComponent;
        }

        public void AddChild(Panel child, GComponent holdComponent = null)
        {
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            Assert.IsFalse(Childs.Contains(child), $"Panel({this}) already contain Child({child})");
            child.SetParent(this, holdComponent);
        }

        public void SetVersionDirty()
        {
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            Versions++;
        }

        /// <summary>
        /// Don't do any OnShow or OnHide call inside
        /// </summary>
        public virtual async UniTask OnInitializeAsync(GComponent root, CancellationToken cancelToken = default)
        {
            Assert.AreNotEqual(null, root, $"GComponent root is null");
            Assert.AreEqual(PanelState.UnInitialize, State, $"Panel({this}) already initialized");

            GRoot.inst.AddChild(root);
            if (Mode != PanelMode.Embed && Mode != PanelMode.Pop)
            {
                root.SetSize(GRoot.inst.width, GRoot.inst.height);
                root.AddRelation(GRoot.inst, RelationType.Size);
            }

            root.sortingOrder = SortingOrder;
            root.visible = false;
            Root = root;

#if UNITY_EDITOR
            DebugInspector.Register(this);
#endif

            await UniTask.Yield();
        }

        public virtual void OnInitializeComplete()
        {
            Assert.AreEqual(PanelState.UnInitialize, State, $"Panel({this}) already initialized");
            State = PanelState.Hide;
        }

        public virtual void OnBeforeShow()
        {
        }

        public virtual void OnShow(object args = null)
        {
            Assert.AreNotEqual(PanelState.UnInitialize, State, $"Panel({this}) not initialized");
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            // StackTraceVisualize.Register(Root.displayObject.gameObject);
            Versions++;
            _DestroyTimer?.Cancel();
            State = PanelState.Open;
            // EventSystem.Instance.Subscribe(this);

            DoShowAnimation();
            //
            // if (!string.IsNullOrEmpty(AudioOnShow))
            //     AudioSystem.Instance.PlaySE2D(AudioOnShow);
        }

        protected virtual void DoShowAnimation()
        {
            // makesure panel has an parent
            if (Root.parent == null)
                GRoot.inst.AddChild(Root);
            Root.visible = true;
        }

        public virtual void OnRestore()
        {
            Assert.AreNotEqual(PanelState.UnInitialize, State, $"Panel({this}) not initialized");
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            Versions++;
            _DestroyTimer?.Cancel();
            State = PanelState.Open;
            // EventSystem.Instance.Subscribe(this);

            DoRestoreAnimation();
        }

        protected virtual void DoRestoreAnimation()
        {
            // makesure panel has an parent
            if (Root.parent == null)
                GRoot.inst.AddChild(Root);
            Root.visible = true;
        }

        public virtual void OnHide()
        {
            Assert.AreNotEqual(PanelState.UnInitialize, State, $"Panel({this}) not initialized");
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            // StackTraceVisualize.Register(Root.displayObject.gameObject);
            Versions++;
            State = PanelState.Hide;
            // EventSystem.Instance.Unsubscribe(this);

            DoHideAnimation();
        }

        protected virtual void DoHideAnimation()
        {
            Root.visible = false;
        }

        public virtual void OnAfterHide()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Assert.AreNotEqual(PanelState.Destroy, State, $"Panel({this}) already destroyed");
            // if (Visible) EventSystem.Instance.Unsubscribe(this);
            Versions++;
            _DestroyTimer?.Cancel();
            SetParent(null);
            Assert.AreEqual(0, Childs.Count, $"Panel({this}) must destroyed by UISystem.DestroyPanel");
            AssetReference.Dispose();
            AssetReference = null;
            Root.Dispose();
            Root = null;
            State = PanelState.Destroy;
        }

        /// <summary>
        /// Calld when panel destroy was suppressed after UISystem.DestroyUnimportantPanels
        /// </summary>
        public virtual void OnDestroySuppressed()
        {
        }
    }
}