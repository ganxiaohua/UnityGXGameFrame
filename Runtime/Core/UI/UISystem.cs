using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Eden.Core.Runtime.UI.Platform;
using FairyGUI;
using UnityEngine;
using UnityEngine.Pool;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem : Singleton<UISystem>
    {
        private readonly List<Panel> panels = new(16);
        private readonly List<Panel> visiblePanels = new(8);
        private readonly List<Panel> monoPanelStack = new(8);
        private readonly List<Panel> delayDestroyPanels = new(8);

        public IKeyboardHandler KeyboardHandler { get; set; } = new DefaultKeyboardHandler();

        public UISystem()
        {
        }

        public GObject CreateGObject(string packageName, string name)
        {
            var reference = new DefaultAssetReference();
            var go = UIPackageHelper.CreateGObjectImmediate(packageName, name, reference);
            DefaultAssetReferenceContainer.Bind(go.displayObject.gameObject, reference);
            return go;
        }

        public async UniTask<GObject> CreateGObjectAsync(string packageName, string name,
                bool fromResources = false, CancellationToken cancelToken = default)
        {
            var reference = new DefaultAssetReference();
            var assetHandleType = fromResources ? typeof(UIPackageResourcesHandle) : typeof(UIPackageAddressableHandle);
            var package = await UIPackageHelper.LoadPackageAsync(packageName, assetHandleType, reference, cancelToken);
            if (package == null)
            {
                reference.Dispose();
                return null;
            }

            var go = UIPackageHelper.CreateGObjectImmediate(packageName, name, reference);
            DefaultAssetReferenceContainer.Bind(go.displayObject.gameObject, reference);
            return go;
        }

        public async UniTask<Panel> CreatePanelAsync(Type type, CancellationToken cancelToken = default)
        {
            var panel = (Panel) GXGameFrame.Instance.RootEntity.GetComponent<UIRootComponents>().AddComponent(type);
            panels.Add(panel);
            var reference = panel.AssetReference;
            var fromResources = (panel.Flags & PanelFlag.Builtin) != 0;
            var assetHandleType = fromResources ? typeof(UIPackageResourcesHandle) : typeof(UIPackageAddressableHandle);
            var package = await UIPackageHelper.LoadPackageAsync(panel.Package, assetHandleType, reference, cancelToken);
            if (package == null)
            {
                reference.Dispose();
                panel.SetVersionDirty();
                panels.RemoveSwapBack(panel);
                return null;
            }

            var go = UIPackageHelper.CreateGObjectImmediate(panel.Package, panel.Name, reference);
            await panel.OnInitializeAsync(go as GComponent, cancelToken);
            panel.OnInitializeComplete();
            return panel;
        }

        public async UniTask<T> CreatePanelAsync<T>(CancellationToken cancelToken = default) where T : Panel, new()
        {
            return (T) (await CreatePanelAsync(typeof(T), cancelToken));
        }

        public void ShowPanel(Panel panel, object args = null)
        {
            // hide panel first if panel is open
            if (panel.State == PanelState.Open) HidePanel(panel);
            Assert.IsFalse(visiblePanels.Contains(panel), $"Bad Panel({panel}, state:{panel.State}) OnShow|OnHide Call");
            panel.OnBeforeShow();
            if (panel.Mode == PanelMode.Mono)
            {
                if (monoPanelStack.TryPeek(out var topPanel) && topPanel.Visible)
                {
                    visiblePanels.RemoveSwapBack(topPanel);
                    topPanel.OnHide();
                }

                monoPanelStack.Remove(panel);
                monoPanelStack.Add(panel);
            }

            visiblePanels.Add(panel);
            panel.OnShow(args);
        }

        /* never used, redundant function?
        public void RestorePanel(Panel panel)
        {
            if (panel.State == PanelState.Open) return;
            Assert.IsFalse(visiblePanels.Contains(panel), $"Bad Panel({panel}, state:{panel.State}) OnShow|OnHide Call");
            if (panel.Mode == PanelMode.Mono)
            {
                if (monoPanelStack.TryPeek(out var topPanel) && topPanel.Visible)
                {
                    visiblePanels.RemoveSwapBack(topPanel);
                    topPanel.OnHide();
                }
                monoPanelStack.Remove(panel);
                monoPanelStack.Add(panel);
            }
            visiblePanels.Add(panel);
            panel.OnRestore();
        }
        */

        public void HidePanel(Panel panel)
        {
            if (panel.State == PanelState.Hide) return;
            Assert.IsTrue(visiblePanels.Contains(panel), $"Bad Panel({panel}, state:{panel.State}) OnShow|OnHide Call");
            visiblePanels.RemoveSwapBack(panel);
            panel.OnHide();
            if (panel.Mode == PanelMode.Mono)
            {
                Assert.IsTrue(monoPanelStack.Count > 0 && monoPanelStack.Peek() == panel, $"Bad Mono Panel({panel}, state:{panel.State}) OnShow|OnHide Call");
                monoPanelStack.Pop();
                if (monoPanelStack.TryPeek(out var topPanel))
                {
                    Assert.IsFalse(visiblePanels.Contains(topPanel), $"Bad Mono Panel({topPanel}, state:{topPanel.State}) OnShow|OnHide Call");
                    visiblePanels.Add(topPanel);
                    topPanel.OnRestore();
                }
            }

            TryDestroyHiddenPanel(panel);
            panel.OnAfterHide();
        }

        public void HideAllPanel()
        {
            MonoStackClear();
            StashClear();

            while (visiblePanels.TryPeek(out var panel))
            {
                Assert.IsTrue(panel.Visible, $"Bad Panel({panel}, state:{panel.State}) OnShow|OnHide Call");
                HidePanel(panel);
            }
        }

        public void DestroyPanel(Panel panel)
        {
            if (panel.State == PanelState.Destroy) return;
            if (panel.State == PanelState.UnInitialize)
            {
                panel.SetVersionDirty();
                // remove first, then add again with latest version
                delayDestroyPanels.Remove(panel);
                delayDestroyPanels.Add(panel);
                return;
            }

            if (panel.Mode == PanelMode.Mono)
            {
                monoPanelStack.Remove(panel);
            }

            // destroy childs here
            while (panel.Childs.TryPeek(out var child))
            {
                Assert.AreNotEqual(PanelState.Destroy, child.State, $"Bad Panel({child}, state:{child.State}) OnDestroy Call");
                DestroyPanel(child);
            }

            panels.RemoveSwapBack(panel);
            if (panel.Visible)
            {
                visiblePanels.RemoveSwapBack(panel);
            }

            GXGameFrame.Instance.RootEntity.GetComponent<UIRootComponents>().RemoveComponent(panel.GetType());
        }

        /// <summary>
        /// Destroy all panels except persistent or stash panels
        /// </summary>
        public void DestroyUnimportantPanels()
        {
            var tmpList = ListPool<Panel>.Get();
            while (panels.TryPop(out var panel))
            {
                if (panel.State == PanelState.Destroy)
                    continue;
                if ((panel.Flags & PanelFlag.Persistent) != 0 || IsInsideStashStack(panel))
                    tmpList.Add(panel);
                else
                    DestroyPanel(panel);
            }

            foreach (var panel in tmpList)
            {
                if (panel.State != PanelState.Destroy)
                {
                    panels.Add(panel);
                }
            }

            ListPool<Panel>.Release(tmpList);
            foreach (var panel in panels)
            {
                panel.OnDestroySuppressed();
            }
        }

        public T FindFirstOfPanel<T>(bool includeInvisible = true) where T : Panel
        {
            return (T) FindFirstOfPanel(typeof(T), includeInvisible);
        }

        public Panel FindFirstOfPanel(Type type, bool includeInvisible = true)
        {
            foreach (var panel in includeInvisible ? panels : visiblePanels)
            {
                if (panel.GetType() == type)
                    return panel;
            }

            return default;
        }

        /// <summary>
        /// Clear mono stack except top element
        /// </summary>
        public void MonoStackClear()
        {
            for (int i = monoPanelStack.Count - 2; i >= 0; i--)
            {
                var panel = monoPanelStack[i];
                monoPanelStack.RemoveAt(i);
                TryDestroyHiddenPanel(panel);
            }
        }

        public Panel GetCurrentMonoPanel()
        {
            if (monoPanelStack.TryPeek(out var panel))
                return panel;
            return null;
        }

        public bool IsDisplayTopmost(Panel panel)
        {
            if (!panel.Visible)
                return false;
            var order = panel.Root.displayObject.renderingOrder;
            foreach (var visiblePanel in visiblePanels)
            {
                if (visiblePanel.Parent == panel)
                    continue;
                if ((visiblePanel.Flags & PanelFlag.NonTopmost) != 0)
                    continue;
                var displayObject = visiblePanel.Root.displayObject;
                if (!displayObject.gameObject.activeInHierarchy)
                    continue;
                if (displayObject.renderingOrder > order)
                    return false;
            }

            return true;
        }

        public bool IsInsideMonoStack(Panel panel)
        {
            return monoPanelStack.Contains(panel);
        }

        public bool IsInsideStashStack(Panel panel)
        {
            foreach (var stashPanels in stashPanelsStack)
            {
                if (stashPanels.Contains(panel))
                    return true;
            }

            return false;
        }

        public void Update()
        {
            for (var i = delayDestroyPanels.Count - 1; i >= 0; i--)
            {
                var panel = (Panel) delayDestroyPanels[i];
                if (panel == null)
                {
                    // destroy operation was abort
                    delayDestroyPanels.RemoveAtSwapBack(i);
                }
                else if (panel.State != PanelState.UnInitialize)
                {
                    // initialize finish, could safely destroy immediately
                    delayDestroyPanels.RemoveAtSwapBack(i);
                    DestroyPanel(panel);
                }
            }

            // remove actually invisible panels
            for (var i = visiblePanels.Count - 1; i >= 0; i--)
            {
                var panel = visiblePanels[i];
                if (!panel.Root.displayObject.gameObject.activeSelf)
                {
                    HidePanel(panel);
                    i = Mathf.Min(i, visiblePanels.Count);
                }
            }

            UpdateScale();

            KeyboardHandler.OnUpdateKeyboardEvent();
        }

        private void TryDestroyHiddenPanel(Panel panel)
        {
            if ((panel.Flags & PanelFlag.Persistent) != 0)
                return;
            if (float.IsInfinity(panel.LifetimeAfterHide))
                return;
            if (panel.Mode == PanelMode.Mono && IsInsideMonoStack(panel))
                return;
            if (IsInsideStashStack(panel))
                return;
            panel.DestroyTimer.Schedule(panel.LifetimeAfterHide);
        }

        public void Dispose()
        {
        }
    }
}