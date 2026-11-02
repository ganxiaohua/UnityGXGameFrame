#if GAMEFRAME_USE_UNITYUI
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFrame.Runtime.UnityUI
{
    public sealed partial class UISystem
    {
        public async UniTask<Panel> LoadUniquePanelAsync(Type type, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();
            var panel = FindFirstOfPanel(type);
            if (panel == null) return await CreatePanelAsync(type, cancelToken);
            if (pendingCreations.TryGetValue(panel, out var pending))
                return await pending.Completion.Task.AttachExternalCancellation(cancelToken);
            return panel;
        }

        public async UniTask<T> LoadUniquePanelAsync<T>(CancellationToken cancelToken = default) where T : Panel, new()
            => (T)await LoadUniquePanelAsync(typeof(T), cancelToken);

        public async UniTask ShowUniquePanelAsync(Type type, object args = null, float percentComplete = 0f,
            CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();
            var panel = FindFirstOfPanel(type);
            var version = 0;
            if (panel == null)
                panel = await CreatePanelAsync(type, cancelToken);
            else if (pendingCreations.TryGetValue(panel, out var pending))
            {
                panel.SetVersionDirty();
                version = panel.Versions;
                panel = await pending.Completion.Task.AttachExternalCancellation(cancelToken);
            }
            else
                version = panel.Versions;

            if (panel == null || panel.PanelState == PanelState.Destroy || panel.Versions != version) return;
            while (percentComplete > panel.AssetReference.PercentComplete)
            {
                await UniTask.Yield(cancelToken);
                if (panel.PanelState == PanelState.Destroy || panel.Versions != version) return;
            }
            cancelToken.ThrowIfCancellationRequested();
            ShowPanel(panel, args);
        }

        public UniTask ShowUniquePanelAsync<T>(object args = null, float percentComplete = 0f,
            CancellationToken cancelToken = default) where T : Panel, new()
            => ShowUniquePanelAsync(typeof(T), args, percentComplete, cancelToken);

        public void HideUniquePanel(Type type)
        {
            var panel = FindFirstOfPanel(type);
            if (panel == null) return;
            if (panel.Visible) HidePanel(panel);
            else panel.SetVersionDirty();
        }

        public void HideUniquePanel<T>() where T : Panel, new() => HideUniquePanel(typeof(T));
        public void DestroyUniquePanel(Type type)
        {
            var panel = FindFirstOfPanel(type);
            if (panel != null) DestroyPanel(panel);
        }
        public void DestroyUniquePanel<T>() where T : Panel, new() => DestroyUniquePanel(typeof(T));
    }
}
#endif
