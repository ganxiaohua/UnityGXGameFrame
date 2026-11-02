#if GAMEFRAME_USE_UNITYUI
using System;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public static bool UsesUnityUI => true;

        public UnityUI.Panel AddUIComponent(Type type)
            => (UnityUI.Panel)RootEntity.GetComponent<UIRootComponents>().AddChild(type);

        public void RemoveUIComponent(UnityUI.Panel panel)
            => RootEntity.GetComponent<UIRootComponents>().RemoveChild(panel);

        static partial void InitializeUISystem() => UnityUI.UISystem.Instance.Initialize();
        static partial void UpdateUISystem() => UnityUI.UISystem.Instance.Update();
        static partial void DisposeUISystem() => UnityUI.UISystem.Instance.Dispose();

#if UNITY_EDITOR
        [Sirenix.OdinInspector.ShowInInspector]
        private UnityUI.UISystem UISystem => UnityUI.UISystem.Instance;
#endif
    }
}
#endif
