#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    public abstract partial class Panel
    {
        [HideMonoScript]
        private class DebugInspector : MonoBehaviour
        {
            private Panel panel;

            public static void Register(Panel panel)
            {
                if (!Application.isPlaying) return;

                var go = panel.Root?.displayObject.gameObject;
                if (!go) return;

                if (!go.TryGetComponent(out DebugInspector panelDebug))
                    panelDebug = go.AddComponent<DebugInspector>();

                panelDebug.panel = panel;
            }

            [ShowInInspector]
            [InlineButton(nameof(OpenScript), "Open")]
            private string Script
            {
                get { return panel.GetType().Name; }
                set { _ = value; }
            }

            private void OpenScript()
            {
                DebugHelper.OpenScript(Script);
            }

            [ShowInInspector]
            private PanelMode Mode
            {
                get { return panel.Mode; }
            }

            [ShowInInspector]
            private PanelFlag Flags
            {
                get { return panel.Flags; }
            }

            [ShowInInspector]
            private int SortingOrder
            {
                get { return panel.SortingOrder; }
            }


            [ShowInInspector]
            private int Version
            {
                get { return panel.Versions; }
            }

            [ShowInInspector]
            private string Lifetime
            {
                get
                {
                    var lifetime = panel.DestroyTimer.ExpireTime;
                    if (!float.IsInfinity(lifetime))
                    {
                        lifetime -= Time.deltaTime;
                        lifetime = Mathf.Max(0, lifetime);
                    }

                    return lifetime.PrettySeconds();
                }
            }

            [ShowInInspector]
            private string LifetimeAfterHide
            {
                get { return panel.LifetimeAfterHide.PrettySeconds(); }
            }

            [ShowInInspector]
            private GameObject Parent
            {
                get { return panel.Parent?.Root?.displayObject?.gameObject; }
            }

            [ShowInInspector] private IAssetReference AssetReference => panel.AssetReference;

            [ShowInInspector, HideLabel, HideReferenceObjectPicker, FoldoutGroup("Custom Data")]
            private Panel Panel
            {
                get => panel;
                set => _ = value;
            }

            private bool Visible => panel.Visible;

            [Button("Show Panel"), HideIf(nameof(Visible)), ButtonGroup]
            private void OdinShowPanel()
            {
                UISystem.Instance.ShowPanel(panel);
            }

            [Button("Hide Panel"), ShowIf(nameof(Visible)), ButtonGroup]
            private void OdinHidePanel()
            {
                UISystem.Instance.HidePanel(panel);
            }

            [Button("Destroy Panel"), GUIColor(1f, 0.5f, 0.3f), ButtonGroup]
            private void OdinDestroy()
            {
                UISystem.Instance.DestroyPanel(panel);
            }
        }
    }
}
#endif