#if !GAMEFRAME_USE_UNITYUI
namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public static bool UsesUnityUI => false;

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
