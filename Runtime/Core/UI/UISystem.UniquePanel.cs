using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        public async UniTask<Panel> LoadUniquePanelAsync(Type type, CancellationToken cancelToken = default)
        {
            var panel = FindFirstOfPanel(type);
            if (panel != null)
            {
                while (panel.State == PanelState.UnInitialize)
                {
                    await UniTask.Yield();
                }
            }

            if (panel == null || panel.State == PanelState.Destroy)
            {
                panel = await CreatePanelAsync(type, cancelToken);
            }

            return panel;
        }

        public async UniTask<T> LoadUniquePanelAsync<T>(CancellationToken cancelToken = default) where T : Panel, new()
        {
            return (T) (await LoadUniquePanelAsync(typeof(T), cancelToken));
        }

        public async UniTask ShowUniquePanelAsync(Type type, object args = null, float percentComplete = 0f,
                CancellationToken cancelToken = default)
        {
            var panel = FindFirstOfPanel(type);
            if (panel != null)
            {
                Assert.AreNotEqual(PanelState.Destroy, panel.State, $"Panel({panel}) already destroyed");

                if (panel.State == PanelState.UnInitialize)
                {
                    panel.SetVersionDirty();

                    var version = panel.Versions;
                    while (panel.State == PanelState.UnInitialize)
                    {
                        await UniTask.Yield();
                        if (cancelToken.IsCancellationRequested)
                            return;
                        if (version != panel.Versions)
                            return;
                    }
                }
            }
            else
            {
                panel = await CreatePanelAsync(type, cancelToken);
                if (panel == null)
                    return;
                if (0 != panel.Versions) // Normally, Panel version is 0 after CreatePanelAsync call
                    return;
            }

            if (percentComplete > 0)
            {
                var version = panel.Versions;
                while (percentComplete > panel.AssetReference.PercentComplete)
                {
                    await UniTask.Yield();
                    if (cancelToken.IsCancellationRequested)
                        return;
                    if (version != panel.Versions)
                        return;
                }
            }

            ShowPanel(panel, args);
        }

        public async UniTask ShowUniquePanelAsync<T>(object args = null, float percentComplete = 0f,
                CancellationToken cancelToken = default) where T : Panel, new()
        {
            await ShowUniquePanelAsync(typeof(T), args, percentComplete, cancelToken);
        }

        public void HideUniquePanel(Type type)
        {
            var panel = FindFirstOfPanel(type);
            if (panel != null)
            {
                Assert.AreNotEqual(PanelState.Destroy, panel.State, $"Panel({panel}) already destroyed");
                if (panel.Visible)
                {
                    HidePanel(panel);
                }
                else
                {
                    // mark dirty only
                    panel.SetVersionDirty();
                }
            }
        }

        public void HideUniquePanel<T>() where T : Panel, new()
        {
            HideUniquePanel(typeof(T));
        }

        public void DestroyUniquePanel(Type type)
        {
            var panel = FindFirstOfPanel(type);
            if (panel != null)
            {
                Assert.AreNotEqual(PanelState.Destroy, panel.State, $"Panel({panel}) already destroyed");
                DestroyPanel(panel);
            }
        }

        public void DestroyUniquePanel<T>() where T : Panel, new()
        {
            DestroyUniquePanel(typeof(T));
        }
    }
}