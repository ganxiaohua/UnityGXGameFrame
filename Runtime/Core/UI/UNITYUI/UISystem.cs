#if GAMEFRAME_USE_UNITYUI
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    public sealed partial class UISystem : Singleton<UISystem>, IDisposable
    {
        private readonly List<Panel> panels = new(16);
        private readonly List<Panel> visiblePanels = new(8);
        private readonly List<Panel> monoPanelStack = new(8);
        private readonly Dictionary<Panel, PendingCreation> pendingCreations = new();
        private UIRootComponents entityOwner;
        private bool disposing;

        private sealed class PendingCreation
        {
            public readonly Panel Panel;
            public readonly UIRootComponents Owner;
            public readonly CancellationTokenSource Cancellation;
            public readonly UniTaskCompletionSource<Panel> Completion = new();

            public PendingCreation(Panel panel, UIRootComponents owner, CancellationToken cancelToken)
            {
                Panel = panel;
                Owner = owner;
                Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
            }
        }

        public UniTask<Panel> CreatePanelAsync(Type type, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();
            if (type == null || !typeof(Panel).IsAssignableFrom(type) || type.IsAbstract)
                throw new ArgumentException("Expected a concrete UnityUI.Panel type.", nameof(type));
            if (disposing) throw new ObjectDisposedException(nameof(UISystem));
            Initialize();
            // Children allow multiple instances; Unique APIs perform their own type lookup.
            var panel = (Panel)entityOwner.AddChild(type);
            panels.Add(panel);
            var pending = new PendingCreation(panel, entityOwner, cancelToken);
            pendingCreations.Add(panel, pending);
            InitializePanelAsync(pending).Forget();
            return pending.Completion.Task.AttachExternalCancellation(cancelToken);
        }

        public async UniTask<T> CreatePanelAsync<T>(CancellationToken cancelToken = default) where T : Panel, new()
            => (T)await CreatePanelAsync(typeof(T), cancelToken);

        private async UniTask InitializePanelAsync(PendingCreation pending)
        {
            var panel = pending.Panel;
            var token = pending.Cancellation.Token;
            GameObject instance = null;
            var succeeded = false;
            try
            {
                var prefab = await LoadPrefabAsync(panel, token).AttachExternalCancellation(token);
                token.ThrowIfCancellationRequested();
                if (prefab == null) throw new InvalidOperationException($"UI prefab not found: {panel.PrefabPath}");
                instance = UnityEngine.Object.Instantiate(prefab, inactiveRoot);
                instance.name = panel.Name;
                if (!instance.TryGetComponent<RectTransform>(out var root))
                    throw new InvalidOperationException($"UI prefab requires a RectTransform: {panel.PrefabPath}");
                panel.BindRoot(root);
                instance = null; // Panel owns the instance from this point.
                await panel.OnInitializeAsync(root, token);
                token.ThrowIfCancellationRequested();
                panel.OnInitializeComplete();
                succeeded = true;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (instance != null) UnityEngine.Object.Destroy(instance);
                pendingCreations.Remove(panel);
                if (!succeeded)
                {
                    // Remove the failed entity before publishing completion to other callers.
                    while (panel.Childs.Count > 0) DestroyPanel(panel.Childs[panel.Childs.Count - 1]);
                    panels.Remove(panel);
                    pending.Owner.RemoveChild(panel);
                }
                pending.Cancellation.Dispose();
                pending.Completion.TrySetResult(succeeded ? panel : null);
            }
        }

        private static async UniTask<GameObject> LoadPrefabAsync(Panel panel, CancellationToken token)
        {
            // Keep a separate reference until a shared load finishes. Cancelling one window
            // must not release AssetManager's shared handle while another window is loading it.
            var loadingReference = new DefaultAssetReference();
            var path = panel.PrefabPath;
            var panelReference = panel.AssetReference;
            try
            {
                var handleType = (panel.Flags & PanelFlag.Builtin) != 0
                    ? typeof(ResourcesPrefabHandle) : typeof(DefaultAssetHandle);
                var handle = await AssetManager.Instance.LoadAsync(path, typeof(GameObject), handleType, loadingReference);
                token.ThrowIfCancellationRequested();
                var prefab = handle.Result as GameObject;
                if (prefab != null) panelReference.RefAsset(path);
                return prefab;
            }
            finally
            {
                loadingReference.Dispose();
            }
        }

        public void ShowPanel(Panel panel, object args = null)
        {
            panel.RequireInitialized();
            if (panel.Visible) return;
            panel.OnBeforeShow();
            if (panel.Mode == PanelMode.Mono)
            {
                var top = GetCurrentMonoPanel();
                if (top != null && top.Visible)
                {
                    visiblePanels.Remove(top);
                    top.OnHide();
                }
                monoPanelStack.Remove(panel);
                monoPanelStack.Add(panel);
            }
            visiblePanels.Add(panel);
            panel.OnShow(args);
        }

        public void HidePanel(Panel panel)
        {
            panel.RequireInitialized();
            if (!panel.Visible) return;
            visiblePanels.Remove(panel);
            panel.OnHide();
            if (panel.Mode == PanelMode.Mono)
            {
                monoPanelStack.Remove(panel);
                RestoreMonoTop();
            }
            TryDestroyHiddenPanel(panel);
            panel.OnAfterHide();
        }

        public void HideAllPanel()
        {
            MonoStackClear();
            StashClear();
            while (visiblePanels.Count > 0) HidePanel(visiblePanels[visiblePanels.Count - 1]);
        }

        public void DestroyPanel(Panel panel)
        {
            if (panel.PanelState == PanelState.Destroy || !panels.Contains(panel)) return;
            if (pendingCreations.TryGetValue(panel, out var pending))
            {
                panel.SetVersionDirty();
                pending.Cancellation.Cancel();
                pending.Completion.TrySetResult(null);
                return; // Do not recycle an entity whose user initialization is still running.
            }
            var wasMonoTop = GetCurrentMonoPanel() == panel;
            monoPanelStack.Remove(panel);
            foreach (var stash in stashPanelsStack) stash.Remove(panel);
            while (panel.Childs.Count > 0) DestroyPanel(panel.Childs[panel.Childs.Count - 1]);
            panels.Remove(panel);
            visiblePanels.Remove(panel);
            entityOwner.RemoveChild(panel);
            if (wasMonoTop && !disposing) RestoreMonoTop();
        }

        public void DestroyUnimportantPanels()
        {
            foreach (var panel in panels.ToArray())
            {
                if (!panels.Contains(panel)) continue;
                if ((panel.Flags & PanelFlag.Persistent) == 0 && !IsInsideStashStack(panel)) DestroyPanel(panel);
            }
            foreach (var panel in panels.ToArray()) panel.OnDestroySuppressed();
        }

        public T FindFirstOfPanel<T>(bool includeInvisible = true) where T : Panel
            => (T)FindFirstOfPanel(typeof(T), includeInvisible);

        public Panel FindFirstOfPanel(Type type, bool includeInvisible = true)
        {
            foreach (var panel in includeInvisible ? panels : visiblePanels)
            {
                if (panel.GetType() != type) continue;
                if (pendingCreations.TryGetValue(panel, out var pending) && pending.Cancellation.IsCancellationRequested) continue;
                return panel;
            }
            return null;
        }

        public void MonoStackClear()
        {
            for (var i = monoPanelStack.Count - 2; i >= 0; i--)
            {
                var panel = monoPanelStack[i];
                monoPanelStack.RemoveAt(i);
                TryDestroyHiddenPanel(panel);
            }
        }

        public Panel GetCurrentMonoPanel() => monoPanelStack.Count == 0 ? null : monoPanelStack[monoPanelStack.Count - 1];
        public bool IsInsideMonoStack(Panel panel) => monoPanelStack.Contains(panel);

        public bool IsInsideStashStack(Panel panel)
        {
            foreach (var stash in stashPanelsStack) if (stash.Contains(panel)) return true;
            return false;
        }

        private void RestoreMonoTop()
        {
            var top = GetCurrentMonoPanel();
            if (top == null || top.Visible) return;
            visiblePanels.Add(top);
            top.OnRestore();
        }

        private void TryDestroyHiddenPanel(Panel panel)
        {
            if (panel.Visible || panel.PanelState == PanelState.Destroy) return;
            if ((panel.Flags & PanelFlag.Persistent) != 0 || float.IsInfinity(panel.LifetimeAfterHide)) return;
            if (IsInsideMonoStack(panel) || IsInsideStashStack(panel)) return;
            panel.DestroyTimer.Schedule(panel.LifetimeAfterHide);
        }

        public void Update()
        {
            for (var i = visiblePanels.Count - 1; i >= 0; i--)
            {
                var panel = visiblePanels[i];
                if (panel.Root == null) DestroyPanel(panel);
                else if (!panel.Root.gameObject.activeSelf) HidePanel(panel);
                i = Mathf.Min(i, visiblePanels.Count);
            }
            if (globalInputEnabled) KeyboardHandler?.OnUpdateKeyboardEvent();
        }

        public void Dispose()
        {
            if (disposing) return;
            disposing = true;
            monoPanelStack.Clear();
            StashClear();
            foreach (var panel in panels.ToArray()) DestroyPanel(panel);
            DestroyRoot();
            TimerSystem.Instance.UIGroup.Clear();
        }
    }
}
#endif
