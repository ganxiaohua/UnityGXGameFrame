#if GAMEFRAME_USE_UNITYUI
namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        public static bool UsesUnityUI => true;

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
