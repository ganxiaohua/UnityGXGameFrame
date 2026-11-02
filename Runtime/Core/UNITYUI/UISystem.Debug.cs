#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GameFrame.Runtime.UnityUI
{
    public sealed partial class UISystem
    {
        [ShowInInspector, ReadOnly] private IReadOnlyList<Panel> Panels => panels;
        [ShowInInspector, ReadOnly] private IReadOnlyList<Panel> VisiblePanels => visiblePanels;
        [ShowInInspector, ReadOnly] private IReadOnlyList<Panel> MonoPanels => monoPanelStack;
        [ShowInInspector, ReadOnly] private IReadOnlyList<List<Panel>> StashedPanels => stashPanelsStack;
        [ShowInInspector] private int InitializingCount => pendingCreations.Count;
    }
}
#endif
