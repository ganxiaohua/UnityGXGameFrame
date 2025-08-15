#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        private struct DebugInfo
        {
            public UISystem UISystem { get; set; }


            [ShowInInspector]
            private List<GameObject> Panels
            {
                get
                {
                    var list = new List<GameObject>();
                    foreach (var panel in UISystem.panels)
                    {
                        list.Add(panel.Root?.displayObject.gameObject);
                    }

                    return list;
                }
            }

            [ShowInInspector]
            private List<GameObject> VisiblePanels
            {
                get
                {
                    var list = new List<GameObject>();
                    foreach (var panel in UISystem.visiblePanels)
                    {
                        list.Add(panel.Root?.displayObject.gameObject);
                    }

                    return list;
                }
            }

            [ShowInInspector]
            private List<GameObject> MonoPanelStack
            {
                get
                {
                    var list = new List<GameObject>();
                    foreach (var panel in UISystem.monoPanelStack)
                    {
                        list.Add(panel.Root?.displayObject.gameObject);
                    }

                    return list;
                }
            }

            [ShowInInspector]
            private List<List<GameObject>> StashPanelsStack
            {
                get
                {
                    var list = new List<List<GameObject>>();
                    foreach (var stashPanels in UISystem.stashPanelsStack)
                    {
                        var list2 = new List<GameObject>();
                        foreach (var panel in stashPanels)
                        {
                            list2.Add(panel.Root?.displayObject.gameObject);
                        }

                        list.Add(list2);
                    }

                    return list;
                }
            }

            [ShowInInspector]
            private List<string> DelayDestroyPanels
            {
                get
                {
                    var list = new List<string>();
                    foreach (var panel in UISystem.delayDestroyPanels)
                    {
                        list.Add(panel.GetType().Name);
                    }

                    return list;
                }
            }

            [Button]
            private void MonoStackClear()
            {
                UISystem.MonoStackClear();
            }

            [Button, ButtonGroup]
            private void StashPush()
            {
                UISystem.StashPush();
            }

            [Button, ButtonGroup]
            private void StashPop()
            {
                UISystem.StashPop();
            }

            [Button, ButtonGroup]
            private void StashClear()
            {
                UISystem.StashClear();
            }
        }

        [ShowInInspector, HideLabel, HideInEditorMode]
        private DebugInfo _DebugInfo
        {
            get => new DebugInfo {UISystem = this};
            set => _ = value;
        }
    }
}
#endif