using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        private readonly List<List<Panel>> stashPanelsStack = new List<List<Panel>>();

        public void StashPush()
        {
            var stashPanels = ListPool<Panel>.Get();
            var top = GetCurrentMonoPanel();
            stashPanels.AddRange(monoPanelStack);
            stashPanelsStack.Add(stashPanels);
            monoPanelStack.Clear();
            if (top != null)
            {
                visiblePanels.RemoveSwapBack(top);
                top.OnHide();
            }

            for (int i = visiblePanels.Count - 1; i >= 0; i--)
            {
                var panel = visiblePanels[i];
                if ((panel.Flags & PanelFlag.Stashable) != 0)
                {
                    stashPanels.Add(panel);
                }
            }

            HidePanelsWithStashFlag();
        }

        public void StashClear()
        {
            for (int i = stashPanelsStack.Count - 1; i >= 0; i--)
            {
                var stashPanels = stashPanelsStack[i];
                stashPanelsStack.RemoveAt(i);
                foreach (var panel in stashPanels)
                {
                    TryDestroyHiddenPanel(panel);
                }

                ListPool<Panel>.Release(stashPanels);
            }

            stashPanelsStack.Clear();
        }

        public void StashPop()
        {
            if (!stashPanelsStack.TryPop(out var stashPanels))
                return;

            var top = GetCurrentMonoPanel();
            HidePanelsWithStashFlag();
            MonoStackClear();
            monoPanelStack.Clear();

            foreach (var panel in stashPanels)
            {
                if (panel.State != PanelState.Destroy)
                {
                    panel.DestroyTimer.Cancel();
                    if (panel.Mode == PanelMode.Mono)
                    {
                        monoPanelStack.Add(panel);
                    }
                    else if (panel.State != PanelState.Open)
                    {
                        visiblePanels.Add(panel);
                        panel.OnRestore();
                    }
                }
            }

            ListPool<Panel>.Release(stashPanels);

            if (top != null && top != GetCurrentMonoPanel() && top.State == PanelState.Open)
            {
                visiblePanels.RemoveSwapBack(top);
                top.OnHide();
                TryDestroyHiddenPanel(top);
            }

            top = GetCurrentMonoPanel();
            if (top != null && top.State != PanelState.Open)
            {
                visiblePanels.Add(top);
                top.OnRestore();
            }
        }

        private void HidePanelsWithStashFlag()
        {
            for (int i = visiblePanels.Count - 1; i >= 0; i--)
            {
                var panel = visiblePanels[i];
                if ((panel.Flags & PanelFlag.Stashable) != 0)
                {
                    visiblePanels.RemoveSwapBack(panel);
                    panel.OnHide();
                    i = Mathf.Min(i, visiblePanels.Count);
                }
            }
        }
    }
}