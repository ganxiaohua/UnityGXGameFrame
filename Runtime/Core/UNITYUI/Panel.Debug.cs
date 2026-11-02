#if UNITY_EDITOR
using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    public abstract partial class Panel
    {
        private void RegisterDebugInspector()
        {
            if (!Application.isPlaying) return;
            var inspector = Root.gameObject.AddComponent<PanelDebugInspector>();
            inspector.Panel = this;
        }
    }
}
#endif
