#if !GAMEFRAME_USE_UNITYUI
using System;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public static bool UsesUnityUI => false;

        public Panel AddUIComponent(Type type)
            => (Panel)RootEntity.GetComponent<UIRootComponents>().AddComponent(type);

        public void RemoveUIComponent(Panel panel)
            => RootEntity.GetComponent<UIRootComponents>().RemoveComponent(panel);

        static partial void InitializeUISystem() { }
        static partial void UpdateUISystem() => UISystem.Instance.Update();
        static partial void DisposeUISystem() => UISystem.Instance.Dispose();

#if UNITY_EDITOR
        [Sirenix.OdinInspector.ShowInInspector]
        private UISystem UISystem => GameFrame.Runtime.UISystem.Instance;
#endif
    }
}
#endif
