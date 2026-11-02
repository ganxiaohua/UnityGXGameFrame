#if GAMEFRAME_USE_UNITYUI
namespace GameFrame.Runtime.UnityUI
{
    public sealed class NoKeyboardHandler : IKeyboardHandler
    {
        public void OnUpdateKeyboardEvent() { }
    }
}
#endif
