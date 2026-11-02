#if GAMEFRAME_USE_UNITYUI
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    [HideMonoScript]
    public sealed class PanelDebugInspector : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] public Panel Panel { get; internal set; }
        [ShowInInspector] private PanelState State => Panel.PanelState;
        [ShowInInspector] private PanelMode Mode => Panel.Mode;
        [ShowInInspector] private int Version => Panel.Versions;
        [ShowInInspector] private string Prefab => Panel.PrefabPath;

        [Button] private void Show() => UISystem.Instance.ShowPanel(Panel);
        [Button] private void Hide() => UISystem.Instance.HidePanel(Panel);
        [Button] private void DestroyPanel() => UISystem.Instance.DestroyPanel(Panel);
    }
}
#endif
#endif
